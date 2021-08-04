using Dynamo.UI.Commands;

namespace Dynamo.ViewModels
{
    public partial class InfoBubbleViewModel : ViewModelBase
    {
        private DelegateCommand updateContentCommand;
        private DelegateCommand updatePositionCommand;
        private DelegateCommand resizeCommand;
        private DelegateCommand showFullContentCommand;
        private DelegateCommand showCondensedContentCommand;
        private DelegateCommand changeInfoBubbleStateCommand;
        private DelegateCommand openDocumentationLinkCommand;
        private DelegateCommand dismissMessageCommand;
        private DelegateCommand undismissMessageCommand;

        public DelegateCommand UpdateContentCommand
        {
            get
            {
                if (updateContentCommand == null)
                    updateContentCommand = new DelegateCommand(UpdateInfoBubbleContent, CanUpdateInfoBubbleCommand);
                return updateContentCommand;
            }
        }

        public DelegateCommand UpdatePositionCommand
        {
            get
            {
                if (updatePositionCommand == null)
                    updatePositionCommand = new DelegateCommand(UpdatePosition, CanUpdatePosition);
                return updatePositionCommand;
            }
        }
        public DelegateCommand ResizeCommand
        {
            get
            {
                if (resizeCommand == null)
                {
                    resizeCommand = new DelegateCommand(Resize, CanResize);
                }
                return resizeCommand;
            }
        }

        // TODO add new command handler
        public DelegateCommand ShowFullContentCommand
        {
            get
            {
                if (showFullContentCommand == null)
                {
                    showFullContentCommand = new DelegateCommand(ShowFullContent, CanShowFullContent);
                }
                return showFullContentCommand;
            }
        }

        // TODO add new command handler
        public DelegateCommand ShowCondensedContentCommand
        {
            get
            {
                if (showCondensedContentCommand == null)
                {
                    showCondensedContentCommand = new DelegateCommand(ShowCondensedContent, CanShowCondensedContent);
                }
                return showCondensedContentCommand;
            }
        }

        public DelegateCommand ChangeInfoBubbleStateCommand
        {
            get
            {
                if (changeInfoBubbleStateCommand == null)
                {
                    changeInfoBubbleStateCommand = new DelegateCommand(ChangeInfoBubbleState, CanChangeInfoBubbleState);
                }
                return changeInfoBubbleStateCommand;
            }
        }

        public DelegateCommand OpenDocumentationLinkCommand
        {
            get
            {
                if (openDocumentationLinkCommand == null)
                {
                    openDocumentationLinkCommand = new DelegateCommand(OpenDocumentationLink, CanOpenDocumentationLink);
                }
                return openDocumentationLinkCommand;
            }
        }

        /// <summary>
        /// Fires when the user manually dismisses a message by clicking the little 'X' button next to it.
        /// Users can only dismiss Info Messages and Warnings - not Errors.
        /// </summary>
        public DelegateCommand DismissMessageCommand
        {
            get
            {
                if (dismissMessageCommand == null)
                {
                    dismissMessageCommand = new DelegateCommand(DismissMessage);
                }
                return dismissMessageCommand;
            }
        }
        
        /// <summary>
        /// Fires when the user manually selects a previously-dismissed message from the node's ContextMenu.
        /// This un-dismisses the message and causes it to reappear above the node again.
        /// </summary>
        public DelegateCommand UndismissMessageCommand
        {
            get
            {
                if (undismissMessageCommand == null)
                {
                    undismissMessageCommand = new DelegateCommand(UndismissMessage);
                }
                return undismissMessageCommand;
            }
        }
    }
}