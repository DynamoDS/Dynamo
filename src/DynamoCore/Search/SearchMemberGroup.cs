using System.Collections.Generic;
using Dynamo.Nodes.Search;
using Dynamo.Search.SearchElements;

namespace Dynamo.Search
{
    public class SearchMemberGroup
    {
        public string Name { get; private set; }

        private List<BrowserInternalElement> members;
        public IEnumerable<BrowserInternalElement> Members
        {
            get { return members; }
        }

        //some UI properties which control style of one MemberGroup

        public SearchMemberGroup(SearchElementBase node)
        {
            members = new List<BrowserInternalElement>();
            members.Add(node);
        }
    }
}
