using System.Collections.Generic;
using System.Web.Http;
using Dynamo.Search.SearchElements;
using Dynamo.Utilities;
using DynamoService;

namespace Dynamo.SelfHostAPI
{
    [CorsSupport]
    public class NodeModelsController : ApiController
    {
        public IEnumerable<NodeModelItem> GetAllNodeModelsWithCategories()
        {
            return dynSettings.Controller.SearchViewModel.GetAllNodeModelsWithCategories();
        }
    }
}
