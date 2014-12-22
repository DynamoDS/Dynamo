using System.Collections.Generic;
using System.Linq;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.UI;
using Dynamo.UI.Controls;

namespace Dynamo.Wpf.ViewModels
{
    public class ClassInformationViewModel : BrowserItemViewModel
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

        public List<HeaderStripItem> primaryHeaderStrip { get; private set; }

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

        private List<BrowserInternalElement> createMembers;
        public IEnumerable<BrowserInternalElement> CreateMembers
        {
            get { return this.createMembers; }
        }

        private List<BrowserInternalElement> actionMembers;
        public IEnumerable<BrowserInternalElement> ActionMembers
        {
            get { return this.actionMembers; }
        }

        private List<BrowserInternalElement> queryMembers;
        public IEnumerable<BrowserInternalElement> QueryMembers
        {
            get { return this.queryMembers; }
        }

        public ClassInformationViewModel()
        {
            createMembers = new List<BrowserInternalElement>();
            actionMembers = new List<BrowserInternalElement>();
            queryMembers = new List<BrowserInternalElement>();
            primaryHeaderStrip = new List<HeaderStripItem>();
        }

        public void PopulateMemberCollections(BrowserItem element)
        {
            createMembers.Clear();
            actionMembers.Clear();
            queryMembers.Clear();
            primaryHeaderStrip.Clear();

            foreach (var subElement in element.Items)
            {
                var nodeSearchEle = subElement as NodeSearchElement;
                // nodeSearchEle is null means that our subelement 
                // is not a leaf of nodes tree.
                // Normally we shouldn't have this situation.
                // TODO: discuss with product management.
                if (nodeSearchEle == null)
                    continue;

                switch (nodeSearchEle.Group)
                {
                    case SearchElementGroup.Create:
                        createMembers.Add(subElement as BrowserInternalElement);
                        break;

                    case SearchElementGroup.Action:
                        actionMembers.Add(subElement as BrowserInternalElement);
                        break;

                    case SearchElementGroup.Query:
                        queryMembers.Add(subElement as BrowserInternalElement);
                        break;
                }
            }

            string headerStripText;
            if (createMembers.Any())
                headerStripText = Configurations.HeaderCreate;
            else if (actionMembers.Any())
                headerStripText = Configurations.HeaderAction;
            else headerStripText = Configurations.HeaderQuery;

            primaryHeaderStrip.Add(new HeaderStripItem() { Text = headerStripText });
        }
    }
}
