using System.ServiceModel;
using Dynamo.Utilities;

namespace Dynamo.Revit.Server
{
    [ServiceContract]
    public interface IDynamoRevitService
    {
        [OperationContract]
        string GetData(int value);

        [OperationContract]
        bool OpenFile(string path);

        [OperationContract]
        bool OpenDynamoWorkspace(string path);

        [OperationContract]
        bool RunDynamoExpression();
    }

}
