using System.Collections.Generic;
using System.Linq;
using Dynamo.Nodes.Search;
using Dynamo.Search.SearchElements;
using Dynamo.UI;
using Dynamo.ViewModels;

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
            classes = new List<BrowserInternalElement>();
            memberGroups = new List<SearchMemberGroup>();
        }

        public void AddMemberToGroup(BrowserInternalElement memberNode)
        {
            string categoryWithGroup = AddGroupToCategory(memberNode.FullCategoryName,
                (memberNode as NodeSearchElement).Group);
            string shortenedCategory = SearchViewModel.MakeShortCategoryString(categoryWithGroup);

            var group = memberGroups.FirstOrDefault(mg => mg.Name == shortenedCategory);
            if (group == null)
            {
                group = new SearchMemberGroup(shortenedCategory);
                memberGroups.Add(group);
            }

            group.AddMember(memberNode);
        }

        public void AddClassToGroup(BrowserInternalElement memberNode)
        {
            // Here is fake implementation.
            // Added not more two different classes.
            // Will be implemented when is clarified which classes
            // should be presented in search results

            const int maxClassesCount = 2;
            if (classes.Count >= maxClassesCount)
                return;

            // Parent should be of 'BrowserInternalElement' type or derived.
            // Root category can't be added to classes list.
            var parent = memberNode.Parent as BrowserInternalElement;
            if (parent == null)
                return;

            if (classes.Any(cl => cl.Name == memberNode.Parent.Name))
                return;

            classes.Add(memberNode.Parent as BrowserInternalElement);
        }

        private string AddGroupToCategory(string category, SearchElementGroup group)
        {
            switch (group)
            {
                case SearchElementGroup.Action:
                    return category + Configurations.CATEGORY_DELIMITER + Configurations.CATEGORY_GROUP_ACTION;
                case SearchElementGroup.Create:
                    return category + Configurations.CATEGORY_DELIMITER + Configurations.CATEGORY_GROUP_CREATE;
                case SearchElementGroup.Query:
                    return category + Configurations.CATEGORY_DELIMITER + Configurations.CATEGORY_GROUP_QUERY;
                default:
                    return category;
            }
        }

        private string GetClassName(string categoryName)
        {
            List<string> splitCat = SearchModel.SplitCategoryName(categoryName);
            return splitCat.Last();
        }
    }
}
