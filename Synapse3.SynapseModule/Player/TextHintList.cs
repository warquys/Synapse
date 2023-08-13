using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Hints;
using MEC;
using Synapse3.SynapseModule.Enums;

namespace Synapse3.SynapseModule.Player;

/// <summary>
/// Do not use Rich Text 
/// </summary>
public class TextHintList
    : ICollection<SynapseTextHint>
{
    #region Properties & Variables
    internal static Dictionary<int, int> SizeMaxSide = new Dictionary<int, int>()
    {
        { 1, 40 },//Total per ligne is 81 
        { 2, 21 },//Total per ligne is 43
        { 3, 15 },//Total per ligne is 31
    };

    internal static Dictionary<int, float> SizeMspace = new Dictionary<int, float>()
    {
        { 1, 0.85f },
        { 2, 1.60f },
        { 3, 2.20f }
    };

    internal const string BaliseMatchRegex = "<.*?>";

    public const int MaxLigne = 54;
    public const int GosthLigne = 15;

    readonly SynapsePlayer _player;
    readonly List<SynapseTextHint> _textHints = new List<SynapseTextHint>();
    
    public ReadOnlyCollection<SynapseTextHint> TextHints => _textHints.AsReadOnly();
    public int Count => TextHints.Count;
    public bool IsReadOnly => false;

    private CoroutineHandle updateCallBack;
    #endregion

    #region Constructor & Destructor
    public TextHintList(SynapsePlayer player)
    {
        _player = player;
    }

    #endregion

    #region Methods
    public void UpdateText()
    {
        Timing.KillCoroutines(updateCallBack);
        RemoveExpierd();

        var hintsCount = _textHints.Count;
        if (hintsCount == 0) return;
        
        var lignes = new Line[MaxLigne];
        for (int i = 0; i < MaxLigne; i++)
            lignes[i] = new Line();

        foreach (var textHint in _textHints.OrderBy(p => p.Priority))
            ProcessHint(lignes, textHint);

        var displayTime = _textHints.OrderBy(p => p.DisplayTimeLeft).First(p => p.Displaying).DisplayTimeLeft + 0.01f;
        var playerHint = new TextHint(GetMessage(lignes), new HintParameter[]
        {
            new StringHintParameter("")
        }, null, displayTime);

        if (hintsCount == 1)
        { 
            playerHint._effects = HintEffectPresets.FadeInAndOut(displayTime);
        }

        _player.Hub.hints.Show(playerHint);
        updateCallBack = Timing.RunCoroutine(CallBackUpdateText(displayTime));
    }

    private void RemoveExpierd()
    {
        for (int i = _textHints.Count - 1; i >= 0; i--)
        {
            var hint = _textHints[i];
            if (!hint.Expired) continue;
            _textHints.RemoveAt(i);
            hint.EndDisplay();
        }
    }


    private void ProcessHint(Line[] lignes, SynapseTextHint hint)
    {
        if (!hint.Displaying) hint.StartDisplay();

        switch (hint.Side)
        {
            case HintSide.Left:
                ProcesseLeft(lignes, hint);
                break;
            case HintSide.Right:
                ProcesseRight(lignes, hint);
                break;
            case HintSide.Midle:
                ProcesseMidle(lignes, hint);
                break;
        }
    }

    private void ProcesseMidle(Line[] lignes, SynapseTextHint hint)
    {
        var textToInsert = hint.IgnoreParsing ?
            new List<AnalysedSide>() { new AnalysedSide(hint.Text, 0, true) } :
            TextSpliter.Splite(hint.Text, SizeMaxSide[hint.Size], 1);
        var ligneCount = textToInsert.Count;
        var ligne = hint.Ligne;
        var size = hint.Size;

        if (ligneCount * hint.Size >= MaxLigne) return;
        if (ligne - hint.Size + 1 < 0) return;

        for (int i = 0; i < ligneCount; i++)
        {
            var dest = ligne + i * size;
            if (!lignes[dest].MidleFree) return;
            for (int j = 1; j < size; j++)
            {
                if (lignes[dest - j].Left != null 
                    || lignes[dest - j].Right != null 
                    || lignes[dest - j].Midle != null)
                    return;
            }
        }

        for (int i = 0; i < ligneCount; i++)
        {
            var dest = ligne + i * size;
            textToInsert[i].SizeMultiplicator = hint.Size;
            lignes[dest].Midle = textToInsert[i];
            for (int j = 1; j < size; j++)
            {
                lignes[dest - j].Gosht = true;
            }

        }
    }
    
    private void ProcesseLeft(Line[] lignes, SynapseTextHint hint)
    {
        var textToInsert = hint.IgnoreParsing ?
            new List<AnalysedSide>() { new AnalysedSide(hint.Text, 0, true) } :
            TextSpliter.Splite(hint.Text, SizeMaxSide[hint.Size], 1);
        var ligneCount = textToInsert.Count;
        var ligne = hint.Ligne;
        var size = hint.Size;

        if (ligneCount * hint.Size >= MaxLigne) return;
        if (ligne - hint.Size + 1 < 0) return;
        
        for (int i = 0; i < ligneCount; i++)
        {
            var dest = ligne + i * size;
            if (!lignes[dest].LeftFree) return;
            for (int j = 1; j < size; j++)
            {
                if (lignes[dest - j].Left != null)
                    return;
            }
        }

        for (int i = 0; i < ligneCount; i++)
        {
            var dest = ligne + i * size;
            textToInsert[i].SizeMultiplicator = hint.Size; 
            lignes[dest].Left = textToInsert[i];
            for (int j = 1; j < size; j++)
            {
                lignes[dest - j].Gosht = true;
            }

        }
    }

    private void ProcesseRight(Line[] lignes, SynapseTextHint hint)
    {
        var textToInsert = hint.IgnoreParsing ?
            new List<AnalysedSide>() { new AnalysedSide(hint.Text, 0, true) } :
            TextSpliter.Splite(hint.Text, SizeMaxSide[hint.Size], 1);
        var ligneCount = textToInsert.Count;
        var ligne = hint.Ligne;
        var size = hint.Size;
        
        if (ligneCount * hint.Size >= MaxLigne) return;
        if (ligne - hint.Size + 1 < 0) return;

        for (int i = 0; i < ligneCount; i++)
        {
            var dest = ligne + i * size;
            if (!lignes[dest].RightFree) return;
            for (int j = 1; j < size; j++)
            {
                if (lignes[dest - j].Right != null) 
                    return;
            }
        }

        for (int i = 0; i < ligneCount; i++)
        {
            var dest = ligne + i * size;
            textToInsert[i].SizeMultiplicator = hint.Size;
            lignes[dest].Right = textToInsert[i];
            for (int j = 1; j < size; j++)
            {
                lignes[dest - j].Gosht = true;
            }

        }
    }

    private string GetMessage(Line[] Lignes)
    {
        //<mspace> allow to get char at same size
        //<size>   is the win space for more txt
        var message = "\n";
        for (int i = 0; i < MaxLigne; i++)
        {
            message += Lignes[i];
        }
        message += new string('\n', GosthLigne);

        return message;
    }

    #region List Methods
    private IEnumerator<float> CallBackUpdateText(float time)
    {
        yield return Timing.WaitForSeconds(Math.Min(time, 2));
        UpdateText();//I can't catch the hint of the client (max ammo, item and ect...) so i override them
        yield break;
    }

    public void AddWithoutUpdate(SynapseTextHint hint)
    {
        _textHints.Add(hint);
    }

    public void Add(SynapseTextHint hint)
    {
        _textHints.Add(hint);
        UpdateText();
    }

    public bool Remove(SynapseTextHint hint)
    {
        if (_textHints.Remove(hint))
        {
            UpdateText();
            return true;
        }
        return false;
    }

    public void Clear()
    {
        if (_textHints.Any())
        {
            _textHints.Clear();
            _player.Hub.hints.Show(new Hints.TextHint("", new HintParameter[]
            {
                new StringHintParameter("")
            }, null, 0.1f));
        }
    }

    public bool Contains(SynapseTextHint hint)
        => _textHints.Contains(hint);

    public void CopyTo(SynapseTextHint[] array, int arrayIndex)
        => _textHints.CopyTo(array, arrayIndex);

    public IEnumerator<SynapseTextHint> GetEnumerator()
        => _textHints.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => _textHints.GetEnumerator();

    #endregion

    #endregion

    #region Nesteds
    public class Line
    {
        // this space is use to sperate the left and right and get a max of 81 char (for the base unite of 1) per line
        internal const string SpaceAtSizeOne = "<mspace=0.65em><size=50%> </mspace></size>";

        public AnalysedSide Left { get; set; }
        public AnalysedSide Right { get; set; }
        public AnalysedSide Midle { get; set; }
        public bool LeftFree => !Gosht && Left == null && Midle == null;
        public bool RightFree => !Gosht && Right == null && Midle == null;
        public bool MidleFree => !Gosht && Left == null && Right == null && Midle == null;
        public bool Gosht { get; set; } = false;

        public override string ToString()
        {
            if (Gosht) return "";
            
            var text = "";
            if (Midle == null)
            {
                text += "<align=\"left\">";
                var leftText = Left;
                var rightText = Right;
                if (leftText != null)
                {
                    var charSpace = SizeMspace[(int)Left.SizeMultiplicator];
                    if (!leftText.IgnoreReformatage)
                        text += $"<mspace={charSpace}em><size={Left.SizeMultiplicator * 50}%>" + leftText;
                    else
                        text += leftText;
                }
                if (rightText != null)
                {
                    var charSpace = SizeMspace[(int)rightText.SizeMultiplicator];
                    var space = new string(' ', SizeMaxSide[(int)rightText.SizeMultiplicator] - rightText.TextWithoutTag.Length);

                    if (!rightText.IgnoreReformatage)
                    {
                        text += $"<pos=50%>{SpaceAtSizeOne}<mspace={charSpace}em><size={rightText.SizeMultiplicator * 50}%>";
                        text += space + rightText;
                    }
                    else
                    {
                        text += SpaceAtSizeOne + rightText;
                    }
                }
            }
            else
            {
                var midleText = Midle;
                if (!midleText.IgnoreReformatage)
                {
                    text += "<align=\"center\">";
                    var charSpace = SizeMspace[(int)midleText.SizeMultiplicator];
                    text += $"<mspace={charSpace}em><size={midleText.SizeMultiplicator * 50}%> " + midleText;
                }
                else
                {
                    text += "<align=\"center\">";
                    text += $" " + midleText;
                }
            }
            text += "<size=50%>\n";
            return text;
        }
    }

    public class AnalysedSide
    {
        public bool IgnoreReformatage { get; set; }
        public string FullText { get; internal set; }
        public string TextWithoutTag { get; internal set; }
        public List<string> Tags { get; internal set; }
        public List<string> NotClosedTags { get; internal set; }
        public float SizeMultiplicator { get; internal set; }

        public int CharLengthNoTag => TextWithoutTag.Length;
        public int CharLengthWithTag => FullText.Length;
        public float ScreenLenght => TextWithoutTag.Length * SizeMultiplicator;

        public AnalysedSide(string text, float charSizeMult, bool ignoreReformatage = false) : this(text, charSizeMult, new List<string>())
        {
            IgnoreReformatage = ignoreReformatage;
        }

        public AnalysedSide(string text, float charSizeMult, List<string> notClosedTags)
        {
            FullText = text;
            SizeMultiplicator = charSizeMult;
            TextWithoutTag = TextSpliter.TextWithoutTag(text); 
            Tags = new List<string>(notClosedTags);
            NotClosedTags = new List<string>();

            var matches = Regex.Matches(text, BaliseMatchRegex);

            foreach (Match match in matches)
            {
                Tags.Add(match.Value);
            }

            var closingTags = Tags.Where(p => p.StartsWith("</")).ToList();
            var openingTags = Tags.Where(p => !p.StartsWith("</")).ToList();
            
            for (int i = openingTags.Count - 1; i >= 0; i--)
            {
                int pos = openingTags[i].IndexOf("=");
                string tag = pos >= 0 ? openingTags[i].Substring(0, pos) + ">" : openingTags[i];
                tag = tag.Replace("<", "</");
                if (closingTags.Contains(tag))
                {
                    closingTags.Remove(tag);
                    openingTags.RemoveAt(i);
                }
            }
            NotClosedTags.AddRange(openingTags);
        }

        public override string ToString()
        {
            return FullText;
        }
    }

    public static class TextSpliter
    {
        internal const string LessReplace = @"＜";
        internal const string GreaterReplace = @"＞";

        public static string TextWithoutTag(string text) => Regex.Replace(text, BaliseMatchRegex, string.Empty);

        internal static List<string> GetClosingTags(List<string> notClosed)
        {
            var result = new List<string>();
            foreach (var tag in notClosed)
            {
                int pos = tag.IndexOf("=");
                string tagString = pos >= 0 ? tag.Substring(0, pos) + ">" : tag;
                tagString = tagString.Replace("<", "</");
                result.Add(tagString);
            }
            return result;
        }

        private static void ProcessLongWord(string word, List<string> list, int lineLength, float charSizeMult)
        {
            //words too big to cut without counting the tags
            var totalCharsSize = 0f;
            var letterIndex = 0;
            var tagOpen = false;
            var parsendWord = "";

            while (letterIndex < word.Length)
            {
                if (word[letterIndex] == '<')
                    tagOpen = true;
                else if (tagOpen && word[letterIndex] == '>')
                    tagOpen = false;
                else
                    totalCharsSize += tagOpen ? 0 : charSizeMult;
                
                if (totalCharsSize > lineLength)
                {
                    totalCharsSize = 0;
                    list.Add(parsendWord);
                    parsendWord = "";
                }
                parsendWord += word[letterIndex];
                letterIndex++;
            }
            if (totalCharsSize > 0)
            {
                list.Add(parsendWord);
            }
        }

        public static List<AnalysedSide> Splite(string text, int lineLength, float charSizeMult = 1)
        {
            text = text.Replace(@"\<", LessReplace);
            text = text.Replace(@"\>", GreaterReplace);

            var result = new List<AnalysedSide>();
            if (TextWithoutTag(text).Length * charSizeMult <= lineLength)
            {
                result.Add(new AnalysedSide(text, charSizeMult));
                return result;
            }

            var lstSplit = text.Split(new Char[] { ' ', ',' }).ToList();
            var lst = new List<string>();
            foreach (var elem in lstSplit)
            {
                if (TextWithoutTag(elem).Length * charSizeMult > lineLength)
                {
                    ProcessLongWord(elem, lst, lineLength, charSizeMult);
                }
                else
                {
                    lst.Add(elem);
                }
            }

            var analysedList = new List<AnalysedSide>();
            var previous = new AnalysedSide("", 1, new List<string>());
            foreach (var elem in lst)
            {
                // the objectif is to carry over unclosed tags to the next one
                previous = new AnalysedSide(elem, charSizeMult, previous.NotClosedTags);
                analysedList.Add(previous);
            }
            // recreate the chaine
            var basestring = text;
            var curSize = analysedList[0].ScreenLenght;
            var curChar = analysedList[0].FullText.Length;

            var notClosed = new List<string>();
            var count = analysedList.Count;

            for (int i = 1; i < count; i++)
            {
                var element = analysedList[i];
                if ((curSize + charSizeMult + element.ScreenLenght) > lineLength - 1)
                {
                    var pos = basestring.IndexOf(analysedList[i - 1].FullText, curChar - analysedList[i - 1].FullText.Length);
                    var ligne = basestring.Substring(0, pos + analysedList[i - 1].FullText.Length);
                    basestring = basestring.Substring(ligne.Length);
                    ligne = String.Join("", notClosed) + ligne;
                    notClosed = analysedList[i - 1].NotClosedTags;

                    if (notClosed.Any())
                    {
                        var closingTags = GetClosingTags(notClosed);
                        ligne += String.Join("", closingTags);
                    }
                    result.Add(new AnalysedSide(ligne, charSizeMult));
                    curSize = element.ScreenLenght;
                    curChar = element.FullText.Length;

                    continue;
                }
                curSize += element.ScreenLenght + charSizeMult;
                curChar += element.FullText.Length + 1;
            }

            if (!String.IsNullOrEmpty(basestring))
            {
                var closingTags = GetClosingTags(notClosed);
                result.Add(new AnalysedSide(String.Join("", notClosed) + basestring + String.Join("", closingTags), charSizeMult));
            }
            return result;
        }

    }

    #endregion
}
