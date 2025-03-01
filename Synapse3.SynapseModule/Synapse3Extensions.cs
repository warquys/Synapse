﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Footprinting;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.Pickups;
using MapGeneration;
using MapGeneration.Distributors;
using MEC;
using Mirror;
using Neuron.Core.Events;
using Neuron.Core.Logging;
using Neuron.Modules.Configs.Localization;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079.Cameras;
using PlayerRoles.PlayableScps.Subroutines;
using PlayerRoles.Ragdolls;
using PlayerRoles.Subroutines;
using PlayerStatsSystem;
using PluginAPI.Core.Interfaces;
using Synapse3.SynapseModule;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Map;
using Synapse3.SynapseModule.Map.Elevators;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Rooms;
using Synapse3.SynapseModule.Map.Schematic;
using Synapse3.SynapseModule.Player;
using UnityEngine;

public static class Synapse3Extensions
{
    private static readonly PlayerService _player;
    private static readonly ItemService _item;
    private static readonly MirrorService _mirror;
    private static readonly SynapseConfigService _config;
    private static readonly RoomService _room;
    private static readonly MapService _map;
    private static readonly RoundService _round;
    private static readonly ServerService _server;
    private static readonly ElevatorService _elevator;
    private static readonly PlayerEvents _playerEvents;
    private static readonly ItemEvents _itemEvents;

    private static Dictionary<GameObject, SynapsePlayer> CachedPlayer = new Dictionary<GameObject, SynapsePlayer>();

    static Synapse3Extensions()
    {
        _player = Synapse.Get<PlayerService>();
        _item = Synapse.Get<ItemService>();
        _mirror = Synapse.Get<MirrorService>();
        _config = Synapse.Get<SynapseConfigService>();
        _room = Synapse.Get<RoomService>();
        _map = Synapse.Get<MapService>();
        _round = Synapse.Get<RoundService>();
        _server = Synapse.Get<ServerService>();
        _elevator = Synapse.Get<ElevatorService>();
        _playerEvents = Synapse.Get<PlayerEvents>();
        _itemEvents = Synapse.Get<ItemEvents>();
    }

    // FunFact: This method is the oldest method in Synapse and was originally created even before Synapse for an Exiled 1.0 Plugin
    /// <summary>
    /// Sends a message to the sender in the RemoteAdmin
    /// </summary>
    public static void RaMessage(this CommandSender sender, string message, bool success = true,
        RaCategory type = RaCategory.None, string name = "")
    {
        var category = "";
        if (type != RaCategory.None)
            category = type.ToString();


        sender.RaReply(
            $"{(string.IsNullOrWhiteSpace(name) ? Assembly.GetCallingAssembly().GetName().Name : name)}#" + message,
            success, true, category);
    }


    /// <summary>
    /// Updates Position Rotation and Scale of an NetworkObject for all players
    /// </summary>
    public static void UpdatePositionRotationScale(this NetworkIdentity identity)
        => NetworkServer.SendToAll(_mirror.GetSpawnMessage(identity));


    /// <summary>
    /// Hides an NetworkObject for a single players
    /// </summary>
    public static void UnSpawnForOnePlayer(this NetworkIdentity identity, SynapsePlayer player)
    {
        var msg = new ObjectDestroyMessage { netId = identity.netId };
        player.Connection.Send(msg);
    }

    //Todo: That ave not the same comportent as UpdatePositionRotationScale
    public static void SpawnForOnePlayer(this NetworkIdentity identity, SynapsePlayer player)
        => player.Connection.Send(_mirror.GetSpawnMessage(identity));

    /// <summary>
    /// Hides an NetworkObject for all Players on the Server that are currently connected
    /// </summary>
    public static void UnSpawnForAllPlayers(this NetworkIdentity identity)
    {
        var msg = new ObjectDestroyMessage { netId = identity.netId };
        NetworkServer.SendToAll(msg);
    }

    public static void SpawnForAllPlayers(this NetworkIdentity identity) => UpdatePositionRotationScale(identity);

    public static bool CheckPermission(this DoorPermissions door, SynapsePlayer player) =>
        CheckPermission(door.RequiredPermissions, player, door.RequireAll);

