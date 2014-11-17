using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
   /// A Revit CurtainPanel
   /// </summary>
   public class CurtainPanel : AbstractFamilyInstance
   {
      #region Properties

      protected CurveArrArray PanelBoundaries
      {
         get
         {
            // This creates a new wall and deletes the old one
            TransactionManager.Instance.EnsureInTransaction(Document);

            var elementAsPanel = InternalElement as Autodesk.Revit.DB.Panel;
            if (elementAsPanel == null)
               throw new Exception("InternalElement of Curtain Panel is not Panel");

            var host = elementAsPanel.Host;

            CurtainGrid hostingGrid = null;
            Autodesk.Revit.DB.CurtainGrid grid = null;
            if (host is Autodesk.Revit.DB.Wall)
            {
               hostingGrid = CurtainGrid.ByElement(UnknownElement.FromExisting(host));
            }
            else
            {
               var gridSet = CurtainGrid.AllCurtainGrids(host);
               var enumGrid = gridSet.GetEnumerator();
               bool found = false;
               for (; enumGrid.MoveNext();)
               {
                  grid = (Autodesk.Revit.DB.CurtainGrid)enumGrid.Current;
                  if (grid.GetPanelIds().Contains(elementAsPanel.Id))
                  {
                     found = true;
                     break;
                  }
               }
               if (!found)
                  throw new Exception("Could not find cell for panel");
            }

            ElementId uGridId = ElementId.InvalidElementId;
            ElementId vGridId = ElementId.InvalidElementId;
            elementAsPanel.GetRefGridLines(ref uGridId, ref vGridId);

            if (grid == null && hostingGrid == null)
               throw new Exception("Could not find cell for panel");

            CurtainCell cell = hostingGrid != null
               ? hostingGrid.InternalCurtainGrid.GetCell(uGridId, vGridId)
               : grid.GetCell(uGridId, vGridId);

            TransactionManager.Instance.TransactionTaskDone();

            if (cell == null)
               throw new Exception("Could not find cell for panel");
            return cell.CurveLoops;
         }
      }

      private PolyCurve[] boundsCache = null;

      public PolyCurve[] Boundaries
      {
         get
         {
            if (boundsCache != null)
               return boundsCache;
            var enumCurveLoops = PanelBoundaries.GetEnumerator();
            var bounds = new List<PolyCurve>();

            for (; enumCurveLoops.MoveNext();)
            {
               var crvs = new CurveArray();
               var crvArr = (CurveArray) enumCurveLoops.Current;
               var enumCurves = crvArr.GetEnumerator();
               for (; enumCurves.MoveNext();)
               {
                  var crv = (Autodesk.Revit.DB.Curve) enumCurves.Current;
                  crvs.Append(crv);
               }
               //try
               //{
                  bounds.Add(Revit.GeometryConversion.RevitToProtoCurve.ToProtoType(crvs));
               //}
               /* debugging code
               catch (Exception e)
               {
                  var cl = new CurveLoop();
                  var enumCurvesCl = crvArr.GetEnumerator();
                  for (; enumCurvesCl.MoveNext(); )
                  {
                     var crv = (Autodesk.Revit.DB.Curve)enumCurvesCl.Current;
                     cl.Append(crv);
                  }
                  double len = cl.GetExactLength();
                  len = len/1.0;
               }
               end of debugging code */
            }
            boundsCache = bounds.ToArray();
            return boundsCache;
         }
      }

      public bool HasPlane
      {
         get
         {
            var enumCurveLoops = PanelBoundaries.GetEnumerator();
            Autodesk.Revit.DB.Plane plane = null;
            for (; enumCurveLoops.MoveNext();)
            {
               var cLoop = new CurveLoop();
               var crvArr = (CurveArray) enumCurveLoops.Current;
               var enumCurves = crvArr.GetEnumerator();
               for (; enumCurves.MoveNext();)
               {
                  var crv = (Autodesk.Revit.DB.Curve) enumCurves.Current;
                  cLoop.Append(crv);
               }
               if (!cLoop.HasPlane())
                  return false;
               var thisPlane = cLoop.GetPlane();
               if (plane == null)
                  plane = thisPlane;
               else if (Math.Abs(plane.Normal.DotProduct(thisPlane.Normal)) < 1.0 - 1.0e-9)
                  return false;
               else
               {
                  if (Math.Abs((plane.Origin - thisPlane.Origin).DotProduct(plane.Normal)) > 1.0e-9)
                     return false;
               }
            }
            return true;
         }
      }

      public Plane PanelPlane
      {
         get
         {
            var enumCurveLoops = PanelBoundaries.GetEnumerator();
            Autodesk.Revit.DB.Plane plane = null;
            for (; enumCurveLoops.MoveNext();)
            {
               var cLoop = new CurveLoop();
               var crvArr = (CurveArray) enumCurveLoops.Current;
               var enumCurves = crvArr.GetEnumerator();
               for (; enumCurves.MoveNext();)
               {
                  var crv = (Autodesk.Revit.DB.Curve) enumCurves.Current;
                  cLoop.Append(crv);
               }
               if (!cLoop.HasPlane())
                  throw new Exception(" Curtain Panel is not planar");
               var thisPlane = cLoop.GetPlane();
               if (plane == null)
                  plane = thisPlane;
               else if (Math.Abs(plane.Normal.DotProduct(thisPlane.Normal)) < 1.0 - 1.0e-9)
                  throw new Exception(" Curtain Panel is not planar");
               else
               {
                  if (Math.Abs((plane.Origin - thisPlane.Origin).DotProduct(plane.Normal)) > 1.0e-9)
                     throw new Exception(" Curtain Panel is not planar");
               }
            }
            if (plane == null)
               throw new Exception(" Curtain Panel is not planar");

             return plane.ToPlane();
         }
      }

      public double Length
      {
         get
         {
            double lengthVal = 0.0;
            var enumCurveLoops = PanelBoundaries.GetEnumerator();
            for (; enumCurveLoops.MoveNext();)
            {
               var crvArr = (CurveArray) enumCurveLoops.Current;
               var enumCurves = crvArr.GetEnumerator();
               for (; enumCurves.MoveNext();)
               {
                  var crv = (Autodesk.Revit.DB.Curve) enumCurves.Current;
                  lengthVal += crv.Length;
               }
            }
             return lengthVal*UnitConverter.HostToDynamoFactor;
         }
      }

      public bool IsRectangular
      {
         get
         {
            var enumCurveLoops = PanelBoundaries.GetEnumerator();
            int num = 0;
            bool result = false;
            for (; enumCurveLoops.MoveNext();)
            {
               if (num > 0)
                  return false;
               num++;
               var cLoop = new CurveLoop();
               var crvArr = (CurveArray) enumCurveLoops.Current;
               var enumCurves = crvArr.GetEnumerator();
               for (; enumCurves.MoveNext();)
               {
                  var crv = (Autodesk.Revit.DB.Curve) enumCurves.Current;
                  cLoop.Append(crv);
               }
               if (!cLoop.HasPlane())
                  return false;
               result = cLoop.IsRectangular(cLoop.GetPlane());
            }
            return result;
         }
      }

      public double Width
      {
         get
         {
            var enumCurveLoops = PanelBoundaries.GetEnumerator();
            int num = 0;
            double result = 0.0;
            for (; enumCurveLoops.MoveNext();)
            {
               if (num > 0)
                  throw new Exception(" Curtain Panel is not rectangular");
               num++;
               var cLoop = new CurveLoop();
               var crvArr = (CurveArray) enumCurveLoops.Current;
               var enumCurves = crvArr.GetEnumerator();
               for (; enumCurves.MoveNext();)
               {
                  var crv = (Autodesk.Revit.DB.Curve) enumCurves.Current;
                  cLoop.Append(crv);
               }
               if (!cLoop.HasPlane())
                  throw new Exception(" Curtain Panel is not rectangular");
               if (!cLoop.IsRectangular(cLoop.GetPlane()))
                  throw new Exception(" Curtain Panel is not rectangular");
               result = cLoop.GetRectangularWidth(cLoop.GetPlane());
            }
             return result*UnitConverter.HostToDynamoFactor;
         }
      }

      public double Height
      {
         get
         {
            var enumCurveLoops = PanelBoundaries.GetEnumerator();
            int num = 0;
            double result = 0.0;
            for (; enumCurveLoops.MoveNext();)
            {
               if (num > 0)
                  throw new Exception(" Curtain Panel is not rectangular");
               num++;
               var cLoop = new CurveLoop();
               var crvArr = (CurveArray) enumCurveLoops.Current;
               var enumCurves = crvArr.GetEnumerator();
               for (; enumCurves.MoveNext();)
               {
                  var crv = (Autodesk.Revit.DB.Curve) enumCurves.Current;
                  cLoop.Append(crv);
               }
               if (!cLoop.HasPlane())
                  throw new Exception(" Curtain Panel is not rectangular");
               if (!cLoop.IsRectangular(cLoop.GetPlane()))
                  throw new Exception(" Curtain Panel is not rectangular");
               result = cLoop.GetRectangularHeight(cLoop.GetPlane());
            }
            return result * UnitConverter.HostToDynamoFactor;
         }
      }

      #endregion

      #region Private constructors

      /// <summary>
      /// Create from an existing Revit Element
      /// </summary>
      /// <param name="panelElement"></param>
      protected CurtainPanel(Autodesk.Revit.DB.Panel panelElement)
      {
         InternalSetFamilyInstance(panelElement);
         boundsCache = null;
      }

      #endregion

      #region Static constructors

      /// <summary>
      ///get curtain panel from element  
      /// </summary>
      /// <param name="panelElement"></param>

      internal static CurtainPanel ByElement(CurtainPanel panelElement)
      {
         var elementAsPanel = panelElement.InternalElement as Autodesk.Revit.DB.Panel;
         if (elementAsPanel == null)
            throw new Exception("Curtain Panel should represent Revit panel");
         return new CurtainPanel(elementAsPanel);
      }

      /// <summary>
      ///get all panels of curtain wall, system or slope galzing roof
      /// </summary>
      /// <param name="hostingElement"></param>
      public static CurtainPanel[] ByElement(Element hostingElement)
      {
         CurtainGridSet thisSet = CurtainGrid.AllCurtainGrids(hostingElement.InternalElement);
         var result = new List<CurtainPanel>();

         var enumGrid = thisSet.GetEnumerator();
         for (; enumGrid.MoveNext(); )
         {
            var grid = (Autodesk.Revit.DB.CurtainGrid)enumGrid.Current;
            var ids = grid.GetPanelIds();
            var idEnum = ids.GetEnumerator();
            for (; idEnum.MoveNext(); )
            {
               var idPanel = idEnum.Current;
               var panel = DocumentManager.Instance.CurrentDBDocument.GetElement(idPanel);
               result.Add(CurtainPanel.FromExisting(panel as Autodesk.Revit.DB.Panel, true));
            }
         }
         return result.ToArray();
      }

      /// <summary>
      /// Construct this type from an existing Revit element.
      /// </summary>
      /// <param name="panel"></param>
      /// <param name="isRevitOwned"></param>
      /// <returns></returns>
      internal static CurtainPanel FromExisting(Autodesk.Revit.DB.Panel panel, bool isRevitOwned)
      {
         if (panel == null)
         {
            throw new ArgumentNullException("panel");
         }
         
         return new CurtainPanel(panel)
         {
            IsRevitOwned = true //making panels in Dynamo is not implemented
         };
      }

      #endregion

      #region public methods

      public Mullion[] SupportingMullions()
      {
         var elementAsPanel = InternalElement as Autodesk.Revit.DB.Panel;
         if (elementAsPanel == null)
            throw new Exception("Curtain Panel should represent Revit panel");
         var bounds = this.Boundaries;

         var host = elementAsPanel.Host;

         //var hostingGrid = CurtainGrid.ByElement(UnknownElement.FromExisting(host));

         var mullions = Mullion.ByElement(UnknownElement.FromExisting(host));//hostingGrid.GetMullions();
         int numberMullions = mullions.Length;
         var result = new List<Mullion>();

         for (int index = 0; index < numberMullions; index++)
         {
            var mullionAtIndex = mullions[index] as Mullion;
            if (mullionAtIndex == null)
               continue;

            var thisCurve = mullionAtIndex.LocationCurve;

            var enumBounds = bounds.GetEnumerator();
            bool neighbor = false;
            for (; enumBounds.MoveNext() && !neighbor; )
            {
               var polycrv = enumBounds.Current as PolyCurve;
               if (polycrv == null)
                  continue;
               var bndCrvs = polycrv.Curves();
               var enumCrv = bndCrvs.GetEnumerator();
               for (; enumCrv.MoveNext(); )
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
               result.Add(mullionAtIndex);
         }
         return result.ToArray();
      }

      public FamilyInstance AsFamilyInstance()
      {
         return FamilyInstance.FromExisting(InternalElement as Autodesk.Revit.DB.FamilyInstance, true);
      }

      public override string ToString()
      {
         return "Curtain Panel";
      }

      #endregion

   }
}