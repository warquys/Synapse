using System;
using Mirror;
using Synapse3.SynapseModule.Enums;
using UnityEngine;

namespace Synapse3.SynapseModule.Player;

public interface ISynapseTextHint
{
    HintSide Side { get; }
    int Size { get; }
    string Text { get; }

    bool Displayed { get; }
    bool Displaying { get; }
    bool Expired { get; }
    float DisplayTime { get; }
    float NextRefresh { get; }
    bool IgnoreParsing { get; }
    int Ligne { get; }
    int Priority { get; }


    void EndDisplay();
    void ResetDisplay();
    void StartDisplay();
}


public class ConstantTextHint : ISynapseTextHint
{
    private bool finish;

    public HintSide Side { get; }
    public int Size { get; }
    public int Priority { get; }

    public bool Displayed { get; internal set; }
    public bool Displaying { get; internal set; }
    public bool Expired => finish;
    public float NextRefresh => finish ? 0 : 1;
    public float DisplayTime => float.MaxValue;
    public string Text { get; set; }
    public int Ligne { get; set; }
    public bool IgnoreParsing { get; set; }

    /// <summary>
    /// Do not use size, ligne or space balise!
    /// \n is not suported create a new TextHint instead
    /// </summary>
    /// <param name="ligne">0 is the top one, the max index is <c><see cref="TextHintList.MaxLigneSize1"/> - 1</c></param>
    public ConstantTextHint(int ligne, string text, HintSide side, int size = 1, int priority = 500)
    {
        Size = Math.Max(Math.Min(size, 3), 1);
        Ligne = Math.Max(Math.Min(ligne, TextHintList.MaxLigneSize1 - 1), 0);
        Side = side;
        Priority = priority;
        Text = text.Replace("\n", "");

    }

    public void EndDisplay()
    {
        finish = true;
        Displayed = true;
        Displaying = false;
    }

    public void StartDisplay()
    {
        finish = false;
        Displaying = true;
        Displayed = false;
    }

    public void ResetDisplay()
    {
        Displayed = false;
        Displaying = false;
        finish = false;
    }

}

public class SynapseTextHint : ISynapseTextHint
{
    private bool forceFinish;

    public HintSide Side { get; }
    public int Size { get; }
    public int Priority { get; }

    public bool Displayed { get; internal set; }
    public bool Displaying { get; internal set; }
    public bool Expired => !forceFinish && Displaying && NextRefresh <= 0;
    
    private float _displayRemoveTime;
    public float NextRefresh => _displayRemoveTime - (float)NetworkTime.time;
    public float DisplayTime { get; set; }
    public string Text { get; set; }
    public int Ligne { get; set; }
    public bool IgnoreParsing { get; set; }

    /// <inheritdoc cref="ConstantTextHint(int, string, HintSide, int, int)"/>
    public SynapseTextHint(int ligne, string text, float displayTime, HintSide side, int size = 1, int priority = 0)
    {
        DisplayTime = displayTime;
        Size = Mathf.Clamp(TextHintList.SizeMin, size, TextHintList.SizeMax);
        Ligne = Mathf.Clamp(TextHintList.MaxLigneSize1 - 1, ligne, 0);
        Side = side;
        Priority = priority;
        Text = text.Replace("\n", "");

    }

    public void EndDisplay()
    {
        Displayed = true;
        Displaying = false;
        forceFinish = true;
    }

    public void StartDisplay()
    {
        forceFinish = false;
        _displayRemoveTime = (float)NetworkTime.time + DisplayTime;
        Displaying = true;
        Displayed = false;
    }

    public void ResetDisplay()
    {
        forceFinish = false;
        Displayed = false;
        Displaying = false;
    }
}
