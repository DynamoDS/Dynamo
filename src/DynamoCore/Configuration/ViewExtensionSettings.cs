namespace Dynamo.Configuration
{
    /// <summary>
    /// Settings that apply to a view extension specifically.
    /// </summary>
    public class ViewExtensionSettings
    {
        /// <summary>
        /// Name of the view extension.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// UniqueId of the view extension.
        /// </summary>
        public string UniqueId { get; set; }
        /// <summary>
        /// Specifies how an extension UI control should be displayed.
        /// </summary>
        public ViewExtensionDisplayMode DisplayMode { get; set; }
        /// <summary>
        /// Window settings for the extension control when displayed in FloatingWindow mode.
        /// </summary>
        public WindowSettings WindowSettings { get; set; }
        /// <summary>
        /// Specifies if the extension was Open in the last session before closing Dynamo, if the property to remember view extension status was enabled.<br/> <c>Default: False</c>
        /// </summary>
        public bool IsOpen { get; set; }
    }

    /// <summary>
    /// Possible display modes for an extension UI control.
    /// </summary>
    public enum ViewExtensionDisplayMode
    {
        /// <summary>
        /// Not really a display mode but rather the absence of one.
        /// </summary>
        Unspecified,
        /// <summary>
        /// Extension control should be displayed docked to the right side.
        /// </summary>
        DockRight,
        /// <summary>
        /// Extension control should be displayed in a floating window.
        /// </summary>
        FloatingWindow
    }

    /// <summary>
    /// Settings that define how to display an extension control in floating window mode.
    /// </summary>
    public class WindowSettings
    {
        /// <summary>
        /// Status of the window, i.e. whether it is maximized.
        /// </summary>
        public WindowStatus Status { get; set; }
        /// <summary>
        /// Coordinates of the leftmost side of the window.
        /// </summary>
        public int Left { get; set; }
        /// <summary>
        /// Coordinates of the topmost side of the window.
        /// </summary>
        public int Top { get; set; }
        /// <summary>
        /// Width of the window.
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Height of the window.
        /// </summary>
        public int Height { get; set; }
    }

    /// <summary>
    /// Possible status of a floating window.
    /// </summary>
    public enum WindowStatus
    {
        /// <summary>
        /// The window can be moved and resized.
        /// </summary>
        Normal,
        /// <summary>
        /// The window is maximized.
        /// </summary>
        Maximized
    }
}