    public static bool CheckPermission(this KeycardPermissions permissions, SynapsePlayer player,
        bool checkCombinedPerms = false)
    {
        var ev2 = new CheckKeyCardPermissionEvent(player, false, permissions);
        if (player.Bypass || (ushort)permissions == 0) ev2.Allow = true;
        if (player.TeamID == (uint)Team.SCPs && permissions.HasFlagFast(KeycardPermissions.ScpOverride))
            ev2.Allow = true;

        if (!ev2.Allow)
        {
            var items = _config.GamePlayConfiguration.RemoteKeyCard
                ? player.Inventory.Items.ToList()
                : new List<SynapseItem> { player.Inventory.ItemInHand };

            foreach (var item in items)
            {
                if (item.ItemCategory != ItemCategory.Keycard || item.Item == null) continue;

                var overlappingPerms = ((KeycardItem)item.Item).Permissions & permissions;
                var ev = new KeyCardInteractEvent(item, ItemInteractState.Finalize, player, permissions)
                {
                    Allow = checkCombinedPerms
                        ? overlappingPerms == permissions
                        : overlappingPerms > KeycardPermissions.None,
                };

                _itemEvents.KeyCardInteract.Raise(ev);
                if (!ev.Allow) continue;
                ev2.Allow = true;
                break;
            }
        }

        _playerEvents.CheckKeyCardPermission.Raise(ev2);
        return ev2.Allow;
    }

    public static SynapsePlayer FastGetSynapsePlayer(this GameObject gameObject)
    {
        if (gameObject == null) return null;
        if (CachedPlayer.TryGetValue(gameObject, out var player))
            return player;
        player = gameObject.GetSynapsePlayer();
        CachedPlayer.Add(gameObject, player);
        return player;
    }

    public static SynapsePlayer FastGetSynapsePlayer(this MonoBehaviour mono)
    {
        var gameObject = mono?.gameObject;
        if (gameObject == null) return null;
        if (CachedPlayer.TryGetValue(gameObject, out var player))
            return player;
        player = gameObject.GetSynapsePlayer();
        CachedPlayer.Add(gameObject, player);
        return player;
    }

    public static SynapsePlayer GetSynapsePlayer(this NetworkConnection connection) =>
        connection?.identity?.GetSynapsePlayer();

    public static SynapsePlayer GetSynapsePlayer(this MonoBehaviour mono) =>
        mono?.gameObject?.GetComponent<SynapsePlayer>();

    public static SynapsePlayer GetSynapsePlayer(this GameObject gameObject) =>
        gameObject?.GetComponent<SynapsePlayer>();

    public static SynapsePlayer GetSynapsePlayer(this CommandSender sender) => _player
        .GetPlayer(x => x.CommandSender == sender, PlayerType.Dummy, PlayerType.Player, PlayerType.Server);

    public static SynapsePlayer GetSynapsePlayer(this StatBase stat) => stat.Hub.GetSynapsePlayer();
    public static SynapsePlayer GetSynapsePlayer(this Footprint footprint) => footprint.Hub?.GetSynapsePlayer();
    public static SynapsePlayer GetSynapsePlayer(this PluginAPI.Core.Player player) => player?.ReferenceHub?.GetSynapsePlayer();
    
    public static SynapsePlayer GetSynapsePlayer<TScpRole>(this StandardSubroutine<TScpRole> role)
        where TScpRole : PlayerRoleBase
        => role.Owner.GetSynapsePlayer();

    public static SynapseItem GetItem(this ItemPickupBase pickupBase) => _item.GetSynapseItem(pickupBase.Info.Serial);
    public static SynapseItem GetItem(this ItemBase itemBase) => _item.GetSynapseItem(itemBase.ItemSerial);

    /// <summary>
    /// Returns a UniversalDamageHandler based upon the given DamageType
    /// </summary>
    public static UniversalDamageHandler GetUniversalDamageHandler(this DamageType type)
    {
        if ((int)type < 0 || (int)type > 23) return new UniversalDamageHandler(0f, DeathTranslations.Unknown);

        return new UniversalDamageHandler(0f, DeathTranslations.TranslationsById[(byte)type]);
    }

    public static DamageType GetDamageType(this DamageHandlerBase handler)
    {
        if (handler == null) return DamageType.Unknown;

        if (!Enum.TryParse<DamageType>(handler.GetType().Name.Replace("DamageHandler", ""), out var type))
            return DamageType.Unknown;

        if (type != DamageType.Universal) return type;
        var id = ((UniversalDamageHandler)handler).TranslationId;
        
        if (id > 23) return DamageType.Universal;
        
        return (DamageType)id;
    }

    public static IRoom GetRoom(this RoomType type) => _room._rooms.FirstOrDefault(x => x.Id == (int)type);

    public static IElevator GetSynapseElevator(this ElevatorManager.ElevatorGroup type) =>
        _elevator.Elevators.FirstOrDefault(x => x.ElevatorId == (uint)type);


