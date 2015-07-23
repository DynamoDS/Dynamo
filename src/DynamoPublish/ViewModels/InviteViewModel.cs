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
    public class InviteViewModel : NotificationObject
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

        private readonly InviteModel model;
        internal InviteModel Model
        {
            get { return model; }
        }
      
        #endregion

        #region Click commands

        public ICommand InviteCommand { get; private set; }

        #endregion

        #region Initialization

        internal InviteViewModel(InviteModel model)
        {
            this.model = model;

            InviteCommand = new DelegateCommand(OnInvite);
        }

        #endregion

        #region Helpers

        private void OnInvite(object obj)
        {
            //if (!model.IsLoggedIn)            
            //    model.Authenticate();            

            //if (!model.IsLoggedIn)
            //    return;

            model.Send();
        }

        #endregion
    }
}
