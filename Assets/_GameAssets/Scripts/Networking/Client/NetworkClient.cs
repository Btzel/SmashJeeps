
using System;
using Unity.Netcode;
using Unity.Networking.Transport.Error;
using UnityEngine.SceneManagement;

public class NetworkClient
{
    private NetworkManager _networkManager;

    public NetworkClient(NetworkManager networkManager)
    {
        _networkManager = networkManager;

        _networkManager.OnClientDisconnectCallback += OnClientDisconnectCallback;
    }

    private void OnClientDisconnectCallback(ulong clientId)
    {
        if (clientId != 0 && clientId != _networkManager.LocalClientId) return;
        
        Disconnect();
    }

    private void Disconnect()
    {
        if (SceneManager.GetActiveScene().name != Consts.SceneNames.MENU_SCENE)
        {
            SceneManager.LoadScene(Consts.SceneNames.MENU_SCENE);
        }

        if (_networkManager.IsConnectedClient)
        {
            _networkManager.Shutdown();
        }
    }
}
