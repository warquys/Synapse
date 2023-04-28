﻿using Neuron.Core.Events;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using MEC;
using PlayerRoles;
using Synapse3.SynapseModule.Dummy;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Map.Elevators;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Schematic;
using UnityEngine;


namespace Synapse3.SynapseModule;

#if DEBUG
public class DebugService : Service
{
    private PlayerEvents _player;
    private MapEvents _map;
    private RoundEvents _round;
    private ItemEvents _item;
    private ScpEvents _scp;
    private ServerEvents _server;
    private EventManager _event;

    public DebugService(PlayerEvents player, MapEvents map, RoundEvents round, ItemEvents item, ScpEvents scp,
        ServerEvents server, EventManager eventManager)
    {
        _player = player;
        _map = map;
        _round = round;
        _item = item;
        _server = server;
        _scp = scp;
        _event = eventManager;
    }

    public override void Enable()
    {
        Synapse.Get<SchematicService>().RegisterSchematic(new SchematicConfiguration()
        {
            Name = "ElevatorChamber",
            Id = 50,
            Primitives = new List<SchematicConfiguration.PrimitiveConfiguration>()
            {
                new SchematicConfiguration.PrimitiveConfiguration()
                {
                    Position = Vector3.down * 0.4f,
                    PrimitiveType = PrimitiveType.Plane,
                    Color = Color.white,
                    Scale = Vector3.one * 0.3f
                }
            }
        });
        Synapse.Get<SchematicService>().RegisterSchematic(new SchematicConfiguration()
        {
            Name = "ElevatorDestination",
            Id = 51,
            Doors = new List<SchematicConfiguration.DoorConfiguration>()
            {
                new SchematicConfiguration.DoorConfiguration()
                {
                    DoorType = SynapseDoor.SpawnableDoorType.Ez,
                    Position = Vector3.forward
                }
            }
        });
        var method = ((Action<IEvent>)Event).Method;
        foreach (var reactor in _event.Reactors)
        {
            if (reactor.Key == typeof(UpdateObjectEvent)) continue;
            if (reactor.Key == typeof(UpdateEvent)) continue;
            if (reactor.Key == typeof(EscapeEvent)) continue;
            if (reactor.Key == typeof(Scp173ObserveEvent)) continue;
            if (reactor.Key == typeof(KeyPressEvent)) continue;
            if (reactor.Key == typeof(SpeakEvent)) continue;
            if (reactor.Key == typeof(SpeakToPlayerEvent)) continue;
            if (reactor.Key == typeof(RoundCheckEndEvent)) continue;
            if (reactor.Key == typeof(SendPlayerDataEvent)) continue;
            if (reactor.Key.IsAbstract) continue;
            reactor.Value.SubscribeUnsafe(this, method);
        }
        _player.KeyPress.Subscribe(OnKeyPress);
        _item.ConsumeItem.Subscribe(ev =>
        {
            if (ev.State == ItemInteractState.Finalize)
                ev.Allow = false;
        });
        _player.Escape.Subscribe(ev =>
        {
            if(ev.EscapeType == EscapeType.NotAssigned)
                Logger.Warn("Escape not assigned");
        });
        _item.ConsumeItem.Subscribe(ev =>
        {
            if (ev.State == ItemInteractState.Finalize)
                ev.Allow = false;
        });
    }
    
    public void Event(IEvent e)
    {
        switch (e)
        {
            default:
                Logger.Warn("Event triggered: " + e.GetType().Name);
                break;
        }
    }

    private SynapseDummy _dummy;
    private void OnKeyPress(KeyPressEvent ev)
    {
        switch (ev.KeyCode)
        {
            case KeyCode.Alpha1:
                Synapse.Get<ElevatorService>().Elevators.FirstOrDefault(x => x.ElevatorId == 99).MoveToNext();
                break;
           
            case KeyCode.Alpha2:
                (Synapse.Get<ElevatorService>().Elevators.FirstOrDefault(x => x.ElevatorId == 99).Chamber as
                    CustomElevatorChamber).Schematic.Position = ev.Player.Position;
                break;
            case KeyCode.Alpha3:
                new SynapseDoor(SynapseDoor.SpawnableDoorType.Hcz, ev.Player.Position, ev.Player.Rotation, Vector3.one)
                {
                    MoveInElevator = true
                };
                break;
            
            case KeyCode.Alpha4:
                var schem = new SynapseSchematic(new SchematicConfiguration()
                {
                    Doors = new List<SchematicConfiguration.DoorConfiguration>()
                    {
                        new SchematicConfiguration.DoorConfiguration()
                        {
                            Position = Vector3.up,
                            DoorType = SynapseDoor.SpawnableDoorType.Ez
                        }
                    },
                    Primitives = new List<SchematicConfiguration.PrimitiveConfiguration>()
                    {
                        new SchematicConfiguration.PrimitiveConfiguration()
                        {
                            Position = Vector3.right,
                            Color = Color.white,
                            PrimitiveType = PrimitiveType.Sphere
                        }
                    }
                })
                {
                    MoveInElevator = true
                };
                schem.Position = ev.Player.Position;
                break;
        }
    }
}
#endif
