using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Neuron.Core;
using Neuron.Core.Events;
using Neuron.Core.Meta;
using Neuron.Modules.Configs;
using NLua;
using NLua.Exceptions;
using Synapse3.SynapseModule.Events;

namespace Synapse3.SynapseModule.LuaScript;

public class LuaService : Service
{
    private readonly ServerEvents _server;
    private readonly NeuronBase _neuronBase;
    private readonly ConfigService _config;
    private readonly RoundEvents _round;

    public Lua Lua { get; private set; }

    private List<SynapseLuaScript> _scripts = new List<SynapseLuaScript>();
    public ReadOnlyCollection<SynapseLuaScript> Scripts => _scripts.AsReadOnly();

    public LuaService(ServerEvents server, RoundEvents round, NeuronBase neuronBase, ConfigService config)
    {
        _server = server;
        _neuronBase = neuronBase;
        _config = config;
        _round = round;
    }

    public bool Add(SynapseLuaScript luaScript)
    {
        if (_scripts.Contains(luaScript))
            return false;
        _scripts.Add(luaScript);
        return true;
    }

    public bool Remove(SynapseLuaScript luaScript)
        => _scripts.Remove(luaScript);

    public override void Enable()
    {
        _server.Reload.Subscribe(Reload);
        _round.Waiting.Subscribe(Whaiting, int.MaxValue);
        LoadScripts();
    }

    public override void Disable()
    {
        _server.Reload.Unsubscribe(Reload);
        _round.Waiting.Unsubscribe(Whaiting);
        UnLoadScripts();
    }

    private void Reload(ReloadEvent ev)
    {
        UnLoadScripts();
        LoadScripts();
        foreach (var script in _scripts)
        {
            if (script.Info.Enable)
                script.Enable();
        }
    }

    private void Whaiting(RoundWaitingEvent ev)
    {
        SynapseLogger<LuaService>.Info("Enabling Lua Scripts");
        if (!ev.FirstTime) return;
        foreach (var script in _scripts)
        {
            if (script.Info.Enable)
                script.Enable();
        }
    }

    internal void UnLoadScripts()
    {
        foreach (var script in _scripts)
        {
            script.Disable();
        }


        _scripts.Clear();

        Lua.Dispose();
        Lua = null;
    }

    internal void LoadScripts()
    {
        Lua = new Lua();
        Lua.LoadCLRPackage();

        foreach (var service in Synapse.Get<ServiceManager>().Services)
        {
            Lua[service.ToString()] = Synapse.Get(service.ServiceType);
        }

        LuaRegistrationHelper.TaggedStaticMethods(Lua, typeof(LuaService));

        var path = _neuronBase.PrepareRelativeDirectory("Scripts");
        var scriptsPaths = Directory.GetDirectories(path);

        foreach (var scriptPath in scriptsPaths)
            LoadScript(scriptPath);
    }

    public SynapseLuaScript LoadScript(string scriptPath)
    {
        try
        {
            var enableLuaPath = $"{scriptPath}/Enable.lua";
            var disableLuaPath = $"{scriptPath}/Disable.lua";
            if (!File.Exists(enableLuaPath))
                File.WriteAllText(enableLuaPath, "-- Enable.lua : Call when the script is enable");
            if (!File.Exists(disableLuaPath))
                File.WriteAllText(disableLuaPath, "-- Disable.lua : Call when the script is disable");
            var enable = Lua.LoadFile(enableLuaPath);
            var disable = Lua.LoadFile(disableLuaPath);
            var config = _config.GetContainer($"{scriptPath}/Config.syml");

            var script = new SynapseLuaScript(config, enable, disable);

            Add(script);
            return script;
        }
        catch (Exception e)
        {
            SynapseLogger<LuaService>.Error($"Fail to load \"{scriptPath}\"\n" + e);
            return null;
        }
    }

    // Lua fonctions
    [LuaMember(Name = "logInfo")]
    public static void LogInfo(object msg)
        => SynapseLogger<LuaService>.Info(msg);

    [LuaMember(Name = "logWarn")]
    public static void LogWarn(object msg)
        => SynapseLogger<LuaService>.Warn(msg);

    [LuaMember(Name = "logError")]
    public static void LogError(object msg)
        => SynapseLogger<LuaService>.Error(msg);

    [LuaMember(Name = "logFatal")]
    public static void LogFatal(object msg)
        => SynapseLogger<LuaService>.Fatal(msg);

    [LuaMember(Name = "logDebug")]
    public static void LogDebug(object msg)
        => SynapseLogger<LuaService>.Debug(msg);

    [LuaMember(Name = "registerEventHandler")]
    public static LuaEventHandler RegisterEventHandler(string eventName, LuaFunction function, int priority = 0)
    {
        var eventManger = Synapse.Get<EventManager>();
        var reactor = eventManger.Reactors.FirstOrDefault(p => p.Key.Name == eventName).Value;
        if (reactor == null)
            reactor = eventManger.Reactors.FirstOrDefault(p => p.Key.FullName == eventName).Value;

        if (reactor == null)
            return null;

        var handler = new LuaEventHandler(function, priority, reactor);
        handler.Register();
        return handler;
    }
}