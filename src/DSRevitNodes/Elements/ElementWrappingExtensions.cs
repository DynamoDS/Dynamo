using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace DSRevitNodes.Elements
{
    [Browsable(false)]
    public static class ElementWrappingExtensions
    {
        /// <summary>
        /// If possible, wrap the element in a DS type
        /// </summary>
        /// <param name="ele"></param>
        /// <param name="isRevitOwned">Whether the returned object should be revit owned or not</param>
        /// <returns></returns>
        public static AbstractElement ToDSType(this Autodesk.Revit.DB.Element ele, bool isRevitOwned)
        {

            // cast to dynamic to dispatch to the appropriate wrapping method
            dynamic dynamicElement = ele;
            return ElementWrappingExtensions.Wrap(dynamicElement, isRevitOwned);

        }

        #region Wrap methods

        public static DSElement Wrap(Autodesk.Revit.DB.Element element, bool isRevitOwned)
        {
            return DSElement.FromExisting(element);
        }

        public static AbstractFamilyInstance Wrap(Autodesk.Revit.DB.FamilyInstance ele, bool isRevitOwned)
        {
            if (AdaptiveComponentInstanceUtils.HasAdaptiveFamilySymbol(ele))
            {
                return DSAdaptiveComponent.FromExisting(ele, isRevitOwned);
            }

            if (ele.StructuralType != StructuralType.NonStructural)
            {
                return DSStructuralFraming.FromExisting(ele, isRevitOwned);
            }

            return DSFamilyInstance.FromExisting(ele, isRevitOwned);
        }

        public static DSDividedPath Wrap(Autodesk.Revit.DB.DividedPath ele, bool isRevitOwned)
        {
            return DSDividedPath.FromExisting(ele, isRevitOwned);
        }

        public static DSDividedSurface Wrap(Autodesk.Revit.DB.DividedSurface ele, bool isRevitOwned)
        {
            return DSDividedSurface.FromExisting(ele, isRevitOwned);
        }

        public static DSFamily Wrap(Autodesk.Revit.DB.Family ele, bool isRevitOwned)
        {
            return DSFamily.FromExisting(ele, isRevitOwned);
        }

        public static DSFamilySymbol Wrap(Autodesk.Revit.DB.FamilySymbol ele, bool isRevitOwned)
        {
            return DSFamilySymbol.FromExisting(ele, isRevitOwned);
        }

        public static DSFloor Wrap(Autodesk.Revit.DB.Floor ele, bool isRevitOwned)
        {
            return DSFloor.FromExisting(ele, isRevitOwned);
        }

        public static DSFloorType Wrap(Autodesk.Revit.DB.FloorType ele, bool isRevitOwned)
        {
            return DSFloorType.FromExisting(ele, isRevitOwned);
        }

        public static DSForm Wrap(Autodesk.Revit.DB.Form ele, bool isRevitOwned)
        {
            return DSForm.FromExisting(ele, isRevitOwned);
        }

        public static DSFreeForm Wrap(Autodesk.Revit.DB.FreeFormElement ele, bool isRevitOwned)
        {
            return DSFreeForm.FromExisting(ele, isRevitOwned);
        }

        public static DSGrid Wrap(Autodesk.Revit.DB.Grid ele, bool isRevitOwned)
        {
            return DSGrid.FromExisting(ele, isRevitOwned);
        }

        public static DSLevel Wrap(Autodesk.Revit.DB.Level ele, bool isRevitOwned)
        {
            return DSLevel.FromExisting(ele, isRevitOwned);
        }

        public static DSModelCurve Wrap(Autodesk.Revit.DB.ModelCurve ele, bool isRevitOwned)
        {
            return DSModelCurve.FromExisting(ele, isRevitOwned);
        }

        public static DSModelText Wrap(Autodesk.Revit.DB.ModelText ele, bool isRevitOwned)
        {
            return DSModelText.FromExisting(ele, isRevitOwned);
        }

        public static DSModelTextType Wrap(Autodesk.Revit.DB.ModelTextType ele, bool isRevitOwned)
        {
            return DSModelTextType.FromExisting(ele, isRevitOwned);
        }

        public static DSReferencePlane Wrap(Autodesk.Revit.DB.ReferencePlane ele, bool isRevitOwned)
        {
            return DSReferencePlane.FromExisting(ele, isRevitOwned);
        }

        public static DSReferencePoint Wrap(Autodesk.Revit.DB.ReferencePoint ele, bool isRevitOwned)
        {
            return DSReferencePoint.FromExisting(ele, isRevitOwned);
        }

        public static DSSketchPlane Wrap(Autodesk.Revit.DB.SketchPlane ele, bool isRevitOwned)
        {
            return DSSketchPlane.FromExisting(ele, isRevitOwned);
        }

        public static DSWall Wrap(Autodesk.Revit.DB.Wall ele, bool isRevitOwned)
        {
            return DSWall.FromExisting(ele, isRevitOwned);
        }

        public static DSWallType Wrap(Autodesk.Revit.DB.WallType ele, bool isRevitOwned)
        {
            return DSWallType.FromExisting(ele, isRevitOwned);
        }

        public static AbstractView3D Wrap(Autodesk.Revit.DB.View3D view, bool isRevitOwned)
        {
            if (view.IsPerspective)
            {
                return DSPerspectiveView.FromExisting(view, isRevitOwned);
            }
            else
            {
                return DSAxonometricView.FromExisting(view, isRevitOwned);
            }
        }

        public static AbstractElement Wrap(Autodesk.Revit.DB.ViewPlan view, bool isRevitOwned)
        {
            if (view.ViewType == ViewType.CeilingPlan)
            {
                return DSCeilingPlanView.FromExisting(view, isRevitOwned);
            }
            else if (view.ViewType == ViewType.FloorPlan)
            {
                return DSFloorPlanView.FromExisting(view, isRevitOwned);
            }
            else
            {
                // unknown type of plan view, just wrap as unknown
                return DSElement.FromExisting(view);
            }
        }

        public static DSSectionView Wrap(Autodesk.Revit.DB.ViewSection view, bool isRevitOwned)
        {
            return DSSectionView.FromExisting(view, isRevitOwned);
        }

        public static DSSheet Wrap(Autodesk.Revit.DB.ViewSheet view, bool isRevitOwned)
        {
            return DSSheet.FromExisting(view, isRevitOwned);
        }

        public static DSDraftingView Wrap(Autodesk.Revit.DB.ViewDrafting view, bool isRevitOwned)
        {
            return DSDraftingView.FromExisting(view, isRevitOwned);
        }

        #endregion

    }

}
