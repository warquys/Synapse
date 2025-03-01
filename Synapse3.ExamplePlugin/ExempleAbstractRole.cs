using Neuron.Core.Meta;
using PlayerRoles;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Map.Rooms;
using Synapse3.SynapseModule.Player;
using Synapse3.SynapseModule.Role;
using UnityEngine;

namespace Synapse3.ExamplePlugin;

[Automatic]
[Role(
    Name = "ExampleAbstractRole",
    Id = 61,
    TeamId = 15
)]
public class ExampleAbstractRole : SynapseAbstractRole
{
    private readonly ExamplePlugin _plugin;

    protected override bool LowerRank(SynapsePlayer player) => CanSeeUnit(player) && player.RoleID != 61;
    protected override bool HigherRank(SynapsePlayer player) => false;
    protected override bool SameRank(SynapsePlayer player) => player.RoleID == 61;

    protected override bool CanSeeUnit(SynapsePlayer player) => player.TeamID == 15;

    public ExampleAbstractRole(ExamplePlugin plugin)
    {
        _plugin = plugin;
    }

    protected override IAbstractRoleConfig GetConfig() => _plugin.Config.AbstractRoleConfig;

    public class Config : IAbstractRoleConfig
    {
        public RoleTypeId Role => RoleTypeId.NtfCaptain;

        public RoleTypeId VisibleRole => RoleTypeId.ChaosConscript;

        public RoleTypeId OwnRole => RoleTypeId.NtfSergeant;

        public bool CustomDisplay => true;
        public bool Hierarchy => true;
        public bool UseCustomUnitName => true;
        public string CustomUnitName => "Special Unit 11";

        public SerializedVector3 Scale => Vector3.one;

        public uint EscapeRole { get; set; } = (uint)RoleTypeId.Spectator;

        public float Health { get; set; } = 200;

        public float MaxHealth { get; set; } = 220;

        public float ArtificialHealth { get; set; } = 0;

        public float MaxArtificialHealth { get; set; } = 100;

        public RoomPoint[] PossibleSpawns { get; set; } = new RoomPoint[]
        {
            new RoomPoint("Surface", new Vector3(8.5875f, -7.672f, -40.53928f), Vector3.zero),
            new RoomPoint("Surface", new Vector3(5.3678f, -7.672f, -40.53928f), Vector3.zero),
        };

        public SerializedPlayerInventory[] PossibleInventories { get; set; } = new SerializedPlayerInventory[]
        {
            new SerializedPlayerInventory()
        };
    }
}