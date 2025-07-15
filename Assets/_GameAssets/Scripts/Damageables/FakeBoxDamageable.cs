using System;
using Cysharp.Threading.Tasks.Triggers;
using Unity.Netcode;
using UnityEngine;

public class FakeBoxDamageable : NetworkBehaviour, IDamageable
{
    [SerializeField] private MysteryBoxSkillsSO _mysteryBoxSkill;
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

    public void Damage(PlayerVehicleController playerVehicleController,string playerName)
    {
        playerVehicleController.CrashVehicle();
        KillScreenUI.Instance.SetSmashedUI(playerName, _mysteryBoxSkill.SkillData.RespawnTimer);
        DestroyRpc();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out ShieldController shieldController))
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(shieldController.OwnerClientId, out var shieldClient))
            {
                if (NetworkManager.Singleton.ConnectedClients.TryGetValue(OwnerClientId, out var client))
                {
                    NetworkObject ownerNetworkObject = client.PlayerObject;
                    
                    if(client.PlayerObject != shieldClient.PlayerObject)
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

    public ulong GetKillerClientId()
    {
        return OwnerClientId;
    }

    public int GetRespawnTimer()
    {
        return _mysteryBoxSkill.SkillData.RespawnTimer;
    }

    public int GetDamageAmount()
    {
        return _mysteryBoxSkill.SkillData.DamageAmount;
    }

    public string GetKillerName()
    {
        ulong killerClientId = GetKillerClientId();
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(killerClientId, out var killetClient))
        {
            string playerName = killetClient.PlayerObject.GetComponent<PlayerNetworkController>().PlayerName.Value.ToString();
            return playerName;
        }

        return string.Empty;
    }

    
}
