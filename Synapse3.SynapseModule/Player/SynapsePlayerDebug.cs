#if DEBUG
using System.Diagnostics;

namespace Synapse3.SynapseModule.Player;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public partial class SynapsePlayer
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public string DebuggerDisplay => PlayerType == Enums.PlayerType.Server ? "Server(1)" : $"{DisplayName}({PlayerId})";

}
#endif