﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Dynamo.Models;

namespace DynamoWebServer.Messages
{
    [DataContract]
    public class RunCommandsMessage : Message
    {
        #region Class Data Members

        /// <summary>
        /// List of recordable commands that should be executed on server
        /// </summary>
        [DataMember]
        public IEnumerable<DynamoModel.RecordableCommand> Commands { get; private set; }

        [DataMember]
        public Guid WorkspaceGuid { get; private set; }

        #endregion
    }
}
