using PluginAPI.Core;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Database;
using System.Data;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Map.Rooms;
using Synapse3.SynapseModule.Map;
using Synapse3.SynapseModule.Permissions;
using Synapse3.SynapseModule.Role;
using Synapse3.SynapseModule.Teams;

namespace Synapse3.SynapseModule.Player;

public class SynapseServerPlayer : SynapsePlayer
{
    private readonly ServerEvents _serverEvents;
    
    public SynapseServerPlayer() : base()
    {
        _serverEvents = Synapse.Get<ServerEvents>();
    }

    /// <inheritdoc cref="SynapsePlayer.IsServer"/>
    public override PlayerType PlayerType => PlayerType.Server;

    public override void Awake()
    {
        Hub = GetComponent<ReferenceHub>();
        GameConsoleTransmission = GetComponent<GameConsoleTransmission>();
        BroadcastController = GetComponent<global::Broadcast>();

        Synapse.Get<PlayerService>().Host = this;
    }

    //Don't Remove this it's a little bit more optimised this way
    public override void OnDestroy() { }

    private void OnApplicationQuit()
    {
        _serverEvents.StopServer.Raise(new StopServerEvent());
    }

    public override TTranslation GetTranslation<TTranslation>(TTranslation translation) => translation.Get();
}