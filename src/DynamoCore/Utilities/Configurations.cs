using System.Windows;
using System.Windows.Media;

using Dynamo.Models;

namespace Dynamo
{
    // SEPARATECORE: bad bad bad!!!!  get ui types out of this class, remove this from UI namespace

    public class Configurations
    {
        #region Dynamo Universal Constants

        // Add 0.5 to place the point in the middle of a pixel to sharpen it
        public static readonly string BackupFolderName = "backup";
        public static readonly string FilePathAttribName = "TargetXmlFilePath";

        #endregion

        // Generic Constants
        public static readonly double PortHeightInPixels = 26.0;

        #region CodeBlockNode
        public static readonly int CBNMaxPortNameLength = 24;
        public static readonly double CBNMaxTextBoxWidth = 500;
        public static readonly double CBNFontSize = 14.67;
        #endregion

        #region Externally Visible Strings

        public static readonly string SessionTraceDataXmlTag = "SessionTraceData";
        public static readonly string NodeTraceDataXmlTag = "NodeTraceData";
        public static readonly string CallsiteTraceDataXmlTag = "CallsiteTraceData";
        public static readonly string NodeIdAttribName = "NodeId";

        #endregion

#if DEBUG
        public const string UpdateDownloadLocation = "http://dyn-builds-dev.s3.amazonaws.com/";
        public const string UpdateSignatureLocation = "http://dyn-builds-dev-sig.s3.amazonaws.com/";
#else
        public const string UpdateDownloadLocation = "http://dyn-builds-data.s3-us-west-2.amazonaws.com/";
        public const string UpdateSignatureLocation = "http://dyn-builds-data-sig.s3-us-west-2.amazonaws.com/";
#endif
    }

}
