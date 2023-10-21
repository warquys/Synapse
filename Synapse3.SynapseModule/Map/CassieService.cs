using System.Collections.Generic;
using System.Linq;
using Neuron.Core.Meta;
using Respawning;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Patching.Patches;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Map;

public class CassieService : Service
{
    // This character is visually like a space but is not considered by SCP sl as a space
    public const char SpecialSpace = ' ';
    
    private readonly PlayerService _player;
    private readonly MapEvents _mapEvents;

    public CassieService(PlayerService player, MapEvents mapEvents)
    {
        _player = player;
        _mapEvents = mapEvents;
    }
    
    public void Announce(string message)
    {
        RespawnEffectsController.PlayCassieAnnouncement(message, true, false, true);
    }

    public void Annonce(List<CassieSentence> message)
    {
        var ev = new CassieMessageEvent(message, CassieSettings.DisplayText, CassieSettings.Break);
        _mapEvents.CassieMessage.Raise(ev);
        string words = string.Join(string.Empty, message);
        AnnounceWithoutEvent(words);
    }

    public void Annonce(List<CassieSentence> message, params CassieSettings[] settings)
    {
        var ev = new CassieMessageEvent(message, settings);
        _mapEvents.CassieMessage.Raise(ev);
        if (!ev.Allow) return;

        string words = string.Join(string.Empty, message);
        AnnounceWithoutEvent(words, 0.3f, 0.2f, ev.Settings.ToArray());
    }

    public void Announce(string message, params CassieSettings[] settings)
        => Announce(message, 0.3f, 0.2f, settings);

    public void Announce(string message, float glitchChance, float jamChance, params CassieSettings[] settings)
    {
        if (settings.Contains(CassieSettings.Glitched))
            message = Glitch(message, glitchChance, jamChance);
        
        RespawnEffectsController.PlayCassieAnnouncement(message, settings.Contains(CassieSettings.Break),
            settings.Contains(CassieSettings.Noise), settings.Contains(CassieSettings.DisplayText));
    }

    /// <summary>
    /// Don't raise <see cref="MapEvents.CassieMessage"/>
    /// </summary>
    public void AnnounceWithoutEvent(string message, float glitchChance, float jamChance, params CassieSettings[] settings)
    {
        CassieMessagePatch.ActivePatch = false;
        Announce(message, glitchChance, jamChance, settings);
        CassieMessagePatch.ActivePatch = true;
    }

    /// <inheritdoc cref="AnnounceWithoutEvent"/>
    public void AnnounceWithoutEvent(string message, params CassieSettings[] settings)
        => AnnounceWithoutEvent(message, 0.3f, 0.2f, settings);

    /// <inheritdoc cref="AnnounceWithoutEvent"/>
    public void AnnounceWithoutEvent(string message)
    {
        CassieMessagePatch.ActivePatch = false;
        Announce(message);
        CassieMessagePatch.ActivePatch = true;
    }

    /// <inheritdoc cref="AnnounceWithoutEvent"/>
    public void AnnonceWithoutEvent(List<CassieSentence> message)
    {
        string words = string.Join(string.Empty, message);
        AnnounceWithoutEvent(words, CassieSettings.DisplayText, CassieSettings.Break);
    }

    /// <inheritdoc cref="AnnounceWithoutEvent"/>
    public void AnnonceWithoutEvent(List<CassieSentence> message, params CassieSettings[] settings)
    {
        string words = string.Join(string.Empty, message);
        AnnounceWithoutEvent(words, 0.3f, 0.2f, settings);
    }

    public void AnnounceScpDeath(string scp, params CassieSettings[] settings)
        => AnnounceScpDeath(scp, ScpContainmentType.Unknown, "Unknown", 0.3f, 0.2f, settings);
    
    public void AnnounceScpDeath(string scp, ScpContainmentType type, string unit = "Unknown",
        float glitchChance = 0.3f, float jamChance = 0.2f, params CassieSettings[] settings)
    {
        var chars = scp.ToArray();
        scp = string.Empty;
        foreach (var key in chars)
        {
            scp += key + " ";
        }

        var message = type switch
        {
            ScpContainmentType.Tesla => $". SCP {scp} SUCCESSFULLY TERMINATED BY AUTOMATIC SECURITY SYSTEM",
            ScpContainmentType.Nuke => $". SCP {scp} SUCCESSFULLY TERMINATED BY ALPHA WARHEAD",
            ScpContainmentType.Decontamination => $". SCP {scp} LOST IN DECONTAMINATION SEQUENCE",
            ScpContainmentType.Mtf => $". SCP {scp} SUCCESSFULLY TERMINATED . CONTAINEDSUCCESSFULLY CONTAINMENTUNIT {unit}",
            ScpContainmentType.Chaos => $". SCP {scp} CONTAINEDSUCCESSFULLY BY CHAOSINSURGENCY",
            ScpContainmentType.Scientist => $". SCP {scp} CONTAINEDSUCCESSFULLY BY SCIENCE PERSONNEL",
            ScpContainmentType.ClassD => $". SCP {scp} CONTAINEDSUCCESSFULLY BY CLASSD PERSONNEL",
            ScpContainmentType.Scp => $"TERMINATED BY SCP {unit}",
            ScpContainmentType.Unknown => $". SCP {scp} SUCCESSFULLY TERMINATED . CONTAINMENTUNIT UNKNOWN",
            _ => $". SCP {scp} SUCCESSFULLY TERMINATED . TERMINATION CAUSE UNSPECIFIED",
        };

        Announce(message, glitchChance, jamChance, settings);
    }

    public float Duration(string message, bool rawNumber = false, float speed = 1f)
        => NineTailedFoxAnnouncer.singleton.CalculateDuration(message, rawNumber, speed);

    public void Broadcast(ushort time, string message)
    {
        foreach (var player in _player.Players)
        {
            player.SendBroadcast(message, time);
        }
    }

    public string Glitch(string message, float glitchChance, float jamChance)
    {
        var oldWords = message.Split(' ');
        var newWords = new List<string>();

        foreach (var word in oldWords)
        {
            newWords.Add(word);

            if (Random.value < glitchChance)
            {
                newWords.Add(".G" + Random.Range(1, 7));
            }

            if (Random.value < jamChance)
            {
                newWords.Add($"JAM_{Random.Range(0, 70):000}_{Random.Range(2, 6)}");
            }
        }

        message = string.Empty;
        foreach (var word in newWords)
        {
            message += word + " ";
        }
        return message;
    }
}

public struct CassieSentence
{
    public string Message { get; set; }

    public string Translation { get; set; }

    public string AsCassie()
    {
        var translation = Translation.Replace(' ', CassieService.SpecialSpace);
        return $"<size=0>__</size>{translation}<size=0> {Message} </size>";
    }

    public override string ToString()
    {
        return AsCassie();
    }
}