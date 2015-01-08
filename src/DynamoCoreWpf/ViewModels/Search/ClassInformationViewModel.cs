using System.Collections.Generic;
using System.Linq;
using Dynamo.Search;
using Dynamo.UI;

namespace Dynamo.Wpf.ViewModels
{
    public class ClassInformationViewModel : NodeCategoryViewModel
    {
        private bool hideClassDetails = false;

        /// <summary>
        /// Specifies whether or not parent is root category (BrowserRootElement).
        /// </summary>
        public bool IsRootCategoryDetails { get; set; }

        /// <summary>
        /// Specifies whether or not instance should be shown as StandardPanel.
        /// </summary>
        public bool ClassDetailsVisibility
        {
            set
            {
                // If a caller sets the 'ClassDetailsVisibility' to 'false',
                // then it is intended that we hide away the class details.
                hideClassDetails = !value;
                RaisePropertyChanged("ClassDetailsVisibility");
            }

            get
            {
                if (hideClassDetails)
                    return false;

                // If we don't forcefully hide the class detail, then the overall 
                // visibility is dependent on the availability of the following lists.
                return createMembers.Any() || actionMembers.Any() || queryMembers.Any();
            }
        }

        public string PrimaryHeaderText { get; set; }
        public string SecondaryHeaderLeftText { get; set; }
        public string SecondaryHeaderRightText { get; set; }
        public bool IsPrimaryHeaderVisible { get; set; }
        public bool IsSecondaryHeaderLeftVisible { get; set; }
        public bool IsSecondaryHeaderRightVisible { get; set; }

        public enum DisplayMode { None, Query, Action };

        /// <summary>
        /// Specifies which of QueryMembers of ActionMembers list is active for the moment.
        /// If any of CreateMembers, ActionMembers or QueryMembers lists is empty
        /// it returns 'None'.
        /// </summary>
        private DisplayMode currentDisplayMode;
        public DisplayMode CurrentDisplayMode
        {
            get
            {
                return currentDisplayMode;
            }
            set
            {
                currentDisplayMode = value;
                RaisePropertyChanged("CurrentDisplayMode");
            }
        }

        private int hiddenSecondaryMembersCount;
        public int HiddenSecondaryMembersCount
        {
            get { return hiddenSecondaryMembersCount; }
            set
            {
                hiddenSecondaryMembersCount = value;
                RaisePropertyChanged("HiddenSecondaryMembersCount");
                RaisePropertyChanged("MoreButtonText");
            }
        }

        public string MoreButtonText
        {
            get
            {
                var count = HiddenSecondaryMembersCount;
                return string.Format(Configurations.MoreButtonTextFormat, count);
            }
        }

        private List<NodeSearchElementViewModel> createMembers;
        public IEnumerable<NodeSearchElementViewModel> CreateMembers
        {
            get { return this.createMembers; }
        }

        private List<NodeSearchElementViewModel> actionMembers;
        public IEnumerable<NodeSearchElementViewModel> ActionMembers
        {
            get { return this.actionMembers; }
        }

        private List<NodeSearchElementViewModel> queryMembers;
        public IEnumerable<NodeSearchElementViewModel> QueryMembers
        {
            get { return this.queryMembers; }
        }

        public ClassInformationViewModel()
            : base("")
        {
            createMembers = new List<NodeSearchElementViewModel>();
            actionMembers = new List<NodeSearchElementViewModel>();
            queryMembers = new List<NodeSearchElementViewModel>();
        }

        public void PopulateMemberCollections(NodeCategoryViewModel element)
        {
            createMembers.Clear();
            actionMembers.Clear();
            queryMembers.Clear();

            foreach (var subElement in element.Entries)
            {
                switch (subElement.Model.Group)
                {
                    case SearchElementGroup.Create:
                        createMembers.Add(subElement);
                        break;

                    case SearchElementGroup.Action:
                        actionMembers.Add(subElement);
                        break;

                    case SearchElementGroup.Query:
                        queryMembers.Add(subElement);
                        break;
                }
            }
        }
    }
}
