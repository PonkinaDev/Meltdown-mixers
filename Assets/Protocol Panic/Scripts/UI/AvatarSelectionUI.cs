using System;
using Fusion;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AvatarSelectionUI : MonoBehaviour
{
    [SerializeField] private Transform _slotsParent;
    [SerializeField] private AvatarSlotUI _slotPrefab;
    [SerializeField] private Button _readyButton;
    [SerializeField] private TextMeshProUGUI _readyButtonLabel;
    [SerializeField] private GameObject _waitingForOthersPanel;
    private bool _isReady;

    private IAvatarSelectionService _service;
    private NetworkRunner _runner;
    private AvatarSlotUI[] _slots;
public static event Action OnShown;
    public static AvatarSelectionUI Instance { get; private set; }
    

// AvatarSelectionUI.cs — reemplaza Awake y Start completos
private void Awake()
{
    Instance = this;
    _readyButton.onClick.AddListener(OnReadyClicked);
    _readyButton.interactable = false;
    _waitingForOthersPanel.SetActive(false);
    gameObject.SetActive(false);

    NetworkAvatarSelection.OnInstanceReady += HandleInstanceReady; // ← suscripción estática, funciona inactivo
}

private void Start() { }

public void Show(IAvatarSelectionService service, NetworkRunner runner)
{
    _service = service;
    _runner = runner;
    _service.OnStateChanged += Refresh;
    NetworkAvatarSelection.OnAllPlayersReady += OnAllPlayersReady;
    BuildSlots();
    gameObject.SetActive(true);
    Refresh();
    OnShown?.Invoke(); // ← avisa que ya está visible
}

public void Hide()
{
    _isReady = false;
    if (_service != null) _service.OnStateChanged -= Refresh;
    NetworkAvatarSelection.OnAllPlayersReady -= OnAllPlayersReady;
    gameObject.SetActive(false);
}

    private void BuildSlots()
    {
        foreach (Transform child in _slotsParent)
            Destroy(child.gameObject);

        _slots = new AvatarSlotUI[_service.AvatarCount];
        for (int i = 0; i < _service.AvatarCount; i++)
        {
            var slot = Instantiate(_slotPrefab, _slotsParent);
            slot.Initialize(i, _service.GetAvatarDefinition(i), _service);
            _slots[i] = slot;
        }
    }

private void Refresh()
{
    if (_runner == null || _slots == null) return;
    if (_isReady) return; // ← bloquea refresh tras confirmar listo

    var localPlayer = _runner.LocalPlayer;
    int localSelection = _service.GetPlayerSelection(localPlayer);

    for (int i = 0; i < _slots.Length; i++)
    {
        bool taken = _service.IsAvatarTaken(i);
        bool selectedByMe = localSelection == i;
        _slots[i].Refresh(taken, selectedByMe);
    }

    _readyButton.interactable = localSelection != -1;
}

private void OnReadyClicked()
{
    _isReady = true;
    _service.RequestReady();
    _readyButton.interactable = false;
    _readyButtonLabel.text = "Listo!";
    _waitingForOthersPanel.SetActive(true);
    SetSlotsInteractable(false);
}

private void SetSlotsInteractable(bool value)
{
    foreach (var slot in _slots)
        slot.SetInteractable(value);
}

    private void OnAllPlayersReady() => Hide();

private void OnDestroy()
{
    _readyButton.onClick.RemoveListener(OnReadyClicked);
    NetworkAvatarSelection.OnInstanceReady -= HandleInstanceReady;
    if (_service != null) _service.OnStateChanged -= Refresh;
    NetworkAvatarSelection.OnAllPlayersReady -= OnAllPlayersReady;
}
private void HandleInstanceReady(NetworkAvatarSelection selection)
{
    Show(selection, selection.Runner);
}

}