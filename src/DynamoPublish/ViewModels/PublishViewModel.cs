using Dynamo.Core;
using Dynamo.UI.Commands;
using System;
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

        #endregion

        #region Click commands

        public ICommand PublishCommand { get; private set; }

        #endregion

        #region Initialization

        public PublishViewModel()
        {
            PublishCommand = new DelegateCommand(OnPublish);
        }

        #endregion


        #region Helpers

        private void OnPublish(object obj)
        {
            
        }

        #endregion
    }
}
