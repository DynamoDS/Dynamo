using System.Windows.Controls;

namespace Dynamo.Wpf.Extensions
{
    /// <summary>
    /// Interface for ViewExtensions that expose custom MenuItems to be injected into Dynamo's main menus.
    /// Extensions implementing this interface can return MenuItems to be added dynamically
    /// to the File menu, Edit menu, or other parts of the UI.
    /// </summary>
    public interface IExtensionMenuProvider
    {
        /// <summary>
        /// Returns the menu item to be inserted into the Dynamo UI.
        /// </summary>
        MenuItem GetFileMenuItem();
    }
}
