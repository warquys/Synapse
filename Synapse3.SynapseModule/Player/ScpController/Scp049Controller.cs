using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079;

namespace Synapse3.SynapseModule.Player.ScpController;

public class Scp049Controller : ScpShieldController<Scp079Role>
{


    public Scp049Controller(SynapsePlayer player) : base(player) { }

    public override RoleTypeId ScpRole => RoleTypeId.Scp049;
}
