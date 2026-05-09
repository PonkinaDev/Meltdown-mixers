using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

// SRP: SOLO maneja conexión y ciclo de vida del runner
// DIP: expone eventos en lugar de llamar sistemas directamente
public class NetworkManager : MonoBehaviour, INetworkService, INetworkRunnerCallbacks
{
    public static NetworkManager Instance { get; private set; }

    // ─── Dependencias inyectadas (DIP) ───────────────────────────────────────
    [SerializeField] private PlayerSpawner _playerSpawner;
    [SerializeField] private FusionSceneLoader _sceneLoader;

    private NetworkRunner _runner;
    public NetworkRunner Runner => _runner;

    // ─── Eventos (OCP: otros sistemas escuchan sin que NetworkManager los conozca)
    public event Action OnConnectedAsHost;
    public event Action OnConnectedAsClient;
    public event Action<int> OnPlayerCountChanged;
    public event Action OnDisconnected;
    private readonly List<PlayerRef> _pendingSpawns = new();
private bool _gameSceneLoaded = false;

    private GameMode _currentMode;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ─── INetworkService ─────────────────────────────────────────────────────

    public async void StartHost()
    {
        _currentMode = GameMode.Host;
        await LaunchRunner(GameMode.Host);
    }

    public async void StartClient()
    {
        _currentMode = GameMode.AutoHostOrClient;
        await LaunchRunner(GameMode.AutoHostOrClient);
    }

    public async void Disconnect()
    {
        if (_runner == null) return;
        await _runner.Shutdown();
        _runner = null;
        OnDisconnected?.Invoke();
    }

    // ─── Launch ──────────────────────────────────────────────────────────────

    private async System.Threading.Tasks.Task LaunchRunner(GameMode mode)
    {
        if (_runner != null)
        {
            await _runner.Shutdown();
            await System.Threading.Tasks.Task.Delay(100);
        }

        if (this == null) return;

        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        var sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);

        var args = new StartGameArgs
        {
            GameMode     = mode,
            SessionName  = "ProtocolPanic",
            Scene        = scene,
            SceneManager = sceneManager,
            PlayerCount  = 2
        };

        var result = await _runner.StartGame(args);

        if (result.Ok)
        {
            // Inicializamos el scene loader con el runner activo
            _sceneLoader.Initialize(_runner);

            bool isHost = _runner.IsServer;
            Debug.Log($"[NetworkManager] Conectado como: {(isHost ? "Host" : "Client")}");

            if (isHost) OnConnectedAsHost?.Invoke();
            else        OnConnectedAsClient?.Invoke();
        }
        else
        {
            Debug.LogError($"[NetworkManager] Error: {result.ErrorMessage}");
        }
    }

    // ─── Callbacks de Fusion ─────────────────────────────────────────────────

void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
{
    Debug.Log($"[NetworkManager] Jugador {player} conectado.");

    if (!runner.IsServer) return;

    // Contamos jugadores para saber cuándo cargar escena
    _pendingSpawns.Add(player);
    OnPlayerCountChanged?.Invoke(_pendingSpawns.Count);

    // Con 2 jugadores cargamos la escena de juego
    if (_pendingSpawns.Count >= 2)
    {
        Invoke(nameof(TriggerSceneLoad), 1.5f);
    }
}

void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner)
{
    Debug.Log("[NetworkManager] Escena cargada.");

    // Solo spawneamos si ya estamos en la GameScene (index 1)
    if (!runner.IsServer) return;

    int currentScene = SceneManager.GetActiveScene().buildIndex;
    if (currentScene != 1) return; // 1 = GameScene en Build Settings

    // Ahora sí spawneamos todos los jugadores pendientes
    foreach (PlayerRef player in _pendingSpawns)
    {
        _playerSpawner.SpawnPlayer(runner, player);
    }
}

void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
{
    _pendingSpawns.Remove(player);
    _playerSpawner.DespawnPlayer(runner, player);
    OnPlayerCountChanged?.Invoke(_pendingSpawns.Count);
}

private void TriggerSceneLoad()
{
    _sceneLoader.LoadGameScene();
}

    // ─── Callbacks vacíos requeridos por Fusion 2 ────────────────────────────
    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner) { }
    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { OnDisconnected?.Invoke(); }
    void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input) { }
    void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    void INetworkRunnerCallbacks.OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner) { }
    void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
}