﻿using System;
using System.Collections.Generic;
using Footprinting;
using HarmonyLib;
using InventorySystem.Items.Armor;
using InventorySystem.Items.Pickups;
using MapGeneration.Distributors;
using Neuron.Core.Meta;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Events;
using Respawning;
using Scp914;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Map;
using Synapse3.SynapseModule.Player;
using UnityEngine;
using static UnityStandardAssets.CinematicEffects.Bloom;

namespace Synapse3.SynapseModule.Patching.Patches;

#if !PATCHLESS
[Automatic]
[SynapsePatch("Scp914Upgrade", PatchType.MapEvent)]
public static class Scp914UpgradePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp914Upgrader), nameof(Scp914Upgrader.Upgrade))]
    public static bool Scp914Upgrade(Collider[] intake, Vector3 moveVector, Scp914Mode mode, Scp914KnobSetting setting)
        => DecoratedMapPatches.OnUpgrade(intake, moveVector, mode, setting);
}

[Automatic]
[SynapsePatch("GeneratorEngage", PatchType.MapEvent)]
public static class GeneratorEngagePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp079Generator), nameof(Scp079Generator.ServerUpdate))]
    public static bool GeneratorEngage(Scp079Generator __instance)
    {
        DecoratedMapPatches.GeneratorUpdate(__instance);
        return false;
    }
}

[Automatic]
[SynapsePatch("GeneratorInteract", PatchType.MapEvent)]
public static class GeneratorInteractPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp079Generator), nameof(Scp079Generator.ServerInteract))]
    public static bool GeneratorInteract(Scp079Generator __instance, ReferenceHub ply, byte colliderId)
    {
        DecoratedMapPatches.OnGenInteract(__instance, ply, colliderId);
        return false;
    }
}

[Automatic]
[SynapsePatch("TeslaPatch", PatchType.MapEvent)]
public static class TeslaPatch
{
    private static readonly MapEvents MapEvents;
    static TeslaPatch() => MapEvents = Synapse.Get<MapEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(TeslaGate), nameof(TeslaGate.PlayerInRange))]
    public static bool PlayerInRange(TeslaGate __instance, ReferenceHub player, out bool __result)
    {
        __result = false;
        try
        {
            var sPlayer = player.GetSynapsePlayer();
            var tesla = __instance.GetSynapseTesla();
            if (sPlayer == null || tesla == null) return false;
            if (!__instance.InRange(player.transform.position)) return false;
            var ev = new TriggerTeslaEvent(sPlayer,
                sPlayer.Invisible < InvisibleMode.Ghost && (sPlayer.CurrentRole is not ITeslaControllerRole teslaRole ||
                                                            teslaRole.CanActivateShock), tesla, false);
            MapEvents.TriggerTesla.RaiseSafely(ev);
            __result = ev.Allow;
            return false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Trigger Tesla Patch failed\n" + ex);
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(TeslaGate), nameof(TeslaGate.IsInIdleRange), typeof(ReferenceHub))]
    public static bool PlayerInIdleRange(TeslaGate __instance, ReferenceHub player, out bool __result)
    {
        __result = false;
        try
        {
            var sPlayer = player.GetSynapsePlayer();
            var tesla = __instance.GetSynapseTesla();
            if (sPlayer == null || tesla == null) return false;
            if (!sPlayer.IsAlive) return false;
            var pos = __instance.transform.position;
            switch (sPlayer.CurrentRole)
            {
                case IFpcRole fpcRole
                    when Vector3.Distance(pos, fpcRole.FpcModule.Position) < __instance.distanceToIdle:
                case ITeslaControllerRole teslaControllerRole
                    when teslaControllerRole.IsInIdleRange(tesla.Gate):
                    var ev = new TriggerTeslaEvent(sPlayer,
                        sPlayer.Invisible < InvisibleMode.Ghost, tesla, true);
                    MapEvents.TriggerTesla.RaiseSafely(ev);
                    __result = ev.Allow;
                    break;
            }

            return false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Trigger Tesla Idle Patch failed\n" + ex);
        }

        return false;
    }
}

[Automatic]
[SynapsePatch("CassieMessage", PatchType.MapEvent)]
public static class CassieMessagePatch
{
    internal static bool ActivePatch = true;
    
    private static readonly MapEvents MapEvents;
    private static readonly CassieService Cassie;

    static CassieMessagePatch()
    {
        MapEvents = Synapse.Get<MapEvents>();
        Cassie = Synapse.Get<CassieService>();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(RespawnEffectsController), nameof(RespawnEffectsController.PlayCassieAnnouncement))]
    public static bool PlayCassieAnnouncement(ref string words, ref bool makeHold, ref bool makeNoise, ref bool customAnnouncement)
    {
        if (!ActivePatch) return true;
        var settings = new List<CassieSettings>();
        if (makeHold)
            settings.Add(CassieSettings.Break);
        if (makeNoise)
            settings.Add(CassieSettings.Noise);
        if (customAnnouncement)
            settings.Add(CassieSettings.DisplayText);
        
        var ev = new CassieMessageEvent(words, settings);
        
        MapEvents.CassieMessage.RaiseSafely(ev);

        makeHold = ev.Settings.Contains(CassieSettings.Break);
        makeNoise = ev.Settings.Contains(CassieSettings.Noise);
        customAnnouncement = ev.Settings.Contains(CassieSettings.DisplayText);

        if (ev.CustomTranslation)
        {
            words = string.Join(string.Empty, ev.CassieSentences);
        }

        return ev.Allow;
    }
}

