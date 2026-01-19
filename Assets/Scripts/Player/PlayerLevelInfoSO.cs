using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家等级信息SO类
/// </summary>
[CreateAssetMenu(fileName = "PlayerLevelInfoSO", menuName = "ScriptableObject/Player/PlayerLevelInfoSO")]
public class PlayerLevelInfoSO : ScriptableObject
{
    [Tooltip("当前等级")]
    public int curLevel;
    
    [Tooltip("升级所需能量")]
    public int energyToUpgrade;
    
    [Tooltip("当前等级攻击力")]
    public int atk;
    
    [Tooltip("当前等级最大血量")]
    public int maxHP;
}
