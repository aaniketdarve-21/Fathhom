using System;
using UnityEngine;
using DG.Tweening;   // DOTween

public class CouplingAnimator : MonoBehaviour
{
    [Header("Assign")]
    public Transform maleHalf;                // whole male assembly root
    public Transform maleFace;                // child on male sealing face (Z+ outwards)
    public Transform maleConnectedAnchor;     // child on female face (Z+ back toward male)

    [Header("Timing (s)")]
    public float connectSlideTime = 0.6f;
    public float lockTwistTime = 0.5f;
    public float unlockTwistTime = 0.5f;
    public float detachSlideTime = 0.6f;

    [Header("Twist")]
    [Tooltip("Positive = clockwise when looking from male toward female along +Z")]
    public float twistZDegrees = 30f;         // rotate around MALE local Z

    [Header("Behaviour")]
    public bool freezeDuringAnim = true;      // temporarily disable GestureManipulator

    // ---------- Events ----------
    public event Action OnConnected;          // after connect + twist completes
    public event Action OnDetached;           // after untwist + slide back completes
    public event Action<bool> OnFlow;         // true = start flow, false = stop

    // runtime
    Vector3 preConnectPos;
    Quaternion preConnectRot;
    Vector3 preConnectLocalEuler;         // for precise local-Z restore
    bool hasConnectPose;               // guard for Detach before Connect
    GestureManipulator gestures;
    Sequence seq;                          // active DOTween sequence
    public bool IsAnimating { get; private set; }

    void Awake()
    {
        if (!maleHalf || !maleFace || !maleConnectedAnchor)
        {
            Debug.LogError("[CouplingAnimator] Assign maleHalf, maleFace, maleConnectedAnchor.");
            enabled = false; return;
        }

#if UNITY_2023_1_OR_NEWER
        gestures = FindFirstObjectByType<GestureManipulator>();
#else
        gestures = FindObjectOfType<GestureManipulator>();
#endif
    }

    void OnDisable() => KillSeq();

    // ---------- Public API ----------
    public void PlayConnect()
    {
        KillSeq();
        Freeze(true);
        IsAnimating = true;

        // Remember user pose (world + local)
        preConnectPos = maleHalf.position;
        preConnectRot = maleHalf.rotation;
        preConnectLocalEuler = maleHalf.localEulerAngles;
        hasConnectPose = true;

        // Seat position (slide only along local Z)
        Vector3 seatPos = PositionToSeatFaceAlongLocalZ(maleHalf, maleFace, maleConnectedAnchor.position);

        // Target local-euler with Z rotated by +twistZDegrees
        Vector3 twistLocal = preConnectLocalEuler;
        twistLocal.z += twistZDegrees;

        seq = DOTween.Sequence().SetUpdate(true)
            .Append(maleHalf.DOMove(seatPos, Mathf.Max(0.01f, connectSlideTime)).SetEase(Ease.OutCubic))
            .Append(maleHalf.DOLocalRotate(twistLocal, Mathf.Max(0.01f, lockTwistTime), RotateMode.Fast).SetEase(Ease.OutBack))
            .OnComplete(() =>
            {
                IsAnimating = false;
                Freeze(false);
                OnConnected?.Invoke();
            })
            .OnKill(() => { IsAnimating = false; Freeze(false); });

        seq.Play();
    }

    public void PlayDetach()
    {
        if (!hasConnectPose) return; // nothing to detach yet

        KillSeq();
        Freeze(true);
        IsAnimating = true;

        // Untwist back to pre-connect local-euler, then slide back to pre-connect pos
        seq = DOTween.Sequence().SetUpdate(true)
            .Append(maleHalf.DOLocalRotate(preConnectLocalEuler, Mathf.Max(0.01f, unlockTwistTime), RotateMode.Fast).SetEase(Ease.InOutSine))
            .Append(maleHalf.DOMove(preConnectPos, Mathf.Max(0.01f, detachSlideTime)).SetEase(Ease.InCubic))
            .AppendCallback(() =>
            {
                // exact restore (just in case of tiny float drift)
                maleHalf.rotation = preConnectRot;
            })
            .OnComplete(() =>
            {
                IsAnimating = false;
                Freeze(false);
                OnDetached?.Invoke();
            })
            .OnKill(() => { IsAnimating = false; Freeze(false); });

        seq.Play();
    }

    public void PlayFlow() => OnFlow?.Invoke(true);
    public void PlayStop() => OnFlow?.Invoke(false);

    // ---------- Math ----------
    /// World position the male root must have after sliding along its local Z so that
    /// maleFace.position == targetFaceWorldPosition (rotation preserved).
    static Vector3 PositionToSeatFaceAlongLocalZ(Transform maleRoot, Transform maleFace, Vector3 targetFaceWorldPosition)
    {
        Vector3 d = targetFaceWorldPosition - maleFace.position;
        float alongZ = Vector3.Dot(d, maleRoot.forward);            // component along local Z
        return maleRoot.position + maleRoot.forward * alongZ;         // move root by that amount
    }

    void Freeze(bool f)
    {
        if (freezeDuringAnim && gestures) gestures.enabled = !f;
    }

    void KillSeq()
    {
        if (seq != null && seq.IsActive()) seq.Kill();
        seq = null;
        IsAnimating = false;
    }
}
