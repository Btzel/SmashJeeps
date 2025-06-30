using Unity.Netcode;
using UnityEngine;

public class MysteryBoxCollectible : NetworkBehaviour, ICollectible
{
    [Header("References")]
    [SerializeField] private MysteryBoxSkillsSO[] _mysteryBoxSkills;
    [SerializeField] private Animator _boxAnimator;
    [SerializeField] private Collider _collider;
    
    [Header("Settings")]
    [SerializeField] private float _respawnTimer;

    public void Collect()
    {
        MysteryBoxSkillsSO skill = GetRandomSkill();
        SkillsUI.Instance.SetSkill(skill.SkillName, skill.SkillIcon);

        CollectRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void CollectRpc()
    {
        AnimateCollection();
        Invoke(nameof(Respawn), _respawnTimer);
    }

    private void AnimateCollection()
    {
        _collider.enabled = false;
        _boxAnimator.SetTrigger(Consts.BoxAnimations.IS_COLLECTED);
    }

    private void Respawn()
    {
        _boxAnimator.SetTrigger(Consts.BoxAnimations.IS_RESPAWNED);
        _collider.enabled = true;
    }

    private MysteryBoxSkillsSO GetRandomSkill()
    {
        int randomIndex = Random.Range(0,_mysteryBoxSkills.Length);
        return _mysteryBoxSkills[randomIndex];
    }
}
