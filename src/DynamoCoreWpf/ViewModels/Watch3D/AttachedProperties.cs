﻿using System.Linq;
using System.Windows;

using HelixToolkit.Wpf.SharpDX;

namespace Dynamo.Wpf.ViewModels.Watch3D
{
    /// <summary>
    /// The AttachedProperties class includes a number of Dependency
    /// Properties used by Dynamo to extend the capabilities of 
    /// GeometryModel3D objects. 
    /// </summary>
    public static class AttachedProperties
    {
        private const float transparentGeometry = 0.5f;
        /// <summary>
        /// A flag indicating whether the geometry renders as selected.
        /// </summary>
        public static readonly DependencyProperty ShowSelectedProperty = DependencyProperty.RegisterAttached(
            "ShowSelected",
            typeof(bool),
            typeof(GeometryModel3D),
            new PropertyMetadata(false, ShowSelectedPropertyChanged));

        public static void SetShowSelected(UIElement element, bool value)
        {
            element.SetValue(ShowSelectedProperty, value);
        }

        public static bool GetShowSelected(UIElement element)
        {
            return (bool)element.GetValue(ShowSelectedProperty);
        }

        private static void ShowSelectedPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if(obj is GeometryModel3D && obj.GetType() != typeof(BillboardTextModel3D))
            {
                var geom = (GeometryModel3D)obj;
                
                if (geom.IsAttached)
                {
                    var host = geom.RenderHost;
                    geom.Detach();
                    geom.Attach(host);
                }
            }
        }

        /// <summary>
        /// A flag indicating whether the geometry has transparency.
        /// </summary>
        public static readonly DependencyProperty HasTransparencyProperty = DependencyProperty.RegisterAttached(
            "HasTransparency",
            typeof (bool),
            typeof (GeometryModel3D),
            new PropertyMetadata(false));

        public static void SetHasTransparencyProperty(UIElement element, bool value)
        {
            element.SetValue(HasTransparencyProperty, value);
        }

        public static bool GetHasTransparencyProperty(UIElement element)
        {
            return (bool) element.GetValue(HasTransparencyProperty);
        }


        /// <summary>
        /// A flag indicating whether the geometry is frozen
        /// </summary>
        public static readonly  DependencyProperty IsFrozenProperty = DependencyProperty.RegisterAttached(
            "IsFrozen",
            typeof(bool),
            typeof(GeometryModel3D),
            new PropertyMetadata(false, IsFrozenPropertyChanged));

        public static void SetIsFrozen(UIElement element, bool value)
        {
            element.SetValue(IsFrozenProperty, value);
        }

        public static bool GetIsFrozen(UIElement element)
        {
            return (bool)element.GetValue(IsFrozenProperty);
        }


        /// <summary>
        /// This updates the transparency on the Geometry object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void IsFrozenPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is GeometryModel3D && obj.GetType() != typeof(BillboardTextModel3D))
            {
                var geom = (GeometryModel3D)obj;
                var colors = geom.Geometry.Colors.ToArray();

                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i].Alpha = colors[i].Alpha * transparentGeometry;
                }

                geom.Geometry.Colors.Clear();
                geom.Geometry.Colors.AddRange(colors);

                var vertex = geom as DynamoGeometryModel3D;
                if (vertex != null)
                {
                    vertex.RequiresPerVertexColoration = true;
                    geom = vertex;
                }

                if (geom.IsAttached)
                {
                    var host = geom.RenderHost;
                    geom.Detach();
                    geom.Attach(host);
                }
            }
        }

    }
}
