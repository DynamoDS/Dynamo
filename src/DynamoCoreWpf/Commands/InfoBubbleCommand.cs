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

    }
}