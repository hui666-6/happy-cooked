using System;
using Unity.Netcode;

public struct PlayerData : IEquatable<PlayerData>,INetworkSerializable
{
    public ulong clientID;
    public int colorId;
    public bool Equals(PlayerData other)
    {
        return clientID == other.clientID && colorId == other.colorId;
    }

    //对playerindex 序列化和反序列化
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientID);
        serializer.SerializeValue(ref colorId);
    }
}
