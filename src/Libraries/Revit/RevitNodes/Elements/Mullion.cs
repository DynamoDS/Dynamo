using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using DSNodeServices;
using Revit.GeometryConversion;
using Revit.GeometryReferences;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Face = Autodesk.DesignScript.Geometry.Face;
using Plane = Autodesk.DesignScript.Geometry.Plane;

namespace Revit.Elements
{
   /// <summary>
   /// A Revit Mullion
   /// </summary>
   public class Mullion : AbstractFamilyInstance
   {
      #region Properties

      public Autodesk.DesignScript.Geometry.Curve LocationCurve
      {
         get
         {
            var elementAsMullion = InternalElement as Autodesk.Revit.DB.Mullion;
            if (elementAsMullion == null)
               throw new Exception("Mullion should represent Revit's Mullion");
            var crv  = elementAsMullion.LocationCurve;
            return Revit.GeometryConversion.RevitToProtoCurve.ToProtoType(crv);
         }
      }
      #endregion

      #region Private constructors

      /// <summary>
      /// Create from an existing Revit Element
      /// </summary>
      /// <param name="mullionElement"></param>
      protected Mullion(Autodesk.Revit.DB.Mullion mullionElement)
      {
         InternalSetFamilyInstance(mullionElement);
      }
      #endregion

      #region Static constructors
      /// <summary>
      ///get curtain panel from element  
      /// </summary>
      /// <param name="mullionElement"></param>

      internal static Mullion ByElement(Mullion mullionElement)
      {
         var elementAsMullion = mullionElement.InternalElement as Autodesk.Revit.DB.Mullion;
         if (elementAsMullion == null)
            throw new Exception("Mullion should represent Revit's Mullion");
         return new Mullion(elementAsMullion);
      }

      /// <summary>
      ///get all mullions of curtain wall, system or slope galzing roof
      /// </summary>
      /// <param name="hostingElement"></param>
      public static Mullion[] ByElement(Element hostingElement)
      {
         CurtainGridSet thisSet = CurtainGrid.AllCurtainGrids(hostingElement.InternalElement);
         var result = new List<Mullion>();

         var enumGrid = thisSet.GetEnumerator();
         for (; enumGrid.MoveNext();)
         {
            var grid = (Autodesk.Revit.DB.CurtainGrid) enumGrid.Current;
            var ids = grid.GetMullionIds();
            var idEnum = ids.GetEnumerator();
            for (; idEnum.MoveNext();)
            {
               var idMullion = idEnum.Current;
               var mullion = DocumentManager.Instance.CurrentDBDocument.GetElement(idMullion);
               result.Add(Mullion.FromExisting(mullion as Autodesk.Revit.DB.Mullion, true));
            }
         }
         return result.ToArray();
      }

      /// <summary>
      /// Construct this type from an existing Revit element.
      /// </summary>
      /// <param name="mullion"></param>
      /// <param name="isRevitOwned"></param>
      /// <returns></returns>
      internal static Mullion FromExisting(Autodesk.Revit.DB.Mullion mullion, bool isRevitOwned)
      {
         if (mullion == null)
         {
            throw new ArgumentNullException("mullion");
         }

         return new Mullion(mullion)
         {
            IsRevitOwned = true //making panels in Dynamo is not implemented
         };
      }
      #endregion

      #region public methods

      public CurtainPanel[] SupportedPanels()
      {
         var elementAsMullion = InternalElement as Autodesk.Revit.DB.Mullion;
         if (elementAsMullion == null)
            throw new Exception("Mullion should represent Revit's Mullion");

         var host = elementAsMullion.Host;

         //var hostingGrid = Panel.ByElement(UnknownElement.FromExisting(host));

         var panels = CurtainPanel.ByElement(UnknownElement.FromExisting(host));

         var result = new List<CurtainPanel>();

         var thisCurve = this.LocationCurve;

         int numberPanels = panels.Length;
         for (int index = 0; index < numberPanels; index++)
         {
            var panelAtIndex = panels[index] as CurtainPanel;
            if (panelAtIndex == null)
               continue;
            var bounds = panelAtIndex.Boundaries;
            var enumBounds = bounds.GetEnumerator();
            bool neighbor = false;
            for (; enumBounds.MoveNext() && !neighbor; )
            {
               var polycrv = enumBounds.Current as PolyCurve;
               if (polycrv == null)
                  continue;
               var bndCrvs = polycrv.Curves();
               var enumCrv = bndCrvs.GetEnumerator();
               for (; enumCrv.MoveNext();)
               {
                  var crv = enumCrv.Current as Autodesk.DesignScript.Geometry.Curve;
                  if (crv == null)
                     continue;
                  var midPoint = crv.PointAtParameter(0.5);
                  if (midPoint.DistanceTo(thisCurve) < 1.0e-7)
                  {
                     neighbor = true;
                     break;
                  }
               }
            }
            if (neighbor)
               result.Add(panelAtIndex);
         }
         return result.ToArray();
      }

      public FamilyInstance AsFamilyInstance()
      {
         return FamilyInstance.FromExisting(InternalElement as Autodesk.Revit.DB.FamilyInstance, true);
      }

      public override string ToString()
      {
         return "Mullion";
      }

      #endregion
   }
}
