using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamo.PackageManager
{
    /// <summary>
    /// This class is for storing package locally.  It is serialized to JSON
    /// and stored with the package itself.  It is deserialized by Dynamo in order
    /// to learn about a package. 
    /// </summary>
    public class PackageHeader
    {
        public string name { get; set; }
        public string description { get; set; }
        public string group { get; set; }
        public List<string> keywords { get; set; }
        public string version { get; set; }
        public string engine_version { get; set; }
    }
}
