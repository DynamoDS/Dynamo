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

        internal static SearchMemberGroup CreateInstance(BrowserInternalElement node)
        {
            // Some logic in which member group name is determined.
            var name = "Mockup";

            return new SearchMemberGroup(name);
        }

        private SearchMemberGroup(string name)
        {
            Name = name;
            members = new List<BrowserInternalElement>();
        }

        //some UI properties which control style of one MemberGroup

        public void AddMember(BrowserInternalElement node)
        {
            members.Add(node);
        }
    }
}
