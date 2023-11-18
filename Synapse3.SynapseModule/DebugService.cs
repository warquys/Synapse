using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Mirror;
using Neuron.Core;
using Neuron.Core.Events;
using Neuron.Core.Meta;
using Neuron.Core.Plugins;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Roles;
using Scp914;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Dummy;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Rooms;
using Synapse3.SynapseModule.Map.Schematic;
using Synapse3.SynapseModule.Map.Scp914;
using Synapse3.SynapseModule.Player;
using Synapse3.SynapseModule.Role;
using UnityEngine;
using YamlDotNet.Core.Tokens;

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
    private RoomService _roomSerivce;

    public DebugService(PlayerEvents player, MapEvents map, RoundEvents round, ItemEvents item, ScpEvents scp,
        ServerEvents server, EventManager eventManager, RoomService room)
    {
        _player = player;
        _map = map;
        _round = round;
        _item = item;
        _server = server;
        _scp = scp;
        _event = eventManager;
        _roomSerivce = room;
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
            if (reactor.Key == typeof(PluginLoadEvent)) continue;
            if (reactor.Key == typeof(UpdateObjectEvent)) continue;
            if (reactor.Key == typeof(UpdateEvent)) continue;
            if (reactor.Key == typeof(EscapeEvent)) continue;
            if (reactor.Key == typeof(Scp173ObserveEvent)) continue;
            if (reactor.Key == typeof(KeyPressEvent)) continue;
            if (reactor.Key == typeof(SpeakEvent)) continue;
            if (reactor.Key == typeof(SpeakToPlayerEvent)) continue;
            if (reactor.Key == typeof(RoundCheckEndEvent)) continue;
            if (reactor.Key == typeof(SendPlayerDataEvent)) continue;
            if (reactor.Key == typeof(HealEvent)) continue;
            if (reactor.Key.IsAbstract) continue;
            reactor.Value.SubscribeUnsafe(this, method);
        }
        _player.KeyPress.Subscribe(OnKeyPress, 100);
        _round.Start.Subscribe(OnStart, 100);
        
        _item.ConsumeItem.Subscribe(ev =>
        {
            if (ev.State == ItemInteractState.Finalize)
                ev.Allow = false;
        }, 100);
        _player.Escape.Subscribe(ev =>
        {
            if(ev.EscapeType == EscapeType.NotAssigned)
                Logger.Warn("Escape not assigned");
        }, 100);

        _round.Waiting.Subscribe(ev =>
        {
            ((SynapseNetworkRoom)_roomSerivce.GetRoom((uint)RoomType.TestingRoom)).Position +=
                Vector3.up * 5;
            ((SynapseNetworkRoom)_roomSerivce.GetRoom((uint)RoomType.Scp330)).Position +=
                Vector3.up * 5;
            Logger.Warn("Room TestingRoom & Scp330 moved");

            var text = "";
            foreach (var prefab in NetworkClient.prefabs)
            {
                text += "\n" + prefab.Value.name + " ID: " + prefab.Key;
            }
            var prefabsFile = Path.Combine(Synapse.Get<NeuronBase>().RelativePath(), "prefabs.txt");
            File.WriteAllText(prefabsFile, text);
            Logger.Warn($"prefabs listed in {prefabsFile}");
        }, 100);

        _scp.Scp3114Attack.Subscribe(ev => ev.Allow = false);
        _scp.Scp3114Strangle.Subscribe(ev => ev.Allow = false);
    }

    private void OnStart(RoundStartEvent args)
    {
        RegisterProcess();
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
         
            case KeyCode.Alpha2:
                SynapseLogger<DebugService>.Info("Effects: ");
                foreach (var effect in ev.Player.PlayerEffectsController.AllEffects)
                {
                    SynapseLogger<DebugService>.Info(effect.name);
                }
                break;

        }
    }

    class CustomRoleTest : SynapseAbstractRole
    {
        class config : IAbstractRoleConfig
        {
            public RoleTypeId Role => RoleTypeId.NtfCaptain;

            public RoleTypeId VisibleRole => RoleTypeId.None;//Unsyc

            public RoleTypeId OwnRole => RoleTypeId.None;

            public uint EscapeRole => (uint)RoleTypeId.ClassD;

            public float Health => 100;

            public float MaxHealth => 120;

            public float ArtificialHealth => 0;

            public float MaxArtificialHealth => 200;

            public RoomPoint[] PossibleSpawns => null;

            public SerializedPlayerInventory[] PossibleInventories => null;

            public bool CustomDisplay => true;

            public bool Hierarchy => true;//Hide all custom info

            public bool UseCustomUnitName => true;//Not present

            public string CustomUnitName => "Hi my unite name";

            public SerializedVector3 Scale => Vector3.one;
        }

        protected override bool CanSeeUnit(SynapsePlayer player) => true;

        protected override bool HigherRank(SynapsePlayer player) => true;
        protected override bool LowerRank(SynapsePlayer player) => false;
        protected override bool SameRank(SynapsePlayer player) => false;

        protected override IAbstractRoleConfig GetConfig() => new config();
    }

    private SynapseDummy SpawnDebugRole(SynapsePlayer player)
    {
        var roleService = Synapse.Get<RoleService>();
        if (!roleService.IsIdRegistered(999))
        {
            roleService.RegisterRole(new RoleAttribute()
            {
                Id = 999,
                Name = "Debug Role",
                RoleScript = typeof(CustomRoleTest),
                TeamId = (uint)Team.FoundationForces
            });
        }
        var dummy = new SynapseDummy(player.Position, player.Rotation, RoleTypeId.ClassD, "Hello", "I can destroy your server at any time!");
        dummy.Player.RoleID = 999;
        return dummy;
    }

    private void SpawnDebugShematic(SynapsePlayer player)
    {
        var schematicService = Synapse.Get<SchematicService>();
        var configuration = new SchematicConfiguration()
        {
            Lockers = new (),
            Primitives = new (),
            Doors = new (),
            Generators = new (),
            Lights = new (),
            WorkStations = new (),
            Targets = new()
        };

        var position = Vector3.zero;

        foreach (var lockerType in (SynapseLocker.LockerType[])Enum.GetValues(typeof(SynapseLocker.LockerType)))
        {
            configuration.Lockers.Add(new SchematicConfiguration.LockerConfiguration()
            {
                DeleteDefaultItems = false,
                LockerType = lockerType,
                CustomAttributes = new List<string>(),
                Position = position,
                Rotation = new Config.SerializedVector3(0, 0, 0),
                Scale = Vector3.one,
                Update = false,
                UpdateFrequency = -1
            });
            position += Vector3.forward;
        }

        foreach (var primitiveType in (PrimitiveType[])Enum.GetValues(typeof(PrimitiveType)))
        {
            configuration.Primitives.Add(new SchematicConfiguration.PrimitiveConfiguration()
            {
                Physics = false,
                Color = Color.white,
                PrimitiveType = primitiveType,
                CustomAttributes = new List<string>(),
                Position = position,
                Rotation = new Config.SerializedVector3(0, 0, 0),
                Scale = Vector3.one,
            });
            position += Vector3.forward;
        }

        foreach (var doorType in (SynapseDoor.SpawnableDoorType[])Enum.GetValues(typeof(SynapseDoor.SpawnableDoorType)))
        {
            if (doorType == SynapseDoor.SpawnableDoorType.None) continue;

            configuration.Doors.Add(new SchematicConfiguration.DoorConfiguration()
            {
                DoorType = doorType,
                Health = 100,
                Locked = false,
                Open = false,
                UnDestroyable = false,
                Update = false,
                UpdateFrequency = -1,
                CustomAttributes = new List<string>(),
                Position = position,
                Rotation = new Config.SerializedVector3(0, 0, 0),
                Scale = Vector3.one,
            });
            position += Vector3.forward;
        }

        configuration.Generators.Add(new SchematicConfiguration.SimpleUpdateConfig()
        {
            Update = true,
            UpdateFrequency = 2,
            CustomAttributes = new List<string>(),
            Position = position,
            Rotation = new Config.SerializedVector3(0, 0, 0),
            Scale = Vector3.one,
        });
        position += Vector3.forward;

        configuration.Lights.Add(new SchematicConfiguration.LightSourceConfiguration()
        {
            Color = Color.white,
            LightIntensity = 20,
            LightRange = 100,
            LightShadows = false,
            CustomAttributes = new List<string>(),
            Position = position,
            Rotation = new Config.SerializedVector3(0, 0, 0),
            Scale = Vector3.one,
        });
        position += Vector3.forward;

        configuration.WorkStations.Add(new SchematicConfiguration.SimpleUpdateConfig()
        {
            Update = true,
            UpdateFrequency = -1,
            CustomAttributes = new List<string>(),
            Position = position,
            Rotation = new Config.SerializedVector3(0, 0, 0),
            Scale = Vector3.one,
        });
        position += Vector3.forward;

        foreach (var targetType in (SynapseTarget.TargetType[])Enum.GetValues(typeof(SynapseTarget.TargetType)))
        {
            configuration.Targets.Add(new SchematicConfiguration.TargetConfiguration()
            {
                TargetType = targetType,
                CustomAttributes = new List<string>(),
                Position = position,
                Rotation = new Config.SerializedVector3(0, 0, 0),
                Scale = Vector3.one,
            });
            position += Vector3.forward;
        }

        Schematic = schematicService.SpawnSchematic(configuration, player.Position, player.Rotation);
    }

    private void RegisterProcess()
    {
        var scp914 = Synapse.Get<Scp914Service>();
        scp914.Synapse914Processors[(uint)ItemType.Medkit].Insert(0, new Process1());
        scp914.Synapse914Processors[(uint)ItemType.Medkit].Insert(0, new Process2());
    }

    class Process1 : ISynapse914Processor
    {
        public bool CreateUpgradedItem(SynapseItem item, Scp914KnobSetting setting, Vector3 position = default)
        {
            item.Destroy();
            new SynapseItem(ItemType.ArmorHeavy, position);
            return true;
        }
    }

    class Process2 : ISynapse914Processor
    {
        public bool CreateUpgradedItem(SynapseItem item, Scp914KnobSetting setting, Vector3 position = default)
        {
            if (UnityEngine.Random.Range(1, 5) > 2)
            {
                item.Destroy();
                new SynapseItem(ItemType.Flashlight, position);
                return true;
            }
            return false;
        }
    }
    
    private SynapseSchematic Schematic;

    private SynapseDummy Dummy;

}

[CustomRoom(
    Id = 999,
    SchematicId = 999,
    Name = "TestRoom"
    )]
class TestRoom : SynapseCustomRoom
{
    public override uint Zone => (uint)ZoneType.Surface;

    public override float VisibleDistance => 5;

    public override float UpdateFrequencyVisble => 1;
}
#endif