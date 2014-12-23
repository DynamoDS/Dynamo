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

        public List<HeaderStripItem> PrimaryHeaderStrip { get; private set; }
        public List<HeaderStripItem> SecondaryHeaderStrip { get; private set; }

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

        public bool AreSecondaryHeadersVisible
        {
            get
            {
                return SecondaryHeaderStrip.Any();
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
            PrimaryHeaderStrip = new List<HeaderStripItem>();
            SecondaryHeaderStrip = new List<HeaderStripItem>();
        }

        public void PopulateMemberCollections(BrowserItem element)
        {
            createMembers.Clear();
            actionMembers.Clear();
            queryMembers.Clear();
            PrimaryHeaderStrip.Clear();
            SecondaryHeaderStrip.Clear();

            foreach (var subElement in element.Items)
            {
                var nodeSearchEle = subElement as NodeSearchElement;                
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

            string headerStripText = string.Empty;
            if (createMembers.Any())
            {
                headerStripText = Configurations.HeaderCreate;
            }

            if (actionMembers.Any())
            {
                if (string.IsNullOrEmpty(headerStripText))
                    headerStripText = Configurations.HeaderAction;
                else
                    SecondaryHeaderStrip.Add(new HeaderStripItem() { Text = Configurations.HeaderAction });
            }

            if (queryMembers.Any())
            {
                if (string.IsNullOrEmpty(headerStripText))
                    headerStripText = Configurations.HeaderQuery;
                else
                    SecondaryHeaderStrip.Add(new HeaderStripItem() { Text = Configurations.HeaderQuery });
            }

            PrimaryHeaderStrip.Add(new HeaderStripItem() { Text = headerStripText });
        }
    }
}
