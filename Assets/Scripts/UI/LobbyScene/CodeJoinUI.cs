using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CodeJoinUI : BaseUI
{
    [Space(10)]
    [Header("子UI")]
    [Tooltip("邀请码输入框")]
    public TMP_InputField codeInputField;
    [Tooltip("重新输入按钮")]
    public Button reInputButton;
    [Tooltip("确认按钮")]
    public Button confirmButton;
    [Tooltip("关闭按钮")]
    public Button closeButton;

    private void Start()
    {
        InitCodeJoinUI();
    }

    private void InitCodeJoinUI()
    {
        // Todo: 邀请码输入逻辑
        
        closeButton.onClick.AddListener(Hide);
    }
}
