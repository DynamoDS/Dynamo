using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using Dynamo.Configuration;
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

        private bool isRequiredPropertiesVisible = false;
        private DelegateCommand addCustomPropertyCommand;
        private ObservableCollection<CustomPropertyControl> customProperties;
        private ObservableCollection<RequiredProperty> requiredProperties;

        /// <summary>
        /// Command used to add new custom properties to the CustomProperty collection
        /// </summary>
        public DelegateCommand AddCustomPropertyCommand
        {
            get => addCustomPropertyCommand;
            set => addCustomPropertyCommand = value;
        }


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
        /// Controls whether RequiredProperties are displayed in the GraphMetadataViewExtension.
        /// When there are no RequiredProperties, this section collapses.
        /// </summary>
        public bool IsRequiredPropertiesVisible
        {
            get => isRequiredPropertiesVisible;
            set
            {
                isRequiredPropertiesVisible = value;
                RaisePropertyChanged(nameof(IsRequiredPropertiesVisible));
            }
        }


        /// <summary>
        /// Collection of CustomProperties
        /// </summary>
        public ObservableCollection<CustomPropertyControl> CustomProperties
        {
            get => customProperties;
            set => customProperties = value;
        }

        /// <summary>
        /// Collection of RequiredProperties
        /// </summary>
        public ObservableCollection<RequiredProperty> RequiredProperties
        {
            get => requiredProperties;

            set
            {
                requiredProperties = value;
                RaisePropertyChanged(nameof(RequiredProperties));
            }
        }


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
            RequiredProperties = new ObservableCollection<RequiredProperty>();

            RequiredProperties.CollectionChanged += UpdateRequiredPropertiesVisibility;

            if (viewLoadedParams.StartupParams.Preferences is PreferenceSettings preferenceSettings)
            {
                extension.PreferenceSettings = preferenceSettings;
            }

            extension.PreferenceSettings.RequiredProperties.CollectionChanged += RequiredPropertiesOnCollectionChanged;

            foreach (RequiredProperty requiredProperty in extension.PreferenceSettings.RequiredProperties)
            {
                requiredProperty.PropertyChanged += RequiredPropertyOnPropertyChanged;
            }

            InitializeCommands();
        }

        /// <summary>
        /// Reads the DynamoSettings.xml file and creates new RequiredProperties by *key only*.
        /// All values are set in the GraphMetadataViewExtension OnWorkspaceOpen event handler.
        /// </summary>
        private void InitializeRequiredProperties()
        {
            // To prevent duplicate keys being added
            List<string> resolvedKeys = new List<string>();

            // Initializing the ExtensionRequiredProperties based on the RequiredProperties saved in the XML
            foreach (RequiredProperty requiredProperty in extension.PreferenceSettings.RequiredProperties)
            {
                // Removing blanks and duplicates
                if (string.IsNullOrEmpty(requiredProperty.Key) || resolvedKeys.Contains(requiredProperty.Key)) continue;

                string value = requiredProperty.ValueIsGlobal ? requiredProperty.GlobalValue : requiredProperty.GraphValue;

                RequiredProperty requiredPropertyToUpdate = RequiredProperties
                    .FirstOrDefault(x => x.Key == requiredProperty.Key);

                if (requiredPropertyToUpdate != null)
                {
                    requiredPropertyToUpdate.GraphValue = requiredProperty.GraphValue;
                }
                else
                {
                    AddRequiredProperty
                    (
                        requiredProperty.UniqueId,
                        requiredProperty.Key,
                        value,
                        requiredProperty.ValueIsGlobal,
                        false
                    );
                    resolvedKeys.Add(requiredProperty.Key);
                }
            }
        }
        
        /// <summary>
        /// Sets listener to the PropertyChanged event of new RequiredProperties as they are added/removed from the PreferenceSettings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RequiredPropertiesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (RequiredProperty requiredProperty in e.NewItems)
                {
                    requiredProperty.PropertyChanged += RequiredPropertyOnPropertyChanged;
                    
                    AddRequiredProperty
                    (
                        requiredProperty.UniqueId,
                        requiredProperty.Key,
                        requiredProperty.GraphValue,
                        requiredProperty.ValueIsGlobal,
                        true
                    );
                }
            }

            if (e.OldItems != null)
            {
                foreach (RequiredProperty requiredProperty in e.OldItems)
                {
                    requiredProperty.PropertyChanged -= RequiredPropertyOnPropertyChanged;
                    DeleteRequiredProperty(requiredProperty);
                }
            }
        }
        
        private void OnCurrentWorkspaceChanged(IWorkspaceModel obj)
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
            
            ClearRequiredPropertyValues();
            InitializeRequiredProperties();
        }

        /// <summary>
        /// Hides or shows RequiredProperties in the extension window as is needed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateRequiredPropertiesVisibility(object sender, NotifyCollectionChangedEventArgs e)
        {
            IsRequiredPropertiesVisible = RequiredProperties != null && RequiredProperties.Count > 0;
        }

        /// <summary>
        /// Called when the current Dynamo workspace changes. Resets the GraphValue of all RequiredProperties
        /// </summary>
        private void ClearRequiredPropertyValues()
        {
            foreach (RequiredProperty requiredProperty in RequiredProperties)
            {
                requiredProperty.GraphValue = null;
            }
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
            viewLoadedParams.OpenViewExtension("Graph Status");
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

            if (markChange)
            {
                MarkCurrentWorkspaceModified();
            }
        }

        /// <summary>
        /// Takes care of all the setup for RequiredProperties on the ViewExtension, including
        /// wiring up event handlers and determining whether the current workspace has changed.
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="propertyKey"></param>
        /// <param name="propertyValue"></param>
        /// <param name="isValueGlobal"></param>
        /// <param name="markChange"></param>
        internal void AddRequiredProperty(string uniqueId, string propertyKey, string propertyValue, bool isValueGlobal, bool markChange = true)
        {
            RequiredProperty requiredProperty = new RequiredProperty
            {
                UniqueId = uniqueId,
                Key = propertyKey,
                GraphValue = propertyValue,
                ValueIsGlobal = isValueGlobal
            };

            RequiredProperties.Add(requiredProperty);

            if (markChange)
            {
                MarkCurrentWorkspaceModified();
            }
        }

        internal void DeleteRequiredProperty(RequiredProperty requiredProperty)
        {
            RequiredProperty requiredPropertyToDelete = RequiredProperties
                .FirstOrDefault(x => x.UniqueId == requiredProperty.UniqueId);

            if (requiredPropertyToDelete == null) return;

            RequiredProperties.Remove(requiredPropertyToDelete);

            MarkCurrentWorkspaceModified();
        }

        private void RequiredPropertyOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender is RequiredProperty requiredProperty)) return;

            RequiredProperty requiredPropertyToUpdate = RequiredProperties
                .FirstOrDefault(x => x.UniqueId == requiredProperty.UniqueId);

            if (requiredPropertyToUpdate == null) return;

            switch (e.PropertyName)
            {
                case nameof(RequiredProperty.Key):
                    requiredPropertyToUpdate.Key = requiredProperty.Key;
                    break;
                case nameof(RequiredProperty.GraphValue):
                    requiredPropertyToUpdate.GraphValue= requiredProperty.GraphValue;
                    break;
                case nameof(RequiredProperty.ValueIsGlobal):
                    requiredPropertyToUpdate.ValueIsGlobal = requiredProperty.ValueIsGlobal;
                    break;
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
