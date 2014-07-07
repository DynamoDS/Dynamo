using Dynamo.Utilities;
using System.Runtime.Serialization;

namespace Dynamo.Messages
{
    [DataContract]
    class ModelsListMessage : Message
    {
        internal override void Execute(ViewModels.DynamoViewModel dynamoViewModel)
        {
            string json = dynSettings.Controller.SearchViewModel.GetAllNodesWithCategoriesInJson();
            OnAnswer(json);
        }
    }
}
