using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillCoolDownUI : MonoBehaviour
{
    [Space(10)]
    [Header("技能cd UI")]
    [Tooltip("攻击cd UI")]
    public SingleSkillUI attackCdUI;
    [Tooltip("加速cd UI")]
    public SingleSkillUI accelerateCdUI;
    [Tooltip("治疗cd UI")]
    public SingleSkillUI healCdUI;

    private void Start()
    {
        InitSingleSkillUI();
        BindSkillButtons();
        BindSkillCdStartEvent();
    }
    
    private void OnDestroy()
    {
        RemoveSkillCdEvent();
    }
    
    private void InitSingleSkillUI()
    {
        attackCdUI.Init();
        accelerateCdUI.Init();
        healCdUI.Init();
    }
    
    /// <summary>
    /// 绑定cd开始事件
    /// </summary>
    private void BindSkillCdStartEvent()
    {
        EventManager.Instance.AddListener(EventName.AttackCdStarted, attackCdUI.StartSkillCd);
        EventManager.Instance.AddListener(EventName.AccelerateCdStarted, accelerateCdUI.StartSkillCd);
        EventManager.Instance.AddListener(EventName.HealCdStarted, healCdUI.StartSkillCd);
    }

    private void RemoveSkillCdEvent()
    {
        EventManager.Instance.RemoveListener(EventName.AttackCdStarted, attackCdUI.StartSkillCd);
        EventManager.Instance.RemoveListener(EventName.AccelerateCdStarted, accelerateCdUI.StartSkillCd);
        EventManager.Instance.RemoveListener(EventName.HealCdStarted, healCdUI.StartSkillCd);
    }
    

    /// <summary>
    /// 绑定技能按钮
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void BindSkillButtons()
    {
        attackCdUI.BindSkillButton(SkillType.Attack);
        accelerateCdUI.BindSkillButton(SkillType.Accelerate);
        healCdUI.BindSkillButton(SkillType.Heal);
    }
}
