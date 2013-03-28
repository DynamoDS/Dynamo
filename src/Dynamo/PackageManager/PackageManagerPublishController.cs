using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Dynamo.Controls;
using Dynamo.PackageManager;

namespace Dynamo.Nodes.PackageManager
{

    public class PackageManagerPublishController : INotifyPropertyChanged
    {
        #region Properties

            public PackageManagerClient Client { get; internal set; }
            public PackageManagerPublishUI View { get; internal set; }

            public string DialogTitle { get; internal set; }

            private FunctionDefinition _FunctionDefinition;
            public FunctionDefinition FunctionDefinition
            {
                get { return _FunctionDefinition; } 
                set
                {
                    DialogTitle = "Publishing \"" + value.Workspace.Name + "\"";
                    PropertyChanged(this, new PropertyChangedEventArgs("DialogTitle"));
                    _FunctionDefinition = value;
                }
            }

        #endregion

        public PackageManagerPublishController(PackageManagerClient client)
        {
            Client = client;
            DialogTitle = "";
            View = new PackageManagerPublishUI(this);
        }

        internal void Submit()
        {
            // collect any data from the function




            // this._client.Publish(_client.GetPackageUploadFromCurrentWorkspace());

        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

}
