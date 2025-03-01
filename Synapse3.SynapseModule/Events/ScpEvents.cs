﻿using Neuron.Core.Events;
using Neuron.Core.Meta;
using PlayerRoles.PlayableScps.Scp106;
using PlayerRoles.PlayableScps.Scp3114;
using PlayerRoles.PlayableScps.Scp939;
using PlayerStatsSystem;
using PluginAPI.Events;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Map.Elevators;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Rooms;
using Synapse3.SynapseModule.Player;
using UnityEngine;
using EventManager = Neuron.Core.Events.EventManager;

namespace Synapse3.SynapseModule.Events;

public partial class ScpEvents : Service
{
    private readonly EventManager _eventManager;
    private readonly Synapse _synapse;

    public readonly EventReactor<Scp0492AttackEvent> Scp0492Attack = new();
    public readonly EventReactor<Scp049AttackEvent> Scp049Attack = new();
    public readonly EventReactor<Scp049ReviveEvent> Scp049Revive = new();

    public readonly EventReactor<Scp079ContainEvent> Scp079Contain = new();
    public readonly EventReactor<Scp079SwitchCameraEvent> Scp079SwitchCamera = new();
    public readonly EventReactor<Scp079DoorInteractEvent> Scp079DoorInteract = new();
    public readonly EventReactor<Scp079LockDoorEvent> Scp079LockDoor = new();
    public readonly EventReactor<Scp079SpeakerUseEvent> Scp079SpeakerUse = new();
    public readonly EventReactor<Scp079TeslaInteractEvent> Scp079TeslaInteract = new();
    public readonly EventReactor<Scp079BlackOutRoomEvent> Scp079BlackOutRoom = new();
    public readonly EventReactor<Scp079BlackOutZoneEvent> Scp079BlackOutZone = new();
    public readonly EventReactor<Scp079ReleaseAllLocksEvent> Scp079ReleaseAllLocks = new();
    public readonly EventReactor<Scp079LockdownRoomEvent> Scp079LockdownRoom = new();
    public readonly EventReactor<Scp079ElevatorInteractEvent> Scp079ElevatorInteract = new();
    public readonly EventReactor<Scp079PingEvent> Scp079Ping = new();

    public readonly EventReactor<Scp096AttackEvent> Scp096Attack = new();
    public readonly EventReactor<Scp096AddTargetEvent> Scp096AddTarget = new();

    public readonly EventReactor<Scp106AttackEvent> Scp106Attack = new();
    public readonly EventReactor<Scp106LeavePocketEvent> Scp106LeavePocket = new();

    public readonly EventReactor<Scp173AttackEvent> Scp173Attack = new();
    public readonly EventReactor<Scp173ObserveEvent> Scp173Observe = new();
    public readonly EventReactor<Scp173PlaceTantrumEvent> Scp173PlaceTantrum = new();
    public readonly EventReactor<Scp173ActivateBreakneckSpeedEvent> Scp173ActivateBreakneckSpeed = new();

    public readonly EventReactor<Scp939AttackEvent> Scp939Attack = new();

    public readonly EventReactor<Scp3114DisguiseEvent> Scp3114Disguise = new();
    public readonly EventReactor<Scp3114RevealEvent> Scp3114Reveal = new();
    public readonly EventReactor<Scp3114AttackEvent> Scp3114Attack = new();
    public readonly EventReactor<Scp3114StrangleEvent> Scp3114Strangle = new();

    public ScpEvents(EventManager eventManager, Synapse synapse)
    {
        _eventManager = eventManager;
        _synapse = synapse;
    }

