using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Dynamo.Connectors;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.Selection;

namespace Dynamo.Controls
{
    public class GeometryKeeperViewModel : dynViewModelBase
    {
        public GeometryKeeperViewModel()
        {
            dynSettings.Controller.RunCompleted += new DynamoController.RunCompletedHandler(GeometryKeeperViewModel_RunCompleted);
        }

        public void GeometryKeeperViewModel_RunCompleted(object controller, bool success)
        {

        }
    }
}
