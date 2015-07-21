using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.Publish.Models;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;
using Dynamo.Wpf.Authentication;
using Dynamo.Wpf.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Input;

namespace Dynamo.Publish.ViewModels
{
    public class PublishViewModel : NotificationObject
    {
        #region Properties

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                RaisePropertyChanged("Name");
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
            }
        }

        private string shareLink;
        public string ShareLink
        {
            get { return shareLink; }
            set
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
                    RaisePropertyChanged("IsUploading");
                }
            }
        }

        public IEnumerable<IWorkspaceModel> Workspaces { get; set; }
        public IWorkspaceModel CurrentWorkspaceModel { get; set; }

        #endregion

        #region Click commands

        public ICommand PublishCommand { get; private set; }

        #endregion

        #region Initialization

        internal PublishViewModel(PublishModel model)
        {
            this.model = model;

            PublishCommand = new DelegateCommand(OnPublish, CanPublish);
            model.UploadStateChanged += OnModelStateChanged;
        }

        #endregion

        #region Helpers

        private void OnPublish(object obj)
        {
            if (!model.IsLoggedIn)
                model.Authenticate();

            if (!model.IsLoggedIn)
                return;

            model.SendAsynchronously(Workspaces);
        }

        private void OnModelStateChanged(PublishModel.UploadState state)
        {
            IsUploading = state == PublishModel.UploadState.Uploading;
        }

        private bool CanPublish(object obj)
        {
            if (String.IsNullOrWhiteSpace(Name))
            {
                UploadStateMessage = Resource.ProvideWorskspaceNameMessage;
                return false;
            }

            if (String.IsNullOrWhiteSpace(Description))
            {
                UploadStateMessage = Resource.ProvideWorskspaceDescriptionMessage;
                return false;
            }

            if (!model.HasAuthProvider)
            {
                UploadStateMessage = Resource.ProvideAuthProviderMessage;
                return false;
            }

            if(model.State == PublishModel.UploadState.Failed)
            {
                GenerateErrorMessage();
                return false;
            }

            return true;
        }

        private void GenerateErrorMessage()
        {
            switch (model.Error)
            {
                case PublishModel.UploadErrorType.AuthenticationFailed:
                    UploadStateMessage = Resource.AuthenticationFailedMessage;
                    break;
                case PublishModel.UploadErrorType.AuthProviderNotFound:
                    UploadStateMessage = Resource.AuthManagerNotFoundMessage;
                    break;
                case PublishModel.UploadErrorType.ServerNotFound:
                    UploadStateMessage = Resource.ServerNotFoundMessage;
                    break;
                case PublishModel.UploadErrorType.UnknownServerError:
                    UploadStateMessage = Resource.UnknownServerErrorMessage;
                    break;
            }
        }

        #endregion
    }
}
