using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ARSpawnTrigger : MonoBehaviour
{
    /// <summary>
    /// The type of trigger to use to spawn an object.
    /// </summary>
    public enum SpawnTriggerType
    {
        /// <summary>
        /// Spawn an object when the interactor activates its select input
        /// but no selection actually occurs.
        /// </summary>
        SelectAttempt,

        /// <summary>
        /// Spawn an object when an input is performed.
        /// </summary>
        InputAction,
    }

    [SerializeField]
    [Tooltip("The AR ray interactor that determines where to spawn the object.")]
    private XRRayInteractor m_ARInteractor;

    /// <summary>
    /// The AR ray interactor that determines where to spawn the object.
    /// </summary>
    public XRRayInteractor arInteractor
    {
        get => m_ARInteractor;
        set => m_ARInteractor = value;
    }

    [SerializeField]
    [Tooltip("The behavior to use to spawn objects.")]
    private LightshipObjectSpawner m_ObjectSpawner;

    /// <summary>
    /// The behavior to use to spawn objects.
    /// </summary>
    public LightshipObjectSpawner objectSpawner
    {
        get => m_ObjectSpawner;
        set => m_ObjectSpawner = value;
    }

    [SerializeField]
    [Tooltip("Whether to require that the AR Interactor hits an AR Plane with a horizontal up alignment in order to spawn anything.")]
    private bool m_RequireHorizontalUpSurface;

    /// <summary>
    /// Whether to require that the interactor hits an AR Plane with a horizontal up alignment.
    /// </summary>
    public bool requireHorizontalUpSurface
    {
        get => m_RequireHorizontalUpSurface;
        set => m_RequireHorizontalUpSurface = value;
    }

    [SerializeField]
    [Tooltip("The type of trigger to use to spawn an object.")]
    private SpawnTriggerType m_SpawnTriggerType;

    /// <summary>
    /// The type of trigger to use to spawn an object.
    /// </summary>
    public SpawnTriggerType spawnTriggerType
    {
        get => m_SpawnTriggerType;
        set => m_SpawnTriggerType = value;
    }

    [SerializeField]
    private XRInputButtonReader m_SpawnObjectInput = new XRInputButtonReader("Spawn Object");

    /// <summary>
    /// The input used to trigger spawn.
    /// </summary>
    public XRInputButtonReader spawnObjectInput
    {
        get => m_SpawnObjectInput;
        set => XRInputReaderUtility.SetInputProperty(ref m_SpawnObjectInput, value, this);
    }

    [SerializeField]
    [Tooltip("When enabled, spawn will not be triggered if an object is currently selected.")]
    private bool m_BlockSpawnWhenInteractorHasSelection = true;

    /// <summary>
    /// When enabled, spawn will not be triggered if an object is currently selected.
    /// </summary>
    public bool blockSpawnWhenInteractorHasSelection
    {
        get => m_BlockSpawnWhenInteractorHasSelection;
        set => m_BlockSpawnWhenInteractorHasSelection = value;
    }

    private bool m_AttemptSpawn;
    private bool m_EverHadSelection;

    private void OnEnable()
    {
        m_SpawnObjectInput.EnableDirectActionIfModeUsed();
    }

    private void OnDisable()
    {
        m_SpawnObjectInput.DisableDirectActionIfModeUsed();
    }

    private void Start()
    {
        // Find ObjectSpawner if not assigned
//         if (m_ObjectSpawner == null)
//         {
// #if UNITY_2023_1_OR_NEWER
//             m_ObjectSpawner = FindAnyObjectByType<LightsObjectSpawner>();
// #else
// #endif
//         }
        m_ObjectSpawner = FindObjectOfType<LightshipObjectSpawner>();

        // Validate AR Interactor
        if (m_ARInteractor == null)
        {
            Debug.LogError("Missing AR Interactor reference, disabling component.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        // Spawn attempt from previous frame
        if (m_AttemptSpawn)
        {
            ProcessSpawnAttempt();
            return;
        }

        // Determine spawn trigger
        DetermineSpawnTrigger();
    }

    private void ProcessSpawnAttempt()
    {
        m_AttemptSpawn = false;

        // Check for UI overlap
        var isPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(-1);
        
        if (!isPointerOverUI && m_ARInteractor.TryGetCurrentARRaycastHit(out var arRaycastHit))
        {
            // Validate trackable as AR Plane
            if (!(arRaycastHit.trackable is ARPlane arPlane))
                return;

            // Check horizontal surface requirement
            if (m_RequireHorizontalUpSurface && arPlane.alignment != PlaneAlignment.HorizontalUp)
                return;

            // Attempt to spawn object
            Vector2 viewportPosition = m_ObjectSpawner.cameraToFace.WorldToViewportPoint(arRaycastHit.pose.position);
            m_ObjectSpawner.TrySpawnObjectAtViewportPosition(viewportPosition);

            // m_ObjectSpawner.TrySpawnObjectAtViewportPosition(arRaycastHit.pose.position);
        }
    }

    private void DetermineSpawnTrigger()
    {
        var selectState = m_ARInteractor.logicalSelectState;

        // Track selection state if blocking is enabled
        if (m_BlockSpawnWhenInteractorHasSelection)
        {
            if (selectState.wasPerformedThisFrame)
                m_EverHadSelection = m_ARInteractor.hasSelection;
            else if (selectState.active)
                m_EverHadSelection |= m_ARInteractor.hasSelection;
        }

        // Reset spawn attempt
        m_AttemptSpawn = false;

        // Determine spawn trigger based on type
        switch (m_SpawnTriggerType)
        {
            case SpawnTriggerType.SelectAttempt:
                if (selectState.wasCompletedThisFrame)
                    m_AttemptSpawn = !m_ARInteractor.hasSelection && !m_EverHadSelection;
                break;

            case SpawnTriggerType.InputAction:
                if (m_SpawnObjectInput.ReadWasPerformedThisFrame())
                    m_AttemptSpawn = !m_ARInteractor.hasSelection && !m_EverHadSelection;
                break;
        }
    }
}