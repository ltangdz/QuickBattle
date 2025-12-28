using UnityEngine;

public class ScreenCursor : MonoBehaviour
{
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }


    private void Update()
    {
        // 按下 "CapsLock" 切换指针显示
        // Todo: 新输入系统
        if (Application.isMobilePlatform || !gameObject.activeSelf) return;

        transform.position = Input.mousePosition;
    }

}