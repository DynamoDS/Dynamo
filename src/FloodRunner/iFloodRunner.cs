using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Models;

namespace FloodRunner
{
    interface iFloodRunner
    {
        bool AddNode(NodeModel node, object whereToAdd);
        bool DeleteNode(NodeModel node, object whereFromDelete);

        bool AddConnector(ConnectorModel connector, WorkspaceModel whereToAdd);
        bool DeleteConnector(ConnectorModel connector, WorkspaceModel whereFromDelete);

        WorkspaceModel NewWorkspace(string name);
        WorkspaceModel LoadWorkspace(string filePath);
        bool SaveWorkspace(WorkspaceModel workspace, string filePath);

        bool AddNote(NoteModel note, object whereToAdd);
        bool DeleteNote(NoteModel note, object whereFromDelete);

        bool AddPort(PortModel port, object whereToAdd);
        bool DeletePort(PortModel port, object whereFromDelete);
    }
}
