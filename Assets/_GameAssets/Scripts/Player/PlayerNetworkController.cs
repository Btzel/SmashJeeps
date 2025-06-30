using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineCamera _playerCamera;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _playerCamera.gameObject.SetActive(IsOwner);

    }
}
