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
        private const float alphaPropertyFactor = 0.5f;
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
        public static readonly DependencyProperty IsFrozenProperty = DependencyProperty.RegisterAttached(
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
            return (bool)element.GetValue(IsFrozenProperty) &&
                !IsSpecialRenderPackage(element);
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
                if (geom.Geometry == null || geom.Geometry.Colors == null)
                {
                    return;
                }

                var colors = geom.Geometry.Colors.ToArray();

                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i].Alpha = colors[i].Alpha * alphaPropertyFactor;
                }

                geom.Geometry.Colors.Clear();
                geom.Geometry.Colors.AddRange(colors);

                var dynamoGeom3D = geom as DynamoGeometryModel3D;
                if (dynamoGeom3D != null)
                {
                    dynamoGeom3D.RequiresPerVertexColoration = true;
                    geom = dynamoGeom3D;
                }

                if (geom.IsAttached)
                {
                    var host = geom.RenderHost;
                    geom.Detach();
                    geom.Attach(host);
                }
            }
        }

        /// <summary>
        /// A flag indicating whether the geometry is currently under Isolate Selected Geometry mode.
        /// </summary>
        public static readonly DependencyProperty IsolationModeProperty = DependencyProperty.RegisterAttached(
            "IsolationMode",
            typeof(bool),
            typeof(GeometryModel3D),
            new PropertyMetadata(false, IsolationModePropertyChanged));

        public static void SetIsolationMode(UIElement element, bool value)
        {
            element.SetValue(IsolationModeProperty, value);
        }

        public static bool GetIsolationMode(UIElement element)
        {
            return (bool)element.GetValue(IsolationModeProperty);
        }
        
        private static void IsolationModePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is GeometryModel3D && obj.GetType() != typeof(BillboardTextModel3D))
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
        /// A flag indicating whether the geometry is special render package, such as used to draw manipulators.
        /// </summary>
        public static readonly DependencyProperty IsSpecialRenderPackageProperty = DependencyProperty.RegisterAttached(
            "IsSpecialRenderPackage",
            typeof(bool),
            typeof(GeometryModel3D),
            new PropertyMetadata(false));

        public static void SetIsSpecialRenderPackage(UIElement element, bool value)
        {
            element.SetValue(IsSpecialRenderPackageProperty, value);
        }

        public static bool IsSpecialRenderPackage(UIElement element)
        {
            return (bool)element.GetValue(IsSpecialRenderPackageProperty);
        }

    }
}
