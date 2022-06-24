﻿using MapGeneration;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Rooms;

public class SynapseRoom : IRoom
{
    internal SynapseRoom(RoomIdentifier identifier, RoomType type)
    {
        Identifier = identifier;
        RoomType = type;
        LightController = Identifier.GetComponentInChildren<FlickerableLightController>();
        
        //TODO: Add SCP-079 Cams
    }
    
    public RoomIdentifier Identifier { get; }
    
    public Vector3 Position => GameObject.transform.position;
    public Quaternion Rotation => GameObject.transform.rotation;
    public GameObject GameObject => Identifier.gameObject;
    public FlickerableLightController LightController { get; }

    public string Name => RoomType.ToString();
    public RoomType RoomType { get; }
    public int ID => (int)RoomType;
    
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

    public ZoneType ZoneType => (ZoneType)Zone;
    
    public void TurnOffLights(float duration)
    {
        LightController.ServerFlickerLights(duration);
    }
}