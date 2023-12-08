using System.Net;
using GameCore;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Permissions;
using VoiceChat;
using static ServerRoles;

namespace Synapse3.SynapseModule.Player;

public partial class SynapsePlayer
{
    /// <summary>
    /// True if the Rank is not visible for normal players
    /// </summary>
    public bool HideRank
    {
        get => !string.IsNullOrEmpty(ServerRoles.HiddenBadge);
        set
        {
            if (value)
                ServerRoles.TryHideTag();
            else
                ServerRoles.RefreshLocalTag();
        }
    }

    private SynapseGroup _synapseGroup;
    /// <summary>
    /// The Current SynapseGroup and therefore all Permissions of the Player
    /// </summary>
    public SynapseGroup SynapseGroup
    {
        get
        {
            if (_synapseGroup == null)
                return _permission.GetPlayerGroup(this);

            return _synapseGroup;
        }
        set
        {
            if (value == null)
                return;

            _synapseGroup = value;

            RefreshPermission(HideRank);
        }
    }
    
    /// <summary>
    /// True if the Player can Open the RemoteAdmin
    /// </summary>
    public bool RemoteAdminAccess
    {
        get => ServerRoles.RemoteAdmin;
        set
        {
            if (value)
                RaLogin();
            else
                RaLogout();
        }
    }
    
    /// <summary>
    /// Gives the Player access to the RemoteAdmin (doesn't automatically give any Permissions)
    /// </summary>
    public void RaLogin()
    {
        ServerRoles.RemoteAdmin = true;
        ServerRoles.Permissions = SynapseGroup.GetVanillaPermissionValue() | ServerRoles.GlobalPerms;
        if (!ServerRoles.AdminChatPerms)
            ServerRoles.AdminChatPerms = SynapseGroup.HasVanillaPermission(PlayerPermissions.AdminChat);
        ServerRoles.TargetSetRemoteAdmin(true);
        QueryProcessor.SyncCommandsToClient();
    }

    /// <summary>
    /// Removes the Player access to the RemoteAdmin
    /// </summary>
    public void RaLogout()
    {
        Hub.serverRoles.RemoteAdmin = false;
        Hub.serverRoles.TargetSetRemoteAdmin(false);
    }

    /// <summary>
    /// Returns true if the Player has the Permission
    /// </summary>
    /// <param name="permission"></param>
    /// <returns></returns>
    public bool HasPermission(string permission) =>
        PlayerType == PlayerType.Server || SynapseGroup.HasPermission(permission);

