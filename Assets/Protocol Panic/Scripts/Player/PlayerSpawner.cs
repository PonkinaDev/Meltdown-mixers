using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour, IPlayerSpawner
{
    [SerializeField] private NetworkPrefabRef _fallbackPrefab;
    [SerializeField] private AvatarRegistry _registry;

    private readonly Dictionary<PlayerRef, NetworkObject> _spawnedPlayers = new();

    private static readonly Vector3[] SpawnPositions =
    {
        new(-2f, 1f, 0f),
        new( 2f, 1f, 0f)
    };

    public void SpawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer) return;

        NetworkPrefabRef prefab = ResolveAvatarPrefab(player);
        int index = _spawnedPlayers.Count;
        Vector3 pos = index < SpawnPositions.Length ? SpawnPositions[index] : Vector3.zero;

        Debug.Log($"[PlayerSpawner] Spawneando player {player} con prefab válido: {prefab != NetworkPrefabRef.Empty}");

        NetworkObject obj = runner.Spawn(prefab, pos, Quaternion.identity, player);
        _spawnedPlayers[player] = obj;
    }

    public void DespawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        if (!_spawnedPlayers.TryGetValue(player, out NetworkObject obj)) return;
        runner.Despawn(obj);
        _spawnedPlayers.Remove(player);
    }

    public int PlayerCount => _spawnedPlayers.Count;

    private NetworkPrefabRef ResolveAvatarPrefab(PlayerRef player)
    {
        int idx = NetworkAvatarSelection.GetPersistedSelection(player);
        Debug.Log($"[PlayerSpawner] Player {player} → idx persistido: {idx} | _registry null: {_registry == null}");

        if (idx == -1) return _fallbackPrefab;

        var def = _registry.Get(idx);
        Debug.Log($"[PlayerSpawner] AvatarDefinition null: {def == null} | Prefab vacío: {def.Prefab == NetworkPrefabRef.Empty}");

        return def.Prefab;
    }
}