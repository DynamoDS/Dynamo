using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Dynamo.Utilities
{
    static class WpfUtilities
    {
        // walk up the visual tree to find object of type T, starting from initial object
        public static T FindUpVisualTree<T>(DependencyObject initial) where T : DependencyObject
        {
            DependencyObject current = initial;

            while (current != null && current.GetType() != typeof(T))
            {
                current = VisualTreeHelper.GetParent(current);
            }
            return current as T;
        }

        /// <summary>
        /// Finds a Child of a given item in the visual tree. 
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, 
        /// a null parent is being returned.</returns>
        public static T ChildOfType<T>(this DependencyObject parent, string childName = null)
           where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            if (childName != null)
            {
                return parent.ChildrenOfType<T>()
                    .FirstOrDefault(x =>
                    {
                        var xf = x as FrameworkElement;
                        if (xf == null) return false;
                        return xf.Name == childName;
                    });
            }

            return parent.ChildrenOfType<T>().FirstOrDefault();
        }

        public static IEnumerable<DependencyObject> Children(this DependencyObject parent)
        {
            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
                yield return VisualTreeHelper.GetChild(parent, i);
        }

        public static IEnumerable<T> ChildrenOfType<T>(this DependencyObject parent)
          where T : DependencyObject
        {
            foreach (var child in parent.Children())
            {
                var childType = child as T;
                if (childType == null)
                {
                    foreach (var ele in ChildrenOfType<T>(child)) yield return ele;
                }
                else
                {
                    yield return childType;
                }
            }
        }

        //TODO (Vladimir): take a look on ChildrenOfType method and try to rework 
        //                 FindChildren method in that manner.
        //                 http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-6202

        /// <summary>
        /// Call this method to find child elements in a visual tree of a specific type,
        /// given the parent visual element.
        /// </summary>
        /// <param name="parent">The parent visual element from which child visual elements 
        /// are to be located.</param>
        /// <typeparam name="T">The type of child visual element to look for.</typeparam>
        /// <param name="childName">The name of child element to look for. This value can 
        /// be an empty string if child element name is not a search criteria.</param>
        /// <param name="foundChildren">A list of child elements that match the search criteria, 
        /// or an empty list if none is found.</param>
        /// 
        public static void FindChildren<T>(DependencyObject parent, string childName, List<T> foundChildren)
           where T : DependencyObject
        {
            if (parent == null) return;
            if (foundChildren == null) return;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                T childType = child as T;
                if (childType == null)
                {
                    FindChildren<T>(child, childName, foundChildren);
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    if (frameworkElement != null && frameworkElement.Name == childName)
                        foundChildren.Add((T)child);
                }
                else
                {
                    foundChildren.Add((T)child);
                }
            }
        }
    }
}
