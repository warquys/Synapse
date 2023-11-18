using System.Collections.Generic;
using CustomPlayerEffects;
using Neuron.Core.Logging;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp106;
using PlayerRoles.PlayableScps.Subroutines;
using Synapse3.SynapseModule.Config;

namespace Synapse3.SynapseModule.Player.ScpController;


public class Scp106Controller : ScpShieldController<Scp106Role>
{
    public const float OldTraumatizedEffectDuration = 180f;
    private readonly SynapseConfigService _config;

    //TODO: Add the new SCP 106 Ablility
    public Scp106Controller(SynapsePlayer player) : base(player)
    {
        _config = Synapse.Get<SynapseConfigService>();
    }

    public Scp106StalkAbility StalkAbility => Role?.GetSubroutine<Scp106StalkAbility>();
    public Scp106HuntersAtlasAbility HuntersAtlas => Role?.GetSubroutine<Scp106HuntersAtlasAbility>();
    public Scp106Attack Attack => Role?.GetSubroutine<Scp106Attack>();
    public Scp106SinkholeController sinkhole => Role?.GetSubroutine<Scp106SinkholeController>();

    public bool IsUsingPortal => Role.Sinkhole.IsDuringAnimation;
    public bool IsInGround => Role.IsSubmerged;

    /// <summary>
    /// Min 0, Max 100
    /// </summary>
    public float Vigor
    {
        get => Attack?.Vigor?.CurValue * 100 ?? 0f;
        set
        {
            var vigor = Attack?.Vigor;
            vigor.CurValue = value / 100;
        }
    }

    public void CapturePlayer(SynapsePlayer player)
        => CapturePlayer(player, true);

    public void CapturePlayer(SynapsePlayer player, bool cooldown)
    {
        var attack = Attack;
        if (attack == null)
            return;
        if (cooldown)
        {
            attack.SendCooldown(attack._hitCooldown);
            attack.ReduceSinkholeCooldown();
        }
        Synapse3Extensions.RaiseEvent(typeof(Scp106Attack), nameof(Scp106Attack.OnPlayerTeleported), player.Hub, player.Hub);
        var effectsController = player.Hub.playerEffectsController;
        effectsController.EnableEffect<PocketCorroding>();
        attack.Vigor.CurValue += Scp106Attack.VigorCaptureReward;
        PlayersInPocket.Add(player);
        Hitmarker.SendHitmarkerDirectly(attack.Owner, 1f);
    }

    public HashSet<SynapsePlayer> PlayersInPocket { get; } = new();

    protected internal override void ResetDefault()
    {
        PlayersInPocket.Clear();
    }


    public override RoleTypeId ScpRole => RoleTypeId.Scp106;
}