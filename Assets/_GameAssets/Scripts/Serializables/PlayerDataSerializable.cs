using System;
using Unity.Netcode;
using UnityEngine;

public struct PlayerDataSerializable : INetworkSerializeByMemcpy, IEquatable<PlayerDataSerializable>
{
    public ulong ClientId;

    public bool Equals(PlayerDataSerializable other)
    {
        return ClientId == other.ClientId;
    }
}
