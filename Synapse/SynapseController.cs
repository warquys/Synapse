﻿using System;
using CommandSystem.Commands;
using Harmony;
using Synapse.Api.Plugin;
using Synapse.Command;

public class SynapseController
{
    private static bool IsLoaded = false;

    public static bool EnableDatabase = true;

    public static Synapse.Server Server { get; } = new Synapse.Server();

    public static PluginLoader PluginLoader { get; } = new PluginLoader();

    public static Handlers CommandHandlers { get; } = new Handlers();

    public static void Init()
    {
        ServerConsole.AddLog("SynapseController has been invoked", ConsoleColor.Cyan);
        if (IsLoaded) return;
        IsLoaded = true;
        var synapse = new SynapseController();
    }

    internal SynapseController()
    {
        CustomNetworkManager.Modded = true;
        BuildInfoCommand.ModDescription = "A heavily modded server software using extensive runtime patching to make development faster and the usage more accessible to end-users";
        
        PatchMethods();
        Server.Configs.Init();
        PluginLoader.ActivatePlugins();

        Server.Logger.Info("Synapse is now Ready!");
    } 
    
    private void PatchMethods()
    {
        try
        {
            var instance = HarmonyInstance.Create("Synapse.patches");
            instance.PatchAll();
            Server.Logger.Info("Harmony Patching was sucessfully!");
        }
        catch(Exception e)
        {
            Server.Logger.Error($"Harmony Patching throw an Error:\n\n {e}");
        }
    }

    public const int SynapseMajor = 2;
    public const int SynapseMinor = 0;
    public const int SynapsePatch = 0;
    public const string SynapseVersion = "2.0.0";
}
