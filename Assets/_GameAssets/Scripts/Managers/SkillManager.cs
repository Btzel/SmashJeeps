using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SkillManager : NetworkBehaviour
{
    public static SkillManager Instance { get; private set; }

    [SerializeField] private MysteryBoxSkillsSO[] _mysteryBoxSkills;

    private Dictionary<SkillType, MysteryBoxSkillsSO> _skillsDictionary;

    private void Awake()
    {
        Instance = this;

        _skillsDictionary = new Dictionary<SkillType, MysteryBoxSkillsSO>();

        foreach(MysteryBoxSkillsSO skill in _mysteryBoxSkills)
        {
            _skillsDictionary[skill.SkillType] = skill;
        }
    }

    public void ActivateSkill(SkillType skillType,Transform playerTransform, ulong spawnerClientId)
    {
        SkillTransformDataSerializable skillTransformData = new SkillTransformDataSerializable(
            playerTransform.position,
            playerTransform.rotation,
            skillType,
            playerTransform.GetComponent<NetworkObject>()
            );

        if (!IsServer)
        {
            RequestSpawnRpc(skillTransformData, spawnerClientId);
            return;
        }

        SpawnSkill(skillTransformData, spawnerClientId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void RequestSpawnRpc(SkillTransformDataSerializable skillTransformDataSerializable,
        ulong spawnerClientId)
    {
        SpawnSkill(skillTransformDataSerializable, spawnerClientId);
    }

    private void SpawnSkill(SkillTransformDataSerializable skillTransformDataSerializable,
        ulong spawnerClientId)
    {
        if(!_skillsDictionary.TryGetValue(skillTransformDataSerializable.SkillType,out MysteryBoxSkillsSO skillData))
        {
            Debug.LogError($"Spawn Skill: {skillTransformDataSerializable.SkillType} not found");
            return;
        }

        if(skillTransformDataSerializable.SkillType == SkillType.Mine)
        {
            // Mine Special
        }
        else
        {
            Spawn(skillTransformDataSerializable, spawnerClientId, skillData);
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

                if (!skillData.SkillData.ShouldBeAttachedToParent)
                {
                    networkObject.TryRemoveParent();
                }
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
