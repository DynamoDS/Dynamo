using Dynamo.Properties;
using Dynamo.Utilities;

namespace Dynamo.Configuration
{
    public class Configurations
    {
        #region Dynamo Universal Constants

        // Add 0.5 to place the point in the middle of a pixel to sharpen it
        public static readonly string FallbackUiCulture = "en-US";
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
        public static string DynamoVideoTutorials = "http://dynamobim.org/learn/#161";
        public static string DynamoPrimer = "http://dynamoprimer.com/";
        public static string DynamoDownloadLink = "http://dynamobim.org/download/";
        public static string GitHubDynamoLink = "https://github.com/DynamoDS/Dynamo";
        public static string GitHubBugReportingLink = "https://github.com/DynamoDS/Dynamo/issues";
        #endregion

        #region Canvas Configurations
        //public static readonly double Minimum

        // Generic Constants
        public static readonly double PortHeightInPixels = 26;

        // Canvas Control
        public static readonly double ZoomIncrement = 0.05;

        #endregion

        #region Tab Bar Configurations
        // Tabcontrol Settings        
        public static readonly int MinTabsBeforeClipping = 6;
        public static readonly int TabControlMenuWidth = 20;
        public static readonly int TabDefaultWidth = 200;
        #endregion

        #region Information Bubble
        internal static double MaxOpacity = 0.95;

        #region Preview Bubble
        internal static double PreviewTextFontSize = 10;

        internal static double PreviewMaxWidth = 500;
        internal static double PreviewMinWidth = 40;
        internal static double PreviewMinHeight = 30;
        internal static double PreviewDefaultMaxWidth = 300;
        internal static double PreviewDefaultMaxHeight = 200;

        internal static double PreviewCondensedMaxWidth = 300;
        internal static double PreviewCondensedMaxHeight = 200;
        internal static double PreviewCondensedMinWidth = 40;
        internal static double PreviewCondensedMinHeight = 0;
        internal static double PreviewCondensedContentMaxWidth = PreviewCondensedMaxWidth - 10;
        internal static double PreviewCondensedContentMaxHeight = PreviewCondensedMaxHeight - 17;

        internal static double PreviewArrowWidth = 12;
        internal static double PreviewArrowHeight = 6;

        #endregion

        #region Error Bubble

        internal static double ErrorFrameStrokeThickness = 1;

        internal static double ErrorMaxWidth = 300;
        internal static double ErrorMaxHeight = 200;
        internal static double ErrorContentMaxWidth = ErrorMaxWidth - 10;
        internal static double ErrorContentMaxHeight = ErrorMaxHeight - 16;

        internal static double ErrorCondensedMaxWidth = 75;
        internal static double ErrorCondensedMinWidth = 25;
        internal static double ErrorCondensedMaxHeight = 50;
        internal static double ErrorCondensedMinHeight = 25;
        internal static double ErrorCondensedContentMaxWidth = ErrorCondensedMaxWidth - 10;
        internal static double ErrorCondensedContentMaxHeight = ErrorCondensedMaxHeight - 16;

        internal static double ErrorTextFontSize = 13;
        internal static Thickness ErrorContentMargin = new Thickness(5, 5, 5, 12);

        internal static double ErrorArrowWidth = 12;
        internal static double ErrorArrowHeight = 6;
        #endregion

        #region Node Tooltip
        internal static double NodeTooltipFrameStrokeThickness = 1;

        internal static double NodeTooltipMaxWidth = 200;
        internal static double NodeTooltipMaxHeight = 200;
        internal static double NodeTooltipContentMaxWidth = NodeTooltipMaxWidth - 10;
        internal static double NodeTooltipContentMaxHeight = NodeTooltipMaxHeight - 16;

        internal static double NodeTooltipTextFontSize = 11;

        internal static Thickness NodeTooltipContentMarginLeft = new Thickness(11, 5, 5, 5);
        internal static Thickness NodeTooltipContentMarginRight = new Thickness(5, 5, 11, 5);
        internal static Thickness NodeTooltipContentMarginBottom = new Thickness(5, 5, 5, 11);

        internal static double NodeTooltipArrowWidth_BottomConnecting = 12;
        internal static double NodeTooltipArrowHeight_BottomConnecting = 6;
        internal static double NodeTooltipArrowWidth_SideConnecting = 6;
        internal static double NodeTooltipArrowHeight_SideConnecting = 12;

