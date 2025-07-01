using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SkillManager : NetworkBehaviour
{
    [SerializeField] private MysteryBoxSkillsSO[] _mysteryBoxSkills;

    private Dictionary<SkillType, MysteryBoxSkillsSO> _skillsDictionary;

    private void Awake()
    {
        _skillsDictionary = new Dictionary<SkillType, MysteryBoxSkillsSO>();

        foreach(MysteryBoxSkillsSO skill in _mysteryBoxSkills)
        {
            _skillsDictionary[skill.SkillType] = skill;
        }
    }

    private void Spawn(SkillTransformDataSerializable skillTransformDataSerializable,
        ulong spawnerClientId,MysteryBoxSkillsSO skillData)
    {
        if (IsServer)
        {
            Transform skillInstance = Instantiate(skillData.SkillData.SkillPrefab);
            skillInstance.SetPositionAndRotation(
                skillTransformDataSerializable.Position,
                skillTransformDataSerializable.Rotation);
            var networkObject = skillInstance.GetComponent<NetworkObject>();
            networkObject.SpawnWithOwnership(spawnerClientId);

            if(NetworkManager.Singleton.ConnectedClients.TryGetValue(spawnerClientId,out var client))
            {
                if(skillData.SkillType != SkillType.Rocket)
                {
                    networkObject.TrySetParent(client.PlayerObject);
                }
                else
                {
                    // ROCKET SPECIAL
                }

                if (skillData.SkillData.ShouldBeAttachedToParent)
                {
                    networkObject.transform.localPosition = Vector3.zero;
                }

                PositionDataSerializable positionDataSerializable = new PositionDataSerializable(
                    skillInstance.transform.localPosition + skillData.SkillData.SkillOffset
                    );

                UpdateSkillPositionRpc(networkObject.NetworkObjectId, positionDataSerializable);

                // will be continued
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateSkillPositionRpc(ulong objectId, PositionDataSerializable positionDataSerializable)
    {
        if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId,out var networkObject))
        {
            networkObject.transform.localPosition = positionDataSerializable.Position;
        }
    }
}
