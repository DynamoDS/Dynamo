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

        private readonly PublishModel model;
        internal PublishModel Model
        {
            get { return model; }
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

            PublishCommand = new DelegateCommand(OnPublish);
        }

        #endregion

        #region Helpers

        private void OnPublish(object obj)
        {
            if (!model.IsLoggedIn)            
                model.Authenticate();            

            if (!model.IsLoggedIn)
                return;

            model.Send(Workspaces);
        }

        #endregion



    }
}
