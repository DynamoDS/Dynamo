using System;
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

        public DynamoViewModel DynamoViewModel { get; private set; }

        private double zIndex;
        public double ZIndex
        {
            get { return zIndex; }
            set { zIndex = value; RaisePropertyChanged("ZIndex"); }
        }
        private Style infoBubbleStyle;
        public Style InfoBubbleStyle
        {
            get { return infoBubbleStyle; }
            set { infoBubbleStyle = value; RaisePropertyChanged("InfoBubbleStyle"); }
        }
        public string FullContent;

        public Direction connectingDirection;
        public Direction ConnectingDirection
        {
            get { return connectingDirection; }
            set { connectingDirection = value; RaisePropertyChanged("ConnectingDirection"); }
        }

        private string content;
        public string Content
        {
            get { return content; }
            set { content = value; RaisePropertyChanged("Content"); }
        }

        private Uri documentationLink;
        public Uri DocumentationLink
        {
            get { return documentationLink; }
            set { documentationLink = value; RaisePropertyChanged(nameof(DocumentationLink)); }
        }

        public Point targetTopLeft;
        public Point TargetTopLeft
        {
            get { return targetTopLeft; }
            set { targetTopLeft = value; RaisePropertyChanged("TargetTopLeft"); }
        }
        public Point targetBotRight;
        public Point TargetBotRight
        {
            get { return targetBotRight; }
            set { targetBotRight = value; RaisePropertyChanged("TargetBotRight"); }
        }
        private Direction limitedDirection;
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

        private State infoBubbleState;

        public State InfoBubbleState
        {
            get { return infoBubbleState; }
            set { infoBubbleState = value; RaisePropertyChanged("InfoBubbleState"); }
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
