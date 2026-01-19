using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// GameManager.cs
/// 1.控制游戏状态
/// 2.保存本地玩家信息LocalPlayer, 便于其他类调用
/// </summary>
public enum GameState
{
    PlayingState,
    GameOverState,
}
public class GameManager : SingletonNetwork<GameManager>
{
    // private NetworkList<PlayerData> playerDataNetworkList;
    private GameState currentState;

    [Space(10)]
    [Header("游戏流程相关")]
    // [Tooltip("剩余游戏时间")]
    // [SerializeField] private float playingTimer = 0f;
    [Tooltip("最大游戏时间")]
    [SerializeField] private float playingTimerMax = 300f;
    
    private NetworkVariable<float> playingTimer = new NetworkVariable<float>(0f);

    // 游戏结束事件 (计时器结束触发 UI监听)
    public EventHandler OnGameOverEvent;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        
        Debug.Log("初始化游戏状态");
        InitPlayingGameState();
    }

    private bool isStart = false;
    public bool IsStart => isStart;

    private void InitPlayingGameState()
    {
        // 切换游戏状态
        ChangeGameState(GameState.PlayingState);
        
        // 初始化计时器
        playingTimer.Value = playingTimerMax;
        
        // 初始化游戏资源
        ResourceManager.Instance.Init();
        
        isStart = true;
    }

    private void Update()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        
        UpdateGameState();
    }

    private void UpdateGameState()
    {
        if (!isStart) return;
        
        switch (currentState)
        {
            case GameState.PlayingState:
                if (playingTimer.Value >= 0f)
                {
                    playingTimer.Value -= Time.deltaTime;
                }
                else
                {
                    // 处理游戏结束逻辑
                    playingTimer.Value = 0f;
                    ChangeGameState(GameState.GameOverState);
                    GameInput.Instance.DisablePlayerInput();
                    OnGameOverEvent?.Invoke(this, EventArgs.Empty);
                    Debug.Log("游戏结束！！！");
                }
                break;
            
            case GameState.GameOverState:
                break;
        }
    }

    /// <summary>
    /// 更改当前游戏状态
    /// </summary>
    /// <param name="gameState">新状态</param>
    private void ChangeGameState(GameState gameState)
    {
        currentState = gameState;
    }

    public int GetLocalScore()
    {
        return Player.LocalInstance.GetScore();
    }

    public float GetPlayingTimeLeft()
    {
        return playingTimer.Value;
    }
}
