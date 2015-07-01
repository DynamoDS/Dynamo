using Dynamo.Core;
using Dynamo.Models;
using Dynamo.Publish.Models;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;
using Dynamo.Wpf.Authentication;
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

        private PublishModel model;
        public PublishModel Model
        {
            get { return model; }
        }

        /// <summary>
        /// Used as parent window for authentication.
        /// </summary>
        public Views.PublishView PublishView { get; set; }

        public IEnumerable<WorkspaceViewModel> WorkSpaces { get; set; }

        #endregion

        #region Click commands

        public ICommand PublishCommand { get; private set; }

        #endregion

        #region Initialization

        public PublishViewModel(PublishModel model)
        {
            this.model = model;

            PublishCommand = new DelegateCommand(OnPublish);
        }

        #endregion


        #region Helpers

        private void OnPublish(object obj)
        {
            if (!model.IsLoggedIn)
            {
                model.LoginService = new LoginService(PublishView, new WindowsFormsSynchronizationContext());
                model.Authenticate();
            }

            if (!model.IsLoggedIn)
                return;

            model.SendWorkspaces(WorkSpacesModels());
        }

        public IEnumerable<WorkspaceModel> WorkSpacesModels()
        {
            foreach (var ws in WorkSpaces)
                yield return ws.Model;
        }

        #endregion

    }
}