public static class DecoratedMapPatches
{
    private static readonly MapEvents MapEvents;
    static DecoratedMapPatches() => MapEvents = Synapse.Get<MapEvents>();

    public static void OnGenInteract(Scp079Generator gen, ReferenceHub hub, byte interaction)
    {
        try
        {
            if (gen._cooldownStopwatch.IsRunning &&
                gen._cooldownStopwatch.Elapsed.TotalSeconds < gen._targetCooldown) return;

            if (interaction != 0 && !gen.HasFlag(gen._flags, Scp079Generator.GeneratorFlags.Open))
                return;

            gen._cooldownStopwatch.Stop();
            var player = hub.GetSynapsePlayer();

            var nwAllow = EventManager.ExecuteEvent(new PlayerInteractGeneratorEvent(hub, gen,
                (Scp079Generator.GeneratorColliderId)interaction));

            switch (interaction)
            {
                //0 - Request interaction with Generator Doors (doors)
                case 0:
                    if (gen.HasFlag(gen._flags, Scp079Generator.GeneratorFlags.Unlocked))
                    {
                        var isOpen = gen.HasFlag(gen._flags, Scp079Generator.GeneratorFlags.Open);
                        var nwAllow2 = EventManager.ExecuteEvent(
                            isOpen ? new PlayerCloseGeneratorEvent(hub, gen) : new PlayerOpenGeneratorEvent(hub, gen));

                        var ev = new GeneratorInteractEvent(player, nwAllow && nwAllow2, gen.GetSynapseGenerator(),
                            isOpen
                                ? GeneratorInteract.CloseDoor
                                : GeneratorInteract.OpenDoor);

                        Synapse.Get<PlayerEvents>().GeneratorInteract.RaiseSafely(ev);

                        if (ev.Allow)
                        {
                            gen.ServerSetFlag(Scp079Generator.GeneratorFlags.Open, !isOpen);
                            gen._targetCooldown = gen._doorToggleCooldownTime;
                        }
                    }
                    else
                    {
                        var allow = gen._requiredPermission.CheckPermission(player) &&
                                    Synapse3Extensions.CanHarmScp(player, true) &&
                                    EventManager.ExecuteEvent(new PlayerUnlockGeneratorEvent(hub, gen));
                        var ev = new GeneratorInteractEvent(player, allow, gen.GetSynapseGenerator(),
                            GeneratorInteract.UnlockDoor);
                        Synapse.Get<PlayerEvents>().GeneratorInteract.RaiseSafely(ev);

                        if (ev.Allow)
                        {
                            gen.ServerSetFlag(Scp079Generator.GeneratorFlags.Unlocked, true);
                            gen.ServerGrantTicketsConditionally(new Footprint(hub), Scp079Generator.UnlockTokenReward);
                        }
                        else
                        {
                            gen.RpcDenied();
                        }

                        gen._targetCooldown = gen._unlockCooldownTime;
                    }

                    break;

                //1 - Request to swap the Activation State (lever)
                case 1:
                    if ((gen.Activating || Synapse3Extensions.CanHarmScp(player, true)) && !gen.Engaged)
                    {
                        var nwAllow2 = EventManager.ExecuteEvent(
                            gen.Activating
                                ? new PlayerDeactivatedGeneratorEvent(hub, gen)
                                : new PlayerActivateGeneratorEvent(hub, gen));
                        var ev = new GeneratorInteractEvent(player, nwAllow && nwAllow2, gen.GetSynapseGenerator(),
                            gen.Activating ? GeneratorInteract.Cancel : GeneratorInteract.Activate);
                        Synapse.Get<PlayerEvents>().GeneratorInteract.RaiseSafely(ev);
                        if (!ev.Allow) break;

                        gen.Activating = !gen.Activating;
                        if (gen.Activating)
                        {
                            gen._leverStopwatch.Restart();
                            gen._lastActivator = new Footprint(hub);
                        }
                        else
                        {
                            gen._lastActivator = default;
                        }

                        gen._targetCooldown = gen._doorToggleCooldownTime;
                    }

                    break;

                //2 - Request to cancel the activation (cancel button)
                case 2:
                    if (gen.Activating && !gen.Engaged)
                    {
                        var ev = new GeneratorInteractEvent(player,
                            nwAllow && EventManager.ExecuteEvent(new PlayerDeactivatedGeneratorEvent(hub, gen)),
                            gen.GetSynapseGenerator(),
                            GeneratorInteract.Cancel);
                        Synapse.Get<PlayerEvents>().GeneratorInteract.RaiseSafely(ev);
                        if (!ev.Allow) break;

                        gen.ServerSetFlag(Scp079Generator.GeneratorFlags.Activating, false);
                        gen._targetCooldown = gen._unlockCooldownTime;
                        gen._lastActivator = default;
                    }

                    break;

                default:
                    gen._targetCooldown = 1f;
                    break;
            }

            gen._cooldownStopwatch.Restart();
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Generator Interact Patch failed\n" + ex);
        }
    }

