using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PoolManager : Singleton<PoolManager>
{
    [Tooltip("填入将被池化的预制体")]
    [SerializeField] private Pool[] poolArray = null;
    private Transform objectPoolTransform;
    
    // 把所有对象池队列 使用一个字典统一管理
    private Dictionary<int, Queue<Component>> poolDictionary = new Dictionary<int, Queue<Component>>();
    
    [Serializable]
    public struct Pool
    {
        // 对象池容量
        public int poolSize;
        
        // 预制体
        public GameObject prefab;
        
        // 类名
        public string componentType;
    }

    private void Start()
    {
        objectPoolTransform = transform;

        // 初始化对象池
        for (int i = 0; i < poolArray.Length; i++)
        {
            CreatePool(poolArray[i].prefab, poolArray[i].poolSize, poolArray[i].componentType);
        }
    }

    /// <summary>
    /// 初始化对象池
    /// </summary>
    /// <param name="prefab">预制体</param>
    /// <param name="poolSize">对象池大小</param>
    /// <param name="componentType">类名</param>
    private void CreatePool(GameObject prefab, int poolSize, string componentType)
    {
        int poolKey = prefab.GetInstanceID();
        
        string prefabName = prefab.name;
        
        GameObject parentGameObject = new GameObject(prefabName + "Anchor");
        
        parentGameObject.transform.SetParent(objectPoolTransform);

        // 避免对同一prefab重复创建对象池
        if (!poolDictionary.ContainsKey(poolKey))
        {
            poolDictionary.Add(poolKey, new Queue<Component>());

            for (int i = 0; i < poolSize; i++)
            {
                GameObject newObject = Instantiate(prefab, parentGameObject.transform);
                
                newObject.SetActive(false);

                // 初始化: 入队
                poolDictionary[poolKey].Enqueue(newObject.GetComponent(Type.GetType(componentType)));
            }
        }
    }

    /// <summary>
    /// 复用池中对象
    /// </summary>
    /// <param name="prefab">预制体</param>
    /// <param name="position">位置</param>
    /// <param name="rotation">旋转</param>
    /// <returns></returns>
    public Component ReuseComponent(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        int poolKey = prefab.GetInstanceID();

        if (poolDictionary.ContainsKey(poolKey))
        {
            // 出队 从池中取出第一个可用对象
            Component componentToReuse = GetComponentFromPool(poolKey); //poolDictionary[poolKey].Dequeue();
            
            // 重新设置对象
            ResetObject(position, rotation, componentToReuse, prefab);
            
            return componentToReuse;
        }
        else
        {
            Debug.Log($"对象池中没有该预制体 {prefab} 的信息");
            return null;
        }
    }

    /// <summary>
    /// 从池中取出第一个可用对象
    /// </summary>
    /// <param name="poolKey">预制体的全局唯一id (对应对象池在字典中的key)</param>
    /// <returns></returns>
    private Component GetComponentFromPool(int poolKey)
    {
        // 第一个可用对象先出队 再立即入队
        Component componentToReuse = poolDictionary[poolKey].Dequeue();
        poolDictionary[poolKey].Enqueue(componentToReuse);

        if (componentToReuse.gameObject.activeSelf)
        {
            componentToReuse.gameObject.SetActive(false);
        }
        
        return componentToReuse;
    }
    
    /// <summary>
    /// 重现设置池中取出的对象信息
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="componentToReuse"></param>
    /// <param name="prefab"></param>
    private void ResetObject(Vector3 position, Quaternion rotation, Component componentToReuse, GameObject prefab)
    {
        componentToReuse.transform.position = position;
        componentToReuse.transform.rotation = rotation;
        componentToReuse.transform.localScale = prefab.transform.localScale;
    }
}
