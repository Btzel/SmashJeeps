using System;
using Unity.Netcode;
using UnityEngine;

public class LeaderBoardUI : NetworkBehaviour
{

    [SerializeField] private LeaderboardRanking _leaderboardRankingPrefab;
    [SerializeField] private Transform _rankingPrefab;
    private NetworkList<LeaderboardEntitiesSerializable> _leaderboardEntityList;


    
    void Awake()
    {
        _leaderboardEntityList = new NetworkList<LeaderboardEntitiesSerializable>();
    }

    public override void OnNetworkSpawn()
    {

        if (IsClient)
        {
            _leaderboardEntityList.OnListChanged += HandleLeaderboardEntitiesChanged;
        }

        if (IsClient && !IsServer)
        {
            foreach (var entity in _leaderboardEntityList)
            {
                LeaderboardRanking leaderboardRankingInstance
                    = Instantiate(_leaderboardRankingPrefab, _rankingPrefab);
                leaderboardRankingInstance.SetData(
                    entity.ClientId,
                    entity.PlayerName,
                    entity.Score
                );
            }
        }

        if (IsServer)
        {
            PlayerNetworkController[] players = FindObjectsByType<PlayerNetworkController>(FindObjectsSortMode.None);
            foreach (PlayerNetworkController player in players)
            {
                HandlePlayerSpawned(player);
            }

            PlayerNetworkController.OnPlayerSpawned += HandlePlayerSpawned;
            PlayerNetworkController.OnPlayerDespawned += HandlePlayerDespawned;
        }
    }

    private void HandleLeaderboardEntitiesChanged(NetworkListEvent<LeaderboardEntitiesSerializable> changeEvent)
    {
        

        switch (changeEvent.Type)
        {
            case NetworkListEvent<LeaderboardEntitiesSerializable>.EventType.Add:
                LeaderboardRanking leaderboardRankingInstance
                    = Instantiate(_leaderboardRankingPrefab, _rankingPrefab);
                leaderboardRankingInstance.SetData(
                    changeEvent.Value.ClientId,
                    changeEvent.Value.PlayerName,
                    changeEvent.Value.Score
                );
                break;
        }
    }

    private void HandlePlayerSpawned(PlayerNetworkController playerNetworkController)
    {
        _leaderboardEntityList.Add(new LeaderboardEntitiesSerializable
        {
            ClientId = playerNetworkController.OwnerClientId,
            PlayerName = playerNetworkController.PlayerName.Value,
            Score = 0,

        });
        
        
    }

    private void HandlePlayerDespawned(PlayerNetworkController playerNetworkController)
    {
        if (_leaderboardEntityList == null) return;

        foreach (LeaderboardEntitiesSerializable entity in _leaderboardEntityList)
        {
            if (entity.ClientId != playerNetworkController.OwnerClientId) continue;

            _leaderboardEntityList.Remove(entity);
            
            break;
        }

        
    }
}
