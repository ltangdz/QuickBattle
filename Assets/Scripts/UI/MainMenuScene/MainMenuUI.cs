using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : BaseUI
{
    public Button startButton;
    public Button exitButton;
    public TextMeshProUGUI highScoreText;

    private void Start()
    {
        // startButton.onClick.AddListener(() => Loader.LoadScene(Settings.LOBBY_SCENE));
        // 测试用
        startButton.onClick.AddListener(() => Loader.LoadScene(Settings.GAME_SCENE));

        exitButton.onClick.AddListener(Application.Quit);

        highScoreText.text = $"历史最高分: {PlayerPrefs.GetInt(Settings.PLAYER_PREF_HIGH_SCORE)}";
    }
}
