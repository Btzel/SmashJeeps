using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerSkillController : NetworkBehaviour
{
    public static event Action OnTimerFinished;

    [SerializeField] private bool _hasSkillAlready;

    private MysteryBoxSkillsSO _mysteryBoxSkill;
    private bool _isSkillUsed;
    private bool _hasTimerStarted;
    private float _timer;
    private float _timerMax;
    private void Update()
    {

        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Space) && !_isSkillUsed)
        {
            ActivateSkill();
            _isSkillUsed = true;
        }

        if (_hasTimerStarted)
        {
            _timer -= Time.deltaTime;
            SkillsUI.Instance.SetTimerCounterText((int)_timer);
            if(_timer <= 0)
            {
                OnTimerFinished?.Invoke();
                SkillsUI.Instance.SetSkillToNone();
                _hasTimerStarted = false;
                _hasSkillAlready = false;
                
            }
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

        SkillManager.Instance.ActivateSkill(_mysteryBoxSkill.SkillType,transform,OwnerClientId);

        SetSkillToNone();
    }

    private void SetSkillToNone()
    {
        if(_mysteryBoxSkill.SkillUsageType == SkillUsageType.None)
        {
            _hasSkillAlready = false;
            SkillsUI.Instance.SetSkillToNone();
        }

        if(_mysteryBoxSkill.SkillUsageType == SkillUsageType.Timer)
        {
            _hasTimerStarted = true;
            _timerMax = _mysteryBoxSkill.SkillData.SpawnAmountOrTimer;
            _timer = _timerMax;
            Debug.Log("Again");
        }

        
    }

    public bool HasSkillAlready()
    {
        return _hasSkillAlready;
    }
}
