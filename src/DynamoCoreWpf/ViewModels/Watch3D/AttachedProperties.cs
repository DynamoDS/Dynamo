using System;
using System.Windows;
using HelixToolkit.Wpf.SharpDX;

namespace Dynamo.Wpf.ViewModels.Watch3D
{
    /// <summary>
    /// The AttachedProperties class includes a number of Dependency
    /// Properties used by Dynamo to extend the capabilities of 
    /// GeometryModel3D objects. 
    /// </summary>
    [Obsolete("Do not use! This will be moved to a new project in a future version of Dynamo.")]
    internal static class AttachedProperties
    {
        #region Show Selected property

        // TODO: Make private in 3.0
        /// <summary>
        /// A flag indicating whether the geometry renders as selected.
        /// </summary>
        public static readonly DependencyProperty ShowSelectedProperty = DependencyProperty.RegisterAttached(
            "ShowSelected",
            typeof(bool),
            typeof(GeometryModel3D),
            new PropertyMetadata(false, ShowSelectedPropertyChanged));

        public static void SetShowSelected(DependencyObject element, bool value)
        {
            element.SetValue(ShowSelectedProperty, value);
        }

        public static bool GetShowSelected(DependencyObject element)
        {
            return (bool)element.GetValue(ShowSelectedProperty);
        }

        private static void ShowSelectedPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (!(obj is GeometryModel3D && obj.GetType() != typeof(BillboardTextModel3D)))
            {
                return;
            }
            var geom = (GeometryModel3D)obj;

            var meshGeom = geom as DynamoGeometryModel3D;
            if (meshGeom != null)
            {
                HandleMeshPropertyChange(meshGeom, args);
            }

            //implementation for lines and points
            else if (geom is DynamoPointGeometryModel3D || geom is DynamoLineGeometryModel3D)
            {
                HandlePointLinePropertyChange(geom, args);
            }
        }

        #endregion

        #region Transparency property

        // TODO: Make private in 3.0
        /// <summary>
        /// A flag indicating whether the geometry has transparency.
        /// </summary>
        public static readonly DependencyProperty HasTransparencyProperty = DependencyProperty.RegisterAttached(
            "HasTransparency",
            typeof(bool),
            typeof(GeometryModel3D),
            new PropertyMetadata(false));

        public static void SetHasTransparencyProperty(DependencyObject element, bool value)
        {
            element.SetValue(HasTransparencyProperty, value);
        }

        public static bool GetHasTransparencyProperty(DependencyObject element)
        {
            return (bool)element.GetValue(HasTransparencyProperty);
        }

        #endregion

        #region Frozen property

        // TODO: Make private in 3.0
        /// <summary>
        /// A flag indicating whether the geometry is frozen
        /// </summary>
        public static readonly DependencyProperty IsFrozenProperty = DependencyProperty.RegisterAttached(
            "IsFrozen",
            typeof(bool),
            typeof(GeometryModel3D),
            new PropertyMetadata(false, IsFrozenPropertyChanged));

        // TODO: Deprecate and remove in 3.0
        public static void SetIsFrozen(UIElement element, bool value)
        {
            element.SetValue(IsFrozenProperty, value);
        }

        public static void SetIsFrozen(DependencyObject element, bool value)
        {
            element.SetValue(IsFrozenProperty, value);
        }

        public static bool GetIsFrozen(DependencyObject element)
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
            if (!(obj is GeometryModel3D && obj.GetType() != typeof(BillboardTextModel3D)))
            {
                return;
            }
            var geom = (GeometryModel3D)obj;
            if (geom.Geometry == null)
            {
                return;
            }
            var meshGeom = geom as DynamoGeometryModel3D;
            if (meshGeom != null)
            {
                HandleMeshPropertyChange(meshGeom, e);
            }
            else if (geom is DynamoPointGeometryModel3D || geom is DynamoLineGeometryModel3D)
            {
                HandlePointLinePropertyChange(geom, e);
            }
        }

        #endregion

        #region Isolation Mode property

        // TODO: Make private in 3.0
        /// <summary>
        /// A flag indicating whether the geometry is currently under Isolate Selected Geometry mode.
        /// </summary>
        public static readonly DependencyProperty IsolationModeProperty = DependencyProperty.RegisterAttached(
            "IsolationMode",
            typeof(bool),
            typeof(GeometryModel3D),
            new PropertyMetadata(false, IsolationModePropertyChanged));

        public static void SetIsolationMode(DependencyObject element, bool value)
        {
            element.SetValue(IsolationModeProperty, value);
        }

        public static bool GetIsolationMode(DependencyObject element)
        {
            return (bool)element.GetValue(IsolationModeProperty);
        }

        private static void IsolationModePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            //don't isolate the grid, axes, etc
            if (!(obj is GeometryModel3D && !(obj is BillboardTextModel3D) && !IsSpecialRenderPackage(obj)))
            {
                return;
            }
            var geom = (GeometryModel3D)obj;
            var meshGeom = geom as DynamoGeometryModel3D;
            if (meshGeom != null)
            {

                HandleMeshPropertyChange(meshGeom, e);
                meshGeom.RequiresPerVertexColoration = true;

            }
            else if (geom is DynamoPointGeometryModel3D || geom is DynamoLineGeometryModel3D)
            {
                HandlePointLinePropertyChange(geom, e);
            }
        }

        #endregion

        #region Special RenderPackage property

        // TODO: Make private in 3.0
        /// <summary>
        /// A flag indicating whether the geometry is special render package, such as used to draw manipulators.
        /// </summary>
        public static readonly DependencyProperty IsSpecialRenderPackageProperty = DependencyProperty.RegisterAttached(
            "IsSpecialRenderPackage",
            typeof(bool),
            typeof(GeometryModel3D),
            new PropertyMetadata(false,IsSpecialRenderPackagePropertyChanged));

        private static void IsSpecialRenderPackagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            //only need to handle special arrow meshes at this time.
            var meshGeom = d as DynamoGeometryModel3D;
            if (meshGeom != null)
            {
                HandleMeshPropertyChange(meshGeom, args);
            }
        }

        public static void SetIsSpecialRenderPackage(DependencyObject element, bool value)
        {
            element.SetValue(IsSpecialRenderPackageProperty, value);
        }

        public static bool IsSpecialRenderPackage(DependencyObject element)
        {
            return (bool)element.GetValue(IsSpecialRenderPackageProperty);
        }

        #endregion

        #region utils
        
        /// <summary>
        /// Sets the property value on the DynamoMeshCore - this makes its way down to our shader and invalidates the render.
        /// </summary>
        /// <param name="meshGeom"></param>
        /// <param name="args"></param>
        internal static void HandleMeshPropertyChange(DynamoGeometryModel3D meshGeom, DependencyPropertyChangedEventArgs args)
        {
            var meshCore = meshGeom?.SceneNode?.RenderCore as DynamoGeometryMeshCore;
            meshCore?.SetPropertyData(args);
        }

        private static void HandlePointLinePropertyChange(GeometryModel3D pointLineGeom,
            DependencyPropertyChangedEventArgs args)
        {
            if (pointLineGeom is DynamoPointGeometryModel3D || pointLineGeom is DynamoLineGeometryModel3D)
            {
                (pointLineGeom.SceneNode?.RenderCore as DynamoPointLineRenderCore)?.SetPropertyData(args, pointLineGeom);
            } 
        }

        #endregion

    }
}
