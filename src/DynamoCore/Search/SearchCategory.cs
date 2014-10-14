﻿using System.Collections.Generic;
using System.Linq;
using Dynamo.Nodes.Search;
using Dynamo.Search.SearchElements;
using Dynamo.UI;
using Dynamo.ViewModels;
using System.Collections.ObjectModel;

namespace Dynamo.Search
{
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
            string shortenedCategory = SearchViewModel.ShortenCategoryName(categoryWithGroup);

            var group = memberGroups.FirstOrDefault(mg => mg.Name == shortenedCategory);
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
    }
}
