using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class HidePlanesAfterPlacement : MonoBehaviour
{
    ARPlaneManager planes;
    PlaceOnPlane placer;

    Action<GameObject> placedHandler;

    void Awake()
    {
#if UNITY_2023_1_OR_NEWER
        planes = FindFirstObjectByType<ARPlaneManager>();
        placer = FindFirstObjectByType<PlaceOnPlane>();
#else
        planes = FindObjectOfType<ARPlaneManager>();
        placer = FindObjectOfType<PlaceOnPlane>();
#endif
    }

    void OnEnable()
    {
        if (placer != null)
        {
            placedHandler = _ => DisablePlanes();
            placer.OnPlacedObject += placedHandler;
        }
    }

    void OnDisable()
    {
        if (placer != null && placedHandler != null)
            placer.OnPlacedObject -= placedHandler;
    }

    /// <summary>
    /// Disables plane tracking and hides all detected planes.
    /// </summary>
    public void DisablePlanes()
    {
        if (planes == null) return;

        planes.enabled = false;
        foreach (var p in planes.trackables)
        {
            if (p != null)
                p.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Re-enables plane tracking and shows plane visuals again.
    /// </summary>
    public void ShowPlanes()
    {
        if (planes == null) return;

        planes.enabled = true;
        foreach (var p in planes.trackables)
        {
            if (p != null)
                p.gameObject.SetActive(true);
        }
    }
}
