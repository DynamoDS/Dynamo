using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

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
            WarningCondensed, 
            Error,
            ErrorCondensed, 
            Info,
        }
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
        public enum State
        {
            Minimized,
            Pinned
        }

        #region Properties

        private double zIndex;
        private Style infoBubbleStyle;
        public Direction connectingDirection;
        private string content;
        private Uri documentationLink;
        public Point targetTopLeft;
        public Point targetBotRight;
        private Direction limitedDirection;
        private State infoBubbleState;
        private double bubbleWidth = 300.0;

        private ObservableCollection<InfoBubbleDataPacket> nodeInfoToDisplay = new ObservableCollection<InfoBubbleDataPacket>();
        private ObservableCollection<InfoBubbleDataPacket> nodeWarningsToDisplay = new ObservableCollection<InfoBubbleDataPacket>();
        private ObservableCollection<InfoBubbleDataPacket> nodeErrorsToDisplay = new ObservableCollection<InfoBubbleDataPacket>();

        // Determines whether or not the row for each information level is visible. 
        // e.g. The row for warnings might be showing, but errors and info messages are not shown.
        private bool nodeInfoVisible;
        private bool nodeWarningsVisible;
        private bool nodeErrorsVisible;

        // Determines whether the info/warnings/error iterator is shown e.g. (1/4)
        private bool nodeInfoIteratorVisible;
        private bool nodeWarningsIteratorVisible;
        private bool nodeErrorsIteratorVisible;

        // Determines whether or not the body of each information bubble is shown. 
        // This relates to the message text box, and any buttons used to dismiss/display messages.
        private bool nodeInfoSectionExpanded;
        private bool nodeWarningsSectionExpanded;
        private bool nodeErrorsSectionExpanded;

        // Determines whether the info, warnings and errors are displaying just an icon, a single error message
        // or all messages at once.
        private NodeMessageVisibility nodeInfoVisibilityState = NodeMessageVisibility.Icon;
        private NodeMessageVisibility nodeWarningsVisibilityState = NodeMessageVisibility.Icon;
        private NodeMessageVisibility nodeErrorsVisibilityState = NodeMessageVisibility.Icon;

        // Determines whether the 'Show More' or 'Show Less' buttons are visible to the user.
        private bool nodeInfoShowMoreButtonVisible;
        private bool nodeWarningsShowMoreButtonVisible;
        private bool nodeErrorsShowMoreButtonVisible;
        
        // Relates to whether the info/warning/error message bodies display a button saying
        // 'show more' or 'show less'.
        private bool nodeInfoShowLessMessageVisible;
        private bool nodeWarningsShowLessMessageVisible;
        private bool nodeErrorsShowLessMessageVisible;

        // Determines whether the show more/show less buttons are visible to the user
        public bool NodeInfoShowMoreButtonVisible
        {
            get => nodeInfoShowMoreButtonVisible;
            set
            {
                nodeInfoShowMoreButtonVisible = value;
                RaisePropertyChanged(nameof(NodeInfoShowMoreButtonVisible));
            }
        }
        public bool NodeWarningsShowMoreButtonVisible
        {
            get => nodeWarningsShowMoreButtonVisible;
            set
            {
                nodeWarningsShowMoreButtonVisible = value;
                RaisePropertyChanged(nameof(NodeWarningsShowMoreButtonVisible));
            }
        }
        public bool NodeErrorsShowMoreButtonVisible
        {
            get => nodeErrorsShowMoreButtonVisible;
            set
            {
                nodeErrorsShowMoreButtonVisible = value;
                RaisePropertyChanged(nameof(NodeErrorsShowMoreButtonVisible));
            }
        }


        // Determines what the show more/show less buttons say
        public bool NodeInfoShowLessMessageVisible
        {
            get => nodeInfoShowLessMessageVisible;
            set
            {
                nodeInfoShowLessMessageVisible = value;
                RaisePropertyChanged(nameof(NodeInfoShowLessMessageVisible));
            }
        }
        public bool NodeWarningsShowLessMessageVisible
        {
            get => nodeWarningsShowLessMessageVisible;
            set
            {
                nodeWarningsShowLessMessageVisible = value;
                RaisePropertyChanged(nameof(NodeWarningsShowLessMessageVisible));
            }
        }
        public bool NodeErrorsShowLessMessageVisible
        {
            get => nodeErrorsShowLessMessageVisible;
            set
            {
                nodeErrorsShowLessMessageVisible = value;
                RaisePropertyChanged(nameof(NodeErrorsShowLessMessageVisible));
            }
        }

        public string Content
        {
            get { return content; }
            set { content = value; RaisePropertyChanged("Content"); }
        }

        public DynamoViewModel DynamoViewModel { get; private set; }


        public double ZIndex
        {
            get { return zIndex; }
            set { zIndex = value; RaisePropertyChanged("ZIndex"); }
        }

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

        public Point TargetTopLeft
        {
            get { return targetTopLeft; }
            set { targetTopLeft = value; RaisePropertyChanged("TargetTopLeft"); }
        }

        public Point TargetBotRight
        {
            get { return targetBotRight; }
            set { targetBotRight = value; RaisePropertyChanged("TargetBotRight"); }
        }

        public Direction LimitedDirection
        {
            get { return limitedDirection; }
            set { limitedDirection = value; RaisePropertyChanged("LimitedDirection"); }
        }

        public double Left
        {
            get { return 0; }
        }

        public double Top
        {
            get { return 0; }
        }

        public State InfoBubbleState
        {
            get { return infoBubbleState; }
            set { infoBubbleState = value; RaisePropertyChanged("InfoBubbleState"); }
        }


        /// <summary>
        /// The formatted, user-visible string relating to the node's information state
        /// </summary>
        public ObservableCollection<InfoBubbleDataPacket> NodeInfoToDisplay
        {
            get => nodeInfoToDisplay;
            set
            {
                nodeInfoToDisplay = value;
                RaisePropertyChanged(nameof(NodeInfoToDisplay));
            }
        }

        /// <summary>
        /// The formatted, user-visible string relating to the node's warning state
        /// </summary>
        public ObservableCollection<InfoBubbleDataPacket> NodeWarningsToDisplay
        {
            get => nodeWarningsToDisplay;
            set
            {
                nodeWarningsToDisplay = value;
                RaisePropertyChanged(nameof(NodeWarningsToDisplay));
            }
        }

        /// <summary>
        /// The formatted, user-visible string relating to the node's error state
        /// </summary>
        public ObservableCollection<InfoBubbleDataPacket> NodeErrorsToDisplay
        {
            get => nodeErrorsToDisplay;
            set
            {
                nodeErrorsToDisplay = value;
                RaisePropertyChanged(nameof(NodeErrorsToDisplay));
            }
        }

        /// <summary>
        /// A collection of InfoBubbleDataPacket objects that are received when the node executes
        /// and its state changes to reflect errors or warnings that have been detected.
        /// </summary>
        public ObservableCollection<InfoBubbleDataPacket> NodeMessages
        {
            get;
            set;
        } = new ObservableCollection<InfoBubbleDataPacket>();
        
        /// <summary>
        /// A collection of messages this node has received that have been manually dismissed by the user.
        /// </summary>
        public ObservableCollection<InfoBubbleDataPacket> DismissedMessages { get; set; } = new ObservableCollection<InfoBubbleDataPacket>();

        /// <summary>
        /// Used to determine whether the UI container for node Info is visible
        /// </summary>
        public bool NodeInfoVisible
        {
            get => nodeInfoVisible;
            set
            {
                nodeInfoVisible = value;
                RaisePropertyChanged(nameof(NodeInfoVisible));
            }
        }

        /// <summary>
        /// Used to determine whether the UI container for node Warnings is visible
        /// </summary>
        public bool NodeWarningsVisible
        {
            get => nodeWarningsVisible;
            set
            {
                nodeWarningsVisible = value;
                RaisePropertyChanged(nameof(NodeWarningsVisible));
            }
        }

        /// <summary>
        /// Used to determine whether the UI container for node Errors is visible
        /// </summary>
        public bool NodeErrorsVisible
        {
            get => nodeErrorsVisible;
            set
            {
                nodeErrorsVisible = value;
                RaisePropertyChanged(nameof(NodeErrorsVisible));
            }
        }

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
        public enum NodeMessageVisibility { Icon, CollapseMessages, ShowAllMessages }

        /// <summary>
        /// Determines whether the node infos are showing just an icon, a condensed summary of messages or
        /// displaying each message in turn.
        /// </summary>
        public NodeMessageVisibility NodeInfoVisibilityState
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
        public NodeMessageVisibility NodeWarningsVisibilityState
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
        public NodeMessageVisibility NodeErrorsVisibilityState
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
        public bool NodeInfoIteratorVisible
        {
            get => nodeInfoIteratorVisible;
            set
            {
                nodeInfoIteratorVisible = value;
                RaisePropertyChanged(nameof(NodeInfoIteratorVisible));
            }
        }

        public bool NodeWarningsIteratorVisible
        {
            get => nodeWarningsIteratorVisible;
            set
            {
                nodeWarningsIteratorVisible = value;
                RaisePropertyChanged(nameof(NodeWarningsIteratorVisible));
            }
        }

        public bool NodeErrorsIteratorVisible
        {
            get => nodeErrorsIteratorVisible;
            set
            {
                nodeErrorsIteratorVisible = value;
                RaisePropertyChanged(nameof(NodeErrorsIteratorVisible));
            }
        }

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
            this.DynamoViewModel = dynamoViewModel;

            // Default values
            limitedDirection = Direction.None;
            ConnectingDirection = Direction.None;
            Content = string.Empty;
            DocumentationLink = null;
            // To appear above any NodeView elements
            ZIndex = 40; 
            InfoBubbleStyle = Style.None;
            InfoBubbleState = State.Minimized;

            NodeMessages.CollectionChanged += NodeInformation_CollectionChanged;
            
            RefreshNodeInformationalStateDisplay();
        }
        
        /// <summary>
        /// Rebuilds the user-facing message collections when the underlying messages coming from the node evaluation change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NodeInformation_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RefreshNodeInformationalStateDisplay();
        }

        #endregion

        #region Command Methods

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

        private bool CanUpdatePosition(object parameter)
        {
            return true;
        }

        private void Resize(object parameter)
        {
            UpdateContent(FullContent);
        }

        private bool CanResize(object parameter)
        {
            return true;
        }

        private void ChangeInfoBubbleState(object parameter)
        {
            if (parameter is InfoBubbleViewModel.State)
            {
                InfoBubbleViewModel.State newState = (InfoBubbleViewModel.State)parameter;

                InfoBubbleState = newState;
            }
        }

        private bool CanChangeInfoBubbleState(object parameter)
        {
            return true;
        }

        private void OpenDocumentationLink(object parameter)
        {
            if (parameter is InfoBubbleDataPacket infoBubbleDataPacket)
            {
                var link = infoBubbleDataPacket.Link;
                if (link != null)
                {
                    var targetContent = new OpenDocumentationLinkEventArgs((Uri)link);
                    this.DynamoViewModel.OpenDocumentationLink(targetContent);
                    return;
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
        /// Adds the message(s) to the collection of DismissedMessages, then rebuilds the
        /// user-facing messages display from scratch.
        /// </summary>
        /// <param name="parameter"></param>
        private void DismissWarning(object parameter)
        {
            if (!(parameter is InfoBubbleDataPacket infoBubbleDataPacket)) return;

            if (!DismissedMessages.Contains(infoBubbleDataPacket))
            {
                DismissedMessages.Add(infoBubbleDataPacket);
            }
            RefreshNodeInformationalStateDisplay();
        }
        
        private bool CanUndismissWarning(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Accessed via the node's ContextMenu, used to make a user-facing message reappear above the node.
        /// </summary>
        /// <param name="parameter"></param>
        private void UndismissWarning(object parameter)
        {
            if (!(parameter is string value)) return;

            InfoBubbleDataPacket dataPacketToUndismiss = DismissedMessages
                .FirstOrDefault(x => x.Message == value);
            
            DismissedMessages.Remove(dataPacketToUndismiss);

            RefreshNodeInformationalStateDisplay();
        }

        private bool CanDismissWarning(object parameter)
        {
            return true;
        }


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

            ZIndex = 5;
        }

        private bool CanShowFullContent(object parameter)
        {
            return true;
        }

        private void ShowCondensedContent(object parameter)
        {
            if (parameter != null && parameter is InfoBubbleDataPacket)
            {
                InfoBubbleDataPacket data = (InfoBubbleDataPacket)parameter;
                InfoBubbleStyle = data.Style;
            }

            // Generate condensed content
            GenerateContent();

            ZIndex = 3;
        }

        private bool CanShowCondensedContent(object parameter)
        {
            return true;
        }
        #endregion


        #endregion

        #region Private Helper Method

        private void UpdateContent(string text)
        {
            FullContent = text;

            // Generate initial condensed content (if needed) whenever bubble content is updated
            GenerateContent();
        }

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
        /// Refreshes all of the user-facing Node Informational State UI.
        /// </summary>
        public void RefreshNodeInformationalStateDisplay()
        {
            if (NodeMessages == null || NodeMessages.Count < 1)
            {
                NodeInfoVisible = false;
                NodeWarningsVisible = false;
                NodeErrorsVisible = false;
                return;
            }

            ClearUserFacingCollections();

            List<InfoBubbleDataPacket> infoMessages = NodeMessages
                .Where(x => x.Style == Style.Info)
                .ToList();

            List<InfoBubbleDataPacket> warningMessages = NodeMessages
                .Where(x => x.Style == Style.Warning || x.Style == Style.WarningCondensed)
                .ToList();

            List<InfoBubbleDataPacket> errorMessages = NodeMessages
                .Where(x => x.Style == Style.Error || x.Style == Style.ErrorCondensed)
                .ToList();

            // Determines whether 'show more/less' buttons and node message text are visible.
            NodeInfoSectionExpanded = nodeInfoVisibilityState != NodeMessageVisibility.Icon;
            NodeWarningsSectionExpanded = nodeWarningsVisibilityState != NodeMessageVisibility.Icon;
            NodeErrorsSectionExpanded = nodeErrorsVisibilityState != NodeMessageVisibility.Icon;

            // Determining whether to display iterators (1/n) for messages at each level.
            NodeInfoIteratorVisible = infoMessages.Count - DismissedMessages.Count(x => x.Style == Style.Info) > 1;
            NodeWarningsIteratorVisible = warningMessages.Count - DismissedMessages.Count(x => x.Style == Style.Warning || x.Style == Style.WarningCondensed) > 1;
            NodeErrorsIteratorVisible = errorMessages.Count > 1;
            
            // Generating the user-facing display objects.
            List<InfoBubbleDataPacket> displayInfo = GetDisplayMessages(infoMessages, NodeInfoVisibilityState);
            List<InfoBubbleDataPacket> displayWarnings = GetDisplayMessages(warningMessages, NodeWarningsVisibilityState);
            List<InfoBubbleDataPacket> displayErrors = GetDisplayMessages(errorMessages, NodeErrorsVisibilityState);

            // Adding the display objects (if any) to the user-facing collections.
            for (int i = 0; i < displayInfo.Count; i++) NodeInfoToDisplay.Add(displayInfo[i]);
            for (int i = 0; i < displayWarnings.Count; i++) NodeWarningsToDisplay.Add(displayWarnings[i]);
            for (int i = 0; i < displayErrors.Count; i++) NodeErrorsToDisplay.Add(displayErrors[i]);

            // Determining whether to show/hide each level of user-facing messages.
            NodeInfoVisible = NodeInfoToDisplay.Count > 0;
            NodeWarningsVisible = NodeWarningsToDisplay.Count > 0;
            NodeErrorsVisible = NodeErrorsToDisplay.Count > 0;
            
            // We need to show a 'show more/less' button to the user if there is more than one non-dismissed message
            // at each level, and if the node isn't fully-collapsed into an icon.
            int nonDismissedInfoMessageCount = infoMessages.Count - DismissedMessages.Count(x => x.Style == Style.Info);
            int nonDismissedWarningMessageCount = warningMessages.Count - DismissedMessages.Count(x => x.Style == Style.Warning || x.Style == Style.WarningCondensed);
            int errorsCount = NodeMessages.Count(x => x.Style == Style.Error || x.Style == Style.ErrorCondensed);

            // Auto-collapsing sections where necessary
            if (NodeInfoVisibilityState == NodeMessageVisibility.ShowAllMessages && nonDismissedInfoMessageCount < 2)
            {
                NodeInfoVisibilityState = NodeMessageVisibility.CollapseMessages;
                NodeInfoIteratorVisible = false;
            }
            if (NodeWarningsVisibilityState == NodeMessageVisibility.ShowAllMessages && nonDismissedWarningMessageCount < 2)
            {
                NodeWarningsVisibilityState = NodeMessageVisibility.CollapseMessages;
                NodeWarningsIteratorVisible = false;
            }
            if (NodeErrorsVisibilityState == NodeMessageVisibility.ShowAllMessages && errorsCount < 2)
            {
                NodeErrorsVisibilityState = NodeMessageVisibility.CollapseMessages;
                NodeErrorsIteratorVisible = false;
            }

            NodeInfoShowMoreButtonVisible = NodeInfoSectionExpanded && nonDismissedInfoMessageCount > 1;
            NodeWarningsShowMoreButtonVisible = NodeWarningsSectionExpanded && nonDismissedWarningMessageCount > 1;
            NodeErrorsShowMoreButtonVisible = NodeErrorsSectionExpanded && errorsCount > 1;

            // Determines whether the 'show more/less' button says 'show more' or 'show less'.
            NodeInfoShowLessMessageVisible = NodeInfoShowMoreButtonVisible && NodeInfoVisibilityState == NodeMessageVisibility.ShowAllMessages;
            NodeWarningsShowLessMessageVisible = NodeWarningsShowMoreButtonVisible && NodeWarningsVisibilityState == NodeMessageVisibility.ShowAllMessages;
            NodeErrorsShowLessMessageVisible = NodeErrorsShowMoreButtonVisible && NodeErrorsVisibilityState == NodeMessageVisibility.ShowAllMessages;
        }

        /// <summary>
        /// Takes in a list of messages and their corresponding NodeMessageVisibility state and returns
        /// NodeMessage objects for display to the user, with an iterating count where necessary.
        /// </summary>
        /// <param name="infoBubbleDataPackets"></param>
        /// <param name="nodeMessageVisibility"></param>
        /// <returns></returns>
        private List<InfoBubbleDataPacket> GetDisplayMessages(List<InfoBubbleDataPacket> infoBubbleDataPackets, NodeMessageVisibility nodeMessageVisibility)
        {
            List<InfoBubbleDataPacket> displayMessages = new List<InfoBubbleDataPacket>();
            if (infoBubbleDataPackets.Count < 1) return displayMessages;

            Style messageStyle = infoBubbleDataPackets.First().Style;

            // The old API referenced styles such as WarningCondensed and Warning, which we are handling as a single case now.
            // Hence the need to compound behaviours for Warning/WarningCondensed and Error/ErrorCondensed.
            Style alternativeStyle;
            switch (messageStyle)
            {
                case Style.None:
                    alternativeStyle = Style.None;
                    break;
                case Style.Warning:
                    alternativeStyle = Style.WarningCondensed;
                    break;
                case Style.WarningCondensed:
                    alternativeStyle = Style.Warning;
                    break;
                case Style.Error:
                    alternativeStyle = Style.ErrorCondensed;
                    break;
                case Style.ErrorCondensed:
                    alternativeStyle = Style.Error;
                    break;
                case Style.Info:
                    alternativeStyle = Style.None;
                    break;
                default:
                    alternativeStyle = Style.None;
                    break;
            }

            // Filtering the collection of dismissed messages based on the message type, e.g. Warning
            List<string> dismissedMessageStringsOfType = DismissedMessages
                .Where(x => x.Style == messageStyle || x.Style == alternativeStyle)
                .Select(x => x.Text)
                .ToList();

            int nonDismissedMessageCount =
                NodeMessages.Count(x => x.Style == messageStyle || x.Style == alternativeStyle) - DismissedMessages.Count(x => x.Style == messageStyle || x.Style == alternativeStyle);

            
            // Formats user-facing information to suit the redesigned Node Informational State design.
            InfoBubbleDataPacket infoBubbleDataPacket;

            switch (nodeMessageVisibility)
            {
                // The user just sees the icon, no messages.
                case NodeMessageVisibility.Icon:
                    infoBubbleDataPacket = new InfoBubbleDataPacket
                    {
                        Text = infoBubbleDataPackets[0].Text,
                        Message = infoBubbleDataPackets[0].Text,
                        MessageNumber = "",
                        Link = infoBubbleDataPackets[0].Link,
                        Style = infoBubbleDataPackets[0].Style
                    };
                    infoBubbleDataPacket.LinkText = infoBubbleDataPackets[0].Link != null ? infoBubbleDataPackets[0].Link.ToString() : "";
                    displayMessages.Add(infoBubbleDataPacket);
                    break;
                // The user just sees the first message, with a count displaying the total number of collapsed messages at this level.
                case NodeMessageVisibility.CollapseMessages:
                    for (int i = 0; i < infoBubbleDataPackets.Count; i++)
                    {
                        if (dismissedMessageStringsOfType.Contains(infoBubbleDataPackets[i].Text)) continue;
                        string messageNumber = nonDismissedMessageCount < 2 ? "" : $"1/{nonDismissedMessageCount} ";
                        infoBubbleDataPacket = new InfoBubbleDataPacket
                        {
                            Text = infoBubbleDataPackets[i].Text,
                            Message = infoBubbleDataPackets[i].Text,
                            MessageNumber = messageNumber,
                            Link = infoBubbleDataPackets[i].Link,
                            Style = infoBubbleDataPackets[i].Style
                        };
                        infoBubbleDataPacket.LinkText = infoBubbleDataPackets[i].Link != null ? infoBubbleDataPackets[i].Link.ToString() : "";
                        displayMessages.Add(infoBubbleDataPacket);
                        break;
                    }
                    break;
                // The user sees all messages, with an interating counter displayed next to each message.
                case NodeMessageVisibility.ShowAllMessages:
                    // Otherwise we display the iterator
                    int messageIterator = 1;
                    for (int i = 0; i < infoBubbleDataPackets.Count; i++)
                    {
                        if (dismissedMessageStringsOfType.Contains(infoBubbleDataPackets[i].Text)) continue;
                        infoBubbleDataPacket = new InfoBubbleDataPacket
                        {
                            Text = infoBubbleDataPackets[i].Text,
                            Message = infoBubbleDataPackets[i].Text,
                            MessageNumber = $"{messageIterator}/{nonDismissedMessageCount} ",
                            Link = infoBubbleDataPackets[i].Link,
                            Style = infoBubbleDataPackets[i].Style
                        };
                        infoBubbleDataPacket.LinkText = infoBubbleDataPackets[i].Link != null ? infoBubbleDataPackets[i].Link.ToString() : "";
                        displayMessages.Add(infoBubbleDataPacket);
                        messageIterator++;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(nodeMessageVisibility), nodeMessageVisibility, null);
            }
            return displayMessages;
        }

        #endregion
    }

    public struct InfoBubbleDataPacket
    {
        private const string externalLinkIdentifier = "href=";
        public InfoBubbleViewModel.Style Style;
        public Point TopLeft;
        public Point BotRight;
        public string Text;
        internal Uri Link;
        public InfoBubbleViewModel.Direction ConnectingDirection;

        public string Message { get; set; }

        public string MessageNumber { get; set; }

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

            string[] split = text.Split(new string[] { externalLinkIdentifier }, StringSplitOptions.None);

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

    }
}
