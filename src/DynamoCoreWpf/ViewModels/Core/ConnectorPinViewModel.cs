using System;
using System.Linq;
using Dynamo.Configuration;
using Dynamo.Graph;
using Dynamo.Logging;
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
        /// <summary>
        /// Raises a 'remove from group' event for this ConnectorPinViewModel
        /// </summary>
        public event EventHandler RequestRemoveFromGroup;
        public virtual void OnRequestRemoveFromGroup(Object sender, EventArgs e)
        {
            if (RequestRemoveFromGroup != null)
            {
                RequestRemoveFromGroup(this, e);
            }
        }


        #endregion

        #region Properties

        internal readonly WorkspaceViewModel WorkspaceViewModel;
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
        public static double OneThirdWidth
        {
            get
            {
                return ConnectorPinModel.StaticWidth * 0.33333;
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
            get { return model.Y- OneThirdWidth; }
            set
            {
                //Through trial and error using the OneThirdWidth value to offset the pin location works 
                //better than using OneHalf.
                model.Y = value + OneThirdWidth;
                RaisePropertyChanged(nameof(Top));
            }
        }
        private bool isHoveredOver = false;
        /// <summary>
        /// This flag let's the ConnectorViewModel when it can and cannot run a ConnectorContextMenu. It CANNOT
        /// do so when IsHoveredOver for any pin is set to true, as in that case we want only that ConnectorPins 
        /// ContextMenu to be enabled on right click.
        /// </summary>
        [JsonIgnore]
        public bool IsHoveredOver
        {
            get => isHoveredOver;
            set
            {
                if (isHoveredOver == value)
                {
                    return;
                }

                isHoveredOver = value;
                RaisePropertyChanged(nameof(IsHoveredOver));
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

        private bool isCollapsed = false;
        [JsonIgnore]
        public override bool IsCollapsed
        {
            get => isCollapsed;
            set
            {
                if (isCollapsed == value)
                {
                    return;
                }

                isCollapsed = value;
                RaisePropertyChanged(nameof(IsCollapsed));
            }
        }

        private bool isHidden;
        public bool IsHidden
        {
            get => isHidden;
            set
            {
                if (isHidden == value)
                {
                    return;
                }

                isHidden = value;
                RaisePropertyChanged(nameof(IsHidden));
            }
        }


        private bool isTemporarilyVisible;
        /// <summary>
        /// Provides the ViewModel (this) with the visibility state of the Connector.
        /// When set to 'hidden', 'IsHalftone' is set to true, and viceversa.
        /// </summary>
        [JsonIgnore]
        public bool IsTemporarilyVisible
        {
            get
            {
                return isTemporarilyVisible;
            }
            set
            {
                isTemporarilyVisible = value;
                RaisePropertyChanged(nameof(IsTemporarilyVisible));
            }
        }

        private bool isInGroup;
        /// <summary>
        /// Gets or sets whether the pin is in a group and updates the command state when this changes.
        /// </summary>
        [JsonIgnore]
        public bool IsInGroup
        {
            get => isInGroup;
            set
            {
                if (isInGroup == value) return;
                isInGroup = value;
                RaisePropertyChanged(nameof(IsInGroup));

                // Update the command's state whenever the group status changes
                RemovePinFromGroupCommand.RaiseCanExecuteChanged();
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
            Logging.Analytics.TrackEvent(
                Actions.Unpin,
                Categories.ConnectorOperations);
            WorkspaceViewModel.Model.HasUnsavedChanges = true;
        }

        /// <summary>
        /// Delegate command handling the removal of this ConnectorPin from  group.
        /// </summary>
        [JsonIgnore]
        public DelegateCommand RemovePinFromGroupCommand { get; set; }

        private void RemovePinFromGroupCommandExecute(object parameter)
        {
            WorkspaceViewModel.DynamoViewModel.UngroupModelCommand.Execute(null);
            Analytics.TrackEvent(Actions.RemovedFrom, Categories.NodeContextMenuOperations, "ConnectorPin");
        }

        /// <summary>
        /// Determines if the connector pin can be ungrouped based on the selected group state in the workspace.
        /// </summary>
        private bool CanUngroupConnectorPin(object parameter)
        {
            var groups = WorkspaceViewModel.Model.Annotations;
            if (!groups.Any(x => x.IsSelected))
            {
                return (groups.ContainsModel(Model.GUID));
            }
            return false;
        }

        private void InitializeCommands()
        {
            UnpinConnectorCommand = new DelegateCommand(UnpinWireCommandExecute);
            RemovePinFromGroupCommand = new DelegateCommand(RemovePinFromGroupCommandExecute, CanUngroupConnectorPin);
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="workspaceViewModel"></param>
        /// <param name="model"></param>
        public ConnectorPinViewModel(WorkspaceViewModel workspaceViewModel, ConnectorPinModel model)
        {
            this.WorkspaceViewModel = workspaceViewModel;
            this.model = model;
            InitializeCommands();
            model.PropertyChanged += OnPinPropertyChanged;
            ZIndex = ++StaticZIndex; // places the pin on top of all nodes/notes

            DynamoSelection.Instance.Selection.CollectionChanged += SelectionOnCollectionChanged;
        }

        public override void Dispose()
        {
            model.PropertyChanged -= OnPinPropertyChanged;
            DynamoSelection.Instance.Selection.CollectionChanged -= SelectionOnCollectionChanged;
            base.Dispose();
        }

        private void SelectionOnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RemovePinFromGroupCommand.RaiseCanExecuteChanged();
        }

        //respond to changes on the model's properties
        void OnPinPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
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
