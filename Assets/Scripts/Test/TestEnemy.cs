using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人测试类 主要测试玩家攻击方法
/// </summary>
public class TestEnemy : MonoBehaviour, IDamagable
{
    [Space(10)]
    [Header("敌人测试类")]
    [Tooltip("当前血量")]
    [SerializeField] private float curHP = 100;
    [Tooltip("最大血量")]
    [SerializeField] private float maxHP = 100;
    [Tooltip("敌人名称")]
    [SerializeField] private string enemyName = "小怪1";

    [Space(10)] 
    [Header("其他")] 
    [Tooltip("子弹预制体")] public GameObject bulletPrefab;
    [Tooltip("子弹生成位置")] public Transform shootPos;
    
    private float shootTimer = 0f;
    private float shootTimerMax = 1f;

    private void Start()
    {
        // StartCoroutine(AttackCoroutine());
    }

    private void Update()
    {
        if (shootTimer < shootTimerMax)
        {
            shootTimer += Time.deltaTime;
        }
        else
        {
            Bullet bullet = (Bullet)PoolManager.Instance.ReuseComponent(bulletPrefab, shootPos.position, Quaternion.identity);
            bullet.InitBullet(this, transform.forward, shootPos.position);
            shootTimer = 0f;
        }
    }

    private IEnumerator AttackCoroutine()
    {
        while (true)
        {
            // Todo: 对象池优化
            // Bullet bullet = Instantiate(bulletPrefab, shootPos.position, Quaternion.identity).GetComponent<Bullet>();
            Bullet bullet = (Bullet)PoolManager.Instance.ReuseComponent(bulletPrefab, shootPos.position, Quaternion.identity);

            bullet.InitBullet(this, transform.forward, shootPos.position);
            
            yield return new WaitForSeconds(shootTimer);
        }
    }

    public void TakeDamage(float damage, IDamagable source = null)
    {
        curHP -= damage;
        if (source != null)
        {
            Debug.Log($"敌人{enemyName} 受到来自 {source.Name} 的 {damage} 点伤害, 血量 {curHP}/{maxHP}");
        }

        if (curHP <= 0f)
        {
            curHP = 0f;
            
            // 增加玩家击杀数
            Player player = source as Player;
            player?.IncreaseKillAmount();
            
            // 可以增加播放死亡动画等逻辑 用协程实现
            
            // 销毁对象
            Destroy(this.gameObject);
        }
    }

    public float CalculateDamage()
    {
        // 测试 一刀50血
        return 50f;
    }

    public string Name => enemyName;
}
