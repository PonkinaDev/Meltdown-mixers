using Fusion;
public interface IPlayerSpawner
{
    void SpawnPlayer(NetworkRunner runner, PlayerRef player);
    void DespawnPlayer(NetworkRunner runner, PlayerRef player);
}