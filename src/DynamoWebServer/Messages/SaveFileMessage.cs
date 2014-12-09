using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DynamoWebServer.Messages
{
    [DataContract]
    public class SaveFileMessage : Message 
    { 
        /// <summary>
        /// Guid of the specified workspace
        /// </summary>
        [DataMember]
        public string Guid { get; set; }

        /// <summary>
        /// Path to save
        /// </summary>
        [DataMember]
        public string FilePath { get; set; }
    }
}