    /// <summary>
    /// Reloads the Permissions of the Player
    /// </summary>
    /// <param name="hideBadage"></param>
    public void RefreshPermission(bool hideBadage)
    {
        var group = new UserGroup
        {
            BadgeText = SynapseGroup.Badge.ToUpper() == "NONE" ? null : SynapseGroup.Badge,
            BadgeColor = SynapseGroup.Color.ToUpper() == "NONE" ? null : SynapseGroup.Color,
            Cover = SynapseGroup.Cover,
            HiddenByDefault = SynapseGroup.Hidden,
            KickPower = SynapseGroup.KickPower,
            Permissions = SynapseGroup.GetVanillaPermissionValue(),
            RequiredKickPower = SynapseGroup.RequiredKickPower,
            Shared = false
        };

        var globalAccessAllowed = false;
        var badge = AuthenticationManager.AuthenticationResponse.BadgeToken;
        if (badge != null)
        {
            if (badge.Staff)
                globalAccessAllowed = _config.PermissionConfiguration.StaffAccess;
            if (!globalAccessAllowed && badge.Management)
                globalAccessAllowed = _config.PermissionConfiguration.ManagerAccess;
            if (!globalAccessAllowed && badge.GlobalBanning)
                globalAccessAllowed = _config.PermissionConfiguration.GlobalBanTeamAccess;

            if (GlobalPerms != 0 && globalAccessAllowed)
                group.Permissions |= GlobalPerms;
        }

        ServerRoles.Group = group;
        ServerRoles.Permissions = group.Permissions;
        RemoteAdminAccess = SynapseGroup.RemoteAdmin || GlobalRemoteAdmin;
        ServerRoles.AdminChatPerms = PermissionsHandler.IsPermitted(group.Permissions, PlayerPermissions.AdminChat);
        ServerRoles.BadgeCover = group.Cover;
        QueryProcessor.GameplayData = PermissionsHandler.IsPermitted(group.Permissions, PlayerPermissions.GameplayData);

        if (PlayerType == PlayerType.Player)
            ServerRoles.SendRealIds();

        if (string.IsNullOrEmpty(group.BadgeText))
        {
            ServerRoles.SetColor(null);
            ServerRoles.SetText(null);
            if (!string.IsNullOrEmpty(ServerRoles._prevBadge))
            {
                ServerRoles.HiddenBadge = ServerRoles._prevBadge;
                ServerRoles.GlobalHidden = true;
                ServerRoles.RefreshHiddenTag();
            }
        }
        else
        {
            var playerPreferences = ServerRoles._localBadgeVisibilityPreferences;
            if (playerPreferences == ServerRoles.BadgeVisibilityPreferences.Hidden
                || (group.HiddenByDefault && !hideBadage && playerPreferences == ServerRoles.BadgeVisibilityPreferences.NoPreference))
            {
                ServerRoles.BadgeCover = ServerRoles.UserBadgePreferences == BadgePreferences.PreferLocal;
                if (!string.IsNullOrEmpty(ServerRoles.MyText))
                    return;
                ServerRoles.SetText(null);
                ServerRoles.SetColor("default");
                ServerRoles.GlobalHidden = false;
                ServerRoles.HiddenBadge = group.BadgeText;
                ServerRoles.RefreshHiddenTag();
                ServerRoles.TargetSetHiddenRole(Connection, group.BadgeText);
            }
            else
            {
                ServerRoles.HiddenBadge = null;
                ServerRoles.GlobalHidden = false;
                ServerRoles.RpcResetFixed();
                ServerRoles.SetText(group.BadgeText);
                ServerRoles.SetColor(group.BadgeColor);
            }
        }

        var nwStaff = badge?.Staff ?? false;
        var localBadge = nwStaff ||
                   PermissionsHandler.IsPermitted(group.Permissions, PlayerPermissions.ViewHiddenBadges);
        var globalBadge = nwStaff ||
                    PermissionsHandler.IsPermitted(group.Permissions, PlayerPermissions.ViewHiddenGlobalBadges);

        if (localBadge || globalBadge)
            foreach (var player in _player.Players)
            {
                if (!string.IsNullOrEmpty(player.ServerRoles.HiddenBadge) &&
                    (!player.ServerRoles.GlobalHidden || globalBadge) && (player.ServerRoles.GlobalHidden || localBadge))
                    player.ServerRoles.TargetSetHiddenRole(Connection, player.ServerRoles.HiddenBadge);
            }
    }

    /// <summary>
    /// If the Player has Globally Permissions for RemoteAdmin
    /// </summary>
    public bool GlobalRemoteAdmin => AuthenticationManager.RemoteAdminGlobalAccess;
    
    /// <summary>
    /// The Global Permissions of the Player
    /// </summary>
    public ulong GlobalPerms => ServerRoles.GlobalPerms;

    /// <summary>
    /// The vanilla group of the player
    /// </summary>
    public UserGroup Rank
    {
        get => ServerRoles.Group;
        set => ServerRoles.SetGroup(value, value != null && value.Permissions > 0UL, true);
    }

    /// <summary>
    /// The visible color of the player's rank
    /// </summary>
    public string RankColor
    {
        get => Rank.BadgeColor;
        set => ServerRoles.SetColor(value);
    }

    /// <summary>
    /// The visible name of the player's rank
    /// </summary>
    public string RankName
    {
        get => Rank.BadgeText;
        set => ServerRoles.SetText(value);
    }

    /// <summary>
    /// A code which represents the vanilla permissions of the player
    /// </summary>
    public ulong Permission
    {
        get => ServerRoles.Permissions;
        set => ServerRoles.Permissions = value;
    }

    /// <summary>
    /// True if the player is not allowed to use voice chat
    /// </summary>
    public VcMuteFlags MuteFlags
    {
        get => VoiceChatMutes.GetFlags(Hub);
        set => VoiceChatMutes.SetFlags(Hub, value);
    }

    public string CustomRemoteAdminBadge { get; set; } = "";
}