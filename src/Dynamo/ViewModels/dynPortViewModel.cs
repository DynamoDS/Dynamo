using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using Dynamo.Connectors;
using Microsoft.Practices.Prism.Commands;

namespace Dynamo.Connectors
{
    public class dynPortViewModel : dynViewModelBase
    {
        Point center;
        
        ObservableCollection<dynConnectorViewModel> _connectors = new ObservableCollection<dynConnectorViewModel>();

        readonly dynPortModel _port;

        public DelegateCommand SetCenterCommand { get; set; }
        
        public ObservableCollection<dynConnectorViewModel> Connectors
        {
            get { return _connectors; }
            set { _connectors = value; }
        }

        public dynPortModel PortModel
        {
            get { return _port; }
        }

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
                        return PortModel.Owner.InPortData[index].ToolTipString;
                    }
                    else
                    {
                        return PortModel.Owner.OutPortData[index].ToolTipString;
                    }
                }
                return "";
            }
        }

        public dynPortViewModel(dynPortModel port)
        {
            _port = port;
            _port.Connectors.CollectionChanged += Connectors_CollectionChanged;
        }

        void Connectors_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    _connectors.Add(new dynConnectorViewModel(item as dynConnector));
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    _connectors.Remove(_connectors.ToList().First(x => x.ConnectorModel == item));
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
