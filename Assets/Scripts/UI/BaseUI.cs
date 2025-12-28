using UnityEngine;

/// <summary>
/// 所有UI的基类 包括显示/隐藏方法
/// </summary>
public class BaseUI : MonoBehaviour
{
    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}