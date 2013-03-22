using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamo.PackageManager
{
    public class PackageManager
    {

        private static readonly Greg.Client _client = new Greg.Client();
        public Greg.Client Client { get { return _client;  } }

        public PackageManager()
        {

        }

    }
}
