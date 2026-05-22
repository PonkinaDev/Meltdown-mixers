public interface INetworkService
{
    void StartHost();
    void StartClient();
    void Disconnect();

    event System.Action OnConnectedAsHost;
    event System.Action OnConnectedAsClient;
    event System.Action<int> OnPlayerCountChanged;
    event System.Action OnDisconnected;
}