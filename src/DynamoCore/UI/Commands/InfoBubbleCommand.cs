﻿using Dynamo.UI.Commands;

namespace Dynamo.ViewModels
{
    public partial class InfoBubbleViewModel : ViewModelBase
    {
        private DelegateCommand updateContentCommand;
        private DelegateCommand updatePositionCommand;
        private DelegateCommand fadeInCommand;
        private DelegateCommand fadeOutCommand;
        private DelegateCommand instanctCollapseCommand;
        private DelegateCommand setAlwaysVisibleCommand;

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

        public DelegateCommand FadeInCommand
        {
            get
            {
                if (fadeInCommand == null)
                    fadeInCommand = new DelegateCommand(FadeIn, CanFadeIn);
                return fadeInCommand;
            }
        }

        public DelegateCommand FadeOutCommand
        {
            get
            {
                if (fadeOutCommand == null)
                    fadeOutCommand = new DelegateCommand(FadeOut, CanFadeOut);
                return fadeOutCommand;
            }
        }

        public DelegateCommand InstantCollapseCommand
        {
            get
            {
                if (instanctCollapseCommand == null)
                    instanctCollapseCommand = new DelegateCommand(InstantCollapse, CanInstantCollapse);
                return instanctCollapseCommand;
            }
        }

        public DelegateCommand SetAlwaysVisibleCommand
        {
            get
            {
                if (setAlwaysVisibleCommand == null)
                {
                    setAlwaysVisibleCommand = new DelegateCommand(SetAlwaysVisible, CanSetAlwaysVisible);
                }
                return setAlwaysVisibleCommand;
            }
        }
    }
}
