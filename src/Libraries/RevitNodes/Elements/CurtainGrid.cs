using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using DSNodeServices;
using Revit.GeometryReferences;
using RevitServices.Persistence;
using RevitServices.Transactions;
 

namespace Revit.Elements
{
   /// <summary>
   /// A Revit CurtainGrid
   /// </summary>
   internal class CurtainGrid : Element
   {
      #region Properties

      /// <summary>
      /// Internal variable containing the wrapped Revit object
      /// </summary>
      internal Autodesk.Revit.DB.Element InternalCurtainHolderElement { get; private set; }

      internal Autodesk.Revit.DB.Reference InternalGridReference { get; private set; }

      /// <summary>
      /// Reference to the Element
      /// </summary>
      public override Autodesk.Revit.DB.Element InternalElement
      {
         get { return InternalCurtainHolderElement; }
      }

      internal Autodesk.Revit.DB.CurtainGrid InternalCurtainGrid
      {
         get
         {
            CurtainGridSet gridSets = null;
            if (InternalCurtainHolderElement is CurtainSystem)
            {
               var gridAsCurtainSystem = InternalCurtainHolderElement as CurtainSystem;
               gridSets = gridAsCurtainSystem.CurtainGrids;
            }
            else if (InternalCurtainHolderElement is ExtrusionRoof)
            {
               var gridAsExtrusionRoof = InternalCurtainHolderElement as ExtrusionRoof;
               gridSets = gridAsExtrusionRoof.CurtainGrids;
            }
            else if (InternalCurtainHolderElement is FootPrintRoof)
            {
               var gridAsFootPrintRoof = InternalCurtainHolderElement as FootPrintRoof;
               gridSets = gridAsFootPrintRoof.CurtainGrids;
            }
            else if (InternalCurtainHolderElement is Autodesk.Revit.DB.Wall)
            {
               var gridAsWall = InternalCurtainHolderElement as Autodesk.Revit.DB.Wall;
               return gridAsWall.CurtainGrid;
            }

            if (gridSets == null || gridSets.Size < 1)
            {
               throw new Exception("Element has no Curtain Grids");
            }

            if (InternalGridReference != null)
            {
               //search for proper grid here
               var faceObject =
                  InternalCurtainHolderElement.GetGeometryObjectFromReference(InternalGridReference)  as Autodesk.Revit.DB.Face;
               if (faceObject == null)
               {
                  var gridEnumFirst = gridSets.GetEnumerator();
                  gridEnumFirst.MoveNext();
                  return (Autodesk.Revit.DB.CurtainGrid)gridEnumFirst.Current;
               }

               var gridEnum = gridSets.GetEnumerator();
               double averageDiv = -1.0;
               Autodesk.Revit.DB.CurtainGrid bestFitGrid = null;

               for (; gridEnum.MoveNext();)
               {
                  var curCurtainGrid = (Autodesk.Revit.DB.CurtainGrid)gridEnum.Current;
                  //get panel
                  if (bestFitGrid == null)
                     bestFitGrid = curCurtainGrid;
                  var mullionIds = InternalCurtainGrid.GetMullionIds();
                  var mullionEnum = mullionIds.GetEnumerator();
                  int numMullion = 0;
                  double distVal = 0.0;
                  for (; mullionEnum.MoveNext();)
                  {
                     ElementId idMullion = mullionEnum.Current;
                     var mullion = DocumentManager.Instance.CurrentDBDocument.GetElement(idMullion) as Autodesk.Revit.DB.Mullion;
                     if (mullion == null)
                        continue;
                     var curveMullion = mullion.LocationCurve;
                     var midPoint = curveMullion.Evaluate(0.5, true);
                     numMullion += 1;
                     var result = faceObject.Project(midPoint);
                     if (result == null)
                     {
                        numMullion = 0;
                        break;
                     }
                     distVal += result.XYZPoint.DistanceTo(midPoint);
                  }
                  if ( numMullion == 0)
                     continue;
                  distVal = distVal/(double) numMullion;
                  if (averageDiv < 0.0 || distVal < averageDiv)
                  {
                     averageDiv = distVal;
                     bestFitGrid = curCurtainGrid;
                  }
               }
               return bestFitGrid;
               //back up: return first

               //var gridEnum = gridSets.GetEnumerator();
               //gridEnum.MoveNext();
               //return (Autodesk.Revit.DB.CurtainGrid) gridEnum.Current;
            }
            else
            {
               var gridEnum = gridSets.GetEnumerator();
               gridEnum.MoveNext();
               return (Autodesk.Revit.DB.CurtainGrid) gridEnum.Current;
            }
         }
      }

      /// <summary>
      /// number of U lines
      /// </summary>
      public int NumberOfULines 
      {
         get
         {
            return InternalCurtainGrid  == null ? 0  : InternalCurtainGrid.NumULines;
         }
      }

