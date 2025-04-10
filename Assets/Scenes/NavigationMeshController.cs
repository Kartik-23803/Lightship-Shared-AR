using UnityEngine;
using Niantic.Lightship.AR.NavigationMesh;
using UnityEngine.InputSystem;

public class NavigationMeshController : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private LightshipNavMeshManager _navmeshManager;
    [SerializeField] private LightshipNavMeshAgent _agentPrefab;
    [SerializeField] private Joystick _joystick;
    [SerializeField] float _movementThreshold = 0.1f;
    [SerializeField] float _moveSpeed = 2f;

    private LightshipNavMeshAgent _agentInstance;
    private Animator _agentAnimator;

    void Update()
    {
        HandleTouch();
        HandleJoystickMovement();
        UpdateAgentAnimation();
    }

    void UpdateAgentAnimation()
    {
        if (_agentInstance != null && _agentAnimator != null)
        {
            // Check if joystick input is significant
            bool isMoving = Mathf.Abs(_joystick.Horizontal) > _movementThreshold || 
                            Mathf.Abs(_joystick.Vertical) > _movementThreshold;
            _agentAnimator.SetBool("walking", isMoving);
        }
    }

    void HandleJoystickMovement()
    {
        // Null checks
        if (_agentInstance == null || _joystick == null)
            return;

        // Get joystick input
        float horizontalInput = _joystick.Horizontal;
        float verticalInput = _joystick.Vertical;

        // Calculate movement direction
        Vector3 moveDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        // If there's significant joystick input
        if (moveDirection.magnitude >= _movementThreshold)
        {
            // Calculate potential destination
            Vector3 currentPosition = _agentInstance.transform.position;
            Vector3 potentialDestination = currentPosition + moveDirection * _moveSpeed * Time.deltaTime;

            // Use Lightship NavMesh to validate and set destination
            _agentInstance.SetDestination(potentialDestination);

            // Rotate the agent to face movement direction
            if (moveDirection != Vector3.zero)
            {
                _agentInstance.transform.rotation = Quaternion.LookRotation(moveDirection);
            }
        }
    }

    void HandleTouch()
    {
        // Null checks for critical components
        if (_camera == null)
        {
            Debug.LogError("Camera is not assigned!");
            return;
        }

        if (_agentPrefab == null)
        {
            Debug.LogError("Agent Prefab is not assigned!");
            return;
        }

        // Check if touch is available and pressed
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            
            if (touchPosition.x > 0 && touchPosition.x < Screen.width &&
                touchPosition.y > 0 && touchPosition.y < Screen.height)
            {
                Ray ray = _camera.ScreenPointToRay(touchPosition);
                Debug.Log($"Shooting ray from touch position: {touchPosition}");

                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    Debug.Log($"Raycast hit point: {hit.point}");

                    if (_agentInstance == null)
                    {
                        // Create new agent
                        _agentInstance = Instantiate(_agentPrefab);
                        _agentInstance.transform.position = hit.point;
                        
                        // Get animator
                        _agentAnimator = _agentInstance.GetComponentInChildren<Animator>();
                        
                        Debug.Log($"Agent created at: {hit.point}");
                    }
                }
                else
                {
                    Debug.Log("Raycast did not hit anything");
                }
            }
            else
            {
                Debug.Log($"Touch position {touchPosition} is outside screen bounds");
            }
        }
    }

    public void SetVisualization(bool isVisualizationOn)
    {
        // Null check for navmesh manager
        if (_navmeshManager == null)
        {
            Debug.LogWarning("NavMesh Manager is not assigned!");
            return;
        }

        // Null check for NavMesh Renderer
        var navMeshRenderer = _navmeshManager.GetComponent<LightshipNavMeshRenderer>();
        if (navMeshRenderer != null)
        {
            navMeshRenderer.enabled = isVisualizationOn;
        }
        else
        {
            Debug.LogWarning("NavMesh Renderer component not found!");
        }

        // Null check for agent instance
        if (_agentInstance != null)
        {
            var agentPathRenderer = _agentInstance.GetComponent<LightshipNavMeshAgentPathRenderer>();
            if (agentPathRenderer != null)
            {
                agentPathRenderer.enabled = isVisualizationOn;
            }
            else
            {
                Debug.LogWarning("Agent Path Renderer component not found!");
            }
        }
    }

    // Optional: Method to reset or destroy agent
    public void ResetAgent()
    {
        if (_agentInstance != null)
        {
            // Stop animation if resetting
            if (_agentAnimator != null)
            {
                _agentAnimator.SetBool("walking", false);
            }

            Destroy(_agentInstance.gameObject);
            _agentInstance = null;
            _agentAnimator = null;
            
            Debug.Log("Agent has been reset");
        }
        else
        {
            Debug.Log("No agent to reset");
        }
    }
}

// using UnityEngine;
// using Niantic.Lightship.AR.NavigationMesh;
// using UnityEngine.InputSystem;

// public class NavigationMeshController : MonoBehaviour
// {
//     [SerializeField] private Camera _camera;

//     [SerializeField] private LightshipNavMeshManager _navmeshManager;

//     [SerializeField] private LightshipNavMeshAgent _agentPrefab;

//     private LightshipNavMeshAgent _agentInstance;
//     private Vector3 _finalPos;
//     private Animator _agentAnimator;
//     [SerializeField] float _movementThreshold = 0.1f;
//     [SerializeField] Joystick _joystick;
//     [SerializeField] float _moveSpeed = 2f;

