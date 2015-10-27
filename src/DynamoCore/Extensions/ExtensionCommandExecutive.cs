using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Interfaces;
using Dynamo.Logging;
using Dynamo.Models;

namespace Dynamo.Extensions
{
    internal class ExtensionCommandExecutive : ICommandExecutive, ILogSource
    {
        private readonly DynamoModel dynamoModel;

        public ExtensionCommandExecutive(DynamoModel model)
        {
            dynamoModel = model;
        }

        public void ExecuteCommand(DynamoModel.RecordableCommand command)
        {
            // log that the command is being executed
            if (dynamoModel.DebugSettings.VerboseLogging)
            {
                dynamoModel.Logger.Log("Command: " + command);
            }
            // run the command
            dynamoModel.ExecuteCommand(command);

            // clean up or show failure messages
            
        }

        public event Action<ILogMessage> MessageLogged;
    }
}
