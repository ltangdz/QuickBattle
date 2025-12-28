using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 所有有血条的对象 都要实现此接口
/// </summary>
public interface IDamagable
{
    /// <summary>
    /// 处理受到伤害逻辑
    /// </summary>
    /// <param name="damage">伤害大小</param>
    /// <param name="source">伤害来源</param>
    public void TakeDamage(float damage, IDamagable source = null);

    /// <summary>
    /// 自定义伤害计算逻辑
    /// </summary>
    /// <returns>伤害大小</returns>
    public float CalculateDamage();
    
    public string Name { get; }
}
