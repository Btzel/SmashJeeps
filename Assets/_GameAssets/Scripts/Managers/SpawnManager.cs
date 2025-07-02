using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnManager : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _playerPrefab;

    [Header("Transform Lists")]
    [SerializeField] private List<Transform> _spawnPointTransformList;

    private List<int> _availableSpawnIndexList = new List<int>();

    public override void OnNetworkSpawn()
    {
        if(!IsServer) return;

        for(int i = 0; i < _spawnPointTransformList.Count; i++)
        {
            _availableSpawnIndexList.Add(i);
        }

        NetworkManager.OnClientConnectedCallback += SpawnPlayer;
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (_availableSpawnIndexList.Count == 0) return;

        int randomIndex = Random.Range(0, _availableSpawnIndexList.Count);
        int spawnIndex = _availableSpawnIndexList[randomIndex];

        _availableSpawnIndexList.RemoveAt(randomIndex);

        Transform spawnPointTransform = _spawnPointTransformList[spawnIndex];

        GameObject playerInstance = Instantiate(_playerPrefab, 
            spawnPointTransform.position, spawnPointTransform.rotation);

        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        Debug.Log("Player Spawned with id: "+ clientId);
    }
}