      /// <summary>
      /// number of V lines
      /// </summary>
      public int NumberOfVLines 
      {
         get
         {
            return InternalCurtainGrid  == null ? 0  : InternalCurtainGrid.NumVLines;
         }
      }
      /// <summary>
      /// number of V lines
      /// </summary>
      public int NumberOfPanels
      {
         get
         {
            return InternalCurtainGrid == null ? 0 : InternalCurtainGrid.NumPanels;
         }
      }

      /// <summary>
      /// number of V lines
      /// </summary>
      public int NumberOfMullions
      {
         get
         {
            return InternalCurtainGrid == null ? 0 : InternalCurtainGrid.GetMullionIds().Count;
         }
      }
 

      #endregion

      #region Private constructors
      /// <summary>
      /// Create from an existing Revit Element
      /// </summary>
      /// <param name="curtainHolderElement"></param>
      private CurtainGrid(Autodesk.Revit.DB.Element curtainHolderElement)
      {
          InternalCurtainHolderElement = curtainHolderElement;
          InternalGridReference = null; //compute from faceReference  
      }

      private CurtainGrid(Autodesk.Revit.DB.Element curtainHolderElement, Autodesk.Revit.DB.Reference faceReference)
      {
         InternalCurtainHolderElement = curtainHolderElement;
         if (faceReference == null)
            InternalGridReference = null;
         else
         {

            var faceObject =
               InternalCurtainHolderElement.GetGeometryObjectFromReference(InternalGridReference);
            if (!(faceObject is Autodesk.Revit.DB.Face))
               throw new Exception("Reference should be to Face of the Element.");

            InternalGridReference = faceReference; //compute from faceReference  
         }
      }

      #endregion

      #region Public static constructors

      /// <summary>
      ///get curtain grid from element with curtain grid or grids
      /// </summary>
      /// <param name="curtainHolderElement"></param>
      /// <param name="elementFaceReference"></param>

      static public CurtainGrid ByElementAndReference(Element curtainHolderElement, ElementFaceReference elementFaceReference)
      {
         return new CurtainGrid(curtainHolderElement.InternalElement, elementFaceReference.InternalReference);
      }

      /// <summary>
      ///get curtain grid from element with curtain grid or grids
      /// </summary>
      /// <param name="curtainHolderElement"></param>

      static public CurtainGrid ByElement( Element curtainHolderElement)
      {
         return new CurtainGrid(curtainHolderElement.InternalElement);
      }
      

      #endregion

      #region public methods

      public List<Element> GetPanels()
      {
         var panelIds = InternalCurtainGrid.GetPanelIds();
         var panelEnum = panelIds.GetEnumerator();
         var panels = new List<Element>();         

         for (; panelEnum.MoveNext();)
         {
            ElementId idPanel = panelEnum.Current;
            var panel = DocumentManager.Instance.CurrentDBDocument.GetElement(idPanel);
            if (!(panel is Panel))
               continue;
            panels.Add(CurtainPanel.FromExisting(panel as Panel, true));
         }
         return panels;
      }

      public List<Element> GetMullions()
      {
         var mullionIds = InternalCurtainGrid.GetMullionIds();
         var mullionEnum = mullionIds.GetEnumerator();
         var mullions = new List<Element>();

         for (; mullionEnum.MoveNext(); )
         {
            ElementId idMullion = mullionEnum.Current;
            var mullion = DocumentManager.Instance.CurrentDBDocument.GetElement(idMullion);
            mullions.Add(Mullion.FromExisting(mullion as Autodesk.Revit.DB.Mullion, true));
         }
         return mullions;
      }

      public override string ToString()
      {
         return "Curtain Grid";
      }

      internal static CurtainGridSet AllCurtainGrids(Autodesk.Revit.DB.Element revitElement)
      {
         CurtainGridSet gridSets = null;
         if (revitElement is CurtainSystem)
         {
            var gridAsCurtainSystem = revitElement as CurtainSystem;
            gridSets = gridAsCurtainSystem.CurtainGrids;
         }
         else if (revitElement is ExtrusionRoof)
         {
            var gridAsExtrusionRoof = revitElement as ExtrusionRoof;
            gridSets = gridAsExtrusionRoof.CurtainGrids;
         }
         else if (revitElement is FootPrintRoof)
         {
            var gridAsFootPrintRoof = revitElement as FootPrintRoof;
            gridSets = gridAsFootPrintRoof.CurtainGrids;
         }
         else if (revitElement is Autodesk.Revit.DB.Wall)
         {
            var gridAsWall = revitElement as Autodesk.Revit.DB.Wall;
            gridSets = new CurtainGridSet();
            gridSets.Insert(gridAsWall.CurtainGrid);
         }

         if (gridSets == null || gridSets.Size < 1)
         {
            throw new Exception("Element has no Curtain Grids");
         }
         return gridSets;
      }

      #endregion
   }
}
