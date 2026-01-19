using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

/// <summary>
/// 地图资源管理类 (控制资源生成算法)
/// </summary>
public class ResourceManager : SingletonNetwork<ResourceManager>
{
    [Header("核心配置")]
    [Tooltip("能量块预制体")]
    [SerializeField] private Transform energyBlockPrefab;
    [Tooltip("地图当前能量块总量")]
    [SerializeField] private int curEnergyBlockCount = 0;
    [Tooltip("地图初始能量块总量")]
    [SerializeField] private int initEnergyBlockCount = 10;
    [Tooltip("地图最大能量块总量")]
    [SerializeField] private int maxEnergyBlockCount = 60;
    [Tooltip("随机生成区域 用2个顶点表示 左上")]
    [SerializeField] private Vector3 leftTopPoint;
    [Tooltip("随机生成区域 用2个顶点表示 右下")]
    [SerializeField] private Vector3 rightBottomPoint;
    [Tooltip("资源生成 默认y轴高度")]
    [SerializeField] private float energyBlockSpawnHeight = 0f;
    [Tooltip("资源之间最小生成间距（避免重叠）")]
    [SerializeField] private float minSpawnDistance = 2f;
    [Tooltip("资源刷新 最小时间间隔")]
    [SerializeField] private float minRespawnTimeInterval = 1f;
    [Tooltip("资源刷新 最大时间间隔")]
    [SerializeField] private float maxRespawnTimeInterval = 3f;
    [Tooltip("能量块生成时 避免重叠的Layer")]
    [SerializeField] private LayerMask spawnDetectLayerMask;

    public float curSpawnTimeStamp;
    
    /// <summary>
    /// 初始化地图资源
    /// </summary>
    public void Init()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        Debug.Log("ResourceManager 初始化");
        
        for (int i = 0; i < initEnergyBlockCount; i++)
        {
            SpawnEnergyBlock();
        }
        
        EventManager.Instance.AddListener(EventName.EnergyBlockPicked, OnEnergyBlockPicked);
        
        curSpawnTimeStamp = Time.time + Random.Range(minRespawnTimeInterval, maxRespawnTimeInterval);
    }

    private void Update()
    {
        if (!GameManager.Instance.IsStart || !NetworkManager.Singleton.IsServer) return;
        
        // 随机时间间隔后 执行资源补刷
        if (curSpawnTimeStamp < Time.time)
        {
            RespawnEnergyBlock();
            curSpawnTimeStamp = Time.time + Random.Range(minRespawnTimeInterval, maxRespawnTimeInterval);
        }
    }

    /// <summary>
    /// 资源生成算法
    /// </summary>
    public void SpawnEnergyBlock()
    {
        // 随机坐标
        Vector3 spawnPos = GetValidSpawnPos();
        if (spawnPos == Vector3.zero)
        {
            Debug.Log("获取合法坐标失败");
            return;
        }
        
        // 随机旋转
        Quaternion spawnRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        
        // 执行生成逻辑
        // EnergyBlock energyBlock = (EnergyBlock)PoolManager.Instance.ReuseComponent(energyBlockPrefab, spawnPos, spawnRot);
        // var energyBlockNetworkObject = NetworkPoolManager.Instance.ReuseAndShowNetworkObject(energyBlockPrefab, spawnPos, spawnRot);

        Transform energyBlockTransform = Instantiate(energyBlockPrefab);
        energyBlockTransform.position = spawnPos;
        energyBlockTransform.rotation = spawnRot;
        NetworkObject energyBlockNetworkObject = energyBlockTransform.GetComponent<NetworkObject>();
        energyBlockNetworkObject.Spawn();
        
        curEnergyBlockCount++;
    }

    /// <summary>
    /// 获取合法资源生成坐标 失败返回Vector3.zero
    /// </summary>
    /// <returns></returns>
    private Vector3 GetValidSpawnPos()
    {
        // 在10次以内 获得合法随机坐标 避免死循环
        int maxSpawnAttempts = 10;
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            float randomX = Random.Range(leftTopPoint.x, rightBottomPoint.x);
            float randomZ = Random.Range(leftTopPoint.z, rightBottomPoint.z);
            
            // 判断是否发生重叠
            if (CheckOverlapWithOtherEnergyBlocks(randomX, randomZ)) continue;
            
            // 成功生成 返回坐标
            return new Vector3(randomX, energyBlockSpawnHeight, randomZ);
        }
        
        return Vector3.zero;
    }

    private bool CheckOverlapWithOtherEnergyBlocks(float x, float z)
    {
        // 方法一: 获取能量块的对象队列 在队列中查找有没有产生重叠的能量块 (效率极低)
        // int prefabId = energyBlockPrefab.GetInstanceID();
        // var energyBlockQueue = PoolManager.Instance.GetComponentPool(prefabId);
        
        // 方法二: 使用unity物理检测
        // 核心：检测目标位置周围，半径为minSpawnDistance的球形范围内的资源
        // 参数1：球形中心（目标生成位置）
        // 参数2：球形半径（最小间距，确保资源不重叠）
        // 参数3：检测层（仅资源层，过滤无关对象）
        Vector3 targetPos = new Vector3(x, energyBlockSpawnHeight, z);
        Collider[] overlappingColliders = Physics.OverlapSphere(targetPos, minSpawnDistance, spawnDetectLayerMask);
        
        // collider数量>0 则发生重叠
        return overlappingColliders.Length > 0;
    }

    /// <summary>
    /// 能量块补刷机制 (玩家拾取资源后触发)
    /// </summary>
    private void RespawnEnergyBlock()
    {
        int minRespawnCount = 0;
        int maxRespawnCount = 3;
        int respawnCount = Random.Range(minRespawnCount, maxRespawnCount + 1);
        // 随机执行0~3次刷新算法
        for (int i = 0; i < respawnCount; i++)
        {
            if (curEnergyBlockCount >= maxEnergyBlockCount)
            {
                Debug.Log("当前地图能量块数已达上限！！！");
                return;
            }
            
            SpawnEnergyBlock();
        }
    }

    /// <summary>
    /// 执行资源被拾取时的逻辑
    /// </summary>
    private void OnEnergyBlockPicked(object sender, EventArgs eventArgs)
    {
        curEnergyBlockCount--;
        Debug.Log($"当前资源总数: {curEnergyBlockCount}");
        
        RespawnEnergyBlock();
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        float radius = 1f;
        
        // 绘制2个顶点
        Gizmos.DrawWireSphere(leftTopPoint, radius);
        Gizmos.DrawWireSphere(rightBottomPoint, radius);
    }
}
