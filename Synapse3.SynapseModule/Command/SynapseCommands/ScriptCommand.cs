using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neuron.Core.Plugins;
using Neuron.Modules.Commands;
using Neuron.Modules.Commands.Command;
using PluginAPI.Core;
using Synapse3.SynapseModule.LuaScript;

namespace Synapse3.SynapseModule.Command.SynapseCommands;

[SynapseRaCommand(
    CommandName = "Scripts",
    Aliases = new[] { "lua", "LuaScripts" },
    Parameters = new[] { "ScriptName", "(Enable/Disable/Reload)" },
    Permission = "synapse.command.scripts",
    Description = "A command to get info and manage script",
    Platforms = new[] { CommandPlatform.ServerConsole, CommandPlatform.RemoteAdmin, CommandPlatform.ServerConsole }
)]
public class ScriptCommand : SynapseCommand
{
    private readonly LuaService _lua;

    public ScriptCommand(LuaService lua)
    {
        _lua = lua;
    }

    public override void Execute(SynapseContext context, ref CommandResult result)
    {
        if (context.Arguments.Length < 1)
        {
            result.Response = "All Scripts:";

            foreach (var plugi in _lua.Scripts)
                result.Response += $"\n{plugi.Info.Name} Version: {plugi.Info.Version} by {plugi.Info.Author}";

            return;
        }

        string scriptName;
        SynapseLuaScript luaScript;
        switch (context.Arguments.Last().ToUpper())
        {
            case "ENABLE":
                if (context.Player.HasPermission("synapse.command.scripts.manage"))
                    goto NotManagePermision;
                scriptName = string.Join(" ", context.Arguments.Take(context.Arguments.Length - 1));
                luaScript = _lua.Scripts.FirstOrDefault(p => p.Info.Name == scriptName);
                if (luaScript == null)
                    goto NotFount;
                ConfigEnable(luaScript);
                result.Response = "The script is enable until it is set to re-enable or this command is re-executed with \"Disable\"";
                break;

            case "DISABLE":
                if (context.Player.HasPermission("synapse.command.scripts.manage"))
                    goto NotManagePermision; 
                scriptName = string.Join(" ", context.Arguments.Take(context.Arguments.Length - 1));
                luaScript = _lua.Scripts.FirstOrDefault(p => p.Info.Name == scriptName);
                if (luaScript == null)
                    goto NotFount;
                ConfigDisable(luaScript);
                result.Response = "The script is disabled until it is set to re-enable or this command is re-executed with \"Enable\"";
                break;

            case "RELOAD":
                if (context.Player.HasPermission("synapse.command.scripts.manage"))
                    goto NotManagePermision;
                scriptName = string.Join(" ", context.Arguments.Take(context.Arguments.Length - 1));
                luaScript = _lua.Scripts.FirstOrDefault(p => p.Info.Name == scriptName);
                if (luaScript == null)
                    goto NotFount;
                if (!luaScript.Enabled)
                {
                    result.Response = "The script need to be first enable beffor reload it";
                    result.StatusCode = CommandStatusCode.Error;
                    return;
                }

                luaScript.Disable();
                luaScript.Enable();
                break;

            default:
                scriptName = string.Join(" ", context.Arguments);
                luaScript = _lua.Scripts.FirstOrDefault(p => p.Info.Name == scriptName);
                if (luaScript == null)
                    goto NotFount;
                result.Response = $"\n{luaScript.Info.Name}" +
                  $"\n    - Description: {SplitDescription(luaScript.Info.Description, context.Platform)}" +
                  $"\n    - Author: {luaScript.Info.Author}" +
                  $"\n    - Version: {luaScript.Info.Version}" +
                  $"\n    - Enable: {luaScript.Enabled}" +
                  $"\n    - Config Enable: {luaScript.Info.Enable}";
                break;
        };
        return;

    NotFount:
        result.Response = $"Script call \"{scriptName}\" not found";
        result.StatusCode = CommandStatusCode.NotFound;
        return;

    NotManagePermision:
        result.Response = "You don't have permission to enable/disable scripts (synapse.command.scripts.manage)";
        result.StatusCode = CommandStatusCode.Forbidden;
        return;
    }

    private void ConfigDisable(SynapseLuaScript luaScript)
    {
        luaScript.Disable();
        luaScript.Info.Enable = false;
        luaScript.Config.Document.Set(luaScript.Info.Name, luaScript.Info);
        luaScript.Config.Store();
    }

    private void ConfigEnable(SynapseLuaScript luaScript)
    {
        luaScript.Enable();
        luaScript.Info.Enable = true;
        luaScript.Config.Document.Set(luaScript.Info.Name, luaScript.Info);
        luaScript.Config.Store();
    }

    private string SplitDescription(string message, CommandPlatform platform)
    {
        var count = 0;
        var msg = "";

        foreach (var word in message.Split(' '))
        {
            count += word.Length;

            if (count > _maxLetters[platform])
            {
                msg += "\n                ";
                count = 0;
            }

            if (msg == string.Empty)
            {
                msg += word;
            }
            else
            {
                msg += " " + word;
            }
        }

        return msg;
    }

    private readonly Dictionary<CommandPlatform, int> _maxLetters = new()
    {
        { CommandPlatform.PlayerConsole, 50 },
        { CommandPlatform.RemoteAdmin, 50 },
        { CommandPlatform.ServerConsole, 75 }
    };
}
