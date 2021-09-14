using System;
using System.Linq;
using Dynamo.Configuration;
using Dynamo.Graph;
using Dynamo.Selection;
using Dynamo.UI.Commands;
using Dynamo.Wpf.ViewModels.Core;
using Newtonsoft.Json;

namespace Dynamo.ViewModels
{
    public partial class ConnectorPinViewModel : ViewModelBase
    {
        #region Events

        /// <summary>
        /// Raises a 'select' event for this ConnectorPinViewModel
        /// </summary>
        public event EventHandler RequestSelect;
        public virtual void OnRequestSelect(Object sender, EventArgs e)
        {
            if (RequestSelect != null)
            {
                RequestSelect(this, e);
            }
        }

        /// <summary>
        /// Raises a 'redraw' event for this ConnectorPinViewModel
        /// </summary>
        public event EventHandler RequestRedraw;
        public virtual void OnRequestRedraw(Object sender, EventArgs e)
        {
            if (RequestRedraw != null)
            {
                RequestRedraw(this, e);
            }
        }

        /// <summary>
        /// Raises a 'remove' event for this ConnectorPinViewModel
        /// </summary>
        public event EventHandler RequestRemove;
        public virtual void OnRequestRemove(Object sender, EventArgs e)
        {
            RequestRemove(this, e);
        }

        #endregion

        #region Properties

        [JsonIgnore]
        private readonly WorkspaceViewModel WorkspaceViewModel;
        /// initialize the start Z-Index of a pin to a default
        /// zIndex is mutable depending on mouse behaviour
        private int zIndex = Configurations.NodeStartZIndex; 
        /// <summary>
        /// StaticZIndez is static Z-level of all ConnectorPins (which currently is
        /// set to match that of nodes)
        /// </summary>
        internal static int StaticZIndex = Configurations.NodeStartZIndex;

        /// <summary>
        /// ZIndex represents the order on the z-plane in which the pins and other objects appear. 
        /// </summary>
        [JsonIgnore]
        public int ZIndex
        {

            get { return zIndex; }
            set { zIndex = value; RaisePropertyChanged(nameof(ZIndex)); }
        }

        private ConnectorPinModel model;

        /// <summary>
        /// ConnectorPinModel reference (listens to property changes of it).
        /// </summary>
        [JsonIgnore]
        public ConnectorPinModel Model
        {
            get { return model; }
            set
            {
                model = value;
                RaisePropertyChanged(nameof(Model));
            }
        }
      /// <summary>
      /// This property is used to center the ConnectorPinIcon in views
      /// as well as to offset the coordinate used for drawing
      /// bezier connectors through.
      /// </summary>
        [JsonIgnore]
        public double OneThirdWidth
        {
            get
            {
                return Model.Width * 0.33333;
            }
        }

        /// <summary>
        /// Element's center position is two-way bound to this value
        /// </summary>
        public double Left
        {
            get { return model.X; }
            set
            {
                model.X = value;
                RaisePropertyChanged(nameof(Left));
            }
        }

        /// <summary>
        /// Element's center position is two-way bound to this value
        /// </summary>
        public double Top
        {
            get { return model.Y; }
            set
            {
                model.Y = value;
                RaisePropertyChanged(nameof(Top));
            }
        }

        /// <summary>
        /// Provides the ViewModel (this) with the selected state of the ConnectorPinModel.
        /// </summary>
        [JsonIgnore]
        public bool IsSelected
        {
            get { return model.IsSelected; }
        }

        private bool isVisible;
        [JsonIgnore]
        public bool IsVisible
        {
            get
            {
                return isVisible;
            }
            set
            {
                isVisible = value;
                RaisePropertyChanged(nameof(IsVisible));
                RaisePropertyChanged(nameof(IsPartlyVisible));
            }
        }

        private bool isPartlyVisible;
        /// <summary>
        /// Provides the ViewModel (this) with the visibility state of the Connector.
        /// When set to 'hidden', 'IsHalftone' is set to true, and viceversa.
        /// </summary>
        [JsonIgnore]
        public bool IsPartlyVisible
        {
            get
            {
                return isPartlyVisible;
            }
            set
            {
                isPartlyVisible = value;
                RaisePropertyChanged(nameof(IsPartlyVisible));
                RaisePropertyChanged(nameof(IsVisible));
            }
        }

        /// <summary>
        /// This property is purely used for serializing/ deserializing.
        /// In reconstructing ConnectorPins, we need to know what Connector they belong to.
        /// </summary>
        public Guid ConnectorGuid
        {
            get
            {
                return model.ConnectorId;
            }
        }

        #endregion

        #region Commands
        /// <summary>
        /// Delegate command handling the removal of this ConnectorPin from its corresponding connector.
        /// </summary>
        [JsonIgnore]
        public DelegateCommand UnpinConnectorCommand { get; set; }

        private void UnpinWireCommandExecute(object parameter)
        {
            OnRequestRemove(this, EventArgs.Empty);
            WorkspaceViewModel.Model.HasUnsavedChanges = true;
        }

        private void InitializeCommands()
        {
            UnpinConnectorCommand = new DelegateCommand(UnpinWireCommandExecute);
        }

        #endregion

        public ConnectorPinViewModel(WorkspaceViewModel workspaceViewModel, ConnectorPinModel model)
        {
            this.WorkspaceViewModel = workspaceViewModel;
            this.model = model;
            InitializeCommands();
            model.PropertyChanged += pin_PropertyChanged;
            ZIndex = ++StaticZIndex; // places the pin on top of all nodes/notes
        }

        public override void Dispose()
        {
            model.PropertyChanged -= pin_PropertyChanged;
            base.Dispose();
        }

        //respond to changes on the model's properties
        void pin_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ConnectorPinModel.X):
                    OnRequestRedraw(this, EventArgs.Empty);
                    //RaisePropertyChanged(nameof(CenterX));
                    RaisePropertyChanged(nameof(Left));
                    break;
                case nameof(ConnectorPinModel.Y):
                    OnRequestRedraw(this, EventArgs.Empty);
                    //RaisePropertyChanged(nameof(CenterY));
                    RaisePropertyChanged(nameof(Top));
                    break;
                case nameof(ConnectorPinModel.IsSelected):
                    OnRequestSelect(this, EventArgs.Empty);
                    RaisePropertyChanged(nameof(IsSelected));
                    break;
            }
        }
    }
}
