using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyBlock : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Settings.TAG_PLAYER))
        {
            // Todo: 玩家触发能量块逻辑
            // 1. 玩家处理拾取逻辑 (增加积分等)
            // 2. 能量块补充刷新逻辑 (自己消失 随机生成新的能量块)
            this.TriggerEvent(EventName.EnergyBlockPicked);            
            
            // 3. 自身回收到对象池
            gameObject.SetActive(false);
        }
    }
}