    public override void Enable()
    {
        _eventManager.RegisterEvent(Scp0492Attack);
        _eventManager.RegisterEvent(Scp049Attack);
        _eventManager.RegisterEvent(Scp049Revive);

        _eventManager.RegisterEvent(Scp079Contain);
        _eventManager.RegisterEvent(Scp079SwitchCamera);
        _eventManager.RegisterEvent(Scp079DoorInteract);
        _eventManager.RegisterEvent(Scp079LockDoor);
        _eventManager.RegisterEvent(Scp079SpeakerUse);
        _eventManager.RegisterEvent(Scp079TeslaInteract); 
        _eventManager.RegisterEvent(Scp079BlackOutRoom);
        _eventManager.RegisterEvent(Scp079BlackOutZone);
        _eventManager.RegisterEvent(Scp079ReleaseAllLocks);
        _eventManager.RegisterEvent(Scp079LockdownRoom);
        _eventManager.RegisterEvent(Scp079ElevatorInteract);
        _eventManager.RegisterEvent(Scp079Ping);

        _eventManager.RegisterEvent(Scp096Attack);
        _eventManager.RegisterEvent(Scp096AddTarget);

        _eventManager.RegisterEvent(Scp106Attack);
        _eventManager.RegisterEvent(Scp106LeavePocket);

        _eventManager.RegisterEvent(Scp173Attack);
        _eventManager.RegisterEvent(Scp173Observe);
        _eventManager.RegisterEvent(Scp173PlaceTantrum);
        _eventManager.RegisterEvent(Scp173ActivateBreakneckSpeed);

        _eventManager.RegisterEvent(Scp939Attack);

        _eventManager.RegisterEvent(Scp3114Disguise);
        _eventManager.RegisterEvent(Scp3114Reveal);
        _eventManager.RegisterEvent(Scp3114Attack);
        _eventManager.RegisterEvent(Scp3114Strangle);
        
        PluginAPI.Events.EventManager.RegisterEvents(_synapse,this);
    }

    public override void Disable()
    {
        _eventManager.UnregisterEvent(Scp0492Attack);
        _eventManager.UnregisterEvent(Scp049Attack);
        _eventManager.UnregisterEvent(Scp049Revive);

        _eventManager.UnregisterEvent(Scp079Contain);
        _eventManager.UnregisterEvent(Scp079SwitchCamera);
        _eventManager.UnregisterEvent(Scp079DoorInteract);
        _eventManager.UnregisterEvent(Scp079LockDoor);
        _eventManager.UnregisterEvent(Scp079SpeakerUse);
        _eventManager.UnregisterEvent(Scp079TeslaInteract);
        _eventManager.UnregisterEvent(Scp079BlackOutRoom);
        _eventManager.UnregisterEvent(Scp079BlackOutZone);
        _eventManager.UnregisterEvent(Scp079ReleaseAllLocks);
        _eventManager.UnregisterEvent(Scp079LockdownRoom);
        _eventManager.UnregisterEvent(Scp079ElevatorInteract);
        _eventManager.UnregisterEvent(Scp079Ping);

        _eventManager.UnregisterEvent(Scp096Attack);
        _eventManager.UnregisterEvent(Scp096AddTarget);

        _eventManager.UnregisterEvent(Scp106Attack);
        _eventManager.UnregisterEvent(Scp106LeavePocket);

        _eventManager.UnregisterEvent(Scp173Attack);
        _eventManager.UnregisterEvent(Scp173Observe);
        _eventManager.UnregisterEvent(Scp173PlaceTantrum);
        _eventManager.UnregisterEvent(Scp173ActivateBreakneckSpeed);
        
        _eventManager.UnregisterEvent(Scp939Attack);

        _eventManager.UnregisterEvent(Scp3114Disguise);
        _eventManager.UnregisterEvent(Scp3114Reveal);
        _eventManager.UnregisterEvent(Scp3114Attack);
        _eventManager.UnregisterEvent(Scp3114Strangle);

        PluginAPI.Events.EventManager.UnregisterEvents(_synapse, this);
    }
}

public abstract class ScpActionEvent : IEvent
{
    public ScpActionEvent(SynapsePlayer scp, bool allow)
    {
        Scp = scp;
        Allow = allow;
    }

    public SynapsePlayer Scp { get; }

    public bool Allow { get; set; }
}

public abstract class ScpAttackEvent : ScpActionEvent
{
    protected ScpAttackEvent(SynapsePlayer scp, SynapsePlayer victim, float damage) : base(scp,
        Synapse3Extensions.GetHarmPermission(scp, victim))
    {
        Victim = victim;
        Damage = damage;
    }
    
    public abstract ScpAttackType ScpAttackType { get; }
    
    public SynapsePlayer Victim { get; }
    
    public float Damage { get; set; }
    
}

public class Scp049AttackEvent : ScpAttackEvent
{
    public Scp049AttackEvent(SynapsePlayer scp, SynapsePlayer victim, float damage, 
        float cooldown, bool enableCardiac) : base(
        scp, victim, damage)
    {
        Cooldown = cooldown;
        EnableCardiacEffect = enableCardiac;
    }

