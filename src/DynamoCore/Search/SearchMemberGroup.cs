using System.Collections.Generic;
using Dynamo.Nodes.Search;
using System.Linq;

namespace Dynamo.Search
{
    public class SearchMemberGroup
    {
        private readonly List<BrowserInternalElement> members;

        public string Name { get; private set; }

        public IEnumerable<BrowserInternalElement> Members
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

        public bool ContainsMember(BrowserInternalElement member)
        {
           return Members.Contains(member);
        }
    }
}
