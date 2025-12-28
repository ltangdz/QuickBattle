using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomUI : BaseUI
{
    [Space(10)]
    [Header("子UI")]
    [Tooltip("公开创建按钮")]
    public Button createPublicButton;
    [Tooltip("私密创建按钮")]
    public Button createPrivateButton;
    [Tooltip("关闭按钮")]
    public Button closeButton;

    private void Start()
    {
        InitCreateRoomUI();
    }

    private void InitCreateRoomUI()
    {
        // Todo: 创建房间方法
        
        closeButton.onClick.AddListener(Hide);
    }
}