    public static IVanillaRoom GetVanillaRoom(this RoomIdentifier identifier) => (IVanillaRoom)_room._rooms
        .FirstOrDefault(x => x.GameObject == identifier.gameObject);

    public static SynapseDoor GetSynapseDoor(this DoorVariant variant)
    {
        var script = variant.GetComponent<SynapseObjectScript>();

        if (script != null && script.Object is SynapseDoor door)
        {
            return door;
        }

        NeuronLogger.For<Synapse>().Debug("Found DoorVariant without SynapseObjectScript ... creating new SynapseDoor");
        return new SynapseDoor(variant);
    }

    public static SynapseGenerator GetSynapseGenerator(this Scp079Generator generator)
    {
        var script = generator.GetComponent<SynapseObjectScript>();

        if (script != null && script.Object is SynapseGenerator gen)
        {
            return gen;
        }

        NeuronLogger.For<Synapse>()
            .Debug("Found Scp079Generator without SynapseObjectScript ... creating new SynapseGenerator");
        return new SynapseGenerator(generator);
    }

    public static SynapseWorkStation GetSynapseWorkStation(this WorkstationController workstationController)
    {
        var script = workstationController.GetComponent<SynapseObjectScript>();

        if (script != null && script.Object is SynapseWorkStation workStation)
        {
            return workStation;
        }

        NeuronLogger.For<Synapse>()
            .Debug("Found WorkStationController without SynapseObjectScript ... creating new SynapseWorkStation");
        return new SynapseWorkStation(workstationController);
    }

    public static SynapseLocker GetSynapseLocker(this Locker locker)
    {
        var script = locker.GetComponent<SynapseObjectScript>();

        if (script != null && script.Object is SynapseLocker synapseLocker)
        {
            return synapseLocker;
        }

        NeuronLogger.For<Synapse>()
            .Debug("Found Locker without SynapseObjectScript ... creating new SynapseLocker");
        return new SynapseLocker(locker);
    }

    public static SynapseRagDoll GetSynapseRagDoll(this BasicRagdoll rag)
    {
        var script = rag.GetComponent<SynapseObjectScript>();

        if (script != null && script.Object is SynapseRagDoll ragDoll)
        {
            return ragDoll;
        }

        NeuronLogger.For<Synapse>()
            .Debug("Found RagDoll without SynapseObjectScript ... creating new SynapseRagDoll");
        return new SynapseRagDoll(rag);
    }


    public static SynapseTesla GetSynapseTesla(this TeslaGate gate) => _map
        ._synapseTeslas.FirstOrDefault(x => x.Gate == gate);

    public static IElevator GetSynapseElevator(this ElevatorChamber chamber) => _elevator.Elevators.FirstOrDefault(x =>
        x.Chamber is SynapseElevatorChamber synapseChamber && synapseChamber.Chamber == chamber);

    public static SynapseCamera GetCamera(this Scp079Camera cam) => _map
        ._synapseCameras.FirstOrDefault(x => x.Camera == cam);

    public static bool CanHarmScp(SynapsePlayer player, bool message)
    {
        if (player.TeamID != (int)Team.SCPs &&
            player.CustomRole?.GetFriendsID().Any(x => x == (int)Team.SCPs) != true) return true;

        if (message)
            player.SendHint(_config.Translation.Get(player).ScpTeam);
        return false;
    }

    public static bool GetHarmPermission(SynapsePlayer attacker, SynapsePlayer victim, bool ignoreFFConfig = false)
    {
        bool allow;

        if (attacker == null || victim == null)
        {
            allow = true;
            goto Event;
        }

        if (_round.RoundEnded && _config.GamePlayConfiguration.AutoFriendlyFire)
        {
            allow = true;
            goto Event;
        }

        if (attacker == victim)
        {
            allow = true;
            goto Event;
        }

        if (attacker.Team == Team.Dead && victim.Team == Team.Dead)
        {
            allow = false;
            goto Event;
        }

        if (attacker.CustomRole == null && victim.CustomRole == null)
        {
            if (attacker.Team == Team.SCPs && victim.Team == Team.SCPs)
            {
                allow = false;
                goto Event;
            }

            var ff = ignoreFFConfig || _server.FF;

            if (ff)
            {
                allow = true;
                goto Event;
            }

            allow = attacker.Faction != victim.Faction;
            goto Event;
        }

        allow = true;
        if (attacker.CustomRole != null && attacker.CustomRole.GetFriendsID().Any(x => x == victim.TeamID))
        {
            allow = false;
            attacker.SendHint(_config.Translation.Get(attacker).SameTeam);
        }

        if (victim.CustomRole != null && victim.CustomRole.GetFriendsID().Any(x => x == attacker.TeamID))
        {
            allow = false;
            attacker.SendHint(_config.Translation.Get(attacker).SameTeam);
        }

        Event:
        var ev = new HarmPermissionEvent(attacker, victim, allow);
        _playerEvents.HarmPermission.RaiseSafely(ev);
        return ev.Allow;
    }

