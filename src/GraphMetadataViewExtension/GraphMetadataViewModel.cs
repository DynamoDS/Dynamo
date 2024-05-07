using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using Dynamo.Core;
using Dynamo.Graph.Workspaces;
using Dynamo.GraphMetadata.Controls;
using Dynamo.Linting;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;

namespace Dynamo.GraphMetadata
{
    public class GraphMetadataViewModel : NotificationObject
    {
        private readonly ViewLoadedParams viewLoadedParams;
        private readonly GraphMetadataViewExtension extension;
        private HomeWorkspaceModel currentWorkspace;
        private LinterManager linterManager;

        /// <summary>
        /// Command used to add new custom properties to the CustomProperty collection
        /// </summary>
        public DelegateCommand AddCustomPropertyCommand { get; set; }

        public DelegateCommand OpenGraphStatusCommand { get; set; }

        /// <summary>
        /// Description of the current workspace
        /// </summary>
        public string GraphDescription
        {
            get { return currentWorkspace.Description; }
            set
            {
                if (currentWorkspace != null && GraphDescription != value)
                {
                    MarkCurrentWorkspaceModified();
                    currentWorkspace.Description = value;
                    RaisePropertyChanged(nameof(GraphDescription));
                }
            }
        }

        /// <summary>
        /// Author name of the current workspace
        /// </summary>
        public string GraphAuthor
        {
            get { return currentWorkspace.Author; }
            set 
            {
                if (currentWorkspace != null && GraphAuthor != value)
                {
                    MarkCurrentWorkspaceModified();
                    currentWorkspace.Author = value;
                    RaisePropertyChanged(nameof(GraphAuthor));
                }
            }
        }

        /// <summary>
        /// Link to documentation page for current workspace
        /// </summary>
        public Uri HelpLink
        {
            get { return currentWorkspace.GraphDocumentationURL; }
            set 
            {
                if (currentWorkspace != null && HelpLink != value)
                {
                    MarkCurrentWorkspaceModified();
                    currentWorkspace.GraphDocumentationURL = value;
                    RaisePropertyChanged(nameof(HelpLink));
                }
            }
        }

        /// <summary>
        /// Workspace thumbnail as BitmapImage.
        /// </summary>
        public BitmapImage Thumbnail
        {
            get
            {
                var bitmap = ImageFromBase64(currentWorkspace.Thumbnail);
                return bitmap;
            }
            set
            {
                var base64 = value is null ? string.Empty : Base64FromImage(value);
                if (currentWorkspace != null && base64 != currentWorkspace.Thumbnail)
                {
                    MarkCurrentWorkspaceModified();
                    currentWorkspace.Thumbnail = base64;
                    RaisePropertyChanged(nameof(Thumbnail));
                }
            }
        }

        /// <summary>
        /// Collection of CustomProperties
        /// </summary>
        public ObservableCollection<CustomPropertyControl> CustomProperties { get; set; }

        public LinterExtensionDescriptor CurrentLinter => linterManager.ActiveLinter;

        public GraphMetadataViewModel(ViewLoadedParams viewLoadedParams, GraphMetadataViewExtension extension)
        {
            this.viewLoadedParams = viewLoadedParams;
            this.extension = extension;
            this.currentWorkspace = viewLoadedParams.CurrentWorkspaceModel as HomeWorkspaceModel;
            this.linterManager = viewLoadedParams.StartupParams.LinterManager;

            this.viewLoadedParams.CurrentWorkspaceChanged += OnCurrentWorkspaceChanged;
            // using this as CurrentWorkspaceChanged wont trigger if you:
            // Close a saved workspace and open a New homeworkspace..
            // This means that properties defined in the previous opened workspace will still be showed in the extension.
            // CurrentWorkspaceCleared will trigger everytime a graph is closed which allows us to reset the properties. 
            this.viewLoadedParams.CurrentWorkspaceCleared += OnCurrentWorkspaceChanged;
            if (linterManager != null)
            {
                linterManager.PropertyChanged += OnLinterManagerPropertyChange;
            }

            CustomProperties = new ObservableCollection<CustomPropertyControl>();
            InitializeCommands();
        }

