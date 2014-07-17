using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Dynamo.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Dynamo.Messages
{
    [DataContract]
    internal class Message
    {
        #region Class Data Members

        /// <summary>
        /// List of recordable commands that should be executed on server
        /// </summary>
        [DataMember]
        public IEnumerable<DynamoViewModel.RecordableCommand> Commands { get; private set; }

        #endregion
    }
}
