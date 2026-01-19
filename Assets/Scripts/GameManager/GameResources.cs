using UnityEngine;

/// <summary>
/// 全局资源管理类
/// 踩坑: ScriptableObjects文件, 在项目构建后没有加载, 所以需要手动对SO文件进行资源管理
/// </summary>
public class GameResources : MonoBehaviour
{
    private static GameResources instance;

    public static GameResources Instance
    {
        get
        {
            if (!instance)
            {
                instance = Resources.Load<GameResources>("GameResources");
            }
            return instance;
        }
    }
    
    [Space(10)]
    [Header("Player")]
    [Tooltip("玩家等级信息列表")]
    public PlayerLevelInfoListSO playerLevelInfoListSO;
}
