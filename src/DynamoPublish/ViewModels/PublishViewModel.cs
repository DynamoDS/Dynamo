using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Publish.Models;
using Dynamo.Publish.Properties;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;
using Dynamo.Wpf.Authentication;
using Dynamo.Wpf.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using System.Linq;
using Dynamo.Graph;
using Dynamo.Graph.Workspaces;
using Reach;
using Reach.Data;
using Reach.Messages.Data;

namespace Dynamo.Publish.ViewModels
{
    public class PublishViewModel : NotificationObject
    {
        #region Properties

        /// <summary>
        ///     Helps to show error message just 1 time.
        /// </summary>
        private bool firstTimeErrorMessage = true;

        internal Dispatcher UIDispatcher { get; set; }
        
        private readonly PublishModel model;
        internal PublishModel Model
        {
            get { return model; }
        }
        
        /// <summary>
        ///     The name of the customizer.
        /// </summary>
        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                RaisePropertyChanged("Name");
                BeginInvoke(() => PublishCommand.RaiseCanExecuteChanged());
            }
        }
        
        /// <summary>
        ///     The description of the customizer.
        /// </summary>
        private string description;
        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                RaisePropertyChanged("Description");
                BeginInvoke(() => PublishCommand.RaiseCanExecuteChanged());
            }
        }

        /// <summary>
        ///     The URL of the customizer after being published.
        /// </summary>
        private string shareLink;
        public string ShareLink
        {
            get { return shareLink; }
            private set
            {
                shareLink = value;
                RaisePropertyChanged("ShareLink");
                BeginInvoke(() =>
                {
                    VisitCommand.RaiseCanExecuteChanged();
                    CopyLinkCommand.RaiseCanExecuteChanged();
                });
            }
        }
        
        /// <summary>
        ///     A message indicating the state of the publish process.
        /// </summary>
        private string uploadStateMessage;
        public string UploadStateMessage
        {
            get { return uploadStateMessage; }
            private set
            {
                uploadStateMessage = value;
                RaisePropertyChanged("UploadStateMessage");
            }
        }
        
        /// <summary>
        ///     A boolean flag indicating if the customizer is ready for upload.
        /// </summary>
        private bool isReadyToUpload;
        public bool IsReadyToUpload
        {
            get { return isReadyToUpload; }
            private set
            {
                isReadyToUpload = value;
                RaisePropertyChanged("IsReadyToUpload");
            }
        }

        
        /// <summary>
        ///     A flag indicating if the customizer is currently being uploaded.
        /// </summary>
        private bool isUploading;
        public bool IsUploading
        {
            get
            {
                return isUploading;
            }
            set
            {
                if (isUploading != value)
                {
                    isUploading = value;
                    if (isUploading)
                    {
                        UploadStateMessage = Resources.UploadingMessage;
                        IsReadyToUpload = true;
                    }
                    RaisePropertyChanged("IsUploading");
                }
            }
        }

        /// <summary>
        ///     The currently active workspace model in the process of being
        ///     published as a customizer.
        /// </summary>
        public IWorkspaceModel CurrentWorkspaceModel { get; set; }

        /// <summary>
        ///     The information about current cameras to publish with customizer
        /// </summary>
        public IEnumerable<CameraData> Cameras { get; set; } 

        /// <summary>
        ///     The URL of the customizer management page.
        /// </summary>
        public string ManagerURL
        {
            get
            {
                return PublishModel.ManagerURL;
            }
        }

        #endregion

        #region Click commands

        /// <summary>
        ///     A command that causes the upload process to begin.
        /// </summary>
        public DelegateCommand PublishCommand { get; private set; }
        
        /// <summary>
        ///     A command that opens the customizer's URL in the user's browser.
        /// </summary>
        public DelegateCommand VisitCommand { get; private set; }
        
        /// <summary>
        ///     A command that copies the customizer's URL to the user's clipboard.
        /// </summary>
        public DelegateCommand CopyLinkCommand { get; private set; }

        #endregion

        #region Initialization

        internal PublishViewModel(PublishModel model)
        {
            this.model = model;

            PublishCommand = new DelegateCommand(OnPublish, CanPublish);
            VisitCommand = new DelegateCommand(Visit, CanVisit);
            CopyLinkCommand = new DelegateCommand(CopyLink, CanCopyLink);
            model.UploadStateChanged += OnModelStateChanged;
            model.CustomizerURLChanged += OnCustomizerURLChanged;
        }

        #endregion

        #region Helpers

        private void OnPublish(object obj)
        {
            if (!model.IsLoggedIn)
            {
                model.Authenticate();
            }

            if (!model.IsLoggedIn)
            {
                return;
            }

            var workspaceProperties = new WorkspaceProperties
            {
                Name = Name,
                Description = Description,
                Cameras = Cameras
            };

            var workspace = CurrentWorkspaceModel as HomeWorkspaceModel;

            if (workspace == null)
            {
                throw new InvalidOperationException("The CurrentWorkspaceModel must be of type " + typeof(HomeWorkspaceModel).Name);
            }

            model.SendAsync(workspace, workspaceProperties);
        }

        private void Visit(object _)
        {
            try
            {
                System.Diagnostics.Process.Start(ShareLink);
            }
            catch (Exception e)
            {
                model.AsLogger().Log(e);
            }
        }

        private bool CanVisit(object _)
        {
            return !String.IsNullOrEmpty(ShareLink);
        }

        private void CopyLink(object _)
        {
            System.Windows.Clipboard.SetText(ShareLink);
            UploadStateMessage = Resources.LinkCopied;
        }

        private bool CanCopyLink(object _)
        {
            return !String.IsNullOrEmpty(ShareLink);
        }

        private void OnModelStateChanged(PublishModel.UploadState state)
        {
            IsUploading = state == PublishModel.UploadState.Uploading;
            firstTimeErrorMessage = state == PublishModel.UploadState.Failed;
            BeginInvoke(() => PublishCommand.RaiseCanExecuteChanged());
        }

        private void OnCustomizerURLChanged(string url)
        {
            ShareLink = url;
        }

        private bool CanPublish(object obj)
        {
            if (String.IsNullOrWhiteSpace(Name))
            {
                UploadStateMessage = Resources.ProvideWorskspaceNameMessage;
                IsReadyToUpload = false;
                return false;
            }

            if (!model.HasAuthProvider)
            {
                UploadStateMessage = Resources.ProvideAuthProviderMessage;
                IsReadyToUpload = false;
                return false;
            }

            // If workspace is uploading now, we can't upload one more at the same time.
            if (isUploading)
            {
                return false;
            }

            if (model.State == PublishModel.UploadState.Failed)
            {
                if (firstTimeErrorMessage)
                {
                    GenerateErrorMessage();
                    IsReadyToUpload = false;

                    // We should show error message just one time.
                    firstTimeErrorMessage = false;

                    // Even if there is error, user can try submit one more time.
                    // E.g. user typed wrong login or password.
                    return true;
                }
                else
                {
                    model.ClearState();
                }
            }

            UploadStateMessage = (model.State == PublishModel.UploadState.Succeeded) ? Resources.UploadedMessage : Resources.ReadyForPublishMessage;
            IsReadyToUpload = true;
            return true;
        }

        private void GenerateErrorMessage()
        {
            switch (model.Error)
            {
                case PublishModel.UploadErrorType.AuthenticationFailed:
                    UploadStateMessage = Resources.AuthenticationFailedMessage;
                    break;
                case PublishModel.UploadErrorType.EmptyWorkspace:
                    UploadStateMessage = Resources.EmptyWorkspaceMessage;
                    break;
                case PublishModel.UploadErrorType.AuthProviderNotFound:
                    UploadStateMessage = Resources.AuthManagerNotFoundMessage;
                    break;
                case PublishModel.UploadErrorType.ServerNotFound:
                    UploadStateMessage = Resources.ServerNotFoundMessage;
                    break;
                case PublishModel.UploadErrorType.Unauthorized:
                    UploadStateMessage = Resources.PublishUnauthorizedMessage;
                    break;
                case PublishModel.UploadErrorType.InvalidNodes:
                    var nodeList = string.Join(", ", model.InvalidNodeNames);
                    UploadStateMessage = Resources.InvalidNodeMessage + nodeList;
                    break;
                case PublishModel.UploadErrorType.CustomNodeNotFound:
                    UploadStateMessage = string.Format(Resources.CustomNodeDefinitionNotFoundErrorMessage, 
                        model.NotFoundCustomNodeName);
                    break;
                case PublishModel.UploadErrorType.GetWorkspacesError:
                    UploadStateMessage = Resources.GetWorkspacesErrorMessage;
                    break;
                case PublishModel.UploadErrorType.UnknownServerError:
                    UploadStateMessage = Resources.UnknownServerErrorMessage;
                    break;
            }
        }

        private void BeginInvoke(Action action)
        {
            UIDispatcher.BeginInvoke(action);
        }

        internal void ClearShareLink()
        {
            ShareLink = String.Empty;
        }

        #endregion
    }
}