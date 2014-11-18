using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DynamoWebServer.Messages
{
    [DataContract]
    public class UnsavedChangesPresenceMessage : Message
    {
        #region Class Data Members

        /// <summary>
        /// Guid of a specified workspace. Empty string for Home workspace
        /// </summary>
        [DataMember]
        public string WorkspaceGuid { get; set; }

        #endregion
    }
}