    public static TTranslation Get<TTranslation>(this TTranslation translation)
        where TTranslation : Translations<TTranslation>, new()
        => translation.WithLocale(_config.HostingConfiguration.Language);

    public static TTranslation Get<TTranslation>(this TTranslation translation, SynapsePlayer player)
        where TTranslation : Translations<TTranslation>, new()
        => player.GetTranslation(translation);

    public static string Replace(this string msg, Dictionary<string, string> values)
    {
        foreach (var value in values)
        {
            msg = msg.Replace(value.Key, value.Value);
        }

        return msg;
    }

    public static void RaiseSafely<TEventReactor, TEvent>(this TEventReactor reactor, TEvent eventParameter)
        where TEvent : IEvent
        where TEventReactor : EventReactor<TEvent>
    {
        try
        {
            reactor.Raise(eventParameter);
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error($"{eventParameter.GetType().Name} failed\n" + ex);
        }
    }

    private static FieldInfo FindField(Type type, string name)
    {
        var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
        if (field != null)
        {
            return field;
        }

        var baseType = type.BaseType;
        if (baseType == null)
        {
            return null;
        }

        return FindField(baseType, name);
    }

    public static void RaiseEvent(object source, string eventName, params object[] args)
    {
        // Find the delegate and invoke it.
        var delegateField = FindField(source.GetType(), eventName);
        var eventDelegate = delegateField?.GetValue(source) as Delegate;
        eventDelegate?.DynamicInvoke(args);
    }

    public static void RaiseEvent(Type source, string eventName, params object[] args)
    {
        // Find the delegate and invoke it.
        var delegateField = FindField(source, eventName);
        var eventDelegate = delegateField?.GetValue(null) as Delegate;
        eventDelegate?.DynamicInvoke(args);
    }

    public static void RaiseEventSafe(object source, string eventName, bool log, params object[] args)
    {
        // Find the delegate and invoke it.
        var delegateField = FindField(source.GetType(), eventName);
        var eventDelegate = delegateField?.GetValue(source) as Delegate;
        try
        {
            eventDelegate?.DynamicInvoke(args);
        }
        catch (Exception ex)
        {
            if (log)
                SynapseLogger<Synapse>.Error("Error while invoking event " + source.GetType().Name + "." + eventName +
                                             "\n" + ex);
        }
    }

    public static void RaiseEventSafe(Type source, string eventName, bool log, params object[] args)
    {
        // Find the delegate and invoke it.
        var delegateField = FindField(source, eventName);
        var eventDelegate = delegateField?.GetValue(null) as Delegate;
        try
        {
            eventDelegate?.DynamicInvoke(args);
        }
        catch (Exception ex)
        {
            if (log)
                SynapseLogger<Synapse>.Error("Error while invoking event " + source.Name + "." + eventName + "\n" + ex);
        }
    }

    public static T GetSubroutine<T>(this ISubroutinedRole role) where T : SubroutineBase
        => role.SubroutineModule.AllSubroutines.FirstOrDefault(p => p is T) as T;

    public static CoroutineHandle RunSafelyCoroutine(this IEnumerator<float> coroutine) =>
        Timing.RunCoroutine(_RunSafelyCoroutine<Synapse>(coroutine));

    public static CoroutineHandle RunSafelyCoroutine<TName>(this IEnumerator<float> coroutine) =>
        Timing.RunCoroutine(_RunSafelyCoroutine<TName>(coroutine));

    private static IEnumerator<float> _RunSafelyCoroutine<TName>(IEnumerator<float> coroutine)
    {
        var time = 0f;
        var done = false;
        var count = 0;
        while (!done)
        {
            try
            {
                if (coroutine.MoveNext())
                {
                    time = coroutine.Current;
                }
                else
                {
                    done = true;
                }
            }
            catch (Exception ex)
            {
                SynapseLogger<TName>.Error("Execution of coroutine failed at position " + count + ":\n" + ex);
                yield break;
            }

            count++;
            yield return time;
        }
    }
}