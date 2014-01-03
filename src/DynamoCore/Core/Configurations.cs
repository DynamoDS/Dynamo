using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Dynamo.Core
{
    public class Configurations
    {
        #region Dynamo Universal Constants
        // Add 0.5 to place the point in the middle of a pixel to sharpen it
        public static readonly double PixelSharpeningConstant = 0.5;
        #endregion

        #region Usage Reporting Error Message
        public static string GitHubBugReportingLink = "https://github.com/ikeough/Dynamo/issues?state=open";

        public static string UsageReportingErrorMessage = "Uh oh...\n\rWe're sorry - we tried to save your decisions, but something went wrong. The decision probably hasn't been saved. This really shouldn't have happened. Please get in touch via GitHub and send us this information.\n\rYou can also try deleting [FILEPATH] and trying again.\n\rDepending on your previous choice, instrumentation may still be running, if you're worried about this, please consider not using Dynamo until we've got back to you to solve the problem.\n\rSorry about that.";
        #endregion

        #region Canvas Configurations
        //public static readonly double Minimum

        // Grid Settings
        public static readonly int GridSpacing = 100;
        public static readonly int GridThickness = 2;
        public static readonly Color GridLineColor = Color.FromRgb(232, 232, 232);

        // Canvas Control
        public static readonly double ZoomIncrement = 0.05;

        // Double Clicking
        // Maximum distance allowed between first and second click to be accepted as a double click
        public static readonly int DoubleClickAcceptableDistance = 10; // in pixel
        #endregion

        #region Tab Bar Configurations
        // Tabcontrol Settings        
        public static readonly int MinTabsBeforeClipping = 6;
        public static readonly int TabControlMenuWidth = 20;
        public static readonly int TabDefaultWidth = 200;
        #endregion

        #region Information Bubble
        public static int FadeInDurationInMilliseconds = 250;
        public static int FadeOutDurationInMilliseconds = 250;
        public static double MaxOpacity = 0.95;

        #region Preview Bubble

        public static int PreviewMaxListLength = 1000;
        public static int PreviewMaxListDepth = 100;
        public static int PreviewMaxLength = 1000;

        public static int CondensedPreviewMaxLength = 25;

        public static SolidColorBrush PreviewFrameFill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        public static double PreviewFrameStrokeThickness = 1;
        public static SolidColorBrush PreviewFrameStrokeColor = new SolidColorBrush(Color.FromRgb(153, 153, 153));

        public static double PreviewTextFontSize = 10;
        public static FontWeight PreviewTextFontWeight = FontWeights.Light;
        public static SolidColorBrush PreviewTextForeground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
        public static TextWrapping PreviewContentWrapping = TextWrapping.Wrap;
        public static Thickness PreviewContentMargin = new Thickness(5, 12, 5, 5);

        public static double PreviewMaxWidth = 500;
        public static double PreviewMaxHeight;
        public static double PreviewMinWidth = 40;
        public static double PreviewMinHeight = 30;
        public static double PreviewDefaultMaxWidth = 300;
        public static double PreviewDefaultMaxHeight = 200;

        public static double PreviewCondensedMaxWidth = 300;        
        public static double PreviewCondensedMaxHeight = 200;
        public static double PreviewCondensedMinWidth = 40;
        public static double PreviewCondensedMinHeight = 0;
        public static double PreviewCondensedContentMaxWidth = PreviewCondensedMaxWidth - 10;
        public static double PreviewCondensedContentMaxHeight = PreviewCondensedMaxHeight - 17;

        public static double PreviewArrowWidth = 12;
        public static double PreviewArrowHeight = 6;

        #endregion

        #region Error Bubble

        public static SolidColorBrush ErrorFrameFill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        public static double ErrorFrameStrokeThickness = 1;
        public static SolidColorBrush ErrorFrameStrokeColor = new SolidColorBrush(Color.FromRgb(190, 70, 70));

        public static double ErrorMaxWidth = 300;
        public static double ErrorMaxHeight = 200;
        public static double ErrorContentMaxWidth = ErrorMaxWidth - 10;
        public static double ErrorContentMaxHeight = ErrorMaxHeight - 16;

        public static double ErrorCondensedMaxWidth = 75;
        public static double ErrorCondensedMinWidth = 25;
        public static double ErrorCondensedMaxHeight = 50;
        public static double ErrorCondensedMinHeight = 25;
        public static double ErrorCondensedContentMaxWidth = ErrorCondensedMaxWidth - 10;
        public static double ErrorCondensedContentMaxHeight = ErrorCondensedMaxHeight - 16;

        public static double ErrorTextFontSize = 13;
        public static FontWeight ErrorTextFontWeight = FontWeights.Light;
        public static SolidColorBrush ErrorTextForeground = new SolidColorBrush(Color.FromRgb(190, 70, 70));
        public static TextWrapping ErrorContentWrapping = TextWrapping.Wrap;
        public static Thickness ErrorContentMargin = new Thickness(5, 5, 5, 12);

        public static double ErrorArrowWidth = 12;
        public static double ErrorArrowHeight = 6;

        #endregion

        #region Node Tooltip
        public static int ToolTipFadeInDelayInMS = 2000;

        public static SolidColorBrush NodeTooltipFrameFill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        public static double NodeTooltipFrameStrokeThickness = 1;
        public static SolidColorBrush NodeTooltipFrameStrokeColor = new SolidColorBrush(Color.FromRgb(165, 209, 226));

        public static double NodeTooltipMaxWidth = 200;
        public static double NodeTooltipMaxHeight = 200;
        public static double NodeTooltipContentMaxWidth = NodeTooltipMaxWidth - 10;
        public static double NodeTooltipContentMaxHeight = NodeTooltipMaxHeight - 16;

        public static double NodeTooltipTextFontSize = 11;
        public static FontWeight NodeTooltipTextFontWeight = FontWeights.Light;
        public static SolidColorBrush NodeTooltipTextForeground = new SolidColorBrush(Color.FromRgb(98, 140, 153));
        public static TextWrapping NodeTooltipContentWrapping = TextWrapping.Wrap;

        public static Thickness NodeTooltipContentMarginLeft = new Thickness(11, 5, 5, 5);
        public static Thickness NodeTooltipContentMarginRight = new Thickness(5, 5, 11, 5);
        public static Thickness NodeTooltipContentMarginBottom = new Thickness(5, 5, 5, 11);

        public static double NodeTooltipArrowWidth_BottomConnecting = 12;
        public static double NodeTooltipArrowHeight_BottomConnecting = 6;
        public static double NodeTooltipArrowWidth_SideConnecting = 6;
        public static double NodeTooltipArrowHeight_SideConnecting = 12;

        #endregion

        #region Library Item Tooltip

        public static SolidColorBrush LibraryTooltipFrameFill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        public static double LibraryTooltipFrameStrokeThickness = 1;
        public static SolidColorBrush LibraryTooltipFrameStrokeColor = new SolidColorBrush(Color.FromRgb(165, 209, 226));

        public static double LibraryTooltipMaxWidth = 400;
        public static double LibraryTooltipMaxHeight = 200;
        public static double LibraryTooltipContentMaxWidth = LibraryTooltipMaxWidth - 10;
        public static double LibraryTooltipContentMaxHeight = LibraryTooltipMaxHeight - 17;

        public static double LibraryTooltipTextFontSize = 11;
        public static SolidColorBrush LibraryTooltipTextForeground = new SolidColorBrush(Color.FromRgb(51, 51, 51));
        public static FontWeight LibraryTooltipTextFontWeight = FontWeights.Normal;
        public static TextWrapping LibraryTooltipContentWrapping = TextWrapping.Wrap;
        public static Thickness LibraryTooltipContentMargin = new Thickness(12, 5, 5, 5);

        public static double LibraryTooltipArrowHeight = 12;
        public static double LibraryTooltipArrowWidth = 6;

        #endregion

        #endregion

        #region CodeBlockNode
        public static readonly int CBNMaxPortNameLength = 24;
        public static readonly int CBNMaxTextBoxWidth = 500;
        #endregion
    }

    public class ResourceNames
    {
        public static readonly string ResourceBaseUri = "pack://application:,,,/DynamoCore;component/UI/Images/";
    }
}
