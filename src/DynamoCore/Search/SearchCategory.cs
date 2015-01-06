using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dynamo.Search.SearchElements;
using Dynamo.UI;

namespace Dynamo.Search
{
#if false
    public class SearchCategory
    {
        private readonly ObservableCollection<BrowserItem> classes;
        private readonly List<SearchMemberGroup> memberGroups;

        public string Name { get; private set; }

        public ObservableCollection<BrowserItem> Classes
        {
            get { return classes; }
        }

        public IEnumerable<SearchMemberGroup> MemberGroups
        {
            get { return memberGroups; }
        }

        internal SearchCategory(string name)
        {
            Name = name;
            classes = new ObservableCollection<BrowserItem>();
            memberGroups = new List<SearchMemberGroup>();
        }

        internal void AddMemberToGroup(NodeSearchElement memberNode)
        {
            string categoryWithGroup = AddGroupToCategory(memberNode.FullCategoryName,
                memberNode.Group);
            string shortenedCategory = SearchModel.ShortenCategoryName(categoryWithGroup);

            var group = memberGroups.FirstOrDefault(mg => mg.FullyQualifiedName == shortenedCategory);
            if (group == null)
            {
                group = new SearchMemberGroup(shortenedCategory);
                memberGroups.Add(group);
            }

            group.AddMember(memberNode);
        }

        internal void AddClassToGroup(BrowserInternalElement memberNode)
        {
            // TODO(Vladimir): The following limit of displaying only two classes are 
            // temporary, it should be updated whenever the design intent has been finalized.

            const int maxClassesCount = 2;
            if (classes.Count >= maxClassesCount)
                return;

            // Parent should be of 'BrowserInternalElement' type or derived.
            // Root category can't be added to classes list.
            var parent = memberNode.Parent as BrowserInternalElement;
            if (parent == null)
                return;

            if (!classes.Any(cl => cl.Name == parent.Name))
                classes.Add(parent);
        }

        public bool ContainsClassOrMember(BrowserInternalElement member)
        {
            // Search among classes.
            if (Classes.Any(cl => cl.Equals(member))) return true;

            // Search among member groups.
            return MemberGroups.Any(group => group.ContainsMember(member));
        }

        private string AddGroupToCategory(string category, SearchElementGroup group)
        {
            switch (group)
            {
                case SearchElementGroup.Action:
                    return category + Configurations.CategoryDelimiter + Configurations.CategoryGroupAction;
                case SearchElementGroup.Create:
                    return category + Configurations.CategoryDelimiter + Configurations.CategoryGroupCreate;
                case SearchElementGroup.Query:
                    return category + Configurations.CategoryDelimiter + Configurations.CategoryGroupQuery;
                default:
                    return category;
            }
        }

        public void SortChildren()
        {
            Classes.ToList().ForEach(x => x.RecursivelySort());
            MemberGroups.ToList().ForEach(x => x.Sort());
        }
    }
#endif
}
