using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Dynamo.Logging;
using Dynamo.Wpf.ViewModels.Core;

namespace Dynamo.ViewModels
{
    public class InfoBubbleEventArgs : EventArgs
    {
        public enum Request
        {
            FadeIn, FadeOut, Show, Hide
        }

        public Request RequestType { get; private set; }

        public InfoBubbleEventArgs(Request request)
        {
            this.RequestType = request;
        }
    }

    public delegate void InfoBubbleEventHandler(object sender, InfoBubbleEventArgs e);

    public partial class InfoBubbleViewModel : ViewModelBase
    {
        public enum Style
        {
            None, 
            Warning,
            WarningCondensed, // Obsolete
            Error,
            ErrorCondensed, // Obsolete
            Info,
        }

        [Obsolete]
        public enum Direction
        {
            None,
            Left,
            Top,
            Right,
            Bottom,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        [Obsolete]
        public enum State
        {
            Minimized,
            Pinned
        }

        #region Properties

        // Each new node placed on the canvas has an incremented ZIndex
        // In order to stay above these, we need a high ZIndex value. 
        private double zIndex;
        private Style infoBubbleStyle;
        
        [Obsolete]
        public Direction connectingDirection;
        
        [Obsolete]
        private string content;
        private Uri documentationLink;
        
        [Obsolete]
        public Point targetTopLeft;
        
        [Obsolete]
        public Point targetBotRight;
        
        [Obsolete]
        private Direction limitedDirection;
        
        [Obsolete]
        private State infoBubbleState;
        private double bubbleWidth = 300.0;

        // Determines whether or not the body of each information bubble is shown. 
        // This relates to the message text box, and any buttons used to dismiss/display messages.
        private bool nodeInfoSectionExpanded;
        private bool nodeWarningsSectionExpanded;
        private bool nodeErrorsSectionExpanded;

        // Determines whether the info, warnings and errors are displaying just an icon, a single error message,
        // or all messages at once.
        private NodeMessageVisibility nodeInfoVisibilityState = NodeMessageVisibility.Icon;
        private NodeMessageVisibility nodeWarningsVisibilityState = NodeMessageVisibility.Icon;
        private NodeMessageVisibility nodeErrorsVisibilityState = NodeMessageVisibility.Icon;

        // Relates to whether the info/warning/error message bodies display a button saying
        // 'show more' or 'show less'.
        private bool nodeInfoShowLessMessageVisible;
        private bool nodeWarningsShowLessMessageVisible;
        private bool nodeErrorsShowLessMessageVisible;

        /// <summary>
        /// Determines whether any messages are shown to the user at all; switches chevron icon on/off in the view.
        /// Nodes inside of collapsed groups do not display an error bubble.
        /// </summary>
        public bool DoesNodeDisplayMessages =>
            !IsCollapsed &&
            NodeWarningsToDisplay.Count + NodeErrorsToDisplay.Count + NodeInfoToDisplay.Count > 0;
        
        /// <summary>
        /// Determines whether the show more/show less buttons are visible to the user at the Info Message level.
        /// </summary>
        public bool NodeInfoShowMoreButtonVisible => NodeInfoSectionExpanded && NodeInfoIteratorVisible;

        /// <summary>
        /// Determines whether the show more/show less buttons are visible to the user at the Warning level.
        /// </summary>
        public bool NodeWarningsShowMoreButtonVisible => NodeWarningsSectionExpanded && NodeWarningsIteratorVisible;
        
        /// <summary>
        /// Determines whether the show more/show less buttons are visible to the user at the Error level.
        /// </summary>
        public bool NodeErrorsShowMoreButtonVisible => NodeErrorsSectionExpanded && NodeErrorsIteratorVisible;

        /// <summary>
        /// Determines what the show more/show less buttons say at the Info Message level, which depends on whether
        /// the NodeMessageVisibility at this level is set to CollapseMessages or ShowAllMessages.
        /// </summary>
        public bool NodeInfoShowLessMessageVisible
        {
            get => nodeInfoShowLessMessageVisible;
            set
            {
                nodeInfoShowLessMessageVisible = value;
                RaisePropertyChanged(nameof(NodeInfoShowLessMessageVisible));
            }
        }

        /// <summary>
        /// Determines what the show more/show less buttons say at the Warning level, which depends on whether
        /// the NodeMessageVisibility at this level is set to CollapseMessages or ShowAllMessages.
        /// </summary>
        public bool NodeWarningsShowLessMessageVisible
        {
            get => nodeWarningsShowLessMessageVisible;
            set
            {
                nodeWarningsShowLessMessageVisible = value;
                RaisePropertyChanged(nameof(NodeWarningsShowLessMessageVisible));
            }
        }

