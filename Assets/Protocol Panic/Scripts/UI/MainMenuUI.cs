using UnityEngine;
using UnityEngine.UI;
using TMPro;

// SRP: solo maneja la UI del menú
// DIP: depende de INetworkService, no de NetworkManager directamente
public class MainMenuUI : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject _panelMainMenu;
    [SerializeField] private GameObject _panelLobby;
    [SerializeField] private GameObject _panelLoading;

    [Header("Botones")]
    [SerializeField] private Button _btnHost;
    [SerializeField] private Button _btnJoin;
    [SerializeField] private Button _btnQuit;
    [SerializeField] private Button _btnCancel;

    [Header("Textos")]
    [SerializeField] private TextMeshProUGUI _txtStatus;
    [SerializeField] private TextMeshProUGUI _txtPlayers;

    // DIP: dependemos de la interfaz, no de la clase concreta
    private INetworkService _networkService;

    private void Start()
    {
        // Buscamos la implementación de INetworkService
        _networkService = FindFirstObjectByType<NetworkManager>();

        if (_networkService == null)
        {
            Debug.LogError("[MainMenuUI] No se encontró INetworkService.");
            return;
        }

        // OCP: nos suscribimos a eventos, no preguntamos estado constantemente
        _networkService.OnConnectedAsHost    += HandleConnectedAsHost;
        _networkService.OnConnectedAsClient  += HandleConnectedAsClient;
        _networkService.OnPlayerCountChanged += HandlePlayerCountChanged;
        _networkService.OnDisconnected       += HandleDisconnected;

        _btnHost.onClick.AddListener(OnHostClicked);
        _btnJoin.onClick.AddListener(OnJoinClicked);
        _btnQuit.onClick.AddListener(OnQuitClicked);
        _btnCancel.onClick.AddListener(OnCancelClicked);

        ShowPanel(_panelMainMenu);
    }

    private void OnDestroy()
    {
        // Limpieza de eventos para evitar memory leaks
        if (_networkService == null) return;
        _networkService.OnConnectedAsHost    -= HandleConnectedAsHost;
        _networkService.OnConnectedAsClient  -= HandleConnectedAsClient;
        _networkService.OnPlayerCountChanged -= HandlePlayerCountChanged;
        _networkService.OnDisconnected       -= HandleDisconnected;
    }

    // ─── Botones ─────────────────────────────────────────────────────────────

    private void OnHostClicked()
    {
        ShowPanel(_panelLoading);
        SetStatus("Creando sala...");
        _networkService.StartHost();
    }

    private void OnJoinClicked()
    {
        ShowPanel(_panelLoading);
        SetStatus("Buscando sala...");
        _networkService.StartClient();
    }

    private void OnQuitClicked() => Application.Quit();

    private void OnCancelClicked()
    {
        _networkService.Disconnect();
        ShowPanel(_panelMainMenu);
    }

    // ─── Handlers de eventos ─────────────────────────────────────────────────

    private void HandleConnectedAsHost()
    {
        ShowPanel(_panelLobby);
        SetStatus("Sala creada — esperando jugador...");
        SetPlayers("Jugadores: 1/2");
    }

    private void HandleConnectedAsClient()
    {
        ShowPanel(_panelLobby);
        SetStatus("Conectado a la sala");
    }

    private void HandlePlayerCountChanged(int count)
    {
        SetPlayers($"Jugadores: {count}/2");

        if (count >= 2)
        {
            SetStatus("¡Ambos listos! Cargando juego...");
        }
    }

    private void HandleDisconnected()
    {
        ShowPanel(_panelMainMenu);
        SetStatus("Desconectado");
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private void ShowPanel(GameObject panel)
    {
        _panelMainMenu.SetActive(false);
        _panelLobby.SetActive(false);
        _panelLoading.SetActive(false);
        panel?.SetActive(true);
    }

    private void SetStatus(string msg)
    {
        if (_txtStatus != null)
            _txtStatus.text = msg;
    }

    private void SetPlayers(string msg)
    {
        if (_txtPlayers != null)
            _txtPlayers.text = msg;
    }
}