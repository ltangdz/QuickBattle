using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Settings : MonoBehaviour
{
    #region Tags

    public const string TAG_PLAYER = "Player";
    public const string TAG_GROUND = "Ground";

    #endregion

    #region Player Prefs

    public const string PLAYER_PREF_PLAYER_NAME = "PlayerName";
    public const string PLAYER_PREF_HIGH_SCORE = "HighScore";

    #endregion

    #region Scene Name

    public const string LOADING_SCENE = "LoadingScene";
    public const string MAIN_MENU_SCENE = "MainMenuScene";
    public const string LOBBY_SCENE = "LobbyScene";
    public const string GAME_SCENE = "GameScene";

    #endregion
}
