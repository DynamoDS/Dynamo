using System;

namespace Dynamo.Wpf.Extensions
{
    /// <summary>
    /// A class of extensions for the MenuBarType Enum.
    /// </summary>
    internal static class MenuBarTypeExtensions
    {
        /// <summary>
        /// A method to extract the appropriate localized string 
        /// representing the header name of this menu type
        /// i.e. file -> "_File"
        /// </summary>
        /// <param name="type"></param>
        /// <returns>A localized string used as the menu header</returns>
        internal static string ToDisplayString(this MenuBarType type)
        {
            switch (type)
            {
                case MenuBarType.File: return Properties.Resources.DynamoViewFileMenu;
                case MenuBarType.Edit: return Properties.Resources.DynamoViewEditMenu;
                case MenuBarType.View: return Properties.Resources.DynamoViewViewMenu;
                case MenuBarType.Help: return Properties.Resources.DynamoViewHelpMenu;
                case MenuBarType.Packages: return Properties.Resources.DynamoViewPackageMenu;

                default: throw new ArgumentOutOfRangeException("type");
            }
        }
    }
}