    public static void GeneratorUpdate(Scp079Generator generator)
    {
        try
        {
            var engageReady = generator._currentTime >= generator._totalActivationTime;
            if (!engageReady)
            {
                var time = Mathf.FloorToInt(generator._totalActivationTime - generator._currentTime);
                if (time != generator._syncTime)
                    generator.Network_syncTime = (short)time;
            }

            if (generator.ActivationReady)
            {
                if (engageReady && !generator.Engaged)
                {
                    var ev = new GeneratorEngageEvent(generator.GetSynapseGenerator());
                    MapEvents.GeneratorEngage.RaiseSafely(ev);

                    if (!ev.Allow || ev.ForcedUnAllow ||
                        !EventManager.ExecuteEvent(new GeneratorActivatedEvent(generator)))
                        return;

                    generator.Engaged = true;
                    generator.Activating = false;
                    return;
                }

                generator._currentTime += Time.deltaTime;
            }
            else
            {
                if (generator._currentTime == 0f || engageReady)
                    return;

                generator._currentTime -= generator.DropdownSpeed * Time.deltaTime;
            }

            generator._currentTime = Mathf.Clamp(generator._currentTime, 0f, generator._totalActivationTime);
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Generator Update Patch failed\n" + ex);
        }
    }

    public static bool OnUpgrade(Collider[] intake, Vector3 moveVector, Scp914Mode mode, Scp914KnobSetting setting)
    {
        try
        {
            var list = new HashSet<GameObject>();
            var players = new List<SynapsePlayer>();
            var items = new List<SynapseItem>();

            var inventory = (mode & Scp914Mode.Inventory) == Scp914Mode.Inventory;
            var heldOnly = inventory && (mode & Scp914Mode.Held) == Scp914Mode.Held;

            foreach (var collider in intake)
            {
                var gameObject = collider.transform.root.gameObject;
                if (!list.Add(gameObject)) continue;

                if (gameObject.TryGetComponent<SynapsePlayer>(out var player))
                {
                    players.Add(player);
                }
                else if (gameObject.TryGetComponent<ItemPickupBase>(out var pickup))
                {
                    var item = pickup.GetItem();
                    if (item is { CanBePickedUp: true })
                        items.Add(item);
                }
            }

            var ev = new Scp914UpgradeEvent(players, items)
            {
                MoveVector = moveVector
            };
            MapEvents.Scp914Upgrade.RaiseSafely(ev);

            if (!ev.Allow)
                return false;

            foreach (var player in ev.Players)
            {
                if (ev.MovePlayers)
                    player.Position = player.transform.position + ev.MoveVector;

                if (heldOnly)
                {
                    if (player.Inventory.ItemInHand != SynapseItem.None)
                    {
                        var destroy = true;
                        foreach (var processor in player.Inventory.ItemInHand.UpgradeProcessors)
                        {
                            if (processor.CreateUpgradedItem(player.Inventory.ItemInHand, setting))
                            {
                                destroy = false;
                                break;
                            }
                        }
                        if (destroy)
                            player.Inventory.ItemInHand.Destroy();
                    }
                }
                else if (inventory)
                {
                    foreach (var item in player.Inventory.Items)
                    {
                        Scp914Upgrader.OnPickupUpgraded?.Invoke(item.Pickup, setting);
                        var destroy = true;
                        foreach (var processor in item.UpgradeProcessors)
                        {
                            if (processor.CreateUpgradedItem(item, setting))
                            {
                                destroy = false;
                                break;
                            }
                        }
                        if (destroy)
                            player.Inventory.ItemInHand.Destroy();
                    }
                }

                BodyArmorUtils.RemoveEverythingExceedingLimits(player.VanillaInventory,
                    player.VanillaInventory.TryGetBodyArmor(out var armor) ? armor : null);
            }

            foreach (var item in ev.Items)
            {
                var destroy = true;
                foreach (var processor in item.UpgradeProcessors)
                {
                    if (processor.CreateUpgradedItem(item, setting, 
                        ev.MoveItems ? item.Position + ev.MoveVector : item.Position))
                    {
                        destroy = false;
                        break;
                    }
                }
                if (destroy)
                    item.Destroy();
            }

            return false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Scp914 Upgrade Patch failed\n" + ex);
            return true;
        }
    }
}
#endif