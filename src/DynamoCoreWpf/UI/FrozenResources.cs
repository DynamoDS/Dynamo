using System.Windows.Media;

namespace Dynamo.UI
{
    /// <summary>
    /// This class is put in place to store Freezable objects that are made 
    /// globally available as constant values. These resources are created the 
    /// first time any of the static members of FrozenResources is accessed.
    /// The static constructor is responsible of initializing its data members
    /// and freeze them at the same time.
    /// 
    /// This is done to avoid memory leaks. Consider the following use case:
    /// 
    ///     var myRectangle = new Rectangle();
    ///     myRectangle.Fill = FrozenResources.PreviewIconPinnedBrush;
    /// 
    /// Innocent as it is, the above code does lead to memory leaks. The second
    /// assignment made "myRectangle" subscribe to "PreviewIconPinnedBrush.Changed"
    /// event, causing "PreviewIconPinnedBrush" to reference "myRectangle" 
    /// internally (since it needs to notify "myRectangle" when its color updates).
    /// One would expect "myRectangle" gets garbage collected when it goes out of 
    /// scope, but this reference keeps "myRectangle" alive for as long as the 
    /// "PreviewIconPinnedBrush" is alive. Since it is a static, "myRectangle" 
    /// does not get released during collection and gets promoted to Gen 1. The 
    /// following will not cause a reference from "PreviewIconPinnedBrush" to 
    /// "myRectangle", avoiding any memory leaks:
    /// 
    ///     var myRectangle = new Rectangle();
    ///     FrozenResources.PreviewIconPinnedBrush.Freeze();
    ///     myRectangle.Fill = FrozenResources.PreviewIconPinnedBrush;
    /// 
    /// </summary>
    /// 
    public class FrozenResources
    {
        public static readonly SolidColorBrush PreviewIconPinnedBrush;
        public static readonly SolidColorBrush PreviewIconClickedBrush;
        public static readonly SolidColorBrush PreviewIconHoverBrush;
        public static readonly SolidColorBrush PreviewIconNormalBrush;

        #region Legacy Info Bubble related data members

        // TODO(Ben): Remove these once Info Bubble has been completely removed.
        public static readonly SolidColorBrush WarningFrameFill;
        public static readonly SolidColorBrush WarningFrameStrokeColor;
        public static readonly SolidColorBrush WarningTextForeground;
        public static readonly SolidColorBrush ErrorFrameFill;
        public static readonly SolidColorBrush ErrorFrameStrokeColor;
        public static readonly SolidColorBrush ErrorTextForeground;

        #endregion

        static FrozenResources()
        {
            PreviewIconPinnedBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x97, 0x93, 0x8E));
            PreviewIconClickedBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xA4, 0xA0, 0x9A));
            PreviewIconHoverBrush = new SolidColorBrush(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF));
            PreviewIconNormalBrush = new SolidColorBrush(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF));

            PreviewIconPinnedBrush.Freeze();
            PreviewIconClickedBrush.Freeze();
            PreviewIconHoverBrush.Freeze();
            PreviewIconNormalBrush.Freeze();

            #region Legacy Info Bubble related data members

            // TODO(Ben): Remove these once Info Bubble has been completely removed.
            WarningFrameFill = new SolidColorBrush(Color.FromRgb(0xff, 0xef, 0xa0));
            WarningFrameStrokeColor = new SolidColorBrush(Color.FromRgb(0xf2, 0xbd, 0x53));
            WarningTextForeground = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));
            ErrorFrameFill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            ErrorFrameStrokeColor = new SolidColorBrush(Color.FromRgb(190, 70, 70));
            ErrorTextForeground = new SolidColorBrush(Color.FromRgb(190, 70, 70));

            WarningFrameFill.Freeze();
            WarningFrameStrokeColor.Freeze();
            WarningTextForeground.Freeze();
            ErrorFrameFill.Freeze();
            ErrorFrameStrokeColor.Freeze();
            ErrorTextForeground.Freeze();

            #endregion
        }
    }
}