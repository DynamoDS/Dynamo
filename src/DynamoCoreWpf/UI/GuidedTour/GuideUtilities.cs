using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dynamo.Controls;
using Dynamo.Utilities;
using Dynamo.ViewModels;

namespace Dynamo.Wpf.UI.GuidedTour
{
    internal class GuideUtilities
    {
        /// <summary>
        /// Static method that finds a UIElement child based in the child name of a given root item in the Visual Tree. 
        /// </summary>
        /// <param name="parent">Root element in which the search will start</param>
        /// <param name="childName">Name of child to be found in the VisualTree </param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, a null parent is being returned.</returns>
        internal static UIElement FindChild(DependencyObject parent, string childName)
        {
            MenuItem menuItem = parent as MenuItem;

            UIElement foundChild;
            //Due that the child to find can be a MenuItem we need to call a different method for that
            if (menuItem != null)
                foundChild = FindChildInMenuItem(parent, childName);
            else
                foundChild = FindChildInVisualTree(parent, childName);

            return foundChild;
        }

        /// <summary>
        /// Find a Sub MenuItem based in childName passed as parameter
        /// </summary>
        /// <param name="parent">Main Window in which the child will be searched </param>
        /// <param name="childName">Name of the Sub Menu Item</param>
        /// <returns></returns>
        internal static UIElement FindChildInMenuItem(DependencyObject parent, string childName)
        {
            // Confirm parent is valid. 
            if (parent == null) return null;

            // Confirm child name is valid. 
            if (string.IsNullOrEmpty(childName)) return null;

            UIElement foundChild = null;

            MenuItem menuItem = parent as MenuItem;

            foreach (var item in menuItem.Items)
            {
                var innerMenuItem = item as MenuItem;

                if (innerMenuItem != null)
                {
                    // If the child's name match the searching string
                    if (innerMenuItem.Name.Equals(childName))
                    {
                        foundChild = innerMenuItem;
                        break;
                    }
                }
            }

            return foundChild;
        }

        /// <summary>
        /// This method will Find a child element in the WPF VisualTree of a Window
        /// </summary>
        /// <param name="parent">This represents the Window in which the child will be searched</param>
        /// <param name="childName">Child UIElement Name</param>
        /// <returns></returns>
        internal static UIElement FindChildInVisualTree(DependencyObject parent, string childName)
        {

            // Confirm parent is valid. 
            if (parent == null) return null;

            // Confirm child name is valid. 
            if (string.IsNullOrEmpty(childName)) return null;

            UIElement foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child != null)
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name match the searching string
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        foundChild = (UIElement)child;
                        break;
                    }
                    else
                    {
                        foundChild = FindChild(child, childName);

                        // If the child is found, break so we do not overwrite the found child. 
                        if (foundChild != null) break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (UIElement)child;
                    break;
                }
            }

            return foundChild;
        }

        /// <summary>
        /// Due that some Windows are opened dynamically, they are in the Owned Windows but not in the DynamoView VisualTree
        /// </summary>
        /// <param name="windowName">String name that represent the Window that will be search</param>
        /// <param name="mainWindow">The main Window, usually will be the DynamoView</param>
        /// <returns></returns>
        internal static Window FindWindowOwned(string windowName, Window mainWindow)
        {
            Window findWindow = null;
            foreach (Window window in mainWindow.OwnedWindows)
            {
                if (window.Name.Equals(windowName))
                {
                    findWindow = window;
                    break;
                }
            }
            return findWindow;
        }

        /// <summary>
        /// This method will find a Node in the Workspace based in the Node ID provided
        /// </summary>
        /// <param name="mainWindow">Dynamo Window</param>
        /// <param name="nodeID">ID of the node to be found</param>
        /// <returns>The instance of the NodeView found</returns>
        internal static NodeView FindNodeByID(UIElement mainWindow, string nodeID)
        {
            var nodeViewChildren = mainWindow.ChildrenOfType<NodeView>();

            //Means that we don't have nodes in the Workspace
            if (nodeViewChildren == null || nodeViewChildren.Count() == 0) return null;

            //Get the first CoordinateSystem.ByOrigin node
            var byOriginNode = (from nodeView in nodeViewChildren
                                where (nodeView.DataContext is NodeViewModel) &&
                                      (nodeView.DataContext as NodeViewModel).Id.ToString().Replace("-", "") == nodeID
                                select nodeView).FirstOrDefault();

            return byOriginNode;
        }

        internal static List<NodeView> FindNodesOfType(UIElement mainWindow, string nodeType)
        {
            var nodeViewChildren = mainWindow.ChildrenOfType<NodeView>();

            //Means that we don't have nodes in the Workspace
            if (nodeViewChildren == null || nodeViewChildren.Count() == 0) return null;

            //Get all the ByOrigin nodes in the Workspace
            var nodeViewsByOrigin = (from nodeView in nodeViewChildren
                             where (nodeView.DataContext is NodeViewModel) &&
                                   (nodeView.DataContext as NodeViewModel).Name.Equals(nodeType)
                             select nodeView).ToList();

            return nodeViewsByOrigin;
        }

        /// <summary>
        /// This method will close a specific Window owned by another Window
        /// </summary>
        /// <param name="windowName">The name of the Window to be closed</param>
        /// <param name="mainWindow">MainWindow container of the owned Window</param>
        internal static void CloseWindowOwned(string windowName, Window mainWindow)
        {
            Window findWindow = null;
            foreach (Window window in mainWindow.OwnedWindows)
            {
                if (window.Name.Equals(windowName))
                {
                    findWindow = window;
                    break;
                }
            }
            if (findWindow != null)
                findWindow.Close();
        }
    }
}
