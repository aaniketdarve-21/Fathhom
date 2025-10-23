using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;              // New Input System
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(Collider))]
public class GestureManipulator : MonoBehaviour
{
    [Header("Scaling (relative to size at spawn)")]
    [Tooltip("Minimum scale multiplier relative to the size at spawn.")]
    [Range(0.05f, 1f)] public float minScaleFactor = 0.2f;
    [Tooltip("Maximum scale multiplier relative to the size at spawn.")]
    [Range(1f, 10f)] public float maxScaleFactor = 5f;

    [Header("Rotation")]
    public float rotationSpeed = 1.0f; // multiplier for two-finger twist

    private ARRaycastManager raycastManager;
    private static readonly List<ARRaycastHit> hits = new();

    // gesture state
    private bool isDragging;           // one-finger drag on plane
    private float prevTwoFingerDist;    // for pinch
    private float prevTwoFingerAngle;   // for twist

    // scale clamps (absolute, computed from size at spawn)
    private float baseUniformScale;     // assumes uniform scaling (x==y==z)
    private float minAbsScale, maxAbsScale;

    void Awake()
    {
#if UNITY_2023_1_OR_NEWER
        raycastManager = UnityEngine.Object.FindFirstObjectByType<ARRaycastManager>();
#else
        raycastManager = UnityEngine.Object.FindObjectOfType<ARRaycastManager>();
#endif
    }

    // Use Start (not Awake) so PlaceOnPlane has set the initial scale
    void Start()
    {
        baseUniformScale = transform.localScale.x;
        minAbsScale = baseUniformScale * minScaleFactor;
        maxAbsScale = baseUniformScale * maxScaleFactor;
    }

    void Update()
    {
        var ts = Touchscreen.current;
        if (ts == null) return;

        // ----- two-finger gestures: scale + rotate -----
        if (ts.touches.Count >= 2 &&
            ts.touches[0].isInProgress && ts.touches[1].isInProgress)
        {
            Vector2 p0 = ts.touches[0].position.ReadValue();
            Vector2 p1 = ts.touches[1].position.ReadValue();

            float dist = Vector2.Distance(p0, p1);
            float angle = Mathf.Atan2(p1.y - p0.y, p1.x - p0.x) * Mathf.Rad2Deg;

            // pinch (scale)
            if (prevTwoFingerDist > 0f)
            {
                float deltaFactor = dist / prevTwoFingerDist; // >1 grow, <1 shrink
                float newUniform = Mathf.Clamp(transform.localScale.x * deltaFactor, minAbsScale, maxAbsScale);
                transform.localScale = new Vector3(newUniform, newUniform, newUniform);
            }
            prevTwoFingerDist = dist;

            // twist (rotate around Y)
            if (!float.IsNaN(prevTwoFingerAngle))
            {
                float delta = Mathf.DeltaAngle(prevTwoFingerAngle, angle);
                transform.Rotate(0f, delta * rotationSpeed, 0f, Space.World);
            }
            prevTwoFingerAngle = angle;

            isDragging = false; // while two-fingering, ignore drag
            return;
        }
        else
        {
            // reset two-finger memory when fingers lifted
            prevTwoFingerDist = 0f;
            prevTwoFingerAngle = 0f;
        }

        // ----- one finger drag on plane -----
        var primary = ts.primaryTouch;
        if (primary.press.wasPressedThisFrame) isDragging = true;
        if (primary.press.wasReleasedThisFrame) isDragging = false;

        if (isDragging && raycastManager != null && primary.isInProgress)
        {
            Vector2 pos = primary.position.ReadValue();
            if (raycastManager.Raycast(pos, hits, TrackableType.PlaneWithinPolygon))
            {
                var hit = hits[0].pose;
                transform.position = new Vector3(hit.position.x, hit.position.y, hit.position.z);
            }
        }
    }
}
