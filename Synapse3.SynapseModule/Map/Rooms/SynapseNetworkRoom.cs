﻿using System.Collections.Generic;
using System.Linq;
using MapGeneration;
using Mirror;
using Synapse3.SynapseModule.Map.Schematic;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Rooms;

public class SynapseNetworkRoom : NetworkSynapseObject, IRoom
{
    internal SynapseNetworkRoom(RoomIdentifier identifier, RoomType type)
    {
        Identifier = identifier;
        RoomType = type;
        NetworkIdentity = GetNetworkIdentity(type);
        LightController = Identifier.GetComponentInChildren<FlickerableLightController>();
        
        var comp = identifier.gameObject.AddComponent<SynapseObjectScript>();
        comp.Object = this;
    }

    public RoomIdentifier Identifier { get; }
    public FlickerableLightController LightController { get; }
    public override NetworkIdentity NetworkIdentity { get; }
    public override GameObject GameObject => Identifier.gameObject;
    
    public override ObjectType Type => ObjectType.Room;
    public string Name => RoomType.ToString();
    public int ID => (int)RoomType;
    public RoomType RoomType { get; }
    public ZoneType ZoneType => (ZoneType)Zone;
    public int Zone
    {
        get
        {
            switch (Position.y)
            {
                case 0f:
                    return (int)ZoneType.LCZ;

                case 1000f:
                    return (int)ZoneType.Surface;

                case -1000f:
                    if (Name.Contains("HCZ"))
                        return (int)ZoneType.HCZ;

                    return (int)ZoneType.Entrance;

                case -2000f:
                    return (int)ZoneType.Pocket;

                default:
                    return (int)ZoneType.None;
            }
        }
    }

    public void TurnOffLights(float duration)
    {
        LightController.ServerFlickerLights(duration);
    }

    public override void OnDestroy()
    {
        Synapse.Get<RoomService>()._rooms.Remove(this);
        base.OnDestroy();
    }

    private static List<NetworkIdentity> _networkIdentities;
    private static NetworkIdentity GetNetworkIdentity(RoomType room)
    {
        if (_networkIdentities == null || _networkIdentities.All(x => x == null))
            _networkIdentities = Synapse.GetObjectsOf<NetworkIdentity>().Where(x => x.name.Contains("All"))
                .ToList();
        switch (room)
        {
            case RoomType.Scp330:
                return _networkIdentities.FirstOrDefault(x => x?.assetId == new System.Guid("17f38aa5-1bc8-8bc4-0ad1-fffcbe4214ae"));

            case RoomType.Scp939:
                return _networkIdentities.FirstOrDefault(x => x?.assetId == new System.Guid("d1566564-d477-24c4-c953-c619898e4751"));

            case RoomType.Scp106:
                return _networkIdentities.FirstOrDefault(x => x?.assetId == new System.Guid("c1ae9ee4-cc8e-0794-3b2c-358aa6e57565"));

            default: return null;
        }
    }
}