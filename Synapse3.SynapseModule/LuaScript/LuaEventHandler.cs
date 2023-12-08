using System;
using System.Reflection;
using Neuron.Core.Events;
using NLua;

namespace Synapse3.SynapseModule.LuaScript;

public class LuaEventHandler
{
    private static readonly MethodInfo methodInfo = typeof(LuaEventHandler).GetMethod(nameof(UnsafeHandle));

    public readonly LuaFunction function;
    public readonly int priority;
    public readonly IEventReactor reactor;

    public object EventHandler { get; private set; }
    
    public LuaEventHandler(LuaFunction function, int priority, IEventReactor reactor)
    {
        this.reactor = reactor;
        this.function = function;
        this.priority = priority;
    }

    public void Unregister()
    {
        if (EventHandler == null) return;
        reactor.UnsubscribeUnsafe(EventHandler);
        EventHandler = null;
    }

    public void Register()
    {
        if (EventHandler != null) return;
        EventHandler = reactor.SubscribeUnsafe(this, methodInfo, priority);
    }

    public void UnsafeHandle(object ev)
    {
        try
        {
            function.Call(ev);
        }
        catch (Exception e)
        {
            SynapseLogger<LuaEventHandler>.Error($"Error while handling event {ev}:\n{e}");
        }
    }

}
