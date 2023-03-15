using Dynamo.Properties;
using Dynamo.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dynamo.Configuration
{
    /// <summary>
    /// This class contains properties that are used in Dynamo.
    /// </summary>
    public class Configurations
    {
        #region Dynamo Universal Constants

        /// <summary>
        /// User interface culture
        /// </summary>
        public static readonly string FallbackUiCulture = "en-US";

        /// <summary>
        /// Name of backup folder
        /// </summary>
        public static readonly string BackupFolderName = "backup";

        /// <summary>
        /// Name of XML attribute which contains target file path. 
        /// </summary>
        public static readonly string FilePathAttribName = "TargetXmlFilePath";

        /// <summary>
        /// Default width of Double Slider
        /// </summary>
        public static readonly double DoubleSliderTextBoxWidth = 55.0;

        /// <summary>
        /// Default width of Integer Slider
        /// </summary>
        public static readonly double IntegerSliderTextBoxWidth = 30.0;

        /// <summary>
        /// Maximum width of Watch Node
        /// </summary>
        [Obsolete("This property is no longer used. Remove in Dynamo 3.0")]
        public static readonly double MaxWatchNodeWidth = 280.0;

        /// <summary>
        /// Maximum height of Watch Node
        /// </summary>
        [Obsolete("This property is no longer used. Remove in Dynamo 3.0")]
        public static readonly double MaxWatchNodeHeight = 310.0;

        #endregion

        #region Usage Reporting Error Message

        /// <summary>
        /// Link to Dynamo site
        /// </summary>
        public static string DynamoSiteLink = "http://dynamobim.org/";

        /// <summary>
        /// Link to Dynamo wiki
        /// </summary>
        public static string DynamoWikiLink = "https://github.com/DynamoDS/Dynamo/wiki";

        /// <summary>
        /// Link to Dynamo forum
        /// </summary>
        public static string DynamoBimForum = "http://dynamobim.org/forums/forum/dyn/";

        /// <summary>
        /// DynamoTeam email
        /// </summary>
        public static string DynamoTeamEmail = "mailto:team@dynamobim.org";

        /// <summary>
        /// Link to Dynamo video tutorials
        /// </summary>
        public static string DynamoVideoTutorials = "http://dynamobim.org/learn/#161";

        /// <summary>
        /// Link to Dynamo dictionary
        /// </summary>
        public static string DynamoDictionary = "http://dictionary.dynamobim.com/2/";

        /// <summary>
        /// Link to Dynamo primer
        /// </summary>
        public static string DynamoPrimer = "http://primer2.dynamobim.org/";

        /// <summary>
        /// Link to Dynamo download page
        /// </summary>
        public static string DynamoDownloadLink = "http://dynamobim.org/download/";

        /// <summary>
        /// Link to Dynamo's github repo
        /// </summary>
        public static string GitHubDynamoLink = "https://github.com/DynamoDS/Dynamo";

        /// <summary>
        /// Link to Dynamo's issues on github
        /// </summary>
        public static string GitHubBugReportingLink = "https://github.com/DynamoDS/Dynamo/issues/new";
        #endregion

        #region Canvas Configurations
        //public static readonly double Minimum

        /// <summary>
        /// Generic Constants
        /// </summary>
        public static readonly double PortHeightInPixels = 34;

        /// <summary>
        /// Canvas Control
        /// </summary>
        public static readonly double ZoomIncrement = 0.05;

        /// <summary>
        /// Global zoom threshold level for node overlay to apply (zoom-in/out behavior) 
        /// </summary>
        public static readonly double ZoomThreshold = 0.4;

        /// <summary>
        /// Global zoom threshold for renaming Nodes and Notes 
        /// </summary>
        public static readonly double ZoomDirectEditThreshold = 0.5;

        #endregion

        #region Tab Bar Configurations
        /// <summary>
        /// Count of tabs before clipping
        /// </summary>       
        public static readonly int MinTabsBeforeClipping = 6;

        /// <summary>
        /// Default width of Tab control menu
        /// </summary>
        public static readonly int TabControlMenuWidth = 20;

        /// <summary>
        /// Default width of tab
        /// </summary>
        public static readonly int TabDefaultWidth = 225;

        /// <summary>
        /// Minimum width of tab in the extensions side bar
        /// </summary>
        public static readonly int ExtensionsSideBarTabMinWidth = 150;

        #endregion

        #region Information Bubble

        /// <summary>
        /// Maximal opacity of bubble
        /// </summary>
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

        /// <summary>
        ///     Default height of CodeBlock's port. Now obsolete.
        ///     Inputs height are set in the normal way, outputs height is set to CodeBlockOutputPortHeightInPixels
        /// </summary>
        [Obsolete] public static readonly double CodeBlockPortHeightInPixels = 17.573333333333336;

        /// <summary>
        ///     Code Block outputs have a condensed port height
        /// </summary>
        public static readonly double CodeBlockOutputPortHeightInPixels = 16.345;

        /// <summary>
        ///     Maximal port name length of CodeBlock
        /// </summary>
        public static readonly int CBNMaxPortNameLength = 24;

        /// <summary>
        ///     Highlighting File
        /// </summary>
        public static readonly string HighlightingFile =
            "DesignScript.Resources.SyntaxHighlighting.xshd";

        #endregion

        #region Externally Visible Strings

        #region Legacy XML sessionTraceData serialization
        internal static readonly string SessionTraceDataXmlTag = "SessionTraceData";
        #endregion

        #region Bindings Json serialization
        internal static readonly string BindingsTag = "Bindings";
        internal static readonly string BingdingTag = "Binding";
        internal static readonly string NodeIdAttribName = "NodeId";
        internal static readonly string CallSiteID = "CallSiteID";
        #endregion

        #endregion

        #region NodeView

        /// <summary>
        /// Start ZIndex for nodes is set to 3, because 1 is for groups, 2 is for connectors.
        /// </summary>
        internal static readonly int NodeStartZIndex = 3;

        #endregion

        #region Preview Control Settings

        internal static readonly double MaxExpandedPreviewWidth = 280.0;
        internal static readonly double MaxExpandedPreviewHeight = 310.0;
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

        /// <summary>
        ///     Default backup file name prefix
        /// </summary>
        public static string BackupFileNamePrefix = "backup";

        #endregion

        #region Locale info and constants

        /// <summary>
        /// Supported locales as a list
        /// </summary>
        internal static readonly List<string> SupportedLocaleList = new List<string>() { "en-US", "cs-CZ", "de-DE", "es-ES", "fr-FR", "it-IT", "ja-JP", "ko-KR", "pl-PL", "pt-BR", "ru-RU", "zh-CN", "zh-TW" };

        /// <summary>
        /// Supported languages and locales as a dictionary in the current thread locale
        /// </summary>
        public static Dictionary<string, string> SupportedLocaleDic
        {
            get
            {
                // Dynamically create a dictionary mapping languages and locales in the current thread locale
                // This is done so that Preferences Panel could display the languages selections using the current locale
                return Properties.Resources.DynamoLanguages_noxlate.Split(',').
                    Zip(SupportedLocaleList, (k, v) => (k, v)).ToDictionary(x => x.k, x => x.v);
            }
        }

        #endregion
    }
}
