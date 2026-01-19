using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 静态类 保存事件名称
// 可以用enum代替 但是装箱拆箱会带来性能损耗
public static class EventName
{
    public const string EnergyBlockPicked = "EnergyBlockPicked";
    
    // Player.cs
    public const string LocalScoreChanged = "LocalScoreChanged";
    public const string LocalKillAmountChanged = "LocalKillAmountChanged";
    public const string LocalEnergyChanged = "LocalEnergyChanged";
    public const string LocalHpChanged = "LocalHpChanged";
    public const string LocalLevelChanged = "LocalLevelChanged";
    public const string AttackCdStarted = "AttackCdStarted";
    public const string AccelerateCdStarted = "AccelerateCdStarted";
    public const string HealCdStarted = "HealCdStarted";
    
    // NetworkPoolManager.cs
    public const string NetworkObjectPoolCreated = "NetworkObjectPoolCreated";
}
