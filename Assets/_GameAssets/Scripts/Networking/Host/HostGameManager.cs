using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class HostGameManager : IDisposable
{
    private const int MAX_CONNECTIONS = 4;
    public NetworkServer NetworkServer { get; private set; }

    private Allocation _allocation;
    private string _joinCode;
    private string _lobbyId;

   public async UniTask StartHostAsync()
    {
        try
        {
            _allocation = await RelayService.Instance.CreateAllocationAsync(MAX_CONNECTIONS);
        }
        catch (Exception exception)
        {
            Debug.LogError(exception);
            return;
        }

        try
        {
            _joinCode = await RelayService.Instance.GetJoinCodeAsync(_allocation.AllocationId);
            Debug.Log(_joinCode);
        }
        catch (Exception exception)
        {
            Debug.LogError(exception);
            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(AllocationUtils.ToRelayServerData(_allocation, "dtls"));

        try
        {
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions();
            createLobbyOptions.IsPrivate = false;
            createLobbyOptions.Data = new Dictionary<string, DataObject>()
            {
                {
                    "JoinCode",new DataObject
                    (
                        visibility: DataObject.VisibilityOptions.Member,
                        value: _joinCode
                    )
                }
            };

            string playerName = PlayerPrefs.GetString(Consts.PlayerData.PLAYER_NAME, "Noname");

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync
                ($"{playerName}'s Lobby", MAX_CONNECTIONS,createLobbyOptions);

            _lobbyId = lobby.Id;

            HostSingleton.Instance.StartCoroutine(HeartbeatLobby(15));
        }
        catch (LobbyServiceException lobbyServiceException)
        {
            Debug.LogError(lobbyServiceException);
            return;
        }

        NetworkServer = new NetworkServer(NetworkManager.Singleton);

        UserData userData = new UserData
        {
            UserName = PlayerPrefs.GetString(Consts.PlayerData.PLAYER_NAME, "Noname"),
            UserAuthId = AuthenticationService.Instance.PlayerId
        };

        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;



        NetworkManager.Singleton.StartHost();

        NetworkManager.Singleton.SceneManager.LoadScene(Consts.SceneNames.GAME_SCENE,
            UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private IEnumerator HeartbeatLobby(float waitTimeSeconds)
    {
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(waitTimeSeconds);

        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(_lobbyId);
            yield return delay;
        }
    }

    public async void ShutDown()
    {
        HostSingleton.Instance.StopCoroutine(nameof(HeartbeatLobby));
        if (!string.IsNullOrEmpty(_lobbyId))
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(_lobbyId);
            }
            catch (LobbyServiceException lobbyServiceException)
            {
                Debug.Log(lobbyServiceException);

            }

            _lobbyId = string.Empty;
        }

        NetworkServer?.Dispose();
    }

    public void Dispose()
    {
        ShutDown();
    }
}
