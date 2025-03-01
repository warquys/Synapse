﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using PlayerRoles;
using Syml;

namespace Synapse3.SynapseModule.Config;

/// <summary>
/// The Synapse Configuration Section for all GamePlay related stuff
/// </summary>
[Serializable]
[DocumentSection("GamePlay")]
public class GamePlayConfiguration : IDocumentSection
{
    [Description("If enabled everyone can attack everyone after the Round ended")]
    public bool AutoFriendlyFire { get; set; } = true;

    [Description("If enabled a Player don't need to equip his keycard to use it")]
    public bool RemoteKeyCard{ get; set; } = false;

    [Description("If enabled the Warhead button can be closed again ")]
    public bool WarheadButtonClosable { get; set; } = false;
    
    [Description("All Scp's in this list are able to Speak to Humans")]
    public List<uint> SpeakingScp { get; set; } = new List<uint>
    { 
        
    };

    [Description("When enabled Spectators will hear the SCP Voice chat upon selecting a SCP")]
    public bool SpectatorListenOnSCPs { get; set; } = true;

    [Description("When enabled are Chaos and SCP's forced to kill each other since otherwise the Round won't end")]
    public bool ChaosAndScpEnemy { get; set; } = false;

    [Description("Every Role in this List won't stop SCP-173 from moving when the player is looking at it")]
    public List<uint> CantObserve173 { get; set; } = new()
    {
        (uint)RoleTypeId.Scp173,
        (uint)RoleTypeId.Scp106,
        (uint)RoleTypeId.Scp049,
        (uint)RoleTypeId.Scp079,
        (uint)RoleTypeId.Scp096,
        (uint)RoleTypeId.Scp0492,
        (uint)RoleTypeId.Scp939,
        (uint)RoleTypeId.Tutorial
    };

    public List<uint> CantObserve096 { get; set; } = new()
    {
        (uint)RoleTypeId.Scp173,
        (uint)RoleTypeId.Scp106,
        (uint)RoleTypeId.Scp049,
        (uint)RoleTypeId.Scp079,
        (uint)RoleTypeId.Scp096,
        (uint)RoleTypeId.Scp0492,
        (uint)RoleTypeId.Scp939,
        (uint)RoleTypeId.Tutorial
    };

    [Description("If enabled when an ntf handcuffs a player other than class D he can evacuate and become a cadet same for the chaos")]
    public bool AnyRoleCuffedJoinEnemy { get; set; } = true;
    public bool GuardEscape { get; set; } = true;

    [Description("If enabled, 106 can caputure like beffor 1.13")]
    public bool OldScp106Attack { get; set; } = false;

}