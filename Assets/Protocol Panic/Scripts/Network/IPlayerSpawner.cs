using Fusion;

// SRP: solo sabe hacer spawn de jugadores
// ISP: interfaz pequeña y específica
public interface IPlayerSpawner
{
    void SpawnPlayer(NetworkRunner runner, PlayerRef player);
    void DespawnPlayer(NetworkRunner runner, PlayerRef player);
}