//     void Start()
//     {
//         _finalPos = Vector3.zero;
//     }

//     void Update()
//     {
//         HandleTouch();
//         HandleJoystickMovement();
//         UpdateAgentAnimation();
//     }

//     void UpdateAgentAnimation()
//     {
//         if (_agentInstance != null && _agentAnimator != null)
//         {
//             bool isMoving = Vector3.Distance(_agentInstance.transform.position, _finalPos) > _movementThreshold || 
//                                 Mathf.Abs(_joystick.Horizontal) > 0.1f || Mathf.Abs(_joystick.Vertical) > 0.1f;;
//             _agentAnimator.SetBool("walking", isMoving);
//         }
//     }

//     public void SetVisualization(bool isVisualizationOn)
//     {
//         // Null check for navmesh manager
//         if (_navmeshManager == null)
//         {
//             Debug.LogWarning("NavMesh Manager is not assigned!");
//             return;
//         }

//         // Null check for NavMesh Renderer
//         var navMeshRenderer = _navmeshManager.GetComponent<LightshipNavMeshRenderer>();
//         if (navMeshRenderer != null)
//         {
//             navMeshRenderer.enabled = isVisualizationOn;
//         }
//         else
//         {
//             Debug.LogWarning("NavMesh Renderer component not found!");
//         }

//         // Null check for agent instance
//         if (_agentInstance != null)
//         {
//             var agentPathRenderer = _agentInstance.GetComponent<LightshipNavMeshAgentPathRenderer>();
//             if (agentPathRenderer != null)
//             {
//                 agentPathRenderer.enabled = isVisualizationOn;
//             }
//             else
//             {
//                 Debug.LogWarning("Agent Path Renderer component not found!");
//             }
//         }
//     }

//     void HandleJoystickMovement()
//     {
//         // Null checks
//         if (_agentInstance == null || _joystick == null)
//             return;

//         // Get joystick input
//         float horizontalInput = _joystick.Horizontal;
//         float verticalInput = _joystick.Vertical;

//         // Calculate movement direction
//         Vector3 moveDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;

//         // If there's significant joystick input
//         if (moveDirection.magnitude >= 0.1f)
//         {
//             // Calculate new position
//             Vector3 newPosition = _agentInstance.transform.position + moveDirection * _moveSpeed * Time.deltaTime;

//             // Use NavMesh to find a valid position
//             UnityEngine.AI.NavMeshHit hit;
//             if (UnityEngine.AI.NavMesh.SamplePosition(newPosition, out hit, 1f, UnityEngine.AI.NavMesh.AllAreas))
//             {
//                 // Move the agent
//                 // _agentInstance.transform.position = hit.position;
//                 _agentInstance.SetDestination(newPosition);

//                 // Rotate the agent to face movement direction
//                 if (moveDirection != Vector3.zero)
//                 {
//                     _agentInstance.transform.rotation = Quaternion.LookRotation(moveDirection);
//                 }
//             }
//         }
//     }

//     void HandleTouch()
//     {
//         // Null checks for critical components
//         if (_camera == null)
//         {
//             Debug.LogError("Camera is not assigned!");
//             return;
//         }

//         if (_agentPrefab == null)
//         {
//             Debug.LogError("Agent Prefab is not assigned!");
//             return;
//         }

//         // Check if touch is available and pressed
//         if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
//         {
//             Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            
//             if (touchPosition.x > 0 && touchPosition.x < Screen.width &&
//                 touchPosition.y > 0 && touchPosition.y < Screen.height)
//             {
//                 Ray ray = _camera.ScreenPointToRay(touchPosition);
//                 Debug.Log($"Shooting ray from touch position: {touchPosition}");

//                 RaycastHit hit;
//                 if (Physics.Raycast(ray, out hit))
//                 {
//                     Debug.Log($"Raycast hit point: {hit.point}");

//                     if (_agentInstance == null)
//                     {
//                         // Create new agent
//                         _agentInstance = Instantiate(_agentPrefab);
//                         _agentInstance.transform.position = hit.point;
                        
//                         // Get animator
//                         _agentAnimator = _agentInstance.GetComponentInChildren<Animator>();
                        
//                         Debug.Log($"Agent created at: {hit.point}");
//                     }
//                     // else
//                     // {
//                     //     // Set new destination
//                     //     _finalPos = hit.point;
//                     //     _agentInstance.SetDestination(hit.point);
                        
//                     //     Debug.Log($"Agent destination set to: {hit.point}");
//                     // }
//                 }
//                 else
//                 {
//                     Debug.Log("Raycast did not hit anything");
//                 }
//             }
//             else
//             {
//                 Debug.Log($"Touch position {touchPosition} is outside screen bounds");
//             }
//         }
//     }

//     // Optional: Method to reset or destroy agent
//     public void ResetAgent()
//     {
//         if (_agentInstance != null)
//         {
//             // Stop animation if resetting
//             if (_agentAnimator != null)
//             {
//                 _agentAnimator.SetBool("walking", false);
//             }

//             Destroy(_agentInstance.gameObject);
//             _agentInstance = null;
//             _agentAnimator = null;
//             _finalPos = Vector3.zero;
            
//             Debug.Log("Agent has been reset");
//         }
//         else
//         {
//             Debug.Log("No agent to reset");
//         }
//     }
// }