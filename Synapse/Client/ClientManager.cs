using System.Collections.Generic;

namespace Synapse.Client
{
    public class ClientManager
    {
        internal ClientManager() { }

        /// <summary>
        /// A Boolean that presents whether the Synapse Client feature is activated on this server or not
        /// </summary>
        public bool IsSynapseClientEnabled { get; private set; } = false;

        /// <summary>
        /// The SpawnController for loading prefabs
        /// </summary>
        public SpawnController SpawnController { get; } = new SpawnController();

        /// <summary>
        /// The ConnectionData for all Players on the server
        /// </summary>
        /// <remarks>
        /// It only contains data about players connected with the Synapse Client
        /// </remarks>
        public Dictionary<string, ClientConnectionData> Clients { get; set; } =
            new Dictionary<string, ClientConnectionData>();

        internal void Initialise()
        {
            IsSynapseClientEnabled = true;

            if (!IsSynapseClientEnabled) return;

            new EventHandlers();
        }
    }
}