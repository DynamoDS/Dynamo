using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace DynamoRevitServer
{
    [ServiceContract]
    public interface IDynamoRevitService
    {
        [OperationContract]
        string GetData(int value);

        [OperationContract]
        bool OpenFile(string path);
    }

}
