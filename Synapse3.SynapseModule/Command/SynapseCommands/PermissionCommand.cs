using Neuron.Modules.Commands;
using Neuron.Modules.Commands.Command;
using Synapse3.SynapseModule.Permissions;

namespace Synapse3.SynapseModule.Command.SynapseCommands;

[SynapseRaCommand(
    CommandName = "Permission",
    Aliases = new[] {"pm", "perm", "perms", "permissions"},
    Description = "A command to manage the permission system",
    Platforms = new[] { CommandPlatform.ServerConsole, CommandPlatform.RemoteAdmin }
)]
public class PermissionCommand : SynapseCommand
{
    private readonly PermissionService _permission;

    public PermissionCommand(PermissionService permission)
    {
        _permission = permission;
    }

    public override void Execute(SynapseContext context, ref CommandResult result)
    {
        if (context.Arguments.Length < 1) context.Arguments = new[] { "" };

        switch (context.Arguments[0].ToUpper())
        {
            case "ME":
                var group = context.Player.SynapseGroup;
                result.Response = "Your " + group.GetCompressiveInfo();
                break;

            case "GROUP":
                if (!context.Player.HasPermission("synapse.permission.groups"))
                {
                    result.Response = "You don't have permission to get all groups (synapse.permission.groups)";
                    result.StatusCode = CommandStatusCode.Forbidden;
                    break;
                }

                if (context.Arguments.Length < 2)
                {
                    result.Response = "Missing group name";
                    result.StatusCode = CommandStatusCode.BadSyntax;
                    break;
                }

                var groupInfo = context.Arguments[1];
                result.Response = groupInfo + " " + _permission.GetCompressiveInfo(groupInfo);
                break;

            case "GROUPS":
                if (!context.Player.HasPermission("synapse.permission.groups"))
                {
                    result.Response = "You don't have permission to get all groups (synapse.permission.groups)";
                    result.StatusCode = CommandStatusCode.Forbidden;
                    break;
                }

                var msg = "All Groups:";
                foreach (var pair in _permission.Groups)
                    msg += $"\n{pair.Key} Badge: {pair.Value.Badge}";

                result.Response = msg;
                break;

            case "SETGROUP":
                if (!context.Player.HasPermission("synapse.permission.setgroup"))
                {
                    result.Response = "You don't have permission to set groups (synapse.permission.setgroup)";
                    result.StatusCode = CommandStatusCode.Forbidden;
                    break;
                }

                if (context.Arguments.Length < 3)
                {
                    result.Response = "Missing parameters";
                    result.StatusCode = CommandStatusCode.BadSyntax;
                    break;
                }

                var playerid = context.Arguments[2];

                if (context.Arguments[1] == "-1")
                {
                    _permission.RemovePlayerGroup(playerid);
                    result.Response = $"Removed {playerid} player group.";
                    break;
                }

                var setGroup = context.Arguments[1];
                try
                {
                    if (_permission.AddPlayerToGroup(setGroup, playerid))
                    {
                        result.Response = $"Set {playerid} player group to {setGroup}.";
                        break;
                    }

                    result.Response = "Invalid UserID or GroupName";
                    result.StatusCode = CommandStatusCode.BadSyntax;
                }
                catch
                {
                    result.Response = "Invalid GroupName";
                    result.StatusCode = CommandStatusCode.BadSyntax;
                }

                break;

            case "DELETE":
                if (!context.Player.HasPermission("synapse.permission.delete"))
                {
                    result.Response = "You don't have permission to delete groups (synapse.permission.delete)";
                    result.StatusCode = CommandStatusCode.Forbidden;
                    break;
                }

                if (context.Arguments.Length < 2)
                {
                    result.Response = "Missing group name";
                    result.StatusCode = CommandStatusCode.BadSyntax;
                }

                if (_permission.DeleteServerGroup(context.Arguments[1]))
                {
                    result.Response = "Group successfully deleted";
                }
                else
                {
                    result.Response = "No Group with that Name was found";
                    result.StatusCode = CommandStatusCode.BadSyntax;
                }
                break;

            default:
                result.Response = "All Permission Commands:" +
                                  "\nPermission me - Gives you information about your Role" +
                                  "\nPermission group {Group} - Gives you information about a Group" + 
                                  "\nPermission groups - Gives you a List of All Groups" +
                                  "\nPermission setgroup {Group} {UserID} - Sets a User group" +
                                  "\nPermission delete {Group}";
                break;
        }
    }
}