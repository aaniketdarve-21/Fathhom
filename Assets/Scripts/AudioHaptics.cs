using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioHapticsController : MonoBehaviour
{
    [Header("Assign")]
    public CouplingAnimator coupling;   // drag the object with CouplingAnimator

    [Header("Clips")]
    public AudioClip connectClip;
    public AudioClip detachClip;
    public AudioClip flowStartClip;
    public AudioClip flowStopClip;

    AudioSource src;

    void Awake()
    {
        src = GetComponent<AudioSource>();
        if (!coupling)
        {
#if UNITY_2023_1_OR_NEWER
            coupling = FindFirstObjectByType<CouplingAnimator>();
#else
            coupling = FindObjectOfType<CouplingAnimator>();
#endif
        }
    }

    void OnEnable()
    {
        if (!coupling) return;
        coupling.OnConnected += OnConnected;
        coupling.OnDetached += OnDetached;
        coupling.OnFlow += OnFlow;
    }

    void OnDisable()
    {
        if (!coupling) return;
        coupling.OnConnected -= OnConnected;
        coupling.OnDetached -= OnDetached;
        coupling.OnFlow -= OnFlow;
    }

    void OnConnected()
    {
        Play(connectClip);
        VibrateLight();
    }

    void OnDetached()
    {
        Play(detachClip);
        VibrateLight();
    }

    void OnFlow(bool started)
    {
        Play(started ? flowStartClip : flowStopClip);
        if (started) VibrateMedium();
    }

    void Play(AudioClip clip)
    {
        if (clip && src) src.PlayOneShot(clip, 1f);
    }

    // ---------- Haptics ----------
    void VibrateLight() => TryVibrate(30);   // ~30ms tap
    void VibrateMedium() => TryVibrate(60);

    void TryVibrate(int ms)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
            using (var buildVersion = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                int sdkInt = buildVersion.GetStatic<int>("SDK_INT");
                if (sdkInt >= 26)
                {
                    using (var vibrationEffect = new AndroidJavaClass("android.os.VibrationEffect"))
                    {
                        var effect = vibrationEffect.CallStatic<AndroidJavaObject>("createOneShot", (long)ms, 100);
                        vibrator.Call("vibrate", effect);
                    }
                }
                else
                {
                    vibrator.Call("vibrate", (long)ms);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[Haptics] Vibrate failed: " + e.Message);
        }
#elif UNITY_IOS && !UNITY_EDITOR
        Handheld.Vibrate(); // iOS fallback
#else
        // Editor or unsupported: no vibration
#endif
    }
}
