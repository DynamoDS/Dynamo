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
        private bool runEnabled = true;
        protected bool canRunDynamically = true;
        protected bool debug = false;
        protected bool dynamicRun = false;

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

        public bool RunEnabled
        {
            get { return runEnabled; }
            set
            {
                runEnabled = value;
                RaisePropertyChanged("RunEnabled");
            }
        }

        public virtual bool CanRunDynamically
        {
            get
            {
                //we don't want to be able to run
                //dynamically if we're in debug mode
                return !debug;
            }
            set
            {
                canRunDynamically = value;
                RaisePropertyChanged("CanRunDynamically");
            }
        }

        public Point CurrentOffset
        {
            get { return zoomBorder.GetTranslateTransformOrigin(); }
            set
            {
                if (zoomBorder != null)
                {
                    zoomBorder.SetTranslateTransformOrigin(value);
                }
                NotifyPropertyChanged("CurrentOffset");
            }
        }

        public dynBenchViewModel()
        {
            Controller = controller;
            sw = new StringWriter();
            ConnectorType = ConnectorType.BEZIER;

        }

        public virtual bool DynamicRunEnabled
        {
            get
            {
                return dynamicRun; //selecting debug now toggles this on/off
            }
            set
            {
                dynamicRun = value;
                RaisePropertyChanged("DynamicRunEnabled");
            }
        }

        public virtual bool RunInDebug
        {
            get { return debug; }
            set
            {
                debug = value;

                //toggle off dynamic run
                CanRunDynamically = !debug;

                if (debug)
                    DynamicRunEnabled = false;

                RaisePropertyChanged("RunInDebug");
            }
        }
    }
}
