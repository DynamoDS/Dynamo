﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dynamo.Search.SearchElements;
using Dynamo.UI;
using Dynamo.Wpf.ViewModels;
using Dynamo.Core;
using System.Windows.Input;
using Dynamo.UI.Commands;

namespace Dynamo.Search
{
    public class SearchCategory : NotificationObject, ISearchEntryViewModel
    {
        private readonly ObservableCollection<NodeCategoryViewModel> classes;
        private readonly List<SearchMemberGroup> memberGroups;

        public string Name { get; private set; }

        private bool isExpanded;
        public bool IsExpanded { get { return isExpanded; } }

        private bool isTopCategory;
        public bool IsTopCategory { get { return isTopCategory; } }

        // TODO: classes functionality.
        //       All functionality marked as 'classes functionality'
        //       Should be implemented as classes are shown in search results.
        //       http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-6198
        public ObservableCollection<NodeCategoryViewModel> Classes
        {
            get { return classes; }
        }

        public IEnumerable<SearchMemberGroup> MemberGroups
        {
            get { return memberGroups; }
        }

        public ICommand ClickedCommand { get; private set; }

        private void OnClicked(object obj)
        {
            isExpanded = !isExpanded;
            RaisePropertyChanged("IsExpanded");
        }

        internal SearchCategory(string name, bool isTopResult = false)
        {
            Name = name;
            classes = new ObservableCollection<NodeCategoryViewModel>();
            memberGroups = new List<SearchMemberGroup>();
            isExpanded = true;
            isTopCategory = isTopResult;

            ClickedCommand = new DelegateCommand(OnClicked);
        }

        internal void AddMemberToGroup(NodeSearchElementViewModel memberNode)
        {
            string categoryWithGroup = AddGroupToCategory(memberNode.Model.FullCategoryName,
                memberNode.Model.Group);
            string shortenedCategory = Nodes.Utilities.ShortenCategoryName(categoryWithGroup);

            var group = memberGroups.FirstOrDefault(mg => mg.FullyQualifiedName == shortenedCategory);
            if (group == null)
            {
                group = new SearchMemberGroup(shortenedCategory, memberNode.Category);
                memberGroups.Add(group);
            }

            group.AddMember(memberNode);
        }

        // TODO: classes functionality.
        internal void AddClassToGroup(NodeCategoryViewModel memberNode)
        {
            // TODO: The following limit of displaying only two classes are 
            // temporary, it should be updated whenever the design intent has been finalized.
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-6199

            //const int maxClassesCount = 2;
            //if (classes.Count >= maxClassesCount)
            //    return;

            // Parent should be of 'BrowserInternalElement' type or derived.
            // Root category can't be added to classes list. 
            // TODO(Vladimir): Implement the logic when classes are shown in search results.
        }

        public bool ContainsClassOrMember(NodeSearchElement member)
        {
            var memberViewModel = new NodeSearchElementViewModel(member, null);

            // TODO(Vladimir): classes functionality.
            //if (Classes.Any(cl => cl.Equals(member))) return true;

            // Search among member groups.
            return MemberGroups.Any(group => group.ContainsMember(memberViewModel));
        }

        private string AddGroupToCategory(string category, SearchElementGroup group)
        {
            switch (group)
            {
                case SearchElementGroup.Action:
                    return category + Configurations.CategoryDelimiterString + Configurations.CategoryGroupAction;
                case SearchElementGroup.Create:
                    return category + Configurations.CategoryDelimiterString + Configurations.CategoryGroupCreate;
                case SearchElementGroup.Query:
                    return category + Configurations.CategoryDelimiterString + Configurations.CategoryGroupQuery;
                default:
                    return category;
            }
        }

        public void SortChildren()
        {
            // TODO(Vladimir): classes functionality.
            //Classes.ToList().ForEach(x => x.RecursivelySort());
            MemberGroups.ToList().ForEach(x => x.Sort());
        }


        public bool Visibility
        {
            get { return true; }
        }

        public bool IsSelected
        {
            get { return false; }
        }

        public string Description
        {
            get { return String.Empty; }
        }

        public ElementTypes ElementType
        {
            get { return ElementTypes.None; }
        }

        public void Dispose()
        {

        }
    }
}
