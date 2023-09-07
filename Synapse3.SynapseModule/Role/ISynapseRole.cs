using System.Collections.Generic;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Role;

public interface ISynapseRole
{
    SynapsePlayer Player { get; set; }
    
    RoleAttribute Attribute { get; set; }

    void Load();

    List<uint> GetFriendsID();

    List<uint> GetEnemiesID();

    /// <summary>
    /// Try Escape is call when the player with this custom role try to escape.
    /// But if the player is cuffed by a player which belong to a custom Team allowing the escape of players. 
    /// The custom Team get the charge of the escape, and this method is not called !
    /// </summary>
    void TryEscape();

    void SpawnPlayer(ISynapseRole previousRole, bool spawnLite);

    void DeSpawn(DeSpawnReason reason);
}