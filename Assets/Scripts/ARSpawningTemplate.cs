using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Samples.ARStarterAssets;

public class ARSpawningTemplate : MonoBehaviour
{
    [SerializeField]
    private Button m_CreateButton;
    public Button createButton
    {
        get => m_CreateButton;
        set => m_CreateButton = value;
    }

    [SerializeField]
    private Button m_DeleteButton;
    public Button deleteButton
    {
        get => m_DeleteButton;
        set => m_DeleteButton = value;
    }

    [SerializeField]
    private GameObject m_ObjectMenu;
    public GameObject objectMenu
    {
        get => m_ObjectMenu;
        set => m_ObjectMenu = value;
    }

    [SerializeField]
    private GameObject m_ModalMenu;
    public GameObject modalMenu
    {
        get => m_ModalMenu;
        set => m_ModalMenu = value;
    }

    [SerializeField]
    private Animator m_ObjectMenuAnimator;
    public Animator objectMenuAnimator
    {
        get => m_ObjectMenuAnimator;
        set => m_ObjectMenuAnimator = value;
    }

    [SerializeField]
    private LightshipObjectSpawner m_ObjectSpawner;
    public LightshipObjectSpawner objectSpawner
    {
        get => m_ObjectSpawner;
        set => m_ObjectSpawner = value;
    }

    [SerializeField]
    private Button m_CancelButton;
    public Button cancelButton
    {
        get => m_CancelButton;
        set => m_CancelButton = value;
    }

    [SerializeField]
    private XRInteractionGroup m_InteractionGroup;
    public XRInteractionGroup interactionGroup
    {
        get => m_InteractionGroup;
        set => m_InteractionGroup = value;
    }

    [SerializeField]
    private GameObject m_DebugPlane;
    public GameObject debugPlane
    {
        get => m_DebugPlane;
        set => m_DebugPlane = value;
    }

    [SerializeField]
    private ARPlaneManager m_PlaneManager;
    public ARPlaneManager planeManager
    {
        get => m_PlaneManager;
        set => m_PlaneManager = value;
    }

    [SerializeField]
    private GameObject m_DebugMenu;
    public GameObject debugMenu
    {
        get => m_DebugMenu;
        set => m_DebugMenu = value;
    }

    [SerializeField]
    private DebugSlider m_DebugPlaneSlider;
    public DebugSlider debugPlaneSlider
    {
        get => m_DebugPlaneSlider;
        set => m_DebugPlaneSlider = value;
    }

    [SerializeField]
    private DebugSlider m_DebugMenuSlider;
    public DebugSlider debugMenuSlider
    {
        get => m_DebugMenuSlider;
        set => m_DebugMenuSlider = value;
    }

    [SerializeField]
    private XRInputValueReader<Vector2> m_TapStartPositionInput = new XRInputValueReader<Vector2>("Tap Start Position");
    public XRInputValueReader<Vector2> tapStartPositionInput
    {
        get => m_TapStartPositionInput;
        set => XRInputReaderUtility.SetInputProperty(ref m_TapStartPositionInput, value, this);
    }

    [SerializeField]
    private XRInputValueReader<Vector2> m_DragCurrentPositionInput = new XRInputValueReader<Vector2>("Drag Current Position");
    public XRInputValueReader<Vector2> dragCurrentPositionInput
    {
        get => m_DragCurrentPositionInput;
        set => XRInputReaderUtility.SetInputProperty(ref m_DragCurrentPositionInput, value, this);
    }

    private bool m_IsPointerOverUI;
    private bool m_ShowObjectMenu;
    private bool m_ShowOptionsModal;
    private bool m_InitializingDebugMenu;
    private Vector2 m_ObjectButtonOffset = Vector2.zero;
    private Vector2 m_ObjectMenuOffset = Vector2.zero;
    private readonly List<ARFeatheredPlaneMeshVisualizerCompanion> featheredPlaneMeshVisualizerCompanions = new List<ARFeatheredPlaneMeshVisualizerCompanion>();

    private void OnEnable()
    {
        m_CreateButton.onClick.AddListener(ShowMenu);
        m_CancelButton.onClick.AddListener(HideMenu);
        m_DeleteButton.onClick.AddListener(DeleteFocusedObject);
        m_PlaneManager.planesChanged += OnPlaneChanged;
    }

    private void OnDisable()
    {
        m_ShowObjectMenu = false;
        m_CreateButton.onClick.RemoveListener(ShowMenu);
        m_CancelButton.onClick.RemoveListener(HideMenu);
        m_DeleteButton.onClick.RemoveListener(DeleteFocusedObject);
        m_PlaneManager.planesChanged -= OnPlaneChanged;
    }

    private void Start()
    {
        // Auto turn on/off debug menu
        m_DebugMenu.SetActive(true);
        m_InitializingDebugMenu = true;

        InitializeDebugMenuOffsets();
        HideMenu();
        m_PlaneManager.planePrefab = m_DebugPlane;
    }

    private void Update()
    {
        if (m_InitializingDebugMenu)
        {
            m_DebugMenu.SetActive(false);
            m_InitializingDebugMenu = false;
        }

        HandleMenuInteraction();
        UpdateButtonVisibility();
    }

    private void HandleMenuInteraction()
    {
        if (m_ShowObjectMenu || m_ShowOptionsModal)
        {
            if (!m_IsPointerOverUI && (m_TapStartPositionInput.TryReadValue(out _) || m_DragCurrentPositionInput.TryReadValue(out _)))
            {
                if (m_ShowObjectMenu)
                    HideMenu();

                if (m_ShowOptionsModal)
                    m_ModalMenu.SetActive(false);
            }

            m_IsPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(-1);
        }
        else
        {
            m_IsPointerOverUI = false;
        }
    }

