using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Dynamo.Controls;
using Dynamo.PackageManager;
using Dynamo.Search;

namespace Dynamo.Nodes.PackageManager
{
    public class PackageManagerLoginController
    {
        public PackageManagerClient Client { get; internal set; }
        public PackageManagerLoginUI View { get; internal set; }
        public dynBench Bench { get; internal set; }

        public PackageManagerLoginController(dynBench bench, PackageManagerClient client)
        {
            Client = client;
            View = new PackageManagerLoginUI(this);

            this.Bench = bench;
        }

        public void LoginButtonClick(object sender, RoutedEventArgs e)
        {
            Client.Login();
        }

    }
}
