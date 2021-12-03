﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Interop;
using Dynamo.Wpf.UI.GuidedTour;
using Dynamo.Wpf.ViewModels.GuidedTour;

namespace Dynamo.Wpf.Views.GuidedTour
{
    /// <summary>
    /// Interaction logic for PopupWindow.xaml
    /// </summary>
    public partial class PopupWindow : Popup
    {
        private const int popupBordersOffSet = 10;
        private PopupWindowViewModel popupViewModel;
        private HostControlInfo hostControlInfo;
        private bool isClosingTour;

        private const string packagesTourName = "packages";

        internal WebBrowserWindow webBrowserWindow;

        public PopupWindow(PopupWindowViewModel viewModel, HostControlInfo hInfo)
        {
            InitializeComponent();

            hostControlInfo = hInfo;
            
            if (viewModel != null)
                popupViewModel = viewModel;

            DataContext = popupViewModel;

            //Due that we are drawing the Direction pointers on left and right side of the Canvas (10 width each one) then we need to add 20
            RootLayout.Width = popupViewModel.Width + (popupBordersOffSet * 2); 
            //Also a shadow of 10 pixels in top and 10 pixels at the bottom will be shown then we need to add 20
            RootLayout.Height = popupViewModel.Height + (popupBordersOffSet * 2);

            //The BackgroundRectangle represent the tooltip background rectangle that is drawn over a Canvas
            //Needs to be moved 10 pixels over the X axis to show the direction pointers (Height was already increased above)
            //Needs to be moved 10 pixels over the Y axis to show the shadow at top and bottom.
            BackgroundRectangle.Rect = new Rect(popupBordersOffSet, popupBordersOffSet, popupViewModel.Width, popupViewModel.Height);

            //Setting the host over which the popup will appear and the placement mode
            PlacementTarget = hInfo.HostUIElement;
            Placement = hInfo.PopupPlacement;

            //The CustomRichTextBox has a margin of 10 in left and 10 in right, also there is a indentation for drawing the PointerDirection of 10 of each side so that's why we are subtracting 40.
            ContentRichTextBox.Width = popupViewModel.Width - (popupBordersOffSet * 4);
            HorizontalOffset = hInfo.HorizontalPopupOffSet;
            VerticalOffset = hInfo.VerticalPopupOffSet;

            Opened += PopupWindow_Opened;
            Closed += PopupWindow_Closed;

            isClosingTour = false;
        }

        private void PopupWindow_Closed(object sender, EventArgs e)
        {
            if (webBrowserWindow != null)
                webBrowserWindow.IsOpen = false;

            if(isClosingTour)
            {
                Opened -= PopupWindow_Opened;
                Closed -= PopupWindow_Closed;
            }
        }

        private void PopupWindow_Opened(object sender, EventArgs e)
        {
            if (hostControlInfo.HtmlPage != null && !string.IsNullOrEmpty(hostControlInfo.HtmlPage.FileName))
            {
                ContentRichTextBox.Visibility = Visibility.Hidden;

                webBrowserWindow = new WebBrowserWindow(popupViewModel, hostControlInfo);
                webBrowserWindow.IsOpen = true;
            }
        }

        private void StartTourButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            GuideFlowEvents.OnGuidedTourNext();            
        }

        private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            IsOpen = false;
            if (popupViewModel.Step.GuideName.ToLower() == packagesTourName)
            {
                GuideFlowEvents.OnGuidedTourFinish(popupViewModel.Step.GuideName);
            }
            else
            {
                isClosingTour = true;
                popupViewModel.Step.OnStepClosed(popupViewModel.Step.Name, popupViewModel.Step.StepType);
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            GuideFlowEvents.OnGuidedTourNext();            
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            GuideFlowEvents.OnGuidedTourPrev();           
        }
    }
}
