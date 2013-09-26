using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Core.Automation;
using Dynamo.Models;
using Dynamo.Utilities;

namespace Dynamo.ViewModels
{
    partial class DynamoViewModel
    {
        #region Workspace Command Entry Point

        private List<RecordableCommand> recordedCommands = null;

        internal void ExecuteCommand(RecordableCommand command)
        {
            // In the playback mode 'this.recordedCommands' will be 
            // 'null' so that the incoming command will not be recorded.
            if (null != recordedCommands)
                recordedCommands.Add(command);

            command.Execute(this);
        }

        #endregion

        #region The Actual Command Handlers

        internal void CreateNodeImpl(CreateNodeCommand command)
        {
            this.Model.CreateNodeInternal(command, null);
        }

        #endregion

        #region Private Class Helper Methods

        private NodeModel CreateNodeInstance(string nodeName)
        {
            return null;
        }

        #endregion
    }
}
