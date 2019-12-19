using System.Windows;
using System.Windows.Media;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;

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
            if(obj is GeometryModel3D && obj.GetType() != typeof(BillboardTextModel3D))
            {
                var geom = (GeometryModel3D)obj;
                // TODO DYN-973: Need new mechanism to trigger render update after selected/frozen/isolated properties change

                var meshGeom = geom as DynamoGeometryModel3D;
                if (meshGeom != null)
                {
                    if ((bool)args.NewValue)
                    {
                        meshGeom.Material = HelixWatch3DViewModel.SelectedMaterial;
                        //meshGeom.Material = new VertColorMaterial();

                        //var colors = geom.Geometry.Colors.ToArray();
                        //var len = colors.Length;

                        //for (int i = 0; i < len; i++)
                        //{
                        //    colors[i].Blue = 0.0f;
                        //    colors[i].Green = 0.0f;
                        //    colors[i].Red = 1f;
                        //    colors[i].Alpha = 1f;
                        //}

                        //var colorCollection = new Color4Collection(colors);
                        //geom.Geometry.Colors = colorCollection;
                        //geom.Geometry.UpdateColors();
                    }
                    else
                    {
                        meshGeom.Material = GetIsolationMode(meshGeom) ?
                            HelixWatch3DViewModel.TransparentMaterial : HelixWatch3DViewModel.WhiteMaterial;
                    }
                }
                
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
            typeof (bool),
            typeof (GeometryModel3D),
            new PropertyMetadata(false));

        public static void SetHasTransparencyProperty(DependencyObject element, bool value)
        {
            element.SetValue(HasTransparencyProperty, value);
        }

        public static bool GetHasTransparencyProperty(DependencyObject element)
        {
            return (bool) element.GetValue(HasTransparencyProperty);
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
            if (obj is GeometryModel3D && obj.GetType() != typeof(BillboardTextModel3D))
            {
                var geom = (GeometryModel3D)obj;
                if (geom.Geometry == null)
                {
                    return;
                }
                // TODO DYN-973: Need new mechanism to trigger render update after selected/frozen/isolated properties change

                var dynamoGeom3D = geom as DynamoGeometryModel3D;
                if (dynamoGeom3D != null)
                {
                    if ((bool)e.NewValue)
                    {
                        dynamoGeom3D.Material = HelixWatch3DViewModel.TransparentMaterial;
                    }
                    else
                    {
                        dynamoGeom3D.Material = GetIsolationMode(dynamoGeom3D) ? 
                            HelixWatch3DViewModel.TransparentMaterial : HelixWatch3DViewModel.WhiteMaterial;
                    }

                    dynamoGeom3D.RequiresPerVertexColoration = true;
                    geom = dynamoGeom3D;
                }

                //var colors = geom.Geometry.Colors.ToArray();

                //for (int i = 0; i < colors.Length; i++)
                //{
                //    colors[i].Alpha = colors[i].Alpha * alphaPropertyFactor;
                //}

                //var colorCollection = new Color4Collection(colors);
                //geom.Geometry.Colors = colorCollection;
                //geom.Geometry.UpdateColors();

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
            if (obj is GeometryModel3D && obj.GetType() != typeof(BillboardTextModel3D))
            {
                var geom = (GeometryModel3D)obj;

                // TODO DYN-973: Need new mechanism to trigger render update after selected/frozen/isolated properties change
                var dynamoGeom3D = geom as DynamoGeometryModel3D;
                if (dynamoGeom3D != null)
                {
                    if ((bool)e.NewValue)
                    {
                        dynamoGeom3D.Material = HelixWatch3DViewModel.TransparentMaterial;
                    }
                    else
                    {
                        dynamoGeom3D.Material = HelixWatch3DViewModel.WhiteMaterial;
                    }

                    dynamoGeom3D.RequiresPerVertexColoration = true;
                }
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
            new PropertyMetadata(false));

        public static void SetIsSpecialRenderPackage(DependencyObject element, bool value)
        {
            element.SetValue(IsSpecialRenderPackageProperty, value);
        }

        public static bool IsSpecialRenderPackage(DependencyObject element)
        {
            return (bool)element.GetValue(IsSpecialRenderPackageProperty);
        }

        #endregion

        //#region Active Geometry property

        //public static readonly DependencyProperty ActiveGeometryProperty = DependencyProperty.RegisterAttached(
        //    "ActiveGeometry",
        //    typeof(GeometryModel3D),
        //    typeof(Viewport3DX),
        //    new PropertyMetadata(null, ActiveGeometryPropertyChanged));

        //public static void SetActiveGeometry(DependencyObject element, GeometryModel3D value)
        //{
        //    element.SetValue(ActiveGeometryProperty, value);
        //}

        //public static GeometryModel3D GetActiveGeometry(DependencyObject element)
        //{
        //    return (GeometryModel3D)element.GetValue(ActiveGeometryProperty);
        //}

        //private static void ActiveGeometryPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        //{

        //}

        //#endregion


    }
}
