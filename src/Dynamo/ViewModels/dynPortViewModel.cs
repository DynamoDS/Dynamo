using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using Dynamo.Connectors;
using Microsoft.Practices.Prism.Commands;

namespace Dynamo.Connectors
{
    public class dynPortViewModel : dynViewModelBase
    {
        Point center;
        
        ObservableCollection<dynConnectorViewModel> connectors = new ObservableCollection<dynConnectorViewModel>();

        private dynPortModel _port;

        public DelegateCommand SetCenterCommand { get; set; }
        
        public ObservableCollection<dynConnectorViewModel> Connectors
        {
            get { return connectors; }
            set { connectors = value; }
        }

        public dynPortModel PortModel { get; set; }

        public Point Center
        {
            get { return UpdateCenter(); }
            set { center = value; }
        }

        public string ToolTipContent
        {
            get
            {
                if (PortModel.Owner != null)
                {
                    if (PortType == Dynamo.Connectors.PortType.INPUT)
                    {
                        return PortModel.Owner.NodeLogic.InPortData[index].ToolTipString;
                    }
                    else
                    {
                        return PortModel.Owner.NodeLogic.OutPortData[index].ToolTipString;
                    }
                }
                return "";
            }
        }

        public dynPortViewModel(dynPortModel port)
        {
            _port = port;
            port.Connectors.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Connectors_CollectionChanged);
        }

        void Connectors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    connectors.Add(new dynConnectorViewModel(item));
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    connectors.Remove();
                }
            }
        }

        //Point UpdateCenter()
        //{
        //    GeneralTransform transform = portCircle.TransformToAncestor(dynSettings.Workbench);
        //    Point rootPoint = transform.Transform(new Point(portCircle.Width / 2, portCircle.Height / 2));
        //    return new Point(rootPoint.X, rootPoint.Y);
        //}
    }
}
