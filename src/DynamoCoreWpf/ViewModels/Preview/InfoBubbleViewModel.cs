using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

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

        private ObservableCollection<NodeMessage> nodeInfoToDisplay = new ObservableCollection<NodeMessage>();
        private ObservableCollection<NodeMessage> nodeWarningsToDisplay = new ObservableCollection<NodeMessage>();
        private ObservableCollection<NodeMessage> nodeErrorsToDisplay = new ObservableCollection<NodeMessage>();

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

        /// <summary>
        /// Determines whether the info, warnings and errors are displaying just an icon, a single error message
        /// or all messages at once.
        /// </summary>
        private NodeMessageVisibility nodeInfoVisibilityState = NodeMessageVisibility.Icon;
        private NodeMessageVisibility nodeWarningsVisibilityState = NodeMessageVisibility.Icon;
        private NodeMessageVisibility nodeErrorsVisibilityState = NodeMessageVisibility.Icon;

        /// <summary>
        /// Determines whether the 'Show More' or 'Show Less' buttons are visible to the user.
        /// </summary>
        private bool nodeInfoShowMoreButtonVisible;
        private bool nodeWarningsShowMoreButtonVisible;
        private bool nodeErrorsShowMoreButtonVisible;
        
        /// <summary>
        /// Relates to whether the info/warning/error message bodies display a button saying
        /// 'show more' or 'show less'.
        /// </summary>
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
        public ObservableCollection<NodeMessage> NodeInfoToDisplay
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
        public ObservableCollection<NodeMessage> NodeWarningsToDisplay
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
        public ObservableCollection<NodeMessage> NodeErrorsToDisplay
        {
            get => nodeErrorsToDisplay;
            set
            {
                nodeErrorsToDisplay = value;
                RaisePropertyChanged(nameof(NodeErrorsToDisplay));
            }
        }

        /// <summary>
        /// A collection of strings that are categorised as 'Information' level, the least important kind of message
        /// </summary>
        public ObservableCollection<string> NodeInfo { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// A collection of strings that are categorised as 'Warning' level, an important but dismissable kind of message
        /// </summary>
        public ObservableCollection<string> NodeWarnings { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// A collection of strings that are categorised as 'Error' level, the most important kind of message
        /// </summary>
        public ObservableCollection<string> NodeErrors { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// A collection of dismissed info messages for this node
        /// </summary>
        public ObservableCollection<string> DismissedNodeInfo { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// A collection of dismissed warnings for this node
        /// </summary>
        public ObservableCollection<string> DismissedNodeWarnings { get; set; } = new ObservableCollection<string>();
        
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
                NodeInfoSectionExpanded = nodeInfoVisibilityState != NodeMessageVisibility.Icon;
                FormatNodeMessages(NodeInfo, NodeInfoToDisplay, NodeInfoVisibilityState, DismissedNodeInfo);
                if (NodeInfo.Count - DismissedNodeInfo.Count > 1) NodeInfoIteratorVisible = true;
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
                NodeWarningsSectionExpanded = nodeWarningsVisibilityState != NodeMessageVisibility.Icon;
                FormatNodeMessages(NodeWarnings, NodeWarningsToDisplay, NodeWarningsVisibilityState, DismissedNodeWarnings);
                if (NodeWarnings.Count - DismissedNodeWarnings.Count > 1) NodeWarningsIteratorVisible = true;
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
                NodeErrorsSectionExpanded = NodeErrorsVisibilityState != NodeMessageVisibility.Icon;
                FormatNodeMessages(NodeErrors, NodeErrorsToDisplay, NodeErrorsVisibilityState);
                if (NodeErrors.Count > 1) NodeErrorsIteratorVisible = true;
            }
        }

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
            ZIndex = 3;
            InfoBubbleStyle = Style.None;
            InfoBubbleState = State.Minimized;

            NodeInfo.CollectionChanged += NodeInformation_CollectionChanged;
            NodeWarnings.CollectionChanged += NodeInformation_CollectionChanged;
            NodeErrors.CollectionChanged += NodeInformation_CollectionChanged;
            DismissedNodeInfo.CollectionChanged += NodeInformation_CollectionChanged;
            DismissedNodeWarnings.CollectionChanged += NodeInformation_CollectionChanged;

            RebuildNodeInformationalStateDisplay();
        }
        
        private void NodeInformation_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RebuildNodeInformationalStateDisplay();
        }

        public void RebuildNodeInformationalStateDisplay()
        {
            // Clears all collections of user-facing messages
            NodeErrorsToDisplay.Clear();
            NodeWarningsToDisplay.Clear();
            NodeInfoToDisplay.Clear();

            // Going through all received messages and formatting them according to the visibility state
            FormatNodeMessages(NodeErrors, NodeErrorsToDisplay, NodeErrorsVisibilityState);
            FormatNodeMessages(NodeWarnings, NodeWarningsToDisplay, NodeWarningsVisibilityState, DismissedNodeWarnings);
            FormatNodeMessages(NodeInfo, NodeInfoToDisplay, NodeInfoVisibilityState, DismissedNodeInfo);

            // Determining which warning levels will be displayed to the user
            NodeInfoVisible = NodeInfoToDisplay.Count > 0;
            NodeWarningsVisible = NodeWarningsToDisplay.Count > 0;
            NodeErrorsVisible = NodeErrorsToDisplay.Count > 0;

            // Determining whether the number of non-dismissed messages is greater than 1, in which case
            // a show more/less button is displayed to the user to help manage the number of visible messages.
            NodeInfoShowMoreButtonVisible = NodeInfo.Count - DismissedNodeInfo.Count > 1;
            NodeWarningsShowMoreButtonVisible = NodeWarnings.Count - DismissedNodeWarnings.Count > 1;
            NodeErrorsShowMoreButtonVisible = NodeErrors.Count > 1;

            // Setting whether iterators (e.g. Error 1/5) are visible for each warning level
            if (NodeErrorsToDisplay.Select(x => x.MessageNumber).Distinct().Count() > 1) NodeErrorsIteratorVisible = true;
            if (NodeWarningsToDisplay.Select(x => x.MessageNumber).Distinct().Count() > 1) NodeWarningsIteratorVisible = true;
            if (NodeInfoToDisplay.Select(x => x.MessageNumber).Distinct().Count() > 1) NodeInfoIteratorVisible = true;
            
            NodeInfoSectionExpanded = NodeInfoVisibilityState != NodeMessageVisibility.Icon;
            NodeWarningsSectionExpanded = NodeWarningsVisibilityState != NodeMessageVisibility.Icon;
            NodeErrorsSectionExpanded = NodeErrorsVisibilityState != NodeMessageVisibility.Icon;
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
            var url = parameter as Uri;
            if (url != null)
            {
                var targetContent = new OpenDocumentationLinkEventArgs((Uri)parameter);
                this.DynamoViewModel.OpenDocumentationLink(targetContent);
            }
        }

        private bool CanOpenDocumentationLink(object parameter)
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
        /// Used for displaying a message to the user alongside a number indicating that message's
        /// enumerated value e.g. 'Error 1/4'
        /// </summary>
        public class NodeMessage
        {
            public string Message { get; set; }
            public string MessageNumber { get; set; }
        }

        /// <summary>
        /// Used to generate the user-facing information relating to a node, such as errors or warnings.
        /// </summary>
        /// <param name="messages">An ObservableCollection of strings, either info, warnings or errors.</param>
        /// <param name="targetCollection">An ObservableCollection messages to be updated.</param>
        /// <param name="nodeMessageVisibility">An enum value, determining how each level of NodeInformationState messages is to be displayed.</param>
        private void FormatNodeMessages
        (
            ObservableCollection<string> messages,
            ObservableCollection<NodeMessage> targetCollection,
            NodeMessageVisibility nodeMessageVisibility,
            ObservableCollection<string> targetDismissedStrings = null
        )
        {
            if (messages == null || messages.Count < 1) return;
            if (targetDismissedStrings == null) targetDismissedStrings = new ObservableCollection<string>();

            targetCollection.Clear();
            
            int nonDismissedMessageCount = messages
                .Count(x => !targetDismissedStrings.Contains(x));

            switch (nodeMessageVisibility)
            {
                // The user just sees the icon, no messages.
                case NodeMessageVisibility.Icon:
                    targetCollection.Add(new NodeMessage
                    {
                        Message = messages[0],
                        MessageNumber = ""
                    });
                    break;
                // The user just sees the first message, with a count displaying the total number of collapsed messages at this level.
                case NodeMessageVisibility.CollapseMessages:
                    for (int i = 0; i < messages.Count; i++)
                    {
                        if (targetDismissedStrings.Contains(messages[i])) continue;
                        targetCollection.Add(new NodeMessage
                        {
                            Message = messages[i],
                            MessageNumber = $"1/{nonDismissedMessageCount} "
                        });
                        break;
                    }
                    break;
                // The user sees all messages, with an interating counter displayed next to each message.
                case NodeMessageVisibility.ShowAllMessages:
                    // Otherwise we display the iterator
                    for (int i = 0; i < messages.Count; i++)
                    {
                        if (targetDismissedStrings.Contains(messages[i])) continue;
                        targetCollection.Add(new NodeMessage
                        {
                            Message = messages[i],
                            MessageNumber = $"{i+1}/{nonDismissedMessageCount} "
                        });
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(nodeMessageVisibility), nodeMessageVisibility, null);
            }
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
