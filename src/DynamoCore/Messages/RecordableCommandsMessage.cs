using System.Collections.Generic;
using System.Runtime.Serialization;

using Dynamo.ViewModels;

namespace Dynamo.Messages
{
    [DataContract]
    class RecordableCommandsMessage : Message
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
