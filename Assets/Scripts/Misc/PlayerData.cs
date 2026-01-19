using System;
using Unity.Collections;
using Unity.Netcode;

public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    public ulong clientId;
    public int colorId;
    public int level;
    public int score;
    public FixedString64Bytes playerName;
    
    public bool Equals(PlayerData other)
    {
        return clientId == other.clientId && colorId == other.colorId && level == other.level && score == other.score &&
               playerName == other.playerName;
    }


    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref colorId);
        serializer.SerializeValue(ref level);
        serializer.SerializeValue(ref score);
        serializer.SerializeValue(ref playerName);
    }
}