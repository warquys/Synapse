using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079;

namespace Synapse3.SynapseModule.Player.ScpController;

public class Scp0492Controller : ScpShieldController<Scp079Role>
{

    public Scp0492Controller(SynapsePlayer player) : base(player) { }

    public override RoleTypeId ScpRole => RoleTypeId.Scp0492;
}
