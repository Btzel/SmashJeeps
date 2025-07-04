using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SkillManager : NetworkBehaviour
{
    public static SkillManager Instance { get; private set; }

    public event Action OnMineCountReduced;

    [SerializeField] private MysteryBoxSkillsSO[] _mysteryBoxSkills;
    [SerializeField] private LayerMask _groundLayer;

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

    private async void SpawnSkill(SkillTransformDataSerializable skillTransformDataSerializable,
        ulong spawnerClientId)
    {
        if(!_skillsDictionary.TryGetValue(skillTransformDataSerializable.SkillType,out MysteryBoxSkillsSO skillData))
        {
            Debug.LogError($"Spawn Skill: {skillTransformDataSerializable.SkillType} not found");
            return;
        }

        if(skillTransformDataSerializable.SkillType == SkillType.Mine)
        {
            Vector3 spawnPosition = skillTransformDataSerializable.Position;
            Vector3 spawnDirection = skillTransformDataSerializable.Rotation * Vector3.forward;

            for(int i = 0; i < skillData.SkillData.SpawnAmountOrTimer; i++)
            {
                Vector3 offset = spawnDirection * (i * 3f);

                skillTransformDataSerializable.Position = spawnPosition + offset;

                Spawn(skillTransformDataSerializable, spawnerClientId, skillData);
                await UniTask.Delay(200);
                OnMineCountReduced?.Invoke();
            }
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
                    PlayerSkillController playerSkillController = client.PlayerObject.GetComponent<PlayerSkillController>();
                    networkObject.transform.localPosition = playerSkillController.GetRocketLaunchPosition();
                    return;
                }

                if (skillData.SkillData.ShouldBeAttachedToParent)
                {
                    networkObject.transform.localPosition = Vector3.zero;
                }

                PositionDataSerializable positionDataSerializable = new PositionDataSerializable(
                    skillInstance.transform.localPosition + skillData.SkillData.SkillOffset
                    );

                UpdateSkillPositionRpc(networkObject.NetworkObjectId, positionDataSerializable,false);

                if (!skillData.SkillData.ShouldBeAttachedToParent)
                {
                    networkObject.TryRemoveParent();

                    if(skillData.SkillType == SkillType.FakeBox)
                    {
                        float groundHeight = GetGroundHeight(skillData, skillInstance.position);

                        positionDataSerializable = new PositionDataSerializable(
                            new Vector3(
                            skillInstance.transform.position.x,
                            groundHeight,
                            skillInstance.transform.position.z));

                        UpdateSkillPositionRpc(networkObject.NetworkObjectId, positionDataSerializable,true);
                    }
                }
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateSkillPositionRpc(ulong objectId, PositionDataSerializable positionDataSerializable,
        bool isSpecialPosition)
    {
        if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId,out var networkObject))
        {
            if (isSpecialPosition)
            {
                networkObject.transform.position = positionDataSerializable.Position;
            }
            else
            {
                networkObject.transform.localPosition = positionDataSerializable.Position;
            }


        }
    }

    private float GetGroundHeight(MysteryBoxSkillsSO skillData,Vector3 position)
    {
        if(Physics.Raycast(new Vector3(position.x,position.y,position.z),Vector3.down,
            out RaycastHit hit, 10f, _groundLayer))
        {
            return skillData.SkillData.SkillOffset.y;
        }

        return skillData.SkillData.SkillOffset.y;

    }
}
