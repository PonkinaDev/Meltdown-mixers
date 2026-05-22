using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour, INetworkService, INetworkRunnerCallbacks
{
    public static NetworkManager Instance { get; private set; }

    [Header("Dependencies")]
    [SerializeField] private PlayerSpawner _playerSpawner;
    [SerializeField] private FusionSceneLoader _sceneLoader;
    [SerializeField] private NetworkObject _avatarSelectionPrefab;

    private NetworkRunner _runner;
    public NetworkRunner Runner => _runner;

    public event Action OnConnectedAsHost;
    public event Action OnConnectedAsClient;
    public event Action<int> OnPlayerCountChanged;
    public event Action OnDisconnected;

    private readonly List<PlayerRef> _pendingSpawns = new();
    private bool _avatarSelectionSpawned;
    private bool _gameSceneReady;
    private bool _selectionsReady;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        NetworkAvatarSelection.OnAllPlayersReady += HandleAllPlayersReady;
    }

    private void OnDisable()
    {
        NetworkAvatarSelection.OnAllPlayersReady -= HandleAllPlayersReady;
    }

    public async void StartHost()
    {
        await LaunchRunner(GameMode.Host);
    }

    public async void StartClient()
    {
        await LaunchRunner(GameMode.Client);
    }

    public async void Disconnect()
    {
        if (_runner == null) return;
        await _runner.Shutdown();
        _runner = null;
        OnDisconnected?.Invoke();
    }

    private async System.Threading.Tasks.Task LaunchRunner(GameMode mode)
    {
        if (_runner != null)
        {
            await _runner.Shutdown();
            await System.Threading.Tasks.Task.Delay(100);
        }

        if (this == null) return;

        _avatarSelectionSpawned = false;
        _gameSceneReady = false;
        _selectionsReady = false;

        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);

        PlayerInputHandler inputHandler = GetComponent<PlayerInputHandler>();
        if (inputHandler != null)
            _runner.AddCallbacks(inputHandler);

        var sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
        var currentScene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);

        var args = new StartGameArgs
        {
            GameMode     = mode,
            SessionName  = "ProtocolPanic",
            Scene        = currentScene,
            SceneManager = sceneManager,
            PlayerCount  = 2
        };

        var result = await _runner.StartGame(args);

        if (!result.Ok)
        {
            Debug.LogError($"[NetworkManager] Error: {result.ErrorMessage}");
            return;
        }

        _sceneLoader.Initialize(_runner);

        if (_runner.IsServer)
            OnConnectedAsHost?.Invoke();
        else
            OnConnectedAsClient?.Invoke();
    }

    void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        _pendingSpawns.Add(player);
        OnPlayerCountChanged?.Invoke(_pendingSpawns.Count);

        if (!runner.IsServer) return;

        if (_pendingSpawns.Count >= 2 && !_avatarSelectionSpawned)
            SpawnAvatarSelection();
    }

    void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner)
    {
        if (!runner.IsServer) return;

        int currentScene = SceneManager.GetActiveScene().buildIndex;
        if (currentScene != 1) return;

        Debug.Log("[NetworkManager] OnSceneLoadDone — escena de juego lista");
        _gameSceneReady = true;
        TrySpawnPlayers(runner);
    }

    void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        _pendingSpawns.Remove(player);
        OnPlayerCountChanged?.Invoke(_pendingSpawns.Count);

        if (!runner.IsServer) return;
        _playerSpawner.DespawnPlayer(runner, player);
    }

    private void SpawnAvatarSelection()
    {
        _avatarSelectionSpawned = true;
        Debug.Log("[NetworkManager] Spawneando AvatarSelection");
        _runner.Spawn(_avatarSelectionPrefab);
    }

    private void HandleAllPlayersReady()
    {
        Debug.Log("[NetworkManager] Todos listos, cargando escena");
        _selectionsReady = true;
        _sceneLoader.LoadGameScene();
    }

    private void TrySpawnPlayers(NetworkRunner runner)
    {
        if (!_gameSceneReady || !_selectionsReady)
        {
            Debug.Log($"[NetworkManager] TrySpawnPlayers esperando — gameSceneReady:{_gameSceneReady} selectionsReady:{_selectionsReady}");

            if (_gameSceneReady && !_selectionsReady)
                StartCoroutine(WaitForSelectionsAndSpawn(runner));

            return;
        }

        DoSpawnPlayers(runner);
    }

    private IEnumerator WaitForSelectionsAndSpawn(NetworkRunner runner)
    {
        Debug.Log("[NetworkManager] Esperando selecciones persistidas...");
        float timeout = 5f;

        while (timeout > 0f)
        {
            bool allReady = true;
            foreach (PlayerRef player in _pendingSpawns)
            {
                if (NetworkAvatarSelection.GetPersistedSelection(player) == -1)
                {
                    allReady = false;
                    break;
                }
            }

            if (allReady)
            {
                Debug.Log("[NetworkManager] Selecciones recibidas, spawneando jugadores");
                DoSpawnPlayers(runner);
                yield break;
            }

            timeout -= Time.deltaTime;
            yield return null;
        }

        Debug.LogWarning("[NetworkManager] Timeout esperando selecciones — spawneando con fallback");
        DoSpawnPlayers(runner);
    }

    private void DoSpawnPlayers(NetworkRunner runner)
    {
        foreach (PlayerRef player in _pendingSpawns)
        {
            Debug.Log($"[NetworkManager] Spawneando player {player}");
            _playerSpawner.SpawnPlayer(runner, player);
        }
    }

    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner) { }
    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) => OnDisconnected?.Invoke();
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