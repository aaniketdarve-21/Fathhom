using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaceOnPlane : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public ARRaycastManager raycastManager;
    public GameObject placeablePrefab;

    [Header("Spawn Settings")]
    [Tooltip("Uniform scale applied once at spawn. 1 = import size, 0.05 = 5% of import size.")]
    [Range(0.005f, 1f)]
    public float initialScale = 0.05f;

    // Event for when object is placed
    public event Action<GameObject> OnPlacedObject;

    public GameObject SpawnedObject { get; private set; }
    public bool HasPlaced => SpawnedObject != null;

    static readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Update()
    {
        // Skip if already placed or no touch device
        if (SpawnedObject != null || Touchscreen.current == null) return;

        var touch = Touchscreen.current.primaryTouch;
        if (!touch.press.wasPressedThisFrame) return;

        Vector2 screenPos = touch.position.ReadValue();

        if (raycastManager == null) return;

        if (raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
        {
            var hitPose = hits[0].pose;

            // Instantiate model
            SpawnedObject = Instantiate(placeablePrefab, hitPose.position, hitPose.rotation);
            SpawnedObject.transform.localScale = Vector3.one * initialScale;

            // Face toward camera for better orientation
            var cam = Camera.main;
            if (cam != null)
            {
                Vector3 look = new Vector3(cam.transform.position.x, hitPose.position.y, cam.transform.position.z);
                SpawnedObject.transform.LookAt(look);
            }

            OnPlacedObject?.Invoke(SpawnedObject);
        }
    }

    /// <summary>
    /// Destroys the placed object to allow re-placement.
    /// </summary>
    public void ResetPlacement()
    {
        if (SpawnedObject != null)
        {
            Destroy(SpawnedObject);
            SpawnedObject = null;
        }
    }
}
