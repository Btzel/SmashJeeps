using Unity.Netcode;
using UnityEngine;

public class PlayerSkillController : NetworkBehaviour
{
    [SerializeField] private bool _hasSkillAlready;

    private MysteryBoxSkillsSO _mysteryBoxSkill;
    private bool _isSkillUsed;
    private void Update()
    {

        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Space) && !_isSkillUsed)
        {
            ActivateSkill();
            _isSkillUsed = true;
        }
    }
    public void SetupSkill(MysteryBoxSkillsSO skill)
    {
        _mysteryBoxSkill = skill;
        _hasSkillAlready = true;
        _isSkillUsed = false;
    }

    public void ActivateSkill()
    {
        if (!_hasSkillAlready) return;

        SkillsUI.Instance.SetSkillToNone();
        _hasSkillAlready = false;
        Debug.Log($"Skill Used: {_mysteryBoxSkill.SkillType}");
    }

    public bool HasSkillAlready()
    {
        return _hasSkillAlready;
    }
}
