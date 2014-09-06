using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Models;
using Dynamo.ViewModels;

namespace FloodRunner
{
    class MockFloodRunner : iFloodRunner
    {
        #region iFloodRunner Members

        public bool AddNode(NodeModel node, object whereToAdd)
        {
            return true;
        }

        public bool DeleteNode(NodeModel node, object whereFromDelete)
        {
            return true;
        }

        public bool AddConnector(ConnectorModel connector, object whereToAdd)
        {
            return true;
        }

        public bool DeleteConnector(ConnectorModel connector, object whereFromDelete)
        {
            return true;
        }

        public WorkspaceModel NewWorkspace(string name)
        {
            var model = DynamoModel.Start(
                new DynamoModel.StartConfiguration()
                {
                    StartInTestMode = true
                });

            return new HomeWorkspaceModel(model);
        }

        public WorkspaceModel LoadWorkspace(string filePath)
        {
            var model = DynamoModel.Start(
                new DynamoModel.StartConfiguration()
                {
                    StartInTestMode = true
                });

            return new HomeWorkspaceModel(model);
        }

        public bool SaveWorkspace(WorkspaceModel workspace, string filePath)
        {
            return true;
        }

        public bool AddNote(NoteModel note, object whereToAdd)
        {
            return true;
        }

        public bool DeleteNote(NoteModel note, object whereFromDelete)
        {
            return true;
        }

        public bool AddPort(PortModel port, object whereToAdd)
        {
            return true;
        }

        public bool DeletePort(PortModel port, object whereFromDelete)
        {
            return true;
        }

        #endregion

        #region iFloodRunner Members


        public bool AddConnector(ConnectorModel connector, WorkspaceModel whereToAdd)
        {
            throw new NotImplementedException();
        }

        public bool DeleteConnector(ConnectorModel connector, WorkspaceModel whereFromDelete)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
