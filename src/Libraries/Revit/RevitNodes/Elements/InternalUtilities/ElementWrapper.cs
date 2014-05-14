using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Revit.Elements.Views;
using View3D = Revit.Elements.Views.View3D;

namespace Revit.Elements
{
    /// <summary>
    /// Element wrapper supplies tools for wrapping Autodesk.Revit.DB.Element types
    /// in their associated Revit.Elements.Element wrapper
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public static class ElementWrapper
    {
        /// <summary>
        /// If possible, wrap the element in a DS type
        /// </summary>
        /// <param name="ele"></param>
        /// <param name="isRevitOwned">Whether the returned object should be revit owned or not</param>
        /// <returns></returns>
        public static Element ToDSType(this Autodesk.Revit.DB.Element ele, bool isRevitOwned)
        {
            // cast to dynamic to dispatch to the appropriate wrapping method
            dynamic dynamicElement = ele;
            return ElementWrapper.Wrap(dynamicElement, isRevitOwned);

        }

        #region Wrap methods

        public static UnknownElement Wrap(Autodesk.Revit.DB.Element element, bool isRevitOwned)
        {
            return UnknownElement.FromExisting(element);
        }

        public static AbstractFamilyInstance Wrap(Autodesk.Revit.DB.FamilyInstance ele, bool isRevitOwned)
        {
            if (AdaptiveComponentInstanceUtils.HasAdaptiveFamilySymbol(ele))
            {
                return AdaptiveComponent.FromExisting(ele, isRevitOwned);
            }

            if (ele.StructuralType != Autodesk.Revit.DB.Structure.StructuralType.NonStructural)
            {
                return StructuralFraming.FromExisting(ele, isRevitOwned);
            }

            return FamilyInstance.FromExisting(ele, isRevitOwned);
        }

        public static DividedPath Wrap(Autodesk.Revit.DB.DividedPath ele, bool isRevitOwned)
        {
            return DividedPath.FromExisting(ele, isRevitOwned);
        }

        public static DividedSurface Wrap(Autodesk.Revit.DB.DividedSurface ele, bool isRevitOwned)
        {
            return DividedSurface.FromExisting(ele, isRevitOwned);
        }

        public static Family Wrap(Autodesk.Revit.DB.Family ele, bool isRevitOwned)
        {
            return Family.FromExisting(ele, isRevitOwned);
        }

        public static FamilySymbol Wrap(Autodesk.Revit.DB.FamilySymbol ele, bool isRevitOwned)
        {
            return FamilySymbol.FromExisting(ele, isRevitOwned);
        }

        public static Floor Wrap(Autodesk.Revit.DB.Floor ele, bool isRevitOwned)
        {
            return Floor.FromExisting(ele, isRevitOwned);
        }

        public static FloorType Wrap(Autodesk.Revit.DB.FloorType ele, bool isRevitOwned)
        {
            return FloorType.FromExisting(ele, isRevitOwned);
        }

        public static Form Wrap(Autodesk.Revit.DB.Form ele, bool isRevitOwned)
        {
            return Form.FromExisting(ele, isRevitOwned);
        }

        [SupressImportIntoVM]
        public static FreeForm Wrap(Autodesk.Revit.DB.FreeFormElement ele, bool isRevitOwned)
        {
            return FreeForm.FromExisting(ele, isRevitOwned);
        }

        public static Grid Wrap(Autodesk.Revit.DB.Grid ele, bool isRevitOwned)
        {
            return Grid.FromExisting(ele, isRevitOwned);
        }

        public static Level Wrap(Autodesk.Revit.DB.Level ele, bool isRevitOwned)
        {
            return Level.FromExisting(ele, isRevitOwned);
        }

        public static ModelCurve Wrap(Autodesk.Revit.DB.ModelCurve ele, bool isRevitOwned)
        {
            return ModelCurve.FromExisting(ele, isRevitOwned);
        }

        public static CurveByPoints Wrap(Autodesk.Revit.DB.CurveByPoints ele, bool isRevitOwned)
        {
            return CurveByPoints.FromExisting(ele, isRevitOwned);
        }

        public static ModelText Wrap(Autodesk.Revit.DB.ModelText ele, bool isRevitOwned)
        {
            return ModelText.FromExisting(ele, isRevitOwned);
        }

        public static ModelTextType Wrap(Autodesk.Revit.DB.ModelTextType ele, bool isRevitOwned)
        {
            return ModelTextType.FromExisting(ele, isRevitOwned);
        }

        public static ReferencePlane Wrap(Autodesk.Revit.DB.ReferencePlane ele, bool isRevitOwned)
        {
            return ReferencePlane.FromExisting(ele, isRevitOwned);
        }

        public static ReferencePoint Wrap(Autodesk.Revit.DB.ReferencePoint ele, bool isRevitOwned)
        {
            return ReferencePoint.FromExisting(ele, isRevitOwned);
        }

        public static SketchPlane Wrap(Autodesk.Revit.DB.SketchPlane ele, bool isRevitOwned)
        {
            return SketchPlane.FromExisting(ele, isRevitOwned);
        }

        public static Wall Wrap(Autodesk.Revit.DB.Wall ele, bool isRevitOwned)
        {
            return Wall.FromExisting(ele, isRevitOwned);
        }

        public static WallType Wrap(Autodesk.Revit.DB.WallType ele, bool isRevitOwned)
        {
            return WallType.FromExisting(ele, isRevitOwned);
        }

        public static View3D Wrap(Autodesk.Revit.DB.View3D view, bool isRevitOwned)
        {
            if (view.IsPerspective)
            {
                return PerspectiveView.FromExisting(view, isRevitOwned);
            }
            else
            {
                return AxonometricView.FromExisting(view, isRevitOwned);
            }
        }

        public static Element Wrap(Autodesk.Revit.DB.ViewPlan view, bool isRevitOwned)
        {
            if (view.ViewType == ViewType.CeilingPlan)
            {
                return CeilingPlanView.FromExisting(view, isRevitOwned);
            }
            else if (view.ViewType == ViewType.FloorPlan)
            {
                return FloorPlanView.FromExisting(view, isRevitOwned);
            }
            else
            {
                // unknown type of plan view, just wrap as unknown
                return UnknownElement.FromExisting(view);
            }
        }

        public static SectionView Wrap(Autodesk.Revit.DB.ViewSection view, bool isRevitOwned)
        {
            return SectionView.FromExisting(view, isRevitOwned);
        }

        public static Sheet Wrap(Autodesk.Revit.DB.ViewSheet view, bool isRevitOwned)
        {
            return Sheet.FromExisting(view, isRevitOwned);
        }

        public static DraftingView Wrap(Autodesk.Revit.DB.ViewDrafting view, bool isRevitOwned)
        {
            return DraftingView.FromExisting(view, isRevitOwned);
        }

        [SupressImportIntoVM]
        public static Topography Wrap(Autodesk.Revit.DB.Architecture.TopographySurface topoSurface, bool isRevitOwned)
        {
            return Topography.FromExisting(topoSurface, isRevitOwned);
        }

        public static object Wrap(Autodesk.Revit.DB.Panel ele, bool isRevitOwned)
        {
            if (AdaptiveComponentInstanceUtils.IsAdaptiveFamilySymbol(ele.Symbol))
            {
                return AdaptiveComponent.FromExisting(ele, isRevitOwned);
            }

           return CurtainPanel.FromExisting(ele, isRevitOwned);
        }

        public static Mullion Wrap(Autodesk.Revit.DB.Mullion ele, bool isRevitOwned)
        {
           return Mullion.FromExisting(ele, isRevitOwned);
        }

        #endregion

    }

}
