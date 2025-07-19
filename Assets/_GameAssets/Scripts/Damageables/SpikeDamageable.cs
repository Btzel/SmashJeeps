using Unity.Netcode;
using UnityEngine;

public class SpikeDamageable : NetworkBehaviour, IDamageable
{
    [SerializeField] private MysteryBoxSkillsSO _mysteryBoxSkill;
    [SerializeField] private GameObject _explosionParticlesPrefab;

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
        PlayParticlesRpc(playerVehicleController.transform.position);
        KillScreenUI.Instance.SetSmashedUI(playerName, _mysteryBoxSkill.SkillData.RespawnTimer);


    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DestroyRpc()
    {
        if (IsServer)
        {
            Destroy(gameObject);
        }
    }
    [Rpc(SendTo.Server)]
    private void PlayParticlesRpc(Vector3 VehiclePosition = default)
    {
        if (IsServer)
        {
            GameObject explosionParticlesInstance = Instantiate(_explosionParticlesPrefab, VehiclePosition, Quaternion.identity);
            explosionParticlesInstance.GetComponent<NetworkObject>().Spawn();
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