    public override ScpAttackType ScpAttackType => ScpAttackType.Scp049Touch;
    
    public bool EnableCardiacEffect { get; set; }

    public float Cooldown { get; set; }
}

public class Scp0492AttackEvent : ScpAttackEvent
{
    public Scp0492AttackEvent(SynapsePlayer scp, SynapsePlayer victim, float damage) : base(scp, victim, damage) { }

    public override ScpAttackType ScpAttackType => ScpAttackType.Scp0492Scratch;
}

public class Scp096AttackEvent : ScpAttackEvent
{
    public Scp096AttackEvent(SynapsePlayer scp, SynapsePlayer victim, Scp096DamageHandler.AttackType attackType,
        float damage) : base(scp, victim, damage)
    {
        ScpAttackType = attackType switch
        {
            Scp096DamageHandler.AttackType.SlapLeft => ScpAttackType.Scp096Slap,
            Scp096DamageHandler.AttackType.SlapRight => ScpAttackType.Scp096Slap,
            Scp096DamageHandler.AttackType.Charge => ScpAttackType.Scp096Charge,
            Scp096DamageHandler.AttackType.GateKill => ScpAttackType.Scp096TearGate,
            _ => ScpAttackType.Scp096Slap,
        };
    }

    public override ScpAttackType ScpAttackType { get; }
}

public class Scp173AttackEvent : ScpAttackEvent
{
    public Scp173AttackEvent(SynapsePlayer scp, SynapsePlayer victim, float damage, bool tp) :
        base(scp, victim, damage)
    {
        ScpAttackType = tp ? ScpAttackType.Scp173Tp : ScpAttackType.Scp173Snap;
    }

    public override ScpAttackType ScpAttackType { get; }
}

public class Scp939AttackEvent : ScpAttackEvent
{
    public Scp939AttackEvent(SynapsePlayer scp, SynapsePlayer victim, float damage, Scp939DamageType type) : base(scp, victim, damage) 
    {
        switch (type)
        {
            case Scp939DamageType.Claw:
                ScpAttackType = ScpAttackType.Scp939Claw;
                    break;
            case Scp939DamageType.LungeTarget:
                ScpAttackType = ScpAttackType.Scp939Lunge;
                    break;
            case Scp939DamageType.LungeSecondary:
                ScpAttackType = ScpAttackType.Scp939LungeSecondary;
                break;
            case Scp939DamageType.None:
            default:
                ScpAttackType = ScpAttackType.Scp939None;
                break;
        }
    }
    
    public override ScpAttackType ScpAttackType { get; }
}

public class Scp106AttackEvent : ScpAttackEvent
{
    public Scp106AttackEvent(SynapsePlayer scp, SynapsePlayer victim, ScpAttackType scpAttackType) : base(scp, victim, 0) 
    {
        ScpAttackType = scpAttackType;
        switch (ScpAttackType)
        {
            case ScpAttackType.Scp106OldGrab:
                TakeToPocket = true;
                Cooldown = scp.MainScpController.Scp106.Attack._hitCooldown;
                VigoreReward = Scp106Attack.VigorCaptureReward * 100;
                SinkholeCooldown = -Scp106Attack.CooldownReductionReward;
                if (Allow)
                    Allow = PluginAPI.Events.EventManager.ExecuteEvent(new Scp106TeleportPlayerEvent(scp, victim));
                break;
            case ScpAttackType.Scp106Termination:
                Damage = Scp106Attack.InstaKillAmount;
                TakeToPocket = false;
                Cooldown = scp.MainScpController.Scp106.Attack._hitCooldown;
                VigoreReward = Scp106Attack.VigorCaptureReward * 100;
                SinkholeCooldown = -Scp106Attack.CooldownReductionReward;
                break;
            case ScpAttackType.Scp106NewGrab:
                Damage = Scp106Attack.AttackDamage;
                TakeToPocket = true;
                Cooldown = scp.MainScpController.Scp106.Attack._hitCooldown;
                VigoreReward = Scp106Attack.VigorCaptureReward * 100;
                SinkholeCooldown = -Scp106Attack.CooldownReductionReward;
                if (Allow)
                    Allow = PluginAPI.Events.EventManager.ExecuteEvent(new Scp106TeleportPlayerEvent(scp, victim)); break;
            case ScpAttackType.Scp106Corroding:
                Damage = Scp106Attack.AttackDamage;
                TakeToPocket = false;
                Cooldown = scp.MainScpController.Scp106.Attack._hitCooldown;
                VigoreReward = 0;
                SinkholeCooldown = -Scp106Attack.CooldownReductionReward;
                break;
            default:
                break;
        }

    }

