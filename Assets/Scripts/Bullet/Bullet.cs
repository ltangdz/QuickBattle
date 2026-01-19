using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using Random = UnityEngine.Random;

public class Bullet : NetworkBehaviour
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

    private NetworkVariable<bool> isFiring = new NetworkVariable<bool>(false);

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    private void Update()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        UpdateFlyingTimer();
    }

    private void UpdateFlyingTimer()
    {
        flyingTimer += Time.deltaTime;
        if (flyingTimer >= flyingTimerMax)
        {
            DestroySelf();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateFlyingTimerServerRpc()
    {

    }
    
    /// <summary>
    /// 在服务器销毁自身
    /// </summary>
    private void DestroySelf()
    {
        flyingTimer = 0f;
        rb.velocity = Vector3.zero;
        isFiring.Value = false;
        
        Debug.Log("子弹自动回收");
        Destroy(gameObject);
    }

    /// <summary>
    /// 初始化弹药 只能在服务器调用
    /// </summary>
    /// <param name="source">弹药来源</param>
    /// <param name="shootDir">射击方向</param>
    public void InitBullet(IDamagable source, Vector3 shootDir, Vector3 shootPos)
    {
        Debug.Log("InitBullet");
        
        rb.isKinematic = false;
        bulletOwner = source;
        damage = source.CalculateDamage();
        
        Vector3 rgb = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        meshRenderer.material.color = new Color(rgb.x, rgb.y, rgb.z);
        
        isFiring.Value = true;

        transform.position = shootPos;
        
        rb.AddForce(shootDir * flyingSpeed, ForceMode.VelocityChange);

        InitBulletClientRpc(rgb);
    }

    [ClientRpc]
    private void InitBulletClientRpc(Vector3 rgb)
    {
        // 设置子弹颜色 (服务器广播)
        // Todo: 目前随机颜色 后面改成和玩家一样的颜色
        meshRenderer.material.color = new Color(rgb.x, rgb.y, rgb.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isFiring.Value || !NetworkManager.Singleton.IsServer) return;

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
}