        /// <summary>
        /// Determines what the show more/show less buttons say at the Error Message level, which depends on whether
        /// the NodeMessageVisibility at this level is set to CollapseMessages or ShowAllMessages.
        /// </summary>
        public bool NodeErrorsShowLessMessageVisible
        {
            get => nodeErrorsShowLessMessageVisible;
            set
            {
                nodeErrorsShowLessMessageVisible = value;
                RaisePropertyChanged(nameof(NodeErrorsShowLessMessageVisible));
            }
        }

        [Obsolete]
        public string Content
        {
            get { return content; }
            set { content = value; RaisePropertyChanged("Content"); }
        }

        public DynamoViewModel DynamoViewModel { get; private set; }


        public double ZIndex
        {
            get => zIndex; 
            set
            {
                zIndex = value;
                RaisePropertyChanged(nameof(ZIndex));
            }
        }

        /// <summary>
        /// Bound to the width of the InfoBubbleView, to ensure it has the same width as the NodeModel's width.
        /// </summary>
        public double BubbleWidth
        {
            get => bubbleWidth;
            set
            {
                bubbleWidth = value;
                RaisePropertyChanged(nameof(BubbleWidth));
            }
        }

        public Style InfoBubbleStyle
        {
            get { return infoBubbleStyle; }
            set { infoBubbleStyle = value; RaisePropertyChanged("InfoBubbleStyle"); }
        }

        [Obsolete]
        public string FullContent;

        public Direction ConnectingDirection
        {
            get { return connectingDirection; }
            set { connectingDirection = value; RaisePropertyChanged("ConnectingDirection"); }
        }

        public Uri DocumentationLink
        {
            get { return documentationLink; }
            set { documentationLink = value; RaisePropertyChanged(nameof(DocumentationLink)); }
        }

        [Obsolete]
        public Point TargetTopLeft
        {
            get { return targetTopLeft; }
            set { targetTopLeft = value; RaisePropertyChanged("TargetTopLeft"); }
        }
        
        [Obsolete]
        public Point TargetBotRight
        {
            get { return targetBotRight; }
            set { targetBotRight = value; RaisePropertyChanged("TargetBotRight"); }
        }

        [Obsolete]
        public Direction LimitedDirection
        {
            get { return limitedDirection; }
            set { limitedDirection = value; RaisePropertyChanged("LimitedDirection"); }
        }

        [Obsolete]
        public double Left
        {
            get { return 0; }
        }

        [Obsolete]
        public double Top
        {
            get { return 0; }
        }

        [Obsolete]
        public State InfoBubbleState
        {
            get { return infoBubbleState; }
            set { infoBubbleState = value; RaisePropertyChanged("InfoBubbleState"); }
        }

        private bool isCollapsed;

        /// <summary>
        /// Reports whether the node this InfoBubble is inside a collapsed group.
        /// </summary>
        public override bool IsCollapsed
        {
            get => isCollapsed;
            set
            {
                isCollapsed = value;
                RaisePropertyChanged(nameof(IsCollapsed));
                RaisePropertyChanged(nameof(DoesNodeDisplayMessages));
            }
        }


        /// <summary>
        /// A collection of formatted, user-visible strings relating to the node's information state
        /// </summary>
        public ObservableCollection<InfoBubbleDataPacket> NodeInfoToDisplay { get; } = new ObservableCollection<InfoBubbleDataPacket>();

        /// <summary>
        /// A collection of formatted, user-visible strings relating to the node's warning state
        /// </summary>
        public ObservableCollection<InfoBubbleDataPacket> NodeWarningsToDisplay { get; } = new ObservableCollection<InfoBubbleDataPacket>();

        /// <summary>
        /// A collection of formatted, user-visible strings relating to the node's error state
        /// </summary>
        public ObservableCollection<InfoBubbleDataPacket> NodeErrorsToDisplay { get; } = new ObservableCollection<InfoBubbleDataPacket>();    

        /// <summary>
        /// A collection of InfoBubbleDataPacket objects that are received when the node executes
        /// and its state changes to reflect errors or warnings that have been detected.
        /// </summary>
        public ObservableCollection<InfoBubbleDataPacket> NodeMessages { get; } = new ObservableCollection<InfoBubbleDataPacket>();
        
        /// <summary>
        /// A collection of messages this node has received that have been manually dismissed by the user.
        /// </summary>
        public ObservableCollection<InfoBubbleDataPacket> DismissedMessages { get; } = new ObservableCollection<InfoBubbleDataPacket>();

        /// <summary>
        /// Used to determine whether the UI container for node Info is visible
        /// </summary>
        public bool NodeInfoVisible => NodeInfoToDisplay.Count > 0;

