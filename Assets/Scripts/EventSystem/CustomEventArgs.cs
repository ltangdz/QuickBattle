using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 自定义事件类
public class LocalScoreChangedEventArgs : EventArgs
{
    public int newScore;
}

public class LocalKillAmountChangedEventArgs : EventArgs
{
    public int newKillAmount;
}

public class LocalLevelChangedEventArgs : EventArgs
{
    public int newLevel;
}

public class LocalEnergyChangedEventArgs : EventArgs
{
    public int newEnergy;
    public int energyToUpgrade;
}

public class LocalHpChangedEventArgs : EventArgs
{
    public float newHP;
    public float maxHP;
}

public class SkillCdStartEventArgs : EventArgs
{
    public float maxCd;
}