    public override ScpAttackType ScpAttackType { get; }
    
    public bool TakeToPocket { get; set; }
    
    public float Cooldown { get; set; }

    public float SinkholeCooldown { get; set; }

    public float VigoreReward { get; set; }
}

public class Scp106MissAttackEvent : ScpActionEvent
{
    public Scp106MissAttackEvent(SynapsePlayer scp, float cooldown) : base(scp, true) 
    {
        Cooldown = cooldown;
        IgnoreMiss = false;
    }

    public float Cooldown { get; set; } 

    public bool IgnoreMiss { get; set; }
}

public class Scp173ObserveEvent : PlayerInteractEvent
{
    public Scp173ObserveEvent(SynapsePlayer player, bool allow, SynapsePlayer scp) : base(player, allow)
    {
        Scp = scp;
    }

    public SynapsePlayer Scp { get; }
}

public class Scp173PlaceTantrumEvent : ScpActionEvent
{
    public Scp173PlaceTantrumEvent(SynapsePlayer scp, float coolDown) : base (scp , true) 
    {
        CoolDown = coolDown;
    }

    public float CoolDown { get; set; }
}

public class Scp173ActivateBreakneckSpeedEvent : ScpActionEvent
{
    public Scp173ActivateBreakneckSpeedEvent(SynapsePlayer scp, bool activate) : base(scp, true)
    {
        Activate = activate;
    }

    public bool Activate { get; }
}

public class Scp049ReviveEvent : ScpActionEvent
{
    public Scp049ReviveEvent(SynapsePlayer scp, SynapsePlayer humanToRevive, SynapseRagDoll ragDoll, bool finishRevive) : base(scp, true)
    {
        HumanToRevive = humanToRevive;
        RagDoll = ragDoll;
        FinishRevive = finishRevive;
    }
    
    public SynapsePlayer HumanToRevive { get; }
    
    public SynapseRagDoll RagDoll { get; }
    
    public bool FinishRevive { get; }

}

public class Scp106LeavePocketEvent : PlayerEvent
{
    public Scp106LeavePocketEvent(SynapsePlayer player, bool escapePocket, 
        Vector3 enteredExitPosition, Vector3 exitPosition) : base(player)
    {
        EscapePocket = escapePocket;
        EnteredExitPosition = enteredExitPosition;
        EscapePosition = exitPosition;
    }
    
    public bool EscapePocket { get; set; }
    
    public Vector3 EnteredExitPosition { get; }

    public Vector3 EscapePosition { get; set; }

    public bool Allow { get; set; } = true;
}

public class Scp096AddTargetEvent : PlayerInteractEvent
{
    public Scp096AddTargetEvent(SynapsePlayer player, bool allow, SynapsePlayer scp, bool isForLooking) : base(player, allow)
    {
        Scp = scp;
        IsForLooking = isForLooking;
    }
    
    public SynapsePlayer Scp { get; }
    
    public bool IsForLooking { get; }
}

public class Scp079ContainEvent : IEvent
{
    public Scp079ContainEvent(Scp079ContainmentStatus status)
    {
        Status = status;
    }

    public Scp079ContainmentStatus Status { get; }

    public bool Allow { get; set; } = true;
}

public class Scp079InteractEvent : ScpActionEvent
{
    public int Cost { get; set; }

    public Scp079InteractEvent(SynapsePlayer scp, bool allow, int cost) : base(scp, allow)
    {
        Cost = cost;
    }
}

public class Scp079SwitchCameraEvent : Scp079InteractEvent
{
    public Scp079SwitchCameraEvent(SynapsePlayer scp, SynapseCamera camera, int cost, bool allow)
        : base(scp, allow, cost)
    {
        Camera = camera;
    }

    public SynapseCamera Camera { get; set; }
}

public class Scp079DoorInteractEvent : Scp079InteractEvent
{
    public Scp079DoorInteractEvent(SynapsePlayer scp, SynapseDoor door, int cost) : base(scp, true, cost)
    {
        Door = door;
    }

