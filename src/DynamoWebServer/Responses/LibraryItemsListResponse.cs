using System.Collections.Generic;

namespace DynamoWebServer.Responses
{
    public class LibraryItemsListResponse : Response
    {
        public IEnumerable<object> LibraryItems { get; set; }
    }
}
