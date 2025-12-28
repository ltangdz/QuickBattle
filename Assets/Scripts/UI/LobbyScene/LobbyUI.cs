using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LobbyUI : BaseUI
{
    [Space(10)]
    [Header("其他UI引用")]
    [Tooltip("邀请码加入UI")]
    public CodeJoinUI codeJoinUI;
    [Tooltip("创建房间UI")]
    public CreateRoomUI createRoomUI;
    [Tooltip("房间列表UI")]
    public RoomListUI roomListUI;
    
    [Space(10)]
    [Header("大厅子UI")]
    [Tooltip("创建房间按钮")]
    public Button createRoomButton;
    [Tooltip("快速加入按钮")]
    public Button quickJoinButton;
    [Tooltip("邀请码加入按钮")]
    public Button codeJoinButton;
    
    private void Start()
    {
        InitLobbyUI();
    }

    private void InitLobbyUI()
    {
        codeJoinUI.Hide();
        createRoomUI.Hide();
        
        createRoomButton.onClick.AddListener(OnCreateRoomButtonClicked);
        quickJoinButton.onClick.AddListener(OnQuickJoinButtonClicked);
        codeJoinButton.onClick.AddListener(OnCodeJoinButtonClicked);
    }

    private void OnCreateRoomButtonClicked()
    {
        createRoomUI.Show();
    }
    
    private void OnQuickJoinButtonClicked()
    {
        // Todo: 快速加入逻辑
        Debug.Log("快速加入！");
    }
    
    private void OnCodeJoinButtonClicked()
    {
        codeJoinUI.Show();
    }
}
