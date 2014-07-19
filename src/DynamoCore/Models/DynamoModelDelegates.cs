using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamo.Models
{
    public delegate void CleanupHandler(object sender, EventArgs e);
    public delegate void NodeHandler(NodeModel node);
    public delegate void ConnectorHandler(ConnectorModel connector);
    public delegate void WorkspaceHandler(WorkspaceModel model);
    public delegate void ActionHandler(Action action);
}
