
using System.Runtime.Serialization;
namespace DynamoWebServer.Messages
{
    [DataContract]
    class ClearWorkspaceMessage : Message
    {
        [DataMember]
        public bool ClearOnlyHome { get; set; }
    }
}
