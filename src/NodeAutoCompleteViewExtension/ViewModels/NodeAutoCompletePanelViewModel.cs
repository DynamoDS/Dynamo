using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Windows.Documents;
using Dynamo.Core;
using Dynamo.Engine;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Nodes;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.Search.SearchElements;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels;
using Greg;
using Newtonsoft.Json;
using RestSharp;
using System.Windows;

namespace Dynamo.NodeAutoComplete.ViewModels
{
    public class NodeAutoCompletePanelViewModel : NotificationObject
    {
        internal UIElement DynamoView { get; set; }
        internal DynamoViewModel dynamoViewModel { get; set; }

        //constructor
        internal NodeAutoCompletePanelViewModel(UIElement dynamoView, DynamoViewModel dvm)
        {
            DynamoView = dynamoView;
            dynamoViewModel = dvm;
        }
    }
}
