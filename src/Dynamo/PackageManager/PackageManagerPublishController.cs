using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Controls;
using Dynamo.PackageManager;

namespace Dynamo.Nodes.PackageManager
{

    public class PackageManagerPublishController
    {
        public PackageManagerClient Client { get; internal set; }
        public PackageManagerPublishUI View { get; internal set; }
        public dynBench Bench { get; internal set; }

        public PackageManagerPublishController(dynBench bench, PackageManagerClient client)
        {
            Client = client;
            View = new PackageManagerPublishUI(this);
            this.Bench = bench;
        }

    }

}