        /// <summary>
        /// Used to determine whether the UI container for node Warnings is visible
        /// </summary>
        public bool NodeWarningsVisible => NodeWarningsToDisplay.Count > 0;
        
        /// <summary>
        /// Used to determine whether the UI container for node Errors is visible
        /// </summary>
        public bool NodeErrorsVisible => NodeErrorsToDisplay.Count > 0;
        

        /// <summary>
        /// Used to determine whether the UI container for node Info is expanded
        /// </summary>
        public bool NodeInfoSectionExpanded
        {
            get => nodeInfoSectionExpanded;
            set
            {
                nodeInfoSectionExpanded = value;
                RaisePropertyChanged(nameof(NodeInfoSectionExpanded));
            }
        }

        /// <summary>
        /// Used to determine whether the UI container for node Warnings is expanded
        /// </summary>
        public bool NodeWarningsSectionExpanded
        {
            get => nodeWarningsSectionExpanded;
            set
            {
                nodeWarningsSectionExpanded = value;
                RaisePropertyChanged(nameof(NodeWarningsSectionExpanded));
            }
        }

        /// <summary>
        /// Used to determine whether the UI container for node Errors is expanded
        /// </summary>
        public bool NodeErrorsSectionExpanded
        {
            get => nodeErrorsSectionExpanded;
            set
            {
                nodeErrorsSectionExpanded = value;
                RaisePropertyChanged(nameof(NodeErrorsSectionExpanded));
            }
        }

        /// <summary>
        /// Represents whether a node information state (e.g. warnings) are displaying just the
        /// warning icon, a condensed summary of all messages at this level or fully-displaying each message
        /// </summary>
        internal enum NodeMessageVisibility { Icon, CollapseMessages, ShowAllMessages }

        /// <summary>
        /// Determines whether the node infos are showing just an icon, a condensed summary of messages or
        /// displaying each message in turn.
        /// </summary>
        internal NodeMessageVisibility NodeInfoVisibilityState
        {
            get => nodeInfoVisibilityState;
            set
            {
                nodeInfoVisibilityState = value;
                RaisePropertyChanged(nameof(NodeInfoVisibilityState));
                
                RefreshNodeInformationalStateDisplay();
            }
        }

        /// <summary>
        /// Determines whether the node warnings are showing just an icon, a condensed summary of messages or
        /// displaying each message in turn.
        /// </summary>
        internal NodeMessageVisibility NodeWarningsVisibilityState
        {
            get => nodeWarningsVisibilityState;
            set
            {
                nodeWarningsVisibilityState = value;
                RaisePropertyChanged(nameof(NodeWarningsVisibilityState));
                RefreshNodeInformationalStateDisplay();
            }
        }

        /// <summary>
        /// Determines whether the node errors showing just an icon, a condensed summary of messages or
        /// displaying each message in turn.
        /// </summary>
        internal NodeMessageVisibility NodeErrorsVisibilityState
        {
            get => nodeErrorsVisibilityState;
            set
            {
                nodeErrorsVisibilityState = value;
                RaisePropertyChanged(nameof(NodeErrorsVisibilityState));
                RefreshNodeInformationalStateDisplay();
            }
        }

        /// <summary>
        /// Used to switch out the DataTemplate from one that shows an iterator beside each message
        /// to one which doesn't. This is necessary because empty TextBlock Runs are non zero-width
        /// and cannot have their Width (or Visibility) set manually.
        /// </summary>
        public bool NodeInfoIteratorVisible =>
            GetMessagesOfStyle(NodeMessages, Style.Info).Count -
            GetMessagesOfStyle(DismissedMessages, Style.Info).Count > 1;

        /// <summary>
        /// Used to switch out the DataTemplate from one that shows an iterator beside each message
        /// to one which doesn't. This is necessary because empty TextBlock Runs are non zero-width
        /// and cannot have their Width (or Visibility) set manually.
        /// </summary>
        public bool NodeWarningsIteratorVisible
        {
            get
            {
                List<InfoBubbleDataPacket> messages = GetMessagesOfStyle(NodeMessages, Style.Warning);
                List<InfoBubbleDataPacket> dismissedMessages = GetMessagesOfStyle(DismissedMessages, Style.Warning);
                
                return messages.Count - dismissedMessages.Count > 1;
            }
        }
         
        /// <summary>
        /// Used to switch out the DataTemplate from one that shows an iterator beside each message
        /// to one which doesn't. This is necessary because empty TextBlock Runs are non zero-width
        /// and cannot have their Width (or Visibility) set manually.
        /// </summary>
        public bool NodeErrorsIteratorVisible => GetMessagesOfStyle(NodeMessages, Style.Error).Count > 1;
        
        #endregion

        #region Event Handlers

        public event InfoBubbleEventHandler RequestAction;

