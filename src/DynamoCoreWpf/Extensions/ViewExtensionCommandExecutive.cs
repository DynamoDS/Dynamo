using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Log(LogMessage.Info(string.Format(
                "ViewExtensionCommandExecutive ( UniqueId: {0}, Name: {1}, commandTypeName: {2} )",
                uniqueId, extensionName, command.GetType().Name)));

            // run the command
            dynamoViewModel.ExecuteCommand(command);

            // clean up or show failure messages

        }

        public event Action<ILogMessage> MessageLogged;
        private void Log(ILogMessage obj)
        {
            var handler = MessageLogged;
            if (handler != null) handler(obj);
        }
    }
}
