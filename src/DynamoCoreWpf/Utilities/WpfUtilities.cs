using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using Dynamo.Controls;

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
            foreach(var child in parent.Children())
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


    }
}
