using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media.Imaging;
using Dynamo.Core;
using Dynamo.Graph.Workspaces;
using Dynamo.GraphMetadata.Controls;
using Dynamo.UI.Commands;
using Dynamo.Wpf.Extensions;

namespace Dynamo.GraphMetadata
{
    public class GraphMetadataViewModel : NotificationObject
    {
        private readonly ViewLoadedParams viewLoadedParams;
        private readonly GraphMetadataViewExtension extension;
        private HomeWorkspaceModel currentWorkspace;

        /// <summary>
        /// Command used to add new custom properties to the CustomProperty collection
        /// </summary>
        public DelegateCommand AddCustomPropertyCommand { get; set; }

        /// <summary>
        /// Description of the current workspace
        /// </summary>
        public string GraphDescription
        {
            get { return currentWorkspace.Description; }
            set
            {
                if (this.currentWorkspace != null && GraphDescription != value)
                {
                    this.currentWorkspace.HasUnsavedChanges = true;
                }

                currentWorkspace.Description = value;
                RaisePropertyChanged(nameof(GraphDescription));
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
                if (this.currentWorkspace != null && GraphAuthor != value)
                {
                    this.currentWorkspace.HasUnsavedChanges = true;
                }
                currentWorkspace.Author = value; 
                RaisePropertyChanged(nameof(GraphAuthor));

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
                if (this.currentWorkspace != null && HelpLink != value)
                {
                    this.currentWorkspace.HasUnsavedChanges = true;
                }

                currentWorkspace.GraphDocumentationURL = value; 
                RaisePropertyChanged(nameof(HelpLink)); 
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
                if (this.currentWorkspace != null && base64 != currentWorkspace.Thumbnail)
                {
                    this.currentWorkspace.HasUnsavedChanges = true;
                }

                currentWorkspace.Thumbnail = base64;
                RaisePropertyChanged(nameof(Thumbnail));
            }
        }

        /// <summary>
        /// Collection of CustomProperties
        /// </summary>
        public ObservableCollection<CustomPropertyControl> CustomProperties { get; set; }

        public GraphMetadataViewModel(ViewLoadedParams viewLoadedParams, GraphMetadataViewExtension extension)
        {
            this.viewLoadedParams = viewLoadedParams;
            this.extension = extension;
            this.currentWorkspace = viewLoadedParams.CurrentWorkspaceModel as HomeWorkspaceModel;

            this.viewLoadedParams.CurrentWorkspaceChanged += OnCurrentWorkspaceChanged;
            // using this as CurrentWorkspaceChanged wont trigger if you:
            // Close a saved workspace and open a New homeworkspace..
            // This means that properties defined in the previous opened workspace will still be showed in the extension.
            // CurrentWorkspaceCleared will trigger everytime a graph is closed which allows us to reset the properties. 
            this.viewLoadedParams.CurrentWorkspaceCleared += OnCurrentWorkspaceChanged;

            CustomProperties = new ObservableCollection<CustomPropertyControl>();
            InitializeCommands();
        }

        private void OnCurrentWorkspaceChanged(Graph.Workspaces.IWorkspaceModel obj)
        {
            if (!(obj is HomeWorkspaceModel hwm))
            {
                extension.Closed();
                return;
            }

            if (string.IsNullOrEmpty(hwm.FileName))
            {
                GraphDescription = string.Empty;
                GraphAuthor = string.Empty;
                HelpLink = null;
                Thumbnail = null;
            }

            else
            {
                currentWorkspace = hwm;
                RaisePropertyChanged(nameof(GraphDescription));
                RaisePropertyChanged(nameof(GraphAuthor));
                RaisePropertyChanged(nameof(HelpLink));
                RaisePropertyChanged(nameof(Thumbnail));
            }

            CustomProperties.Clear();
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
        }

        private void AddCustomPropertyExecute(object obj)
        {
            var propName = $"Custom Property {CustomProperties.Count + 1}";
            AddCustomProperty(propName, string.Empty);
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

            if (markChange && this.currentWorkspace != null)
            {
                this.currentWorkspace.HasUnsavedChanges = true;
            }
        }

        private void HandlePropertyChanged(object sender, EventArgs e)
        {
            if (this.currentWorkspace != null)
            {
                this.currentWorkspace.HasUnsavedChanges = true;
            }
        }

        private void HandleDeleteRequest(object sender, EventArgs e)
        {
            if (sender is CustomPropertyControl customProperty)
            {
                customProperty.RequestDelete -= HandleDeleteRequest;
                customProperty.PropertyChanged -= HandlePropertyChanged;
                CustomProperties.Remove(customProperty);
                if (this.currentWorkspace != null)
                {
                    this.currentWorkspace.HasUnsavedChanges = true;
                }
            }
        }

       public void Dispose()
       {
            this.viewLoadedParams.CurrentWorkspaceChanged -= OnCurrentWorkspaceChanged;

            foreach (var cp in CustomProperties)
            {
                cp.RequestDelete -= HandleDeleteRequest;
                cp.PropertyChanged -= HandlePropertyChanged;
            }
       }
    }
}
