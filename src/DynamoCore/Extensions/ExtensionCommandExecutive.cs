using System;
using Dynamo.Logging;
using Dynamo.Models;

namespace Dynamo.Extensions
{
    internal class ExtensionCommandExecutive : ICommandExecutive
    {
        private readonly DynamoModel dynamoModel;

        public ExtensionCommandExecutive(DynamoModel model)
        {
            dynamoModel = model;
            MessageLogged += (message) => { dynamoModel.Logger.Log(message); };
        }

        public void ExecuteCommand(DynamoModel.RecordableCommand command, string uniqueId, string extensionName)
        {
            // log that the command is being executed
            if (dynamoModel.DebugSettings.VerboseLogging)
            {
                dynamoModel.Logger.Log("Command: " + command);
            }
            
            var extnDetails = string.Format(
                "ExtensionCommandExecutive ( UniqueId: {0}, Name: {1}, commandTypeName: {2} )",
                uniqueId, extensionName, command.GetType().Name);
            
            Log(LogMessage.Info(extnDetails));

            try
            {
                // run the command
                dynamoModel.ExecuteCommand(command);
            }
            catch (Exception e)
            {
                // clean up or show failure messages
                Log(LogMessage.Error(string.Format("{0}, from {1}", e.Message, extnDetails)));
            }
            
        }

        public event Action<ILogMessage> MessageLogged;
        private void Log(ILogMessage obj)
        {
            var handler = MessageLogged;
            if (handler != null) handler(obj);
        }
    }
}
