using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Bullet : MonoBehaviour
{
    [Space(10)]
    [Header("子弹属性")]
    [Tooltip("子弹飞行时间")]
    [SerializeField] private float flyingTimer = 0f;
    [Tooltip("子弹飞行最大时间 (超过后销毁)")]
    [SerializeField] private float flyingTimerMax = 5f;
    [Tooltip("子弹飞行速度")]
    [SerializeField] private float flyingSpeed = 50f;
    
    private IDamagable bulletOwner;
    private float damage;
    private Rigidbody rb;
    
    private MeshRenderer meshRenderer;

    private void Update()
    {
        flyingTimer += Time.deltaTime;
        if (flyingTimer >= flyingTimerMax)
        {
            DestroySelf();
        }
    }

    /// <summary>
    /// 初始化弹药
    /// </summary>
    /// <param name="source">弹药来源</param>
    /// <param name="shootDir">射击方向</param>
    public void Init(IDamagable source, Vector3 shootDir)
    {
        gameObject.SetActive(true);
        
        bulletOwner = source;
        damage = source.CalculateDamage();
        rb = GetComponent<Rigidbody>();
        
        meshRenderer = GetComponentInChildren<MeshRenderer>();

        // 设置子弹初速度
        rb.AddForce(shootDir * flyingSpeed, ForceMode.VelocityChange);

        // 设置子弹颜色
        // Todo: 目前随机颜色 后面改成和玩家一样的颜色
        meshRenderer.material.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        // StartCoroutine(DestroyCoroutine());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Settings.TAG_PLAYER))
        {
            IDamagable player = other.GetComponent<IDamagable>();
            if (player != bulletOwner)
            {
                // Debug.Log($"{bulletOwner.Name} 对 {player.Name} 造成了 {damage} 点伤害！");
                
                player.TakeDamage(damage, bulletOwner);
                DestroySelf();
            }
        }
        else
        {
            // 碰到其他障碍物
            DestroySelf();
        }
    }

    private IEnumerator DestroyCoroutine()
    {
        while (flyingTimer < flyingTimerMax)
        {
            flyingTimer += Time.deltaTime;
            yield return null;
        }
        
        DestroySelf();
    }

    /// <summary>
    /// 由于使用了对象池优化技术 所以只是隐藏自己 并非真的销毁
    /// </summary>
    private void DestroySelf()
    {
        flyingTimer = 0f;
        rb.velocity = Vector3.zero;
        
        gameObject.SetActive(false);
    }
}