        internal static double ToolTipTargetGapInPixels = 3.0;
        internal static double NodeButtonHeight = 32; // Height of node button.
        internal static double BottomPanelHeight = 48; // Height of black bottom panel with 2 buttons: Run & Canсel.
        internal static int MaxLengthTooltipCode = 35; // Max length of field code in tooltip, it's near copy icon.
        internal static string NoDescriptionAvailable = Resources.NoDescriptionAvailable;

        #endregion

        #region Library Item Tooltip

        internal static double LibraryTooltipFrameStrokeThickness = 1;

        internal static double LibraryTooltipMaxWidth = 400;
        internal static double LibraryTooltipMaxHeight = 200;
        internal static double LibraryTooltipContentMaxWidth = LibraryTooltipMaxWidth - 10;
        internal static double LibraryTooltipContentMaxHeight = LibraryTooltipMaxHeight - 17;

        internal static double LibraryTooltipTextFontSize = 11;
        internal static Thickness LibraryTooltipContentMargin = new Thickness(12, 5, 5, 5);

        internal static double LibraryTooltipArrowHeight = 12;
        internal static double LibraryTooltipArrowWidth = 6;

        #endregion

        #endregion

        #region CodeBlockNode

        public static readonly double CodeBlockPortHeightInPixels = 17.573333333333336;
        public static readonly int CBNMaxPortNameLength = 24;
        public static readonly string HighlightingFile =
            "DesignScript.Resources.SyntaxHighlighting.xshd";

        #endregion

        #region Externally Visible Strings

        internal static readonly string SessionTraceDataXmlTag = "SessionTraceData";
        internal static readonly string NodeTraceDataXmlTag = "NodeTraceData";
        internal static readonly string CallsiteTraceDataXmlTag = "CallsiteTraceData";
        internal static readonly string NodeIdAttribName = "NodeId";

        #endregion

        #region NodeView

        /// <summary>
        /// Start ZIndex for nodes is set to 3, because 1 is for groups, 2 is for connectors.
        /// </summary>
        internal static readonly int NodeStartZIndex = 3;

        #endregion

        #region Preview Control Settings

        internal static readonly double MaxExpandedPreviewWidth = MaxWatchNodeWidth;
        internal static readonly double MaxExpandedPreviewHeight = MaxWatchNodeHeight;
        internal static readonly double MaxCondensedPreviewWidth = 280.0;
        internal static readonly double MaxCondensedPreviewHeight = 64.0;
        internal static readonly double DefCondensedContentWidth = 33.0;
        internal static readonly double DefCondensedContentHeight = 28.0;

        #endregion

        #region Icon Resources Strings

        internal const string SmallIconPostfix = ".Small";
        internal const string LargeIconPostfix = ".Large";
        internal const string IconResourcesDLL = ".customization.dll";
        internal const string DefaultIcon = "DefaultIcon";
        internal const string DefaultCustomNodeIcon = "DefaultCustomNode";
        internal const string DefaultAssembly = "DynamoCore";

        #endregion

        #region Class button
        internal const int MaxRowNumber = 2;
        internal const int MaxLengthRowClassButtonTitle = 9; // How many characters can be in one row.
        internal const string TwoDots = "..";
        #endregion

        #region LibraryView

        internal const double MinWidthLibraryView = 204;

        internal static string TopResult = Resources.TopResult;
        internal const string CategoryGroupCreate = "Create";
        internal const string CategoryGroupAction = "Actions";
        internal const string CategoryGroupQuery = "Query";
        internal const string CategoryDelimiterString = ".";
        internal const string ShortenedCategoryDelimiter = "-";
        internal const string CategoryDelimiterWithSpaces = " - ";

        internal const string ClassesDefaultName = "Classes";

        internal const string ElementTypeShorthandCategory = "CTGRY";
        internal const string ElementTypeShorthandPackage = "PKG";
        internal const string ElementTypeShorthandImportedDll = "DLL";

        #endregion

        #region ClassInformationView

        internal static string MoreButtonTextFormat = Resources.MoreButtonTextFormat;
        internal static string HeaderCreate = Resources.HeaderCreate;
        internal static string HeaderAction = Resources.HeaderAction;
        internal static string HeaderQuery = Resources.HeaderQuery;

        #endregion

        #region InCanvasSearch

        internal const double InCanvasSearchTextBoxHeight = 40.0;

        #endregion

        #region Backup

        public static string BackupFileNamePrefix = "backup";

        #endregion
    }
}
