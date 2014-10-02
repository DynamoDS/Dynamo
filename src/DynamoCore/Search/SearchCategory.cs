using System.Collections.Generic;
using Dynamo.Nodes.Search;
using Dynamo.Search.SearchElements;

namespace Dynamo.Search
{
    public class SearchCategory
    {
        public string Name { get; private set; }

        private List<BrowserInternalElement> classes;
        public IEnumerable<BrowserInternalElement> Classes
        {
            get { return classes; }
        }

        private List<SearchMemberGroup> memberGroups;
        public IEnumerable<SearchMemberGroup> MemberGroups
        {
            get { return memberGroups; }
        }

        public SearchCategory(string name)
        {
            Name = name;

            // Add class to Classes.

            memberGroups = new List<SearchMemberGroup>();
        }

        public void AddMemberToGroup(BrowserInternalElement node)
        {

        }
    }
}
