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
using Reach;

namespace Dynamo.Publish.ViewModels
{
    public class PublishViewModel : NotificationObject
    {
        #region Properties

        /// <summary>
        ///     Helps to show error message just 1 time.
        /// </summary>
        private bool firstTimeErrorMessage = true;

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

        private string shareLink;
        public string ShareLink
        {
            get { return shareLink; }
            private set
            {
                shareLink = value;
                RaisePropertyChanged("ShareLink");
            }
        }

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

        private readonly PublishModel model;
        internal PublishModel Model
        {
            get { return model; }
        }

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

        internal Dispatcher UIDispatcher { get; set; }

        public IEnumerable<IWorkspaceModel> Workspaces { get; set; }
        public IWorkspaceModel CurrentWorkspaceModel { get; set; }

        public string ManagerURL
        {
            get
            {
                return model.ManagerURL;
            }
        }

        #endregion

        #region Click commands

        public DelegateCommand PublishCommand { get; private set; }

        #endregion

        #region Initialization

        internal PublishViewModel(PublishModel model)
        {
            this.model = model;

            PublishCommand = new DelegateCommand(OnPublish, CanPublish);
            model.UploadStateChanged += OnModelStateChanged;
            model.CustomizerURLChanged += OnCustomizerURLChanged;
        }

        #endregion

        #region Helpers

        private void OnPublish(object obj)
        {
            if (!model.IsLoggedIn)
                model.Authenticate();

            if (!model.IsLoggedIn)
                return;

            var workspaceProperties = new WorkspaceProperties();
            workspaceProperties.Name = Name;
            workspaceProperties.Description = Description;

            model.SendAsynchronously(Workspaces, workspaceProperties);
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

            if (String.IsNullOrWhiteSpace(Description))
            {
                UploadStateMessage = Resources.ProvideWorskspaceDescriptionMessage;
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

            UploadStateMessage = Resources.ReadyForPublishMessage;
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
                case PublishModel.UploadErrorType.AuthProviderNotFound:
                    UploadStateMessage = Resources.AuthManagerNotFoundMessage;
                    break;
                case PublishModel.UploadErrorType.ServerNotFound:
                    UploadStateMessage = Resources.ServerNotFoundMessage;
                    break;
                case PublishModel.UploadErrorType.InvalidNodes:
                    var nodeList = String.Join(", ", model.InvalidNodeNames);
                    UploadStateMessage = Resources.InvalidNodeMessage + nodeList;
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
