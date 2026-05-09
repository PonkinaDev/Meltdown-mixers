using System.Collections.Generic;
using Fusion;
using UnityEngine;

// SRP: SOLO maneja spawn/despawn de jugadores
// OCP: podemos extender sin modificar (nuevos tipos de spawn)
public class PlayerSpawner : MonoBehaviour, IPlayerSpawner
{
    [SerializeField] private NetworkPrefabRef _playerPrefab;

    private readonly Dictionary<PlayerRef, NetworkObject> _spawnedPlayers = new();

    // Posiciones de spawn para cada jugador
private static readonly Vector3[] SpawnPositions =
{
    new(-2f, 1f, 0f),  // Jugador 1
    new( 2f, 1f, 0f)   // Jugador 2
};

    public void SpawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer) return;

        int index = _spawnedPlayers.Count;
        Vector3 pos = index < SpawnPositions.Length
            ? SpawnPositions[index]
            : Vector3.zero;

        NetworkObject obj = runner.Spawn(_playerPrefab, pos, Quaternion.identity, player);
        _spawnedPlayers[player] = obj;

        Debug.Log($"[PlayerSpawner] Jugador {player} spawneado en {pos}");
    }

    public void DespawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        if (!_spawnedPlayers.TryGetValue(player, out NetworkObject obj)) return;

        runner.Despawn(obj);
        _spawnedPlayers.Remove(player);

        Debug.Log($"[PlayerSpawner] Jugador {player} despawneado");
    }

    public int PlayerCount => _spawnedPlayers.Count;
}