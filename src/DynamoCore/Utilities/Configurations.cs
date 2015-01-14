﻿using Dynamo.Utilities;

namespace Dynamo.UI
{
    public class Configurations
    {
        #region Dynamo Universal Constants

        // Add 0.5 to place the point in the middle of a pixel to sharpen it
        public static readonly string BackupFolderName = "backup";
        public static readonly string FilePathAttribName = "TargetXmlFilePath";
        public static readonly double DoubleSliderTextBoxWidth = 55.0;
        public static readonly double IntegerSliderTextBoxWidth = 30.0;
        public static readonly double MaxWatchNodeWidth = 280.0;
        public static readonly double MaxWatchNodeHeight = 310.0;

        #endregion

        #region Usage Reporting Error Message

        public static string DynamoSiteLink = "http://dynamobim.org/";
        public static string DynamoWikiLink = "https://github.com/DynamoDS/Dynamo/wiki";
        public static string DynamoBimForum = "http://dynamobim.org/forums/forum/dyn/";
        public static string DynamoTeamEmail = "mailto:team@dynamobim.org";
        public static string DynamoAdvancedTutorials = "http://dynamobim.org/learn/#7570";
        public static string DynamoVideoTutorials = "http://dynamobim.org/learn/#161";
        public static string DynamoMoreSamples = "http://dynamobim.org/learn/#159";
        public static string DynamoDownloadLink = "http://dynamobim.org/download/";
        public static string GitHubDynamoLink = "https://github.com/DynamoDS/Dynamo";
        public static string GitHubBugReportingLink = "https://github.com/DynamoDS/Dynamo/issues";

        public static string UsageReportingErrorMessage = "Uh oh...\n\rWe're sorry - we tried to save your decisions, but something went wrong. The decision probably hasn't been saved. This really shouldn't have happened. Please get in touch via GitHub and send us this information.\n\rYou can also try deleting [FILEPATH] and trying again.\n\rDepending on your previous choice, instrumentation may still be running, if you're worried about this, please consider not using Dynamo until we've got back to you to solve the problem.\n\rSorry about that.";
        #endregion

        #region Canvas Configurations
        //public static readonly double Minimum

        // Generic Constants
        public static readonly double PortHeightInPixels = 26;

        // Grid Settings
        public static readonly int GridSpacing = 100;
        public static readonly int GridThickness = 2;

        // Canvas Control
        public static readonly double ZoomIncrement = 0.05;

        // Node/geometry view buttons in the canvas
        public static readonly double ButtonHeight = 30.0;

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
        public static double MaxOpacity = 0.95;

        #region Preview Bubble
        public static int CondensedPreviewMaxLength = 25;

        public static double PreviewFrameStrokeThickness = 1;

        public static double PreviewTextFontSize = 10;

        public static double PreviewMaxWidth = 500;
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

        public static double ErrorFrameStrokeThickness = 1;

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
        public static Thickness ErrorContentMargin = new Thickness(5, 5, 5, 12);

        public static double ErrorArrowWidth = 12;
        public static double ErrorArrowHeight = 6;
        #endregion

        #region Node Tooltip
        public static double NodeTooltipFrameStrokeThickness = 1;

        public static double NodeTooltipMaxWidth = 200;
        public static double NodeTooltipMaxHeight = 200;
        public static double NodeTooltipContentMaxWidth = NodeTooltipMaxWidth - 10;
        public static double NodeTooltipContentMaxHeight = NodeTooltipMaxHeight - 16;

        public static double NodeTooltipTextFontSize = 11;

        public static Thickness NodeTooltipContentMarginLeft = new Thickness(11, 5, 5, 5);
        public static Thickness NodeTooltipContentMarginRight = new Thickness(5, 5, 11, 5);
        public static Thickness NodeTooltipContentMarginBottom = new Thickness(5, 5, 5, 11);

        public static double NodeTooltipArrowWidth_BottomConnecting = 12;
        public static double NodeTooltipArrowHeight_BottomConnecting = 6;
        public static double NodeTooltipArrowWidth_SideConnecting = 6;
        public static double NodeTooltipArrowHeight_SideConnecting = 12;

        public static double ToolTipTargetGapInPixels = 3.0;
        #endregion

        #region Library Item Tooltip

        public static double LibraryTooltipFrameStrokeThickness = 1;

        public static double LibraryTooltipMaxWidth = 400;
        public static double LibraryTooltipMaxHeight = 200;
        public static double LibraryTooltipContentMaxWidth = LibraryTooltipMaxWidth - 10;
        public static double LibraryTooltipContentMaxHeight = LibraryTooltipMaxHeight - 17;

        public static double LibraryTooltipTextFontSize = 11;
        public static Thickness LibraryTooltipContentMargin = new Thickness(12, 5, 5, 5);

        public static double LibraryTooltipArrowHeight = 12;
        public static double LibraryTooltipArrowWidth = 6;

        #endregion

        #endregion

        #region CodeBlockNode

        public static readonly double CodeBlockPortHeightInPixels = 17.563333333333336;
        public static readonly int CBNMaxPortNameLength = 24;
        public static readonly string HighlightingFile =
            "DesignScript.Resources.SyntaxHighlighting.xshd";

        #endregion

        #region Externally Visible Strings

        public static readonly string SessionTraceDataXmlTag = "SessionTraceData";
        public static readonly string NodeTraceDataXmlTag = "NodeTraceData";
        public static readonly string CallsiteTraceDataXmlTag = "CallsiteTraceData";
        public static readonly string NodeIdAttribName = "NodeId";

        #endregion

        #region Preview Control Settings

        public static readonly double MaxExpandedPreviewWidth = MaxWatchNodeWidth;
        public static readonly double MaxExpandedPreviewHeight = MaxWatchNodeHeight;
        public static readonly double MaxCondensedPreviewWidth = 280.0;
        public static readonly double MaxCondensedPreviewHeight = 64.0;
        public static readonly double DefCondensedContentWidth = 33.0;
        public static readonly double DefCondensedContentHeight = 28.0;

        #endregion

    }

    public class ResourceNames
    {
        public static readonly string FontResourceUri = "./UI/Fonts/#Open Sans";
        public static readonly string ResourceBaseUri = "pack://application:,,,/DynamoCoreWpf;component/UI/Images/";
    }
}
