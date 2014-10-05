using System.Collections.Generic;
using Dynamo.Nodes.Search;
using Dynamo.ViewModels;
using System.Linq;
using Dynamo.UI;

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
            memberGroups = new List<SearchMemberGroup>();
        }

        public void AddMemberToGroup(BrowserInternalElement node)
        {
            string categoryWithGroup = AddGroupToCategory(node.FullCategoryName, ((SearchElements.NodeSearchElement)node).Group);
            string groupName = SearchViewModel.MakeShortCategoryString(categoryWithGroup);

            var group = memberGroups.FirstOrDefault(mg => mg.Name == groupName);
            if (group == null)
            {
                group = SearchMemberGroup.CreateInstance(groupName);
                memberGroups.Add(group);
            }

            group.AddMember(node);
        }

        public void AddClassToGroup(BrowserInternalElement node)
        {

        }

        private string AddGroupToCategory(string category, SearchElementGroup group)
        {
            switch (group)
            {
                case SearchElementGroup.Action:
                    return category + Configurations.CATEGORY_DELIMITER + SearchModel.CATEGORY_GROUP_ACTIONS;
                case SearchElementGroup.Create:
                    return category + Configurations.CATEGORY_DELIMITER + SearchModel.CATEGORY_GROUP_CREATE;
                case SearchElementGroup.Query:
                    return category + Configurations.CATEGORY_DELIMITER + SearchModel.CATEGORY_GROUP_QUERY;
                default:
                    return category;
            }
        }
    }
}
