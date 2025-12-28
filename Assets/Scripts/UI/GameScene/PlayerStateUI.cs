using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerStateUI : BaseUI
{
    [Space(10)]
    [Header("子UI控件")]
    [Tooltip("能量条")]
    public ProgressBarUI energyBarUI;
    [Tooltip("血条")]
    public ProgressBarUI hpBarUI;
    [Tooltip("等级文本")]
    public TextMeshProUGUI levelText;

    private void Start()
    {
        // 监听事件
        BindProgressBarChangedEvent();
    }

    private void OnDestroy()
    {
        RemoveProgressBarChangedEvent();
    }

    private void BindProgressBarChangedEvent()
    {
        EventManager.Instance.AddListener(EventName.LocalEnergyChanged, OnLocalEnergyChanged);
        EventManager.Instance.AddListener(EventName.LocalHpChanged, OnLocalHpChanged);
        EventManager.Instance.AddListener(EventName.LocalLevelChanged, OnLocalLevelChanged);
    }

    private void RemoveProgressBarChangedEvent()
    {
        EventManager.Instance.RemoveListener(EventName.LocalEnergyChanged, OnLocalEnergyChanged);
        EventManager.Instance.RemoveListener(EventName.LocalHpChanged, OnLocalHpChanged);
        EventManager.Instance.RemoveListener(EventName.LocalLevelChanged, OnLocalLevelChanged);
    }

    private void OnLocalLevelChanged(object sender, EventArgs e)
    {
        if (e is LocalLevelChangedEventArgs localLevelChangedEventArgs)
        {
            int level = localLevelChangedEventArgs.newLevel;
            levelText.text = $"Level {level}";
        }
    }

    private void OnLocalHpChanged(object sender, EventArgs e)
    {
        if (e is LocalHpChangedEventArgs localHpChangedEventArgs)
        {
            float curHP = localHpChangedEventArgs.newHP;
            float maxHP = localHpChangedEventArgs.maxHP;
            
            hpBarUI.SetFillAmount(curHP / maxHP);
            hpBarUI.SetText($"{(int)curHP}/{(int)maxHP}");
        }
    }

    private void OnLocalEnergyChanged(object sender, EventArgs e)
    {
        if (e is LocalEnergyChangedEventArgs localEnergyChangedEventArgs)
        {
            int energy = localEnergyChangedEventArgs.newEnergy;
            int maxEnergy = localEnergyChangedEventArgs.energyToUpgrade;
            
            energyBarUI.SetFillAmount(energy / (float)maxEnergy);
            energyBarUI.SetText($"{energy}/{maxEnergy}");
        }
    }
}
