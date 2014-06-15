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

        static readonly JsonSerializerSettings JsonSettings;
        static Message()
        {
            JsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        /// <summary>
        /// List of recordable commands that should be executed on server
        /// </summary>
        [DataMember]
        public List<DynamoViewModel.RecordableCommand> Commands { get; private set; }

        public string SessionId { get; set; }

        /// <summary>
        /// Send the results of the execution
        /// </summary>
        public event Answer SendAnswer;
        protected void OnAnswer(string message, string id)
        {
            if (SendAnswer != null)
            {
                SendAnswer(message, id);
            }
        }

        #endregion

        #region Public Class Operational Methods

        /// <summary>
        /// This method serializes the Message object in the json form. 
        /// </summary>
        /// <returns>The string can be used for reconstructing Message using Deserialize method</returns>
        internal string Serialize()
        {
            return JsonConvert.SerializeObject(this, JsonSettings);
        }

        /// <summary>
        /// Call this static method to reconstruct a Message from json string
        /// </summary>
        /// <param name="jsonString">Json string that contains all its arguments.</param>
        /// <returns>Reconstructed Message</returns>
        internal static Message Deserialize(string jsonString)
        {
            try
            {
                return JsonConvert.DeserializeObject(jsonString, JsonSettings) as Message;
            }
            catch
            {
                throw new ArgumentException("Invalid jsonString for creating Message");
            }
        }

        #endregion

        #region Abstract methods

        internal virtual void Execute(DynamoViewModel dynamoViewModel)
        {
            if (Commands != null)
            {
                Commands.ForEach(c => c.Execute(dynamoViewModel));
            }
        }

        #endregion
    }

    delegate void Answer(string message, string sessionId);
}
