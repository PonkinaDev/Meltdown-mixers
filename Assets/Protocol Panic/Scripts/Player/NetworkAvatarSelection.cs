using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class NetworkAvatarSelection : NetworkBehaviour, IAvatarSelectionService
{
    private const int MaxPlayers = 4;
    private const int NoSelection = -1;

    [SerializeField] private AvatarRegistry _registry;

    [Networked, Capacity(MaxPlayers)]
    private NetworkArray<int> _selections { get; }

    [Networked, Capacity(MaxPlayers)]
    private NetworkArray<NetworkBool> _ready { get; }

    private ChangeDetector _changes;
    private static readonly Dictionary<PlayerRef, int> _persistedSelections = new();

    public static NetworkAvatarSelection Instance { get; private set; }
    public static event Action OnAllPlayersReady;
    public static event Action<NetworkAvatarSelection> OnInstanceReady;
    public event Action OnStateChanged;

    public int AvatarCount => _registry.Count;

    public override void Spawned()
    {
        if (Instance != null && Instance != this) return;

        Instance = this;
        _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if (Object.HasStateAuthority)
        {
            var selections = _selections;
            var ready = _ready;
            for (int i = 0; i < MaxPlayers; i++)
            {
                selections[i] = NoSelection;
                ready[i] = false;
            }
        }

        OnInstanceReady?.Invoke(this);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Instance == this) Instance = null;
    }

    public override void Render()
    {
        bool anyChange = false;
        foreach (var _ in _changes.DetectChanges(this))
            anyChange = true;
        if (anyChange) OnStateChanged?.Invoke();
    }

    public bool IsAvatarTaken(int avatarIndex)
    {
        for (int i = 0; i < MaxPlayers; i++)
            if (_selections[i] == avatarIndex) return true;
        return false;
    }

    public int GetPlayerSelection(PlayerRef player)
    {
        int slot = PlayerToSlot(player);
        if (slot < 0 || slot >= MaxPlayers) return NoSelection;
        return _selections[slot];
    }

    public AvatarDefinition GetAvatarDefinition(int index) => _registry.Get(index);

    public void RequestSelectAvatar(int avatarIndex) => RPC_Select(Runner.LocalPlayer, avatarIndex);
    public void RequestReady() => RPC_Ready(Runner.LocalPlayer);

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_Select(PlayerRef player, int avatarIndex)
    {
        if (IsAvatarTaken(avatarIndex)) return;
        int slot = PlayerToSlot(player);
        var selections = _selections;
        var ready = _ready;
        selections[slot] = avatarIndex;
        ready[slot] = false;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_Ready(PlayerRef player)
    {
        int slot = PlayerToSlot(player);
        if (_selections[slot] == NoSelection) return;
        var ready = _ready;
        ready[slot] = true;
        CheckAllReady();
    }

    private void CheckAllReady()
    {
        foreach (var player in Runner.ActivePlayers)
        {
            int slot = PlayerToSlot(player);
            if (_selections[slot] == NoSelection || (bool)_ready[slot] == false) return;
        }
        RPC_NotifyAllReady();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_NotifyAllReady()
    {
        foreach (var player in Runner.ActivePlayers)
        {
            int slot = PlayerToSlot(player);
            _persistedSelections[player] = _selections[slot];
            Debug.Log($"[AvatarSelection] Guardando player {player} → avatar {_selections[slot]}");
        }
        OnAllPlayersReady?.Invoke();
    }

    public static int GetPersistedSelection(PlayerRef player)
    {
        return _persistedSelections.TryGetValue(player, out int idx) ? idx : NoSelection;
    }

    public static void ClearPersistedSelections() => _persistedSelections.Clear();

    private static int PlayerToSlot(PlayerRef player) => player.PlayerId - 1;
}