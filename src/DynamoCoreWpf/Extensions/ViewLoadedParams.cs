using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Controls;
using Dynamo.UI.Controls;

namespace Dynamo.Wpf.Extensions
{
    /// <summary>
    /// Application level parameters passed to an extension when the DynamoView is loaded
    /// </summary>
    public class ViewLoadedParams
    {
        // TBD MAGN-7366
        //
        // Implementation notes:
        // 
        // This should be designed primarily to support the separation of the Package Manager from Core
        // and minimize exposing unnecessary innards.
        //
        // It is expected that this class will be extended in the future, so it should stay as minimal as possible.
        //
        // Here's a start on the implementation
        //

        //private readonly DynamoView view;

        //internal ViewLoadedParams(DynamoView view)
        //{
        //    this.view = view;
        //}

        ///// <summary>
        ///// Add a menu item for workspace context click
        ///// </summary>
        ///// <param name="item">The item to insert</param>
        ///// <param name="options">Options object to determine in which cases the MenuItem should be visible</param>
        //public void AddWorkspaceContextClickMenuItem(MenuItem item, WorkspaceContextClickOptions options)
        //{
            
        //}

        //public void AddShortcutBarItem(ShortcutBarItem item)
        //{
        //    // add an item to Dynamo's shortcut bar
        //}

        //public void AddMenuBar(MenuItem item)
        //{
            
        //}

        //public void AddKeyBinding(KeyBinding binding)
        //{
            
        //}
    }
}
