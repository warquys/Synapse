using PlayerRoles;
using PluginAPI.Roles;
using Synapse3.SynapseModule.Config;

namespace Synapse3.SynapseModule.Player.ScpController;

public class MainScpController
{
    //TODO: Check Controllers
    private readonly SynapsePlayer _player;
    private readonly SynapseConfigService _config;

    internal MainScpController(SynapsePlayer player, SynapseConfigService config)
    {
        _player = player;
        Scp079 = new(player);
        Scp096 = new(player);
        Scp106 = new(player);
        Scp173 = new(player);
        Scp939 = new(player);
        Scp049 = new(player);
        Scp0492 = new(player);
        Scp3114 = new(player);
        _config = config;
    }

    public readonly Scp3114Controller Scp3114;

    public readonly Scp106Controller Scp106;

    public readonly Scp079Controller Scp079;

    public readonly Scp096Controller Scp096;

    public readonly Scp173Controller Scp173;

    public readonly Scp939Controller Scp939;

    public readonly Scp049Controller Scp049;

    public readonly Scp0492Controller Scp0492;

    public bool ProximityToggle(out string message, out bool enabled)
    {
        message = "";
        enabled = false;
        if (_player.Team != Team.SCPs) return false;
        if (!_config.GamePlayConfiguration.SpeakingScp.Contains(_player.RoleID) &&
            !_player.HasPermission("synapse.scp-proximity")) return false;
        
        enabled = ProximityChat = !ProximityChat;
        var translation = _config.Translation.Get(_player);
        message = _player.MainScpController.ProximityChat
            ? translation.EnableProximity
            : translation.DisableProximity;
        return true;
    }

    public bool ProximityChat { get; set; }

    public IScpControllerBase CurrentController =>
        _player.RoleType switch
        {
            RoleTypeId.Scp079 => Scp079,
            RoleTypeId.Scp096 => Scp096,
            RoleTypeId.Scp106 => Scp106,
            RoleTypeId.Scp173 => Scp173,
            RoleTypeId.Scp939 => Scp939,
            RoleTypeId.Scp049 => Scp049,
            RoleTypeId.Scp0492 => Scp0492,
            RoleTypeId.Scp3114 => Scp3114,
            _ => null
        };
}