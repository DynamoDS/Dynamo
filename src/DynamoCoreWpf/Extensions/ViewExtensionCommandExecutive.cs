using System;
using Dynamo.Extensions;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.ViewModels;
using LogMessage = Dynamo.Logging.LogMessage;

namespace Dynamo.Wpf.Extensions
{
    internal class ViewExtensionCommandExecutive : ICommandExecutive
    {
        private readonly DynamoViewModel dynamoViewModel;

        public ViewExtensionCommandExecutive(DynamoViewModel model)
        {
            dynamoViewModel = model;
            MessageLogged += (message) => { dynamoViewModel.Model.Logger.Log(message); };
        }

        public void ExecuteCommand(DynamoModel.RecordableCommand command, string uniqueId, string extensionName)
        {
            var extnDetails = string.Format(
                "ViewExtensionCommandExecutive ( UniqueId: {0}, Name: {1}, commandTypeName: {2} )",
                uniqueId, extensionName, command.GetType().Name);

            Log(LogMessage.Info(extnDetails));
            try
            {
                // run the command
                dynamoViewModel.ExecuteCommand(command);
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
