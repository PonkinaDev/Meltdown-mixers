// DIP: dependemos de esta abstracción, no de NetworkManager directamente
// ISP: solo los métodos que realmente necesita la UI
public interface INetworkService
{
    void StartHost();
    void StartClient();
    void Disconnect();

    // Eventos para que otros sistemas reaccionen sin acoplamiento
    event System.Action OnConnectedAsHost;
    event System.Action OnConnectedAsClient;
    event System.Action<int> OnPlayerCountChanged; // cuántos jugadores hay
    event System.Action OnDisconnected;
}