    private void UpdateButtonVisibility()
    {
        if (m_ShowObjectMenu)
        {
            m_DeleteButton.gameObject.SetActive(false);
        }
        else
        {
            m_DeleteButton.gameObject.SetActive(m_InteractionGroup?.focusInteractable != null);
            m_CreateButton.gameObject.SetActive(true);
        }
    }

    public void SetObjectToSpawn(int objectIndex)
    {
        if (m_ObjectSpawner == null)
        {
            Debug.LogWarning("Object Spawner not configured correctly: no ObjectSpawner set.");
            return;
        }

        if (m_ObjectSpawner.objectPrefabs.Count > objectIndex)
        {
            m_ObjectSpawner.spawnOptionIndex = objectIndex;
        }
        else
        {
            Debug.LogWarning("Object Spawner not configured correctly: object index larger than number of Object Prefabs.");
        }

        HideMenu();
    }

    public void ShowHideModal()
    {
        if (m_ModalMenu.activeSelf)
        {
            m_ShowOptionsModal = false;
            m_ModalMenu.SetActive(false);
        }
        else
        {
            m_ShowOptionsModal = true;
            m_ModalMenu.SetActive(true);
        }
    }

    public void ShowHideDebugPlane()
    {
        if (m_DebugPlaneSlider.value == 1)
        {
            m_DebugPlaneSlider.value = 0;
            ChangePlaneVisibility(false);
        }
        else
        {
            m_DebugPlaneSlider.value = 1;
            ChangePlaneVisibility(true);
        }
    }

    public void ShowHideDebugMenu()
    {
        if (m_DebugMenu.activeSelf)
        {
            m_DebugMenuSlider.value = 0;
            m_DebugMenu.SetActive(false);
        }
        else
        {
            m_DebugMenuSlider.value = 1;
            m_DebugMenu.SetActive(true);
            AdjustARDebugMenuPosition();
        }
    }

    public void ClearAllObjects()
    {
        foreach (Transform child in m_ObjectSpawner.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void ShowMenu()
    {
        m_ShowObjectMenu = true;
        m_ObjectMenu.SetActive(true);
        if (!m_ObjectMenuAnimator.GetBool("Show"))
        {
            m_ObjectMenuAnimator.SetBool("Show", true);
        }
        AdjustARDebugMenuPosition();
    }

    public void HideMenu()
    {
        m_ObjectMenuAnimator.SetBool("Show", false);
        m_ShowObjectMenu = false;
        AdjustARDebugMenuPosition();
    }

    private void ChangePlaneVisibility(bool setVisible)
    {
        foreach (var visualizer in featheredPlaneMeshVisualizerCompanions)
        {
            visualizer.visualizeSurfaces = setVisible;
        }
    }

    private void DeleteFocusedObject()
    {
        var currentFocusedObject = m_InteractionGroup.focusInteractable;
        if (currentFocusedObject != null)
        {
            Destroy(currentFocusedObject.transform.gameObject);
        }
    }

    private void InitializeDebugMenuOffsets()
    {
        if (m_CreateButton.TryGetComponent<RectTransform>(out var buttonRect))
            m_ObjectButtonOffset = new Vector2(0f, buttonRect.anchoredPosition.y + buttonRect.rect.height + 10f);
        else
            m_ObjectButtonOffset = new Vector2(0f, 200f);

        if (m_ObjectMenu.TryGetComponent<RectTransform>(out var menuRect))
            m_ObjectMenuOffset = new Vector2(0f, menuRect.anchoredPosition.y + menuRect.rect.height + 10f);
        else
            m_ObjectMenuOffset = new Vector2(0f, 345f);
    }

    private void AdjustARDebugMenuPosition()
    {
        float screenWidthInInches = Screen.width / Screen.dpi;

        if (screenWidthInInches < 5)
        {
            Vector2 menuOffset = m_ShowObjectMenu ? m_ObjectMenuOffset : m_ObjectButtonOffset;

            // Note: The original implementation had detailed positioning for various UI elements.
            // This is a simplified version. You may need to add back the specific positioning 
            // logic for your debug menu if required.
        }
    }

    private void OnPlaneChanged(ARPlanesChangedEventArgs eventArgs)
    {
        if (eventArgs.added.Count > 0)
        {
            foreach (var plane in eventArgs.added)
            {
                if (plane.TryGetComponent<ARFeatheredPlaneMeshVisualizerCompanion>(out var visualizer))
                {
                    featheredPlaneMeshVisualizerCompanions.Add(visualizer);
                    visualizer.visualizeSurfaces = (m_DebugPlaneSlider.value != 0);
                }
            }
        }

        if (eventArgs.removed.Count > 0)
        {
            foreach (var plane in eventArgs.removed)
            {
                if (plane.TryGetComponent<ARFeatheredPlaneMeshVisualizerCompanion>(out var visualizer))
                    featheredPlaneMeshVisualizerCompanions.Remove(visualizer);
            }
        }

        // Fallback if the counts do not match after an update
        if (m_PlaneManager.trackables.count != featheredPlaneMeshVisualizerCompanions.Count)
        {
            featheredPlaneMeshVisualizerCompanions.Clear();
            foreach (var trackable in m_PlaneManager.trackables)
            {
                if (trackable.TryGetComponent<ARFeatheredPlaneMeshVisualizerCompanion>(out var visualizer))
                {
                    featheredPlaneMeshVisualizerCompanions.Add(visualizer);
                    visualizer.visualizeSurfaces = (m_DebugPlaneSlider.value != 0);
                }
            }
        }
    }
}

// // Custom class for debug slider (you might need to implement this or use an existing one)
// public class DebugSlider : MonoBehaviour
// {
//     public float value;
// }