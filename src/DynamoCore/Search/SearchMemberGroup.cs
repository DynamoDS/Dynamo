using System.Collections.Generic;
using Dynamo.Nodes.Search;

namespace Dynamo.Search
{
    public class SearchMemberGroup
    {
        private readonly List<BrowserInternalElement> members;

        internal string Name { get; private set; }

        internal IEnumerable<BrowserInternalElement> Members
        {
            get { return members; }
        }

        internal SearchMemberGroup(string name)
        {
            Name = name;
            members = new List<BrowserInternalElement>();
        }

        //some UI properties which control style of one MemberGroup

        internal void AddMember(BrowserInternalElement node)
        {
            members.Add(node);
        }
    }
}
