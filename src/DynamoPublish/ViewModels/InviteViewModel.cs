using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.Publish.Models;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;
using Dynamo.Wpf.Authentication;
using Dynamo.Wpf.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Dynamo.Publish.Properties;

namespace Dynamo.Publish.ViewModels
{
    public class InviteViewModel : NotificationObject
    {
        #region Properties
              
        private readonly InviteModel model;
        internal InviteModel Model
        {
            get { return model; }
        }

        private string statusText;
        public string StatusText
        {
            get
            {
                return statusText;
            }
            set
            {
                statusText = value;
                RaisePropertyChanged("StatusText");
            }
        }

        private bool hasError;

        public bool HasError
        {
            get { return hasError; }
            set
            {
                hasError = value;
                RaisePropertyChanged("HasError");
            }
        }

        private bool isSent = false;
        public bool IsSent 
        {
            get { return isSent; }
            set
            {
                if (isSent != value) 
                {
                    isSent = value;
                    RaisePropertyChanged("IsSent");
                }                
            }
        }

        private Visibility isTextblockVisible;
        public Visibility IsTextblockVisible
        {
            get
            {
                return isTextblockVisible;                
            }
            set
            {
                isTextblockVisible = value;
                RaisePropertyChanged("IsTextblockVisible");
            }
        }
      
        #endregion

        #region Click commands

        public ICommand InviteCommand { get; private set; }

        #endregion

        #region Initialization

        internal InviteViewModel(InviteModel model)
        {
            this.model = model;    
            model.UpdateStatusMessage +=model_UpdateStatusMessage;           
            IsTextblockVisible = Visibility.Hidden;
            InviteCommand = new DelegateCommand(OnInvite);
        }

        internal void InviteLoad(object sender, EventArgs e)
        {
            var status = model.GetInvitationStatus();

            IsSent = !String.IsNullOrEmpty(status);
            if (status == "pending") 
            {
                model_UpdateStatusMessage(Resources.RequestOnPendingState);
            }
            else if (status == "approved")
            {
                model_UpdateStatusMessage(Resources.RequestApproved);
            }
        }

        private void model_UpdateStatusMessage(string status, bool hasError = false)
        {
            StatusText = status;
            HasError = hasError;
            IsTextblockVisible = Visibility.Visible;
        }
        
        #endregion

        #region Helpers

        private void OnInvite(object obj)
        {
            model_UpdateStatusMessage(Resources.InviteRequestStart);
            if (!model.IsLoggedIn)
            {
                model.Authenticate();
            }

            if (!model.IsLoggedIn)
            {
                model_UpdateStatusMessage(Resources.AuthenticationFailedMessage, true);
                return;
            }

            model.Send();
        }

        #endregion
    }
}