    public SynapseDoor Door { get; }

    public bool Opening => !Door.Open;
}

public class Scp079LockDoorEvent : Scp079InteractEvent
{
    public Scp079LockDoorEvent(SynapsePlayer scp, SynapseDoor door, bool unlock, int cost) : base(scp, true, cost)
    {
        Door = door;
        Unlock = unlock;
    }

    public SynapseDoor Door { get; }

    public bool Unlock { get; }
}

public class Scp079SpeakerUseEvent : Scp079InteractEvent
{
    public Scp079SpeakerUseEvent(SynapsePlayer scp, Vector3 speakerPosition) : base(scp, true, 0)
    {
        SpeakerPosition = speakerPosition;
    }

    public Vector3 SpeakerPosition { get; }
}

public class Scp079TeslaInteractEvent : Scp079InteractEvent
{
    public Scp079TeslaInteractEvent(SynapsePlayer scp, SynapseTesla tesla, int cost) : base(scp, true, cost)
    {
        Tesla = tesla;
    }

    public SynapseTesla Tesla { get; }
}

public class Scp079BlackOutRoomEvent : Scp079InteractEvent
{
    public Scp079BlackOutRoomEvent(SynapsePlayer scp, IVanillaRoom room, int cost) : base(scp, true, cost)
    {
        Room = room;
    }

    public IVanillaRoom Room { get; }
}

public class Scp079BlackOutZoneEvent : Scp079InteractEvent
{
    public Scp079BlackOutZoneEvent(SynapsePlayer scp, ZoneType zone, int cost) : base(scp, true, cost)
    {
        Zone = zone;
    }

    public ZoneType Zone { get; }
}

public class Scp079ReleaseAllLocksEvent : Scp079InteractEvent
{
    public Scp079ReleaseAllLocksEvent(SynapsePlayer scp, int cost) : base(scp, true, cost) { }
}

public class Scp079LockdownRoomEvent : Scp079InteractEvent
{
    public Scp079LockdownRoomEvent(SynapsePlayer scp, int cost, IVanillaRoom room) : base(scp, true, cost)
    {
        Room = room;
    }

    public IVanillaRoom Room { get; set; }
}

public class Scp079ElevatorInteractEvent : Scp079InteractEvent
{
    public Scp079ElevatorInteractEvent(SynapsePlayer scp, int cost, IElevator elevator, int destioantion) : base(scp,
        true, cost)
    {
        Elevator = elevator;
        Destination = destioantion;
    }

    public IElevator Elevator { get; set; }

    public int Destination { get; set; }
}

public class Scp079PingEvent : Scp079InteractEvent
{
    public Scp079PingEvent(SynapsePlayer scp, bool allow, int cost, Scp079PingType pingType, Vector3 position, Vector3 normal) : base(scp, allow, cost)
    {
        PingType = pingType;
        Position = position;
        Normal = normal;
    }
    
    public Scp079PingType PingType { get; set; }
    
    public Vector3 Position { get; set; }
    
    public Vector3 Normal { get; set; }
}

public class Scp3114DisguiseEvent : ScpActionEvent
{
    public Scp3114DisguiseEvent(SynapsePlayer scp, bool starting, bool allow, SynapseRagDoll ragdoll) : base(scp, allow)
    {
        Ragdoll = ragdoll;
        Starting = starting;
    }

    public bool Starting { get; }

    public SynapseRagDoll Ragdoll { get; set; }
}

public class Scp3114RevealEvent : ScpActionEvent
{
    public Scp3114RevealEvent(SynapsePlayer scp, bool allow) : base(scp, allow) { }
}

public class Scp3114AttackEvent : ScpAttackEvent
{
    public override ScpAttackType ScpAttackType => ScpAttackType.Scp3114Slap;

    public Scp3114AttackEvent(SynapsePlayer scp, SynapsePlayer victim, float damage) : base(scp, victim, damage) { }
}

public class Scp3114StrangleEvent : ScpAttackEvent
{
    public override ScpAttackType ScpAttackType => ScpAttackType.Scp3114Strangle;
   
    public Scp3114StrangleEvent(SynapsePlayer scp, SynapsePlayer victim, bool realase) : base(scp, victim, 0)
    {
        Realase = realase;
    }

    public new float Damage => 0;

    public bool Realase { get; }
}