// using System;
// using System.Collections.Generic;
// using Unity.Netcode;
// using Unity.Netcode.Components;
// using UnityEngine;
//
//
// public class NetworkPoolManager : Singleton<NetworkPoolManager>
// {
//     [Tooltip("填入将被池化的预制体")]
//     [SerializeField] private Pool[] poolArray = null;
//     private Transform objectPoolTransform;
//     
//     // 把所有对象池队列 使用一个字典统一管理
//     private Dictionary<int, Queue<Component>> poolDictionary = new Dictionary<int, Queue<Component>>();
//     
//     [Serializable]
//     public struct Pool
//     {
//         // 对象池容量
//         public int poolSize;
//         
//         // 预制体
//         public GameObject prefab;
//         
//         // 类名
//         public string componentType;
//     }
//
//
//     protected override void Awake()
//     {
//         base.Awake();
//         
//         objectPoolTransform = transform;
//     }
//
//     private void Start()
//     {
//         NetworkManager.Singleton.OnServerStarted += NetworkManager_OnServerStarted;
//
//     }
//
//
//     /// <summary>
//     /// 在服务器启动时 初始化对象池
//     /// </summary>
//     private void NetworkManager_OnServerStarted()
//     {
//         if (!NetworkManager.Singleton.IsServer) return;
//         
//         Debug.Log("服务器启动 初始化对象池......");
//         
//         // 初始化对象池
//         for (int i = 0; i < poolArray.Length; i++)
//         {
//             CreatePool(poolArray[i].prefab, poolArray[i].poolSize, poolArray[i].componentType);
//         }
//         
//         // 发布对象池初始完毕事件
//         this.TriggerEvent(EventName.NetworkObjectPoolCreated);
//     }
//
//     /// <summary>
//     /// 初始化对象池 只能在服务器进行
//     /// </summary>
//     /// <param name="prefab">预制体</param>
//     /// <param name="poolSize">对象池大小</param>
//     /// <param name="componentType">类名</param>
//     private void CreatePool(GameObject prefab, int poolSize, string componentType)
//     {
//         int poolKey = prefab.GetInstanceID();
//         
//         string prefabName = prefab.name;
//         
//         GameObject parentGameObject = new GameObject(prefabName + "Anchor");
//         parentGameObject.transform.SetParent(objectPoolTransform);
//
//         
//         // 避免对同一prefab重复创建对象池
//         if (!poolDictionary.ContainsKey(poolKey))
//         {
//             poolDictionary.Add(poolKey, new Queue<Component>());
//
//             for (int i = 0; i < poolSize; i++)
//             {
//                 GameObject newObject = Instantiate(prefab);
//                 NetworkObject newNetworkObject = newObject.GetComponent<NetworkObject>();
//                 
//                 // newObject.transform.SetParent(parentGameObject.transform);
//                 
//                 // 未同步的网络对象 进行网络同步
//                 newNetworkObject.Spawn();
//
//                 HideNetworkObjectClientRpc(newNetworkObject);
//
//                 // 初始化: 入队
//                 poolDictionary[poolKey].Enqueue(newObject.GetComponent(Type.GetType(componentType)));
//             }
//         }
//     }
//
//     [ClientRpc]
//     private void HideNetworkObjectClientRpc(NetworkObjectReference networkObjectReference)
//     {
//         if (networkObjectReference.TryGet(out NetworkObject networkObject))
//         {
//             networkObject.gameObject.SetActive(false);
//         }
//     }
//
//     /// <summary>
//     /// 复用池中对象 只能在服务器调用
//     /// </summary>
//     /// <param name="prefab">预制体</param>
//     /// <param name="position">位置</param>
//     /// <param name="rotation">旋转</param>
//     /// <returns></returns>
//     public NetworkObject ReuseAndShowNetworkObject(GameObject prefab, Vector3 position, Quaternion rotation)
//     {
//         int poolKey = prefab.GetInstanceID();
//
//         if (poolDictionary.ContainsKey(poolKey))
//         {
//             // 出队 从池中取出第一个可用对象
//             Component componentToReuse = GetComponentFromPool(poolKey); //poolDictionary[poolKey].Dequeue();
//             
//             NetworkObject networkObject = componentToReuse.gameObject.GetComponent<NetworkObject>();
//
//             // 重新设置对象
//             ResetNetworkObject(position, rotation, networkObject, prefab);
//             
//             return networkObject;
//         }
//         else
//         {
//             Debug.Log($"对象池中没有该预制体 {prefab} 的信息");
//             return null;
//         }
//     }
//
//     /// <summary>
//     /// 从池中取出第一个可用对象
//     /// </summary>
//     /// <param name="poolKey">预制体的全局唯一id (对应对象池在字典中的key)</param>
//     /// <returns></returns>
//     private Component GetComponentFromPool(int poolKey)
//     {
//         // 第一个可用对象先出队 再立即入队
//         Component componentToReuse = poolDictionary[poolKey].Dequeue();
//         poolDictionary[poolKey].Enqueue(componentToReuse);
//
//         if (componentToReuse.gameObject.activeSelf)
//         {
//             componentToReuse.gameObject.SetActive(false);
//         }
//         
//         return componentToReuse;
//     }
//     
//     /// <summary>
//     /// 重现设置池中取出的对象信息
//     /// </summary>
//     /// <param name="position"></param>
//     /// <param name="rotation"></param>
//     /// <param name="componentToReuse"></param>
//     /// <param name="prefab"></param>
//     private void ResetNetworkObject(Vector3 position, Quaternion rotation, NetworkObject networkObject, GameObject prefab)
//     {
//         // ResetNetworkObjectClientRpc
//         networkObject.transform.position = position;
//         networkObject.transform.rotation = rotation;
//         networkObject.transform.localScale = prefab.transform.localScale;
//         networkObject.gameObject.SetActive(true);
//     }
//     
// }