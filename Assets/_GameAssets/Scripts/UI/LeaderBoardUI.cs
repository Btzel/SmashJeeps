using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;


public class LeaderBoardUI : NetworkBehaviour
{

    [SerializeField] private LeaderboardRanking _leaderboardRankingPrefab;
    [SerializeField] private Transform _rankingPrefab;
    private NetworkList<LeaderboardEntitiesSerializable> _leaderboardEntityList;

    private List<LeaderboardRanking> _leaderboardRankingList = new List<LeaderboardRanking>();

    
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
                _leaderboardRankingList.Add(leaderboardRankingInstance);
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
                _leaderboardRankingList.Add(leaderboardRankingInstance);
                break;
            case NetworkListEvent<LeaderboardEntitiesSerializable>.EventType.Value:
                LeaderboardRanking leaderboardRankingToUpdate
                    = _leaderboardRankingList.FirstOrDefault(
                        x => x.ClientId == changeEvent.Value.ClientId
                    );
                

                if (leaderboardRankingToUpdate != null)
                {
                    leaderboardRankingToUpdate.UpdateScore(changeEvent.Value.Score);
                }
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

        playerNetworkController.GetPlayerScoreController().PlayerScore.OnValueChanged +=
            (oldScore, newScore) => HandleScoreChanged(
                playerNetworkController.OwnerClientId,
                newScore
            );
        
    }

    private void HandleScoreChanged(ulong clientId, int newScore)
    {
        Debug.Log("Score Has Changed");
        for (int i = 0; i < _leaderboardEntityList.Count; i++)
        {
            if (_leaderboardEntityList[i].ClientId != clientId) continue;

            _leaderboardEntityList[i] = new LeaderboardEntitiesSerializable
            {
                ClientId = _leaderboardEntityList[i].ClientId,
                PlayerName = _leaderboardEntityList[i].PlayerName,
                Score = newScore
            };
            return;
        }
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

        playerNetworkController.GetPlayerScoreController().PlayerScore.OnValueChanged -=
            (oldScore, newScore) => HandleScoreChanged(
                playerNetworkController.OwnerClientId,
                newScore
            );


    }
}
