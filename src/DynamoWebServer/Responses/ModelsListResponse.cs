using System.Collections.Generic;

namespace DynamoWebServer.Responses
{
    public class ModelsListResponse : Response
    {
        public IEnumerable<object> Models { get; set; }
    }
}
