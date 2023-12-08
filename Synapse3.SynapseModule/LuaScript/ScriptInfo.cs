using System;
using System.Collections.Generic;
using Neuron.Core.Plugins;
using Syml;

namespace Synapse3.SynapseModule.LuaScript;

[Serializable]
[DocumentSection("ScriptInfo")]
public class ScriptInfo : IDocumentSection
{
    public string Name { get; set; } = "Unknow";
    public string Description { get; set; } = "Unknow";
    public string Author { get; set; } = "Unknow";
    public string Version { get; set; } = "Unknow";

    public bool Enable { get; set; } = true;

}
