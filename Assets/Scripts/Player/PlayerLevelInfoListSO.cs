using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerLevelInfoListSO", menuName = "ScriptableObject/Player/PlayerLevelInfoListSO")]
public class PlayerLevelInfoListSO : ScriptableObject
{
    public List<PlayerLevelInfoSO> playerLevelInfoList;
}