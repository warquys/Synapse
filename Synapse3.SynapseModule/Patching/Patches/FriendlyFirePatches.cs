﻿using System;
using HarmonyLib;
using InventorySystem.Items.ThrowableProjectiles;
using Neuron.Core.Meta;
using PlayerStatsSystem;
using UnityEngine;
using Utils;

namespace Synapse3.SynapseModule.Patching.Patches;

#if !PATCHLESS
[Automatic]
[SynapsePatch("CheckFF", PatchType.FriendlyFire)]
public static class CheckFriendlyFirePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HitboxIdentity), nameof(HitboxIdentity.CheckFriendlyFire), typeof(ReferenceHub),
        typeof(ReferenceHub), typeof(bool))]
    public static bool CheckFriendlyFire(out bool __result, ReferenceHub attacker, ReferenceHub victim,
        bool ignoreConfig)
    {
        try
        {
            __result = Synapse3Extensions.GetHarmPermission(attacker, victim, ignoreConfig);
            return false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Warn("Check FriendlyFire Patch failed\n" + ex);
            __result = true;
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("Flash bang", PatchType.FriendlyFire)]
public static class FlashBangCheckPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(FlashbangGrenade), nameof(FlashbangGrenade.ServerFuseEnd))]
    public static bool FlashBangCheck(FlashbangGrenade __instance)
    {
        try
        {
            var time = __instance._blindingOverDistance.keys[__instance._blindingOverDistance.length - 1].time;
            var squared = time * time;
            var playersHit = 0;

            foreach (var hub in ReferenceHub.AllHubs)
            {
                if ((__instance.transform.position - hub.transform.position).sqrMagnitude <= squared &&
                    hub != __instance.PreviousOwner.Hub &&
                    HitboxIdentity.CheckFriendlyFire(__instance.PreviousOwner.Hub, hub))
                {
                    playersHit++;
                    __instance.ProcessPlayer(hub);
                }
            }

            if (playersHit > 0)
                Hitmarker.SendHitmarkerDirectly(__instance.PreviousOwner.Hub, playersHit);

            ExplosionUtils.ServerSpawnEffect(__instance.transform.position, __instance.Info.ItemId);
            __instance.DestroySelf();

            return false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Warn("Flash bang Patch failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("Player Damage", PatchType.FriendlyFire)]
public static class PlayerDamagePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HitboxIdentity), nameof(HitboxIdentity.Damage))]
    public static bool PlayerDamage(HitboxIdentity __instance, float damage, DamageHandlerBase handler,
        Vector3 exactPos)
    {
        return handler is not AttackerDamageHandler aHandler ||
               Synapse3Extensions.GetHarmPermission(aHandler.Attacker, __instance?.TargetHub);
    }
}
#endif