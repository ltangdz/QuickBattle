using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// 挂载在 MainCamera 上
[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Space(10)]
    [Header("核心配置")]
    [Tooltip("相机跟随目标")]
    [SerializeField] private Transform followTarget;
    [Tooltip("相机偏移量 (推荐(0,-12,20))")]
    [SerializeField] private Vector3 cameraOffset;
    [Tooltip("鼠标灵敏度 (控制相机旋转)")]
    [SerializeField] private float mouseSensitivity = 1.5f;
    [Tooltip("滚轮灵敏度 (控制相机视野缩放)")]
    [SerializeField] private float scrollSensitivity = 35f;

    [Space(10)]
    [Header("视角限制（避免过度旋转）")]
    [Tooltip("当前垂直旋转 (rotation.x)")]
    [SerializeField] private float curRotX = 10f; // 当前垂直旋转角度（控制上下视角）
    [Tooltip("最小垂直角度 (向上仰视限制)")]
    [SerializeField] private float minRotX = 15f;
    [Tooltip("最大垂直角度 (向下俯视限制)")]
    [SerializeField] private float maxRotX = 40f;
    [Tooltip("当前相机位置偏移 (position.y)")]
    [SerializeField] private float curPosOffsetY = 10f;
    [Tooltip("最小偏移")]
    [SerializeField] private float minPosOffsetY = 12.5f;
    [Tooltip("最大偏移")]
    [SerializeField] private float maxPosOffsetY = 25f;
    [Tooltip("最小视野限制")]
    [SerializeField] private float minFOV = 30f;
    [Tooltip("最大视野限制")]
    [SerializeField] private float maxFOV = 55f;

    [Space(10)]
    [Header("平滑配置")]
    [Tooltip("是否平滑旋转 (插值优化)")]
    [SerializeField] private bool useSmooth = true;
    [Tooltip("平滑速度")]
    [SerializeField] private float smoothSpeed = 20f;
    
    [Tooltip("鼠标指针")]
    public ScreenCursor screenCursor;
    

    private Camera followCamera;
    private void Start()
    {
        followCamera = GetComponent<Camera>();

        // 初始化垂直角度（基于初始偏移）
        // curRotX = transform.eulerAngles.x;
        curRotX = minRotX;
        curPosOffsetY = minPosOffsetY;
        
        // 锁定鼠标到屏幕中心（可选，第五人格风格可开启）
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
        
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        // 本地玩家连接
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("本地玩家已连接！");
            followTarget = Player.LocalInstance.transform;
        }
    }

    private void LateUpdate()
    {
        if (followTarget != null)
        {
            UpdateCameraTrans();
        }
    }

    /// <summary>
    /// 更新相机变换
    /// </summary>
    private void UpdateCameraTrans()
    {
        // 1. 获取鼠标输入
        // Pointer pointer = Pointer.current;
        // float mouseX = pointer.delta.ReadValue().x * mouseSensitivity;
        // float mouseY = pointer.delta.ReadValue().y * mouseSensitivity;

        float mouseX = GameInput.Instance.GetMousePosition().x * mouseSensitivity;
        float mouseY = GameInput.Instance.GetMousePosition().y * mouseSensitivity;
        float scroll = GameInput.Instance.GetScrollWheelValue();
        
        // float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity; // 水平旋转 (左右)
        // float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity; // 垂直旋转 (上下)
        // float scroll = Input.GetAxis("Mouse ScrollWheel");          // 滚轮输入 [-1, 1]

        // if (screenCursor.gameObject.activeSelf)
        // {
        //     mouseX = 0f;
        //     mouseY = 0f;
        //     scroll = 0f;
        // }


        // 2. 计算并限制垂直旋转角度（避免过度抬头/低头）
        // curPosOffsetY += mouseY;
        // curRotX += mouseY * 2;
        
        curPosOffsetY = Mathf.Clamp(curPosOffsetY - mouseY, minPosOffsetY, maxPosOffsetY);
        curRotX = Mathf.Clamp(curRotX - mouseY * 2, minRotX, maxRotX);

        // 3. 相机绕玩家旋转（核心逻辑）
        // 水平旋转：相机绕玩家Y轴旋转（左右转向）
        transform.RotateAround(followTarget.position, Vector3.up, mouseX);

        // 4. 调整垂直角度并保持相机与玩家的固定距离
        // 计算相机目标位置：玩家位置 + 旋转后的偏移
        Quaternion targetRot = Quaternion.Euler(curRotX, transform.eulerAngles.y, 0);
        Vector3 targetPos = followTarget.position + targetRot * cameraOffset + Vector3.up * curPosOffsetY;

        // 5. 平滑移动相机（可选）
        if (useSmooth)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * smoothSpeed);
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);
        }
        else
        {
            transform.rotation = targetRot;
            transform.position = targetPos;
        }

        float targetFOV = Mathf.Clamp(followCamera.fieldOfView - scroll * scrollSensitivity, minFOV, maxFOV);
        followCamera.fieldOfView = targetFOV;
    }
}