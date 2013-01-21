using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Dynamo.Elements
{

    [DataContract]
    class NodeRepositoryList
    {
        [DataMember]
        internal List<string> repositories;
    }
}
