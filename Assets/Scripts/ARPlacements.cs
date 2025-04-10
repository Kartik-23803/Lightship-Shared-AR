using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlacements : MonoBehaviour
{
    [SerializeField] private ARRaycastManager arRaycastManager;
    [SerializeField] private ARPlaneManager arPlaneManager;
    [SerializeField] private GameObject objectToSpawn;
    
    private void Start()
    {
        // Comprehensive initialization checks
        Debug.Log("[ARPlacements] Script Started");

        // Ensure Raycast Manager
        if (arRaycastManager == null)
        {
            arRaycastManager = FindObjectOfType<ARRaycastManager>();
        }

        // Ensure Plane Manager
        if (arPlaneManager == null)
        {
            arPlaneManager = FindObjectOfType<ARPlaneManager>();
        }

        // Validate AR components
        ValidateARSetup();
    }

    private void ValidateARSetup()
    {
        if (arRaycastManager == null)
        {
            Debug.LogError("[ARPlacements] NO RAYCAST MANAGER FOUND!");
            return;
        }

        if (arPlaneManager == null)
        {
            Debug.LogError("[ARPlacements] NO PLANE MANAGER FOUND!");
            return;
        }

        // Check if plane tracking is enabled
        if (!arPlaneManager.enabled)
        {
            Debug.LogWarning("[ARPlacements] Plane Manager is NOT ENABLED!");
        }

        // Log detected planes
        Debug.Log($"[ARPlacements] Total Detected Planes: {arPlaneManager.trackables.count}");
    }

    private void Update()
    {
        // Comprehensive input detection
        HandleInputDetection();
    }

    private void HandleInputDetection()
    {
        // Detect input across platforms
        if (Input.GetMouseButtonDown(0))
        {
            ProcessInput(Input.mousePosition);
        }
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                ProcessInput(touch.position);
            }
        }
    }

    private void ProcessInput(Vector2 inputPosition)
    {
        Debug.Log($"[ARPlacements] Input Detected at: {inputPosition}");

        // Check for UI overlap
        if (IsPointerOverUIElement(inputPosition))
        {
            Debug.Log("[ARPlacements] Touch over UI, ignoring");
            return;
        }

        // Attempt object placement
        AttemptObjectPlacement(inputPosition);
    }

    private bool IsPointerOverUIElement(Vector2 screenPosition)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = screenPosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        return results.Count > 0;
    }

    private void AttemptObjectPlacement(Vector2 touchPosition)
    {
        // Validate components before raycast
        if (arRaycastManager == null)
        {
            Debug.LogError("[ARPlacements] Raycast Manager is NULL!");
            return;
        }

        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        
        // Try raycasting
        bool raycastHit = arRaycastManager.Raycast(
            touchPosition, 
            hits, 
            TrackableType.PlaneWithinPolygon
        );

        if (raycastHit && hits.Count > 0)
        {
            // Successful plane detection
            Debug.Log($"[ARPlacements] Raycast Hit {hits.Count} planes");
            
            // Spawn object at first hit point
            Instantiate(
                objectToSpawn, 
                hits[0].pose.position, 
                hits[0].pose.rotation
            );
        }
        else
        {
            Debug.Log("[ARPlacements] No planes detected at touch point");
        }
    }
}