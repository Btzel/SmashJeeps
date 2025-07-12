using System;
using Unity.Netcode;
using UnityEngine;

public class MultiplayerGameManager : NetworkBehaviour
{
    public static MultiplayerGameManager Instance { get; private set; } 
    public event Action OnPlayerDataNetworkListChanged;
    private NetworkList<PlayerDataSerializable> _playerDataNetworkList = new NetworkList<PlayerDataSerializable>();

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _playerDataNetworkList.OnListChanged += _playerDataNetworkList_OnListChanged;
    }


    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _playerDataNetworkList.Clear();

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
        }
    }

    private void OnClientDisconnectCallback(ulong clientId)
    {
        for (int i = 0; i < _playerDataNetworkList.Count; i++)
        {
            PlayerDataSerializable playerData = _playerDataNetworkList[i];
            if (playerData.ClientId == clientId)
            {
                _playerDataNetworkList.RemoveAt(i);
            }
        }
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        for (int i = 0; i < _playerDataNetworkList.Count; i++)
        {
            if (_playerDataNetworkList[i].ClientId == clientId)
            {
                _playerDataNetworkList.RemoveAt(i);
            }
        }


        _playerDataNetworkList.Add(new PlayerDataSerializable
        {
            ClientId = clientId
        });
    }

    private void _playerDataNetworkList_OnListChanged(NetworkListEvent<PlayerDataSerializable> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke();
    }

    public bool IsPlayerIndexConnected(int playerIndex)
    {
        return playerIndex < _playerDataNetworkList.Count;
    }

    public PlayerDataSerializable GetPlayerDataFromPlayerIndex(int playerIndex)
    {
        return _playerDataNetworkList[playerIndex];
    }
}
