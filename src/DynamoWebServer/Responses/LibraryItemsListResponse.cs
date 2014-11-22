using Dynamo.Search.SearchElements;
using System.Collections.Generic;

namespace DynamoWebServer.Responses
{
    public class LibraryItemsListResponse : Response
    {
        public IEnumerable<LibraryItem> LibraryItems { get; private set; }

        public LibraryItemsListResponse(IEnumerable<LibraryItem> libraryItems)
        {
            LibraryItems = libraryItems;
        }
    }
}
