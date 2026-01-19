using UnityEngine;
using UnityEngine.InputSystem;

public class ScreenCursor : MonoBehaviour
{
    public RectTransform rectTransform;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }


    private void Update()
    {
        // 按下 "CapsLock" 切换指针显示
        // Todo: 新输入系统
        if (Application.isMobilePlatform || !gameObject.activeSelf) return;

        Pointer pointer = Pointer.current;
        rectTransform.position = pointer.position.ReadValue();

        // transform.position = Input.mousePosition;
    }

}