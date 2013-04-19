using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Dynamo.Connectors;

namespace Dynamo.Nodes.ViewModels
{
    class dynBenchViewModel:dynViewModelBase
    {
        private string logText;
        private ConnectorType connectorType;
        private Point transformOrigin;
        private bool consoleShowing;
        private dynConnector activeConnector;
        private DynamoController controller;
        public StringWriter sw;

        public string LogText
        {
            get { return logText; }
            set
            {
                logText = value;
                RaisePropertyChanged("LogText");
            }
        }

        public ConnectorType ConnectorType
        {
            get { return connectorType; }
            set
            {
                connectorType = value;
                RaisePropertyChanged("ConnectorType");
            }
        }

        public Point TransformOrigin
        {
            get { return transformOrigin; }
            set
            {
                transformOrigin = value;
                RaisePropertyChanged("TransformOrigin");
            }
        }

        public bool ConsoleShowing
        {
            get { return consoleShowing; }
            set
            {
                consoleShowing = value;
                RaisePropertyChanged("ConsoleShowing");
            }
        }

        public dynConnector ActiveConnector
        {
            get { return activeConnector; }
            set
            {
                activeConnector = value;
                RaisePropertyChanged("ActiveConnector");
            }
        }

        public DynamoController Controller
        {
            get { return controller; }
            set
            {
                controller = value;
                RaisePropertyChanged("ViewModel");
            }
        }

        public dynBenchViewModel()
        {
            Controller = controller;
            sw = new StringWriter();
            ConnectorType = ConnectorType.BEZIER;
        }
    }
}
