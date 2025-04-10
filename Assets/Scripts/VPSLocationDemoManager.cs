using System.Linq;
using Niantic.Lightship.AR.LocationAR;
using Niantic.Lightship.SharedAR.Colocalization;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class VPSLocationDemoManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _statusText;

    [SerializeField]
    private Button _joinAsHostButton;

    [SerializeField]
    private Button _joinAsClientButton;

    [SerializeField]
    private SharedSpaceManager _sharedSpaceManager;

    [SerializeField]
    private ARLocationManager _arLocationManager;

    protected void Start()
    {
        if(_joinAsClientButton != null && _joinAsHostButton != null)
        {
            // Hide UI until VPS is in tracking state
            _joinAsHostButton.gameObject.SetActive(false);
            _joinAsClientButton.gameObject.SetActive(false);

            // UI event listeners
            _joinAsHostButton.onClick.AddListener(OnJoinAsHostClicked);
            _joinAsClientButton.onClick.AddListener(OnJoinAsClientClicked);
        }

        // Netcode connection event callback
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;

        // Set SharedSpaceManager and start it
        _sharedSpaceManager.sharedSpaceManagerStateChanged += OnColocalizationTrackingStateChanged;

        // Set room to join
        // This demo only targets a single ARLocation, so we can just use the first location.
        // For applications that choose from a list, use the specific ARLocation that you are localizing against
        //  as the room ID
        var vpsTrackingOptions = ISharedSpaceTrackingOptions.CreateVpsTrackingOptions(_arLocationManager.ARLocations.First());
        var roomOptions = ISharedSpaceRoomOptions.CreateVpsRoomOptions(
            vpsTrackingOptions,
            "optionalPrefixForRoom_", // room name will be this prefix + Base64 Location anchor data
            32, // set capacity to max
            "Room Description Here!");
        _sharedSpaceManager.StartSharedSpace(vpsTrackingOptions, roomOptions);
    }

    private void OnColocalizationTrackingStateChanged(
        SharedSpaceManager.SharedSpaceManagerStateChangeEventArgs args)
    {
        // Show Join UI
        if (args.Tracking && _joinAsClientButton != null && _joinAsHostButton != null && _statusText != null)
        {
            _statusText.text = $"Localized";
            _joinAsHostButton.gameObject.SetActive(true);
            _joinAsClientButton.gameObject.SetActive(true);
        }
    }
    private void OnJoinAsHostClicked()
    {
        NetworkManager.Singleton.StartHost();
        HideButtons();
    }

    private void OnJoinAsClientClicked()
    {
        NetworkManager.Singleton.StartClient();
        HideButtons();
    }

    private void HideButtons()
    {
        if (_joinAsClientButton != null && _joinAsHostButton != null)
        {
            _joinAsHostButton.gameObject.SetActive(false);
            _joinAsClientButton.gameObject.SetActive(false);
        }
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        _statusText.text = $"Connected: {clientId}";
    }
}