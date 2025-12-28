using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MainUI : BaseUI
{
    [Space(10)]
    [Header("主UI子控件")]
    [Tooltip("游戏计时器文本")]
    public TextMeshProUGUI countDownTimerText;
    [Tooltip("当前得分文本")]
    public TextMeshProUGUI curScoreText;
    [Tooltip("当前击杀文本")]
    public TextMeshProUGUI curKillText;
    [Tooltip("设置按钮")]
    public Button settingButton;
    [Tooltip("鼠标光标")]
    public ScreenCursor screenCursor;
    // 能量条
    
    // 设置UI
    
    // 排行榜UI
    
    
    [Space(10)]
    [Header("子UI引用")]
    [Tooltip("结算界面UI")]
    public GameOverUI gameOverUI;
    [Tooltip("技能cd UI")]
    public SkillCoolDownUI skillCoolDownUI;
    [Tooltip("玩家状态 UI")]
    public PlayerStateUI playerStateUI;
    
    private bool isShowCursor;

    private void Start()
    {
        // 监听分数变化 以及 玩家击杀增加事件
        EventManager.Instance.AddListener(EventName.LocalScoreChanged, UpdateCurScoreText);
        EventManager.Instance.AddListener(EventName.LocalKillAmountChanged, UpdateCurKillText);
        
        UpdateCurScoreText(0);
        UpdateCurKillText(0);
        // gameOverUI.Hide();

        // settingButton.onClick.AddListener();
        
        if (!Application.isMobilePlatform) isShowCursor = true;
    }
    
    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener(EventName.LocalScoreChanged, UpdateCurScoreText);
        EventManager.Instance.RemoveListener(EventName.LocalKillAmountChanged, UpdateCurKillText);
    }

    private void Update()
    {
        // 更新计时器
        UpdateCountDownTimerText();

        if (!Application.isMobilePlatform && Input.GetKeyDown(KeyCode.CapsLock))
        {
            isShowCursor = !isShowCursor;
            screenCursor.gameObject.SetActive(isShowCursor);
        }
    }

    private void UpdateCountDownTimerText()
    {
        // 格式: 剩余时间 xx:xx
        float timeLeft = GameManager.Instance.GetPlayingTimeLeft();
        int minLeft = (int)timeLeft / 60;
        int secLeft = (int)timeLeft % 60;
        countDownTimerText.text = $"剩余时间 {minLeft:D2}:{secLeft:D2}";
    }
    
    private void UpdateCurScoreText(object sender, EventArgs e)
    {
        if (e is LocalScoreChangedEventArgs localScoreChangedEventArgs)
        {
            var score = localScoreChangedEventArgs.newScore;
            UpdateCurScoreText(score);
        }
    }
    
    /// <summary>
    /// 分数变化时调用
    /// </summary>
    private void UpdateCurScoreText(int score)
    {
        // 格式: 当前得分: xxx
        curScoreText.text = $"当前得分: {score}";
    }
    
    private void UpdateCurKillText(object sender, EventArgs e)
    {
        if (e is LocalKillAmountChangedEventArgs localKillAmountChangedEventArgs)
        {
            var killAmount = localKillAmountChangedEventArgs.newKillAmount;
            UpdateCurKillText(killAmount);
        }
    }
    
    /// <summary>
    /// 玩家击杀数变化时调用
    /// </summary>
    private void UpdateCurKillText(int killAmount)
    {
        // 格式: 当前击杀: xxx
        curKillText.text = $"当前击杀: {killAmount}";
    }
}
