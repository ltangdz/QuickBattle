using System.ComponentModel.DataAnnotations;
using System.Numerics;
using MessagePack;
using Key = MessagePack.KeyAttribute;

namespace QuicServer;

// 玩家操作指令（客户端→服务器）
[MessagePackObject]
public class InputCommand
{
    [Key(0)] public int PlayerId;      // 玩家ID
    [Key(1)] public Vector3 MoveDir;   // 移动方向
    [Key(2)] public long Timestamp;    // 时间戳（防重放）
}

// 玩家权威状态（服务器→客户端）
[MessagePackObject]
public class PlayerState
{
    [Key(0)] public int PlayerId;
    [Key(1)] public Vector3 Position;  // 位置
    [Key(2)] public Quaternion Rotation; // 旋转
    [Key(3)] public int Health;        // 血量
    [Key(4)] public long Timestamp;    // 服务器时间戳
}