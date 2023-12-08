using System;
using System.ComponentModel;
using System.Diagnostics.SymbolStore;
using Mono.Cecil.Cil;
using Neuron.Modules.Configs;
using NLua;

namespace Synapse3.SynapseModule.LuaScript;

public class SynapseLuaScript
{
    private LuaFunction _enableFunction;
    private LuaFunction _disableFunction;
    public ConfigContainer Config { get; set; }

    public ScriptInfo Info { get; private set; }

    public bool Enabled { get; private set; }

    public SynapseLuaScript(ConfigContainer config, LuaFunction enable, LuaFunction disable)
    {
        Info = config.Get<ScriptInfo>();
        Config = config;
        _enableFunction = enable;
        _disableFunction = disable;
    }

    public void Enable()
    {
        try
        {
            if (!Enabled)
                _enableFunction.Call();
        }
        catch (Exception e)
        {
            var inner = e.InnerException;
            if (inner != null)
                SynapseLogger<SynapseLuaScript>.Error($"Error while enabling script {Info.Name}\n{e}\nInner:\n{inner}");
            else
                SynapseLogger<SynapseLuaScript>.Error($"Error while enabling script {Info.Name}\n{e}");
        }
    }

    public void Disable()
    {
        try
        {
            if (Enabled)
                _disableFunction.Call();
        }
        catch (Exception e)
        {
            var inner = e.InnerException;
            if (inner != null) 
                SynapseLogger<SynapseLuaScript>.Error($"Error while desabling script {Info.Name}\n{e}\nInner:\n{inner}");
            else
                SynapseLogger<SynapseLuaScript>.Error($"Error while desabling script {Info.Name}\n{e}");
        }
    }
}
