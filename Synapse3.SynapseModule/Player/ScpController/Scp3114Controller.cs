using System.Linq.Expressions;
using CustomPlayerEffects;
using Mirror;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp3114;
using Synapse3.SynapseModule.Map.Objects;
using static PlayerRoles.PlayableScps.Scp3114.Scp3114Identity;
using static PlayerRoles.PlayableScps.Scp3114.Scp3114Strangle;

namespace Synapse3.SynapseModule.Player.ScpController;

public class Scp3114Controller : ScpShieldController<Scp3114Role>
{
    public Scp3114Controller(SynapsePlayer player) : base(player) { }

    public Scp3114Slap Slap => Role?.GetSubroutine<Scp3114Slap>();
    public Scp3114Strangle Strangle => Role?.GetSubroutine<Scp3114Strangle>();
    public Scp3114Identity Identity => Role?.GetSubroutine<Scp3114Identity>();
    public Scp3114VoiceLines VoiceLines => Role?.GetSubroutine<Scp3114VoiceLines>();

    public SynapsePlayer StrangleTarget
    {
        get
        {
            var target = Strangle?.SyncTarget?.Target;
            if (target == null) return null;
            return target.GetSynapsePlayer();
        }
        set
        {
            var strangle = Strangle;
            if (strangle == null) return;
            if (value == Strangle?.SyncTarget?.Target.GetSynapsePlayer()) return;
            if (value != null)
            {
                // Can't strangle non-existent physical players
                //TODO:
/*                if (value.FirstPersonMovement == null) return;
                if (value.CurrentRole is not HumanRole humanRole) return;
                _player.GiveEffect(Enums.Effect.Strangled);
                var targetPosition = strangle.GetStranglePosition(humanRole);
                Synapse3Extensions.RaiseEvent(strangle, nameof(Scp3114Strangle.ServerOnBegin));
                strangle.SyncTarget = new StrangleTarget(value, targetPosition, _player.Position);
                strangle._rpcType = RpcType.TargetResync;
                strangle.ServerSendRpc(toAll: true);*/
            }
            else
            {
                strangle.SyncTarget = null;
                strangle._rpcType = RpcType.AttackInterrupted;
                strangle.ServerSendRpc(toAll: true);
            }
        }
    }

    public SynapseRagDoll DisguiseRagDoll
    {
        get
        {
            var ragdoll = Identity?.CurIdentity.Ragdoll;
            if (ragdoll == null)
                return null;

            return ragdoll.GetSynapseRagDoll();
        }
        set
        {
            var identity = Identity;
            if (identity == null) return;
            identity.CurIdentity.Ragdoll = value.BasicRagDoll;
            identity.ServerResendIdentity();
        }
    }

    public DisguiseStatus DisguiseStatus
    {
        get
        {
            var identity = Identity;
            if (identity == null) return DisguiseStatus.None;
            return identity.CurIdentity.Status;
        }
        set
        {
            var identity = Identity;
            if (identity == null) return;
            identity.CurIdentity.Status = value;
        }
    }

    public float DisguiseDuration
    {
        get
        {
            var identity = Identity;
            if (identity == null) return 0;
            return identity._disguiseDurationSeconds;
        }
        set
        {
            var identity = Identity;
            if (identity == null) return;
            identity._disguiseDurationSeconds = value;
        }
    }

    public byte UnitNameId
    { 
        get
        {
            var identity = Identity;
            if (identity == null) return 0;
            return identity.CurIdentity.UnitNameId;
        }
        set
        {
            var identity = Identity;
            if (identity == null) return;
            identity.CurIdentity.UnitNameId = value;
            identity.ServerResendIdentity();
        }
    }

    public void StopStrangle()
    {
        var strangle = Strangle;
    }

    public void RemoveDisguise()
    {
        var identity = Identity;
        if (identity == null) return;
        identity.CurIdentity.Reset();
        identity.ServerResendIdentity();
    }

    public void PlaySound(Scp3114VoiceLines.VoiceLinesName voiceLine = Scp3114VoiceLines.VoiceLinesName.RandomIdle)
    {
        var voiceLines = VoiceLines;
        if (voiceLines == null) return;
        voiceLines.ServerPlayConditionally(voiceLine);
    }
    
    public override RoleTypeId ScpRole => RoleTypeId.Scp3114;
}