        public void OnRequestAction(InfoBubbleEventArgs e)
        {
            if (RequestAction != null)
                RequestAction(this, e);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dynamoViewModel"></param>
        public InfoBubbleViewModel(DynamoViewModel dynamoViewModel)
        {
            InitializeInfoBubble(dynamoViewModel);
        }

        /// <summary>
        /// This constructor will update the TopBottom and BottomRight positions of the warnings since the beginning.
        /// </summary>
        /// <param name="nodeViewModel">This parameter will be used to get the TopBottom and BottomRight positions of the NodeModel</param>
        public InfoBubbleViewModel(NodeViewModel nodeViewModel)
        {
            if (nodeViewModel != null)
            {
                InitializeInfoBubble(nodeViewModel.DynamoViewModel);

            
                var data = new InfoBubbleDataPacket
                {
                    TopLeft = nodeViewModel.GetTopLeft(),
                    BotRight = nodeViewModel.GetBotRight()
                };
                UpdatePosition(data);
            }
        }

        private void InitializeInfoBubble(DynamoViewModel dynamoViewModel)
        {
            this.DynamoViewModel = dynamoViewModel;

            // Default values
            limitedDirection = Direction.None;
            ConnectingDirection = Direction.None;
            Content = string.Empty;
            DocumentationLink = null;
            InfoBubbleStyle = Style.None;
            InfoBubbleState = State.Minimized;

            NodeMessages.CollectionChanged += NodeInformation_CollectionChanged;

            RefreshNodeInformationalStateDisplay();
        }

        #endregion

        /// <summary>
        /// Rebuilds the user-facing message collections when the underlying messages coming from the node evaluation change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NodeInformation_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RefreshNodeInformationalStateDisplay();
        }

        #region Command Methods

        [Obsolete]
        private void UpdateInfoBubbleContent(object parameter)
        {
            InfoBubbleDataPacket data = (InfoBubbleDataPacket)parameter;

            InfoBubbleStyle = data.Style;
            ConnectingDirection = data.ConnectingDirection;
            UpdateContent(data.Text);
            TargetTopLeft = data.TopLeft;
            TargetBotRight = data.BotRight;
            DocumentationLink = data.Link;
        }

        [Obsolete]
        private bool CanUpdateInfoBubbleCommand(object parameter)
        {
            return true;
        }

        private void UpdatePosition(object parameter)
        {
            InfoBubbleDataPacket data = (InfoBubbleDataPacket)parameter;

            TargetTopLeft = data.TopLeft;
            TargetBotRight = data.BotRight;
        }

        [Obsolete]
        private bool CanUpdatePosition(object parameter)
        {
            return true;
        }

        [Obsolete]
        private void Resize(object parameter)
        {
            UpdateContent(FullContent);
        }

        [Obsolete]
        private bool CanResize(object parameter)
        {
            return true;
        }

        [Obsolete]
        private void ChangeInfoBubbleState(object parameter)
        {
            if (parameter is InfoBubbleViewModel.State)
            {
                InfoBubbleViewModel.State newState = (InfoBubbleViewModel.State)parameter;

                InfoBubbleState = newState;
            }
        }

