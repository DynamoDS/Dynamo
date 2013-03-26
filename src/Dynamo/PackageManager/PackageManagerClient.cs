using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Greg;

namespace Dynamo.PackageManager
{
    public class PackageManagerClient

    {
        public Greg.Client Client { get; internal set; }

        public PackageManagerClient()
        {
            Client = new Greg.Client("https://accounts-dev.autodesk.com", "http://10.142.107.55:8080");
        }

        public void GetAvailable()
        {

            var req = Greg.Requests.HeaderCollectionDownload.ByEngine("dynamo");
            var response = Client.ExecuteAndDeserializeWithContent< List<Greg.Responses.PackageHeader> >(req);

            if (response.success)
            {
                foreach (var header in response.content)
                {
                    Console.WriteLine(header.name);
                }
            }
            
        }

    }
}
