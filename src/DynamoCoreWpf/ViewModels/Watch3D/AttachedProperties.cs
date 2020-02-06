using System;
using System.Linq;
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

        // handles determining color of elementGeometry3Ds when any property is set false
        // as the state of other properties must be checked to determine the correct color / material.
        private static void OnPropertySetFalse(DependencyObject obj)
        {
            if (obj is GeometryModel3D && obj.GetType() != typeof(BillboardTextModel3D))
            {
                //mesh case
                var geom = (GeometryModel3D)obj;
                var meshGeom = geom as DynamoGeometryModel3D;
                if (meshGeom != null)
                {

                    // if selection is not enabled after this property was set false
                    // then determine transparencey. IE selection state should always override
                    // the other properties.
                    if (!GetShowSelected(meshGeom))
                    {
                        if (GetIsolationMode(meshGeom))
                        {
                            meshGeom.Material = HelixWatch3DViewModel.IsolatedMaterial;
                        }
                        else if (GetIsFrozen(meshGeom))
                        {
                            meshGeom.Material = HelixWatch3DViewModel.FrozenMaterial;
                        }
                        else
                        {
                            //TODO handle vertex coloring in all these cases
                            meshGeom.Material = HelixWatch3DViewModel.WhiteMaterial;
                        }

                    }

                }
                //point or line case
                else if (geom is DynamoPointGeometryModel3D || geom is DynamoLineGeometryModel3D)
                {
                    //if selection is not enabled determine if we should reset colors or set transparent colors
                    if (!GetShowSelected(geom))
                    {
                        Color4Collection newColors;
                        if (GetIsolationMode(geom))
                        {
                            //reset the colors
                            RequestResetColorsForDynamoGeometryModel?.Invoke(geom.Tag as string);
                            //then modify alpha
                            newColors = new Color4Collection(geom.Geometry.Colors.Select(col =>
                            {
                                col.Alpha = HelixWatch3DViewModel.IsolatedMaterial.DiffuseColor.Alpha;
                                return col;
                            }));
                            geom.Geometry.Colors = newColors;
                        }
                        else if (GetIsFrozen(geom))
                        {
                            //reset the colors
                            RequestResetColorsForDynamoGeometryModel?.Invoke(geom.Tag as string);
                            //then modify alpha
                            newColors = new Color4Collection(geom.Geometry.Colors.Select(col =>
                            {
                                col.Alpha = HelixWatch3DViewModel.FrozenMaterial.DiffuseColor.Alpha;
                                return col;
                            }));
                            geom.Geometry.Colors = newColors;

                        }
                        //all attached props are false, lets reset colors
                        else
                        {
                            RequestResetColorsForDynamoGeometryModel?.Invoke(geom.Tag as string);
                        }
                    }
                }
            }
        }

        //TODO should this pass the dep object or an id, current wants an id of the node.
        internal static event Action<string> RequestResetColorsForDynamoGeometryModel;
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
            if (obj is GeometryModel3D && obj.GetType() != typeof(BillboardTextModel3D))
            {
                var geom = (GeometryModel3D)obj;
                
                var meshGeom = geom as DynamoGeometryModel3D;
                if (meshGeom != null)
                {   
                    if ((bool)args.NewValue)
                    {
                        //if the item is both selected and isolation mode is on, then we should color the item as normal OR as frozen.
                        if (GetIsolationMode(meshGeom) )
                        {
                            //selected, isolated, and frozen.
                            if (GetIsFrozen(meshGeom))
                            {
                                meshGeom.Material = HelixWatch3DViewModel.FrozenMaterial;
                            }
                            //selected and isolated so color normal material
                            //TODO handle vertex colors later.
                            else
                            {
                                meshGeom.Material = HelixWatch3DViewModel.WhiteMaterial;
                            }
                        }
                        //only selected.
                        else
                        {
                            meshGeom.Material = HelixWatch3DViewModel.SelectedMaterial;
                        }
                    }
                    else
                    {
                        OnPropertySetFalse(meshGeom);
                    }
                }
                else if (geom is DynamoPointGeometryModel3D || geom is DynamoLineGeometryModel3D)
                {

                    if ((bool)args.NewValue)
                    {
                        //if the item is both selected and isolation mode is on, then we should color the item as normal OR as frozen.
                        if (GetIsolationMode(geom))
                        {
                            //selected, isolated, and frozen.
                            if (GetIsFrozen(geom))
                            {
                                //TODO refactor this out into a utility method
                                //reset the colors
                                RequestResetColorsForDynamoGeometryModel?.Invoke(geom.Tag as string);
                                //then modify alpha
                                var newColors = new Color4Collection(geom.Geometry.Colors.Select(col =>
                                {
                                    col.Alpha = HelixWatch3DViewModel.FrozenMaterial.DiffuseColor.Alpha;
                                    return col;
                                }));
                                geom.Geometry.Colors = newColors;
                            }
                            //selected and isolated, so we just reset the colors.
                            else
                            {
                                //reset the colors
                                RequestResetColorsForDynamoGeometryModel?.Invoke(geom.Tag as string);
                            }
                        }
                        //only selected.
                        else
                        {
                            //TODO cache this and update after helix 2.11 is released
                            var selectedColorCollection = new Color4Collection(Enumerable.Repeat(HelixWatch3DViewModel.SelectedMaterial.DiffuseColor, geom.Geometry.Colors.Count));
                            geom.Geometry.Colors = selectedColorCollection;
                        }
                    }
                    else
                    {
                        OnPropertySetFalse(geom);
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
            if (obj is GeometryModel3D && obj.GetType() != typeof(BillboardTextModel3D))
            {
                var geom = (GeometryModel3D)obj;
                if (geom.Geometry == null)
                {
                    return;
                }
                // TODO DYN-973: Need new mechanism to trigger render update after selected/frozen/isolated properties change

                var meshGeom3d = geom as DynamoGeometryModel3D;
                if (meshGeom3d != null)
                {
                    if ((bool)e.NewValue)
                    {
                        meshGeom3d.Material = HelixWatch3DViewModel.FrozenMaterial;
                    }
                    else
                    {
                        OnPropertySetFalse(meshGeom3d);
                    }

                    meshGeom3d.CullMode = SharpDX.Direct3D11.CullMode.Back;

                }
                else if (geom is DynamoPointGeometryModel3D || geom is DynamoLineGeometryModel3D)
                {
                    if ((bool)e.NewValue)
                    {
                        //because color4's are structs we must update the collection this way
                        //as they are not reference types
                        //TODO cache this and update after helix 2.11 is released
                        var frozenColorCollection = new Color4Collection(geom.Geometry.Colors.Select(col =>
                        {
                            col.Alpha = HelixWatch3DViewModel.FrozenMaterial.DiffuseColor.Alpha;
                            return col;
                        })
                            );
                        geom.Geometry.Colors = frozenColorCollection;
                    }
                    else
                    {
                        OnPropertySetFalse(geom);
                    }
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
            //don't isolate the grid, axes, etc
            if (obj is GeometryModel3D && !(obj is BillboardTextModel3D) && !IsSpecialRenderPackage(obj) )
            {
                var geom = (GeometryModel3D)obj;
                var meshGeom3d = geom as DynamoGeometryModel3D;
                if (meshGeom3d != null)
                {
                    if ((bool)e.NewValue)
                    {
                        meshGeom3d.Material = HelixWatch3DViewModel.IsolatedMaterial;
                    }
                    else
                    {
                        OnPropertySetFalse(meshGeom3d);
                    }

                    meshGeom3d.RequiresPerVertexColoration = true;
                }
                else if (geom is DynamoPointGeometryModel3D || geom is DynamoLineGeometryModel3D)
                {
                    if ((bool)e.NewValue)
                    {
                        var isolatedColorCollection = new Color4Collection(geom.Geometry.Colors.Select(col =>
                        {
                            col.Alpha = HelixWatch3DViewModel.IsolatedMaterial.DiffuseColor.Alpha;
                            return col;
                        })
                            );
                        geom.Geometry.Colors = isolatedColorCollection;
                    }
                    else
                    {
                        OnPropertySetFalse(geom);
                    }
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