        [Obsolete]
        private bool CanChangeInfoBubbleState(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Used to open the documentation link relating to an error/warning (if any exists)
        /// </summary>
        /// <param name="parameter"></param>
        private void OpenDocumentationLink(object parameter)
        {
            if (parameter is InfoBubbleDataPacket infoBubbleDataPacket)
            {
                var link = infoBubbleDataPacket.Link;
                if (link != null)
                {
                    var targetContent = new OpenDocumentationLinkEventArgs((Uri)link);
                    this.DynamoViewModel.OpenDocumentationLink(targetContent);
                }
            }
            else
            {
                var url = parameter as Uri;
                if (url != null)
                {
                    var targetContent = new OpenDocumentationLinkEventArgs((Uri)parameter);
                    this.DynamoViewModel.OpenDocumentationLink(targetContent);
                }
            }
        }

        private bool CanOpenDocumentationLink(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Fired by the users to manually 'dismiss' a user-facing message, such as a Warning or an Error.
        /// Adds the message(s) to the collection of DismissedAlerts, then rebuilds the
        /// user-facing messages display from scratch.
        /// </summary>
        /// <param name="parameter"></param>
        private void DismissMessage(object parameter)
        {
            if (!(parameter is InfoBubbleDataPacket infoBubbleDataPacket)) return;

            // If we're dismissing a message we have more than once, we dismiss them all in one go.
            int messageCount = NodeMessages.Count(x => x.Message == infoBubbleDataPacket.Message);

            // This loop handles dismissing multiple messages with the same text.
            for (int i = 0; i < messageCount; i++)
            {
                DismissedMessages.Add(infoBubbleDataPacket);
            }           

            RefreshNodeInformationalStateDisplay();
            Analytics.TrackEvent(Actions.Dismiss, Categories.NodeOperations, "Warning", messageCount);
            ValidateWorkspaceRunStatusMsg();
        }

        /// <summary>
        /// Accessed via the node's ContextMenu, used to make a user-facing message reappear above the node.
        /// </summary>
        /// <param name="parameter"></param>
        private void UndismissMessage(object parameter)
        {
            if (!(parameter is string value)) return;

            // We match the object to undismiss using the full warning message
            for (int i = DismissedMessages.Count - 1; i >= 0; i--)
            {
                if (DismissedMessages[i].Message != value) continue;
                DismissedMessages.Remove(DismissedMessages[i]);
            }
            RefreshNodeInformationalStateDisplay();
            ValidateWorkspaceRunStatusMsg();
        }


        [Obsolete]
        // TODO:Kahheng Refactor away these
        #region TODO:Kahheng Refactor away these
        private void ShowFullContent(object parameter)
        {
            if (parameter != null && parameter is InfoBubbleDataPacket)
            {
                InfoBubbleDataPacket data = (InfoBubbleDataPacket)parameter;
                InfoBubbleStyle = data.Style;
            }

            Content = FullContent;
        }

        [Obsolete]
        private bool CanShowFullContent(object parameter)
        {
            return true;
        }

        [Obsolete]
        private void ShowCondensedContent(object parameter)
        {
            if (parameter != null && parameter is InfoBubbleDataPacket)
            {
                InfoBubbleDataPacket data = (InfoBubbleDataPacket)parameter;
                InfoBubbleStyle = data.Style;
            }

            // Generate condensed content
            GenerateContent();
        }

        [Obsolete]
        private bool CanShowCondensedContent(object parameter)
        {
            return true;
        }
        #endregion
        #endregion

        #region Private Helper Method

        [Obsolete]
        private void UpdateContent(string text)
        {
            FullContent = text;

            // Generate initial condensed content (if needed) whenever bubble content is updated
            GenerateContent();
        }

        [Obsolete]
        private void GenerateContent()
        {
            switch (InfoBubbleStyle)
            {
                case Style.WarningCondensed:
                case Style.ErrorCondensed:
                    Content = "...";
                    break;
                default:
                    Content = FullContent;
                    break;
            }
        }
        #endregion

        /// <summary>
        /// Checks whether to show/hide all user facing collections.
        /// </summary>
        private void UpdateUserFacingCollectionVisibility()
        {
            RaisePropertyChanged(nameof(NodeInfoVisible));
            RaisePropertyChanged(nameof(NodeWarningsVisible));
            RaisePropertyChanged(nameof(NodeErrorsVisible));
        }

        /// <summary>
        /// Clears all collections of user-facing info/warning/errors.
        /// </summary>
        private void ClearUserFacingCollections()
        {
            NodeErrorsToDisplay.Clear();
            NodeWarningsToDisplay.Clear();
            NodeInfoToDisplay.Clear();
        }

        /// <summary>
        /// Determines whether to display the message body to the user.
        /// </summary>
        private void SetMessageSectionsExpansion()
        {
            NodeInfoSectionExpanded = nodeInfoVisibilityState != NodeMessageVisibility.Icon;
            NodeWarningsSectionExpanded = nodeWarningsVisibilityState != NodeMessageVisibility.Icon;
            NodeErrorsSectionExpanded = nodeErrorsVisibilityState != NodeMessageVisibility.Icon;
        }

        /// <summary>
        /// Filters a collection of InfoBubbleDataPackets by whether they have a given style (e.g. Error).
        /// </summary>
        /// <param name="infoBubbleDataPackets"></param>
        /// <param name="style">The style of messages to be returned</param>
        /// <returns></returns>
        private List<InfoBubbleDataPacket> GetMessagesOfStyle(ObservableCollection<InfoBubbleDataPacket> infoBubbleDataPackets, Style style)
        {
            Style secondStyle = GetAlternativeStyle(style);
            return infoBubbleDataPackets.Where(x => x.Style == style || x.Style == secondStyle).ToList();
        }

        /// <summary>
        /// Determines whether the 'show more/less' button says 'show more' or 'show less' for each section.
        /// </summary>
        private void UpdateShowMoreButtonText()
        {
            NodeInfoShowLessMessageVisible = NodeInfoShowMoreButtonVisible && NodeInfoVisibilityState == NodeMessageVisibility.ShowAllMessages;
            NodeWarningsShowLessMessageVisible = NodeWarningsShowMoreButtonVisible && NodeWarningsVisibilityState == NodeMessageVisibility.ShowAllMessages;
            NodeErrorsShowLessMessageVisible = NodeErrorsShowMoreButtonVisible && NodeErrorsVisibilityState == NodeMessageVisibility.ShowAllMessages;
        }

        /// <summary>
        /// Determines whether the 'Show More' button is visible to the user, which displays when
        /// the node is in a CollapsedMessages visibility state and there is more than one message to show
        /// at this particular message level (e.g. Warning).
        /// </summary>
        private void UpdateShowMoreButtonVisibility()
        {
            RaisePropertyChanged(nameof(NodeInfoShowMoreButtonVisible));
            RaisePropertyChanged(nameof(NodeWarningsShowMoreButtonVisible));
            RaisePropertyChanged(nameof(NodeErrorsShowMoreButtonVisible));
        }

        /// <summary>
        /// Triggers an update to the visibility of the iterators beside messages when there is
        /// more than one message per level, (e.g. 5 Warnings, showing 1/5, 2/5, etc).
        /// </summary>
        private void UpdateNodeMessageIteratorsVisibility()
        {
            RaisePropertyChanged(nameof(NodeInfoIteratorVisible));
            RaisePropertyChanged(nameof(NodeWarningsIteratorVisible));
            RaisePropertyChanged(nameof(NodeErrorsIteratorVisible));
        }

        /// <summary>
        /// A UX refinement; if a user dismisses a message and one remains, we need to switch off the iterator (to stop it saying 1/1)
        /// and auto-collapse that section back to CollapseMessages if it was on ShowAllMessages.
        /// </summary>
        /// <param name="nonDismissedInfoMessageCount"></param>
        /// <param name="nonDismissedWarningMessageCount"></param>
        /// <param name="errorsCount"></param>
        private void AutoCollapseSections(int nonDismissedInfoMessageCount, int nonDismissedWarningMessageCount, int errorsCount)
        {
            if (NodeInfoVisibilityState == NodeMessageVisibility.ShowAllMessages && nonDismissedInfoMessageCount < 2)
            {
                NodeInfoVisibilityState = NodeMessageVisibility.CollapseMessages;
                RaisePropertyChanged(nameof(NodeInfoIteratorVisible));
            }
            if (NodeWarningsVisibilityState == NodeMessageVisibility.ShowAllMessages && nonDismissedWarningMessageCount < 2)
            {
                NodeWarningsVisibilityState = NodeMessageVisibility.CollapseMessages;
                RaisePropertyChanged(nameof(NodeWarningsIteratorVisible));
            }
            if (NodeErrorsVisibilityState == NodeMessageVisibility.ShowAllMessages && errorsCount < 2)
            {
                NodeErrorsVisibilityState = NodeMessageVisibility.CollapseMessages;
                RaisePropertyChanged(nameof(NodeErrorsIteratorVisible));
            }
        }

        /// <summary>
        /// Refreshes all of the user-facing Node Informational State UI.
        /// </summary>
        public void RefreshNodeInformationalStateDisplay()
        {
            ClearUserFacingCollections();

            // Messages arriving at the node
            List<InfoBubbleDataPacket> infoMessages = GetMessagesOfStyle(NodeMessages, Style.Info);
            List<InfoBubbleDataPacket> warningMessages = GetMessagesOfStyle(NodeMessages, Style.Warning);
            List<InfoBubbleDataPacket> errorMessages = GetMessagesOfStyle(NodeMessages, Style.Error);

            // Messages which have already been dismissed by the user
            List<InfoBubbleDataPacket> dismissedInfoMessages = GetMessagesOfStyle(DismissedMessages, Style.Info);
            List<InfoBubbleDataPacket> dismissedWarningMessages = GetMessagesOfStyle(DismissedMessages, Style.Warning);

            // The number of messages to actually show the user: messages received minus messages dismissed.
            int nonDismissedInfoMessageCount = infoMessages.Count - dismissedInfoMessages.Count;
            int nonDismissedWarningMessageCount = warningMessages.Count - dismissedWarningMessages.Count;

            // Updates whether each section has been expanded or not; determines whether to display the message body.
            SetMessageSectionsExpansion();
            
            // Generating the user-facing display objects.
            SetDisplayMessages(infoMessages, NodeInfoVisibilityState);
            SetDisplayMessages(warningMessages, NodeWarningsVisibilityState);
            SetDisplayMessages(errorMessages, NodeErrorsVisibilityState);
            
            // A UI refinement; if there are fewer than 2 messages to show at a particular level, we collapse the sections.
            AutoCollapseSections(nonDismissedInfoMessageCount, nonDismissedWarningMessageCount, errorMessages.Count);
            // If there are fewer than 2 messages to show at a level, we don't display the iterating count.
            UpdateNodeMessageIteratorsVisibility();
            // If there are fewer than 2 message to show at a level, we don't display a button with 'show more'.
            UpdateShowMoreButtonVisibility();
            // If the section is already expanded, the 'show more' button says 'show less' instead.
            UpdateShowMoreButtonText();
            // If there are no items to show at a particular message level, the bubble's visibility is collapsed.
            UpdateUserFacingCollectionVisibility();
            
            RaisePropertyChanged(nameof(DoesNodeDisplayMessages));
        }

        /// <summary>
        /// Takes in a style and determines what the corresponding collection of display messages is.
        /// </summary>
        /// <param name="style"></param>
        /// <returns></returns>
        private ObservableCollection<InfoBubbleDataPacket> GetTargetCollection(Style style)
        {
            switch (style)
            {
                case Style.None:
                    return null;
                case Style.Warning:
                case Style.WarningCondensed:
                    return NodeWarningsToDisplay;
                case Style.Error:
                case Style.ErrorCondensed:
                    return NodeErrorsToDisplay;
                case Style.Info:
                    return NodeInfoToDisplay;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Used for handling the old InfoBubble APIs, which kept track of
        /// condensed and expanded styles for errors and warnings.
        /// </summary>
        /// <param name="style"></param>
        /// <returns></returns>
        private Style GetAlternativeStyle(Style style)
        {
            switch (style)
            {
                case Style.None:
                    return Style.None;
                case Style.Warning:
                    return Style.WarningCondensed;
                case Style.WarningCondensed:
                    return Style.Warning;
                case Style.Error:
                    return Style.ErrorCondensed;
                case Style.ErrorCondensed:
                    return Style.Error;
                case Style.Info:
                    return Style.None;
                default:
                    return Style.None;
            }
        }

        /// <summary>
        /// Creates a new object with the same properties as the original InfoBubbleDataPacket.
        /// Needed so that we can display certain properties in a user, formatted in a certain way
        /// without polluting the original object.
        /// </summary>
        /// <param name="infoBubbleDataPacketToCopy"></param>
        /// <returns></returns>
        private InfoBubbleDataPacket DuplicateInfoBubbleDataPacket(InfoBubbleDataPacket infoBubbleDataPacketToCopy)
        {
            return new InfoBubbleDataPacket
            {
                Text = infoBubbleDataPacketToCopy.Text,
                Message = infoBubbleDataPacketToCopy.Message,
                Link = infoBubbleDataPacketToCopy.Link,
                LinkText = infoBubbleDataPacketToCopy.Link != null ? infoBubbleDataPacketToCopy.Link.ToString() : "",
                Style = infoBubbleDataPacketToCopy.Style
            };
        }

        /// <summary>
        /// Takes in a list of messages and their corresponding NodeMessageVisibility state and returns
        /// NodeMessage objects for display to the user, with an iterating count where necessary.
        /// </summary>
        /// <param name="infoBubbleDataPackets"></param>
        /// <param name="nodeMessageVisibility"></param>
        /// <returns></returns>
        private void SetDisplayMessages(List<InfoBubbleDataPacket> infoBubbleDataPackets, NodeMessageVisibility nodeMessageVisibility)
        {
            if (infoBubbleDataPackets.Count < 1) return;

            Style messageStyle = infoBubbleDataPackets.First().Style;
            ObservableCollection<InfoBubbleDataPacket> targetCollection = GetTargetCollection(messageStyle);

            List<InfoBubbleDataPacket> displayMessages = new List<InfoBubbleDataPacket>();
            
            // Filtering the collection of dismissed messages based on the message type, e.g. Warning
            // Note: We're selecting the Text property here, since this is not affected by UI formatted requirements
            // unlike the Message property, which is.
            List<string> dismissedMessageStrings = GetMessagesOfStyle(DismissedMessages, messageStyle)
                .Select(x => x.Text)
                .ToList();

            // The total number of messages we're looking to display at this level.
            int denominator = infoBubbleDataPackets.Count(x => !dismissedMessageStrings.Contains(x.Message));

            // If there are no messages to display to the user, return.
            if (denominator < 1) return;
           
            // Formats user-facing information to suit the redesigned Node Informational State design.
            InfoBubbleDataPacket infoBubbleDataPacket;

            switch (nodeMessageVisibility)
            {
                // The user just sees the icon, no messages.
                case NodeMessageVisibility.Icon:
                    infoBubbleDataPacket = DuplicateInfoBubbleDataPacket(infoBubbleDataPackets[0]);
                    infoBubbleDataPacket.MessageNumber = "";
                    displayMessages.Add(infoBubbleDataPacket);
                    break;
                // The user just sees the first message, with a count displaying the total number of collapsed messages at this level.
                case NodeMessageVisibility.CollapseMessages:
                    for (int i = 0; i < infoBubbleDataPackets.Count; i++)
                    {
                        if (dismissedMessageStrings.Contains(infoBubbleDataPackets[i].Text)) continue;

                        infoBubbleDataPacket = DuplicateInfoBubbleDataPacket(infoBubbleDataPackets[i]);
                        infoBubbleDataPacket.MessageNumber = denominator < 2 ? "" : $"1/{denominator} ";
                        displayMessages.Add(infoBubbleDataPacket);
                        break;
                    }
                    break;
                // The user sees all messages, with an interating counter displayed next to each message.
                case NodeMessageVisibility.ShowAllMessages:
                    // Otherwise we display the iterator
                    int iteratingNumerator = 1;
                    
                    for (int i = 0; i < infoBubbleDataPackets.Count; i++)
                    {
                        if (dismissedMessageStrings.Contains(infoBubbleDataPackets[i].Text)) continue;

                        infoBubbleDataPacket = DuplicateInfoBubbleDataPacket(infoBubbleDataPackets[i]);
                        infoBubbleDataPacket.MessageNumber = $"{iteratingNumerator}/{denominator} ";
                        displayMessages.Add(infoBubbleDataPacket);
                        iteratingNumerator++;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(nodeMessageVisibility), nodeMessageVisibility, null);
            }
            
            if (targetCollection == null) return;
            
            // Adding the messages to the user-facing collection
            for (int i = 0; i < displayMessages.Count; i++) targetCollection.Add(displayMessages[i]);
        }
        
        /// <summary>
        /// Validate and update workspace run status message
        /// </summary>
        internal void ValidateWorkspaceRunStatusMsg()
        {
            // if current workspace is not a home workspace, skip this step
            if(DynamoViewModel.CurrentSpaceViewModel.IsHomeSpace)
            {
                (DynamoViewModel.CurrentSpaceViewModel as HomeWorkspaceViewModel).UpdateRunStatusMsgBasedOnStates();
            }
        }

        /// <summary>
        /// Unsubscribes from any events this class is subscribed to.
        /// </summary>
        public override void Dispose()
        {
            NodeMessages.CollectionChanged -= NodeInformation_CollectionChanged;
        }
    }

    public struct InfoBubbleDataPacket : IEquatable<InfoBubbleDataPacket>
    {
        private const string externalLinkIdentifier = "href=";
        public InfoBubbleViewModel.Style Style;
        public Point TopLeft;
        public Point BotRight;
        public string Text;
        internal Uri Link;
        public InfoBubbleViewModel.Direction ConnectingDirection;

        /// <summary>
        /// The user-facing message a user sees when presented with an info message / warning / error
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Used to display an iterating count next to user-facing messages. Only shows when more than one message exists
        /// at this level (e.g. Warnings).
        /// </summary>
        public string MessageNumber { get; set; }

        /// <summary>
        /// The string representation of a URI pointing to a markdown file which is opened when the user clicks 'Read More' on
        /// a particular info message / warning / error which displays this functionality.
        /// </summary>
        public string LinkText { get; set; }

        public InfoBubbleDataPacket(
            InfoBubbleViewModel.Style style,
            Point topLeft,
            Point botRight,
            string text,
            InfoBubbleViewModel.Direction connectingDirection)
        {
            Style = style;
            TopLeft = topLeft;
            BotRight = botRight;
            Link = ParseLinkFromText(text);
            Text = RemoveLinkFromText(text);
            ConnectingDirection = connectingDirection;
            MessageNumber = "";
            Message = Text;
            LinkText = "";
        }

        //Check if has link
        private static string RemoveLinkFromText(string text)
        {
            // if there is no link, we do nothing
            if (!text.Contains(externalLinkIdentifier)) return text;

            // return the text without the link or identifier
            string[] split = text.Split(new string[] { externalLinkIdentifier }, StringSplitOptions.None);
            return split[0];
        }

        private static Uri ParseLinkFromText(string text)
        {
            // if there is no link, we do nothing
            if (!text.Contains(externalLinkIdentifier)) return null;

            string[] split = text.Split(new string[] { externalLinkIdentifier, Environment.NewLine }, StringSplitOptions.None);

            // if we only have 1 substring, it means there wasn't anything after the identifier
            if (split.Length <= 1) return null;

            // try to parse the link into a URI and clear the link property on failure
            try
            {
                return string.IsNullOrWhiteSpace(split[1]) ? null : new Uri(split[1], UriKind.RelativeOrAbsolute);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool Equals(InfoBubbleDataPacket other)
        {
            return 
                Style == other.Style &&
                Text == other.Text &&
                Link == other.Link &&
                ConnectingDirection == other.ConnectingDirection;
        }
    }
}