        private void OnCurrentWorkspaceChanged(Graph.Workspaces.IWorkspaceModel obj)
        {
            //This event is triggered when a new workspace is opened or when the current workspace is cleared.
            //This event manages state of the workspace properties (ie GraphDescription, GraphAuthor, HelpLink, Thumbnail)
            //as well as the custom properties in the extension which do not live in the workspace model.

            //Todo review if the workspace properties should be managed in the Workspace model.
            //Do to the fact that Dynamo oftens leaves the workspace objs in memory and resets their properties when you open a new workspace
            //the management of state is not straightforward. However it may make more sense to update those properteis with the clearing logic.

            //Handle the case of a custom workspace model opening
            if (!(obj is HomeWorkspaceModel hwm))
            {
                extension.Closed();
                return;
            }

            //Handle workspace change cases in UI.  First is a new workspace or template opening
            //In this case the properties should be cleared
            if (!hwm.IsTemplate && string.IsNullOrEmpty(hwm.FileName) )
            {
                GraphDescription = string.Empty;
                GraphAuthor = string.Empty;
                HelpLink = null;
                Thumbnail = null;
            }
            //Second is switching between an open workspace and open custom node and no state changes are required.
            //This case can also be true if you close an open workspace while focused on a custom node.
            //However in that scenario the first case will be triggered first due to empty filename.
            else if(hwm == currentWorkspace)
            {
                return;
            }
            //Third is a new workspace opening from a saved file
            else
            {
                currentWorkspace = hwm;
                RaisePropertyChanged(nameof(GraphDescription));
                RaisePropertyChanged(nameof(GraphAuthor));
                RaisePropertyChanged(nameof(HelpLink));
                RaisePropertyChanged(nameof(Thumbnail));
            }

            //Clear custom properties for cases one and two.
            CustomProperties.Clear();
        }

        private void OnLinterManagerPropertyChange(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LinterManager.ActiveLinter))
            {
                RaisePropertyChanged(nameof(CurrentLinter));
            }
        }

        private static BitmapImage ImageFromBase64(string b64string)
        {
            if (string.IsNullOrEmpty(b64string))
            {
                return null;
            }

            var bytes = Convert.FromBase64String(b64string);

            using (var stream = new MemoryStream(bytes))
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
                return bitmapImage;
            }            
        }

        private static string Base64FromImage(BitmapImage source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            byte[] data;
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(source));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                data = ms.ToArray();
            }

            return Convert.ToBase64String(data);
        }

        private void InitializeCommands()
        {
            this.AddCustomPropertyCommand = new DelegateCommand(AddCustomPropertyExecute);
            this.OpenGraphStatusCommand = new DelegateCommand(OpenGraphStatusExecute);
        }

        private void OpenGraphStatusExecute(object obj)
        {
            //Open Graph Status view extension            
            viewLoadedParams.OpenViewExtension("3467481b-d20d-4918-a454-bf19fc5c25d7");
        }

        private void AddCustomPropertyExecute(object obj)
        {
            int increment = CustomProperties.Count + 1;
            string propName = DefaultPropertyName(increment);

            //Ensure the property name is unique
            while (CustomProperties.Any(x => x.PropertyName == propName))
            {
                increment++;
                propName = DefaultPropertyName(increment);
            }

            AddCustomProperty(propName, string.Empty);

            string DefaultPropertyName(int number) => Properties.Resources.CustomPropertyControl_CustomPropertyDefault + " " + number;
        }

        internal void AddCustomProperty(string propertyName, string propertyValue, bool markChange = true)
        {
            var control = new CustomPropertyControl
            {
                PropertyName = propertyName,
                PropertyValue = propertyValue
            };

            control.RequestDelete += HandleDeleteRequest;
            control.PropertyChanged += HandlePropertyChanged;
            CustomProperties.Add(control);

            if (markChange)
            {
                MarkCurrentWorkspaceModified();
            }
        }

        private void HandlePropertyChanged(object sender, EventArgs e)
        {
            MarkCurrentWorkspaceModified();
        }

        private void HandleDeleteRequest(object sender, EventArgs e)
        {
            if (sender is CustomPropertyControl customProperty)
            {
                customProperty.RequestDelete -= HandleDeleteRequest;
                customProperty.PropertyChanged -= HandlePropertyChanged;
                CustomProperties.Remove(customProperty);
                MarkCurrentWorkspaceModified();
            }
        }

        private void MarkCurrentWorkspaceModified()
        { 
            if (currentWorkspace != null && !string.IsNullOrEmpty(currentWorkspace.FileName))
            {
                currentWorkspace.HasUnsavedChanges = true;
            }
        }

        public void Dispose()
        {
            this.viewLoadedParams.CurrentWorkspaceChanged -= OnCurrentWorkspaceChanged;
            this.viewLoadedParams.CurrentWorkspaceCleared -= OnCurrentWorkspaceChanged;
            if (linterManager != null)
            {
                linterManager.PropertyChanged -= OnLinterManagerPropertyChange;
            }

            foreach (var cp in CustomProperties)
            {
                cp.RequestDelete -= HandleDeleteRequest;
                cp.PropertyChanged -= HandlePropertyChanged;
            }
        }
    }
}
