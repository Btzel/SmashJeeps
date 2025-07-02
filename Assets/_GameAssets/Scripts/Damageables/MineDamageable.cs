using Unity.Netcode;
using UnityEngine;

public class MineDamageable : NetworkBehaviour,IDamageable
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(OwnerClientId, out var client))
        {
            NetworkObject ownerNetworkObject = client.PlayerObject;
            PlayerVehicleController playerVehicleController = ownerNetworkObject.GetComponent<PlayerVehicleController>();
            playerVehicleController.OnVehicleCrashed += PlayerVehicleController_OnVehicleCrashed;
        }
    }

    private void PlayerVehicleController_OnVehicleCrashed()
    {
        DestroyRpc();
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(OwnerClientId, out var client))
        {
            NetworkObject ownerNetworkObject = client.PlayerObject;
            PlayerVehicleController playerVehicleController = ownerNetworkObject.GetComponent<PlayerVehicleController>();
            playerVehicleController.OnVehicleCrashed -= PlayerVehicleController_OnVehicleCrashed;
        }
    }

    public void Damage(PlayerVehicleController playerVehicleController)
    {
        playerVehicleController.CrashVehicle();
        DestroyRpc();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out ShieldController shieldController))
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(shieldController.OwnerClientId, out var shieldClient))
            {
                if (NetworkManager.Singleton.ConnectedClients.TryGetValue(OwnerClientId, out var client))
                {
                    NetworkObject ownerNetworkObject = client.PlayerObject;

                    if (client.PlayerObject != shieldClient.PlayerObject)
                    {
                        DestroyRpc();
                    }
                }
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DestroyRpc()
    {
        if (IsServer)
        {
            Destroy(gameObject);
        }
    }
}
