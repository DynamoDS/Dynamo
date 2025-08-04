using System.Windows.Controls;

namespace Dynamo.Wpf.Extensions
{
    /// <summary>
    /// Interface to be implemented by ViewExtensions that expose a Graph Metadata menu item.
    /// Allows the extension to provide a menu item that can be inserted into the main Dynamo UI.
    /// </summary>
    public interface IGraphMetadataProvider
    {
        /// <summary>
        /// Returns the menu item that launches the Graph Metadata extension UI.
        /// This item can be injected into any Dynamo menu.
        /// </summary>
        MenuItem GetGraphMetadataMenuItem();
    }
}
