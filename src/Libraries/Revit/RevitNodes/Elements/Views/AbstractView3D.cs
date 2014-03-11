using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Revit.Elements
{
    //[Browsable(false)]
    public abstract class AbstractView3D : AbstractView
    {

        #region Internal properties

        /// <summary>
        /// An internal handle on the Revit element
        /// </summary>
        internal View3D InternalView3D
        {
            get;
            private set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalView3D; }
        }

        #endregion

        #region Private helper methods

        /// <summary>
        /// Build Orientation3D object for eye point and a target point 
        /// </summary>
        /// <param name="eyePoint"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        protected static ViewOrientation3D BuildOrientation3D(XYZ eyePoint, XYZ target)
        {
            var globalUp = XYZ.BasisZ;
            var direction = target.Subtract(eyePoint);
            var up = direction.CrossProduct(globalUp).CrossProduct(direction);
            return new ViewOrientation3D(eyePoint, up, direction);
        }

        /// <summary>
        /// Obtain a sparse point collection outlining a Revit element bt traversing it's
        /// GeometryObject representation
        /// </summary>
        /// <param name="e"></param>
        /// <param name="pts"></param>
        protected static void GetPointCloud(Autodesk.Revit.DB.Element e, List<XYZ> pts)
        {
            var options = new Options()
            {
                ComputeReferences = true,
                DetailLevel = ViewDetailLevel.Coarse,
                IncludeNonVisibleObjects = false
            };

            foreach (var gObj in e.get_Geometry(options))
            {
                if (gObj is Autodesk.Revit.DB.Solid)
                {
                    GetPointCloud(gObj as Autodesk.Revit.DB.Solid, pts);
                }
                else if (gObj is GeometryInstance)
                {
                    GetPointCloud(gObj as GeometryInstance, pts);
                }
            }
        }

        /// <summary>
        /// Obtain a point collection outlining a GeometryObject
        /// </summary>
        /// <param name="geomInst"></param>
        /// <param name="pts"></param>
        protected static void GetPointCloud(GeometryInstance geomInst, List<XYZ> pts)
        {
            foreach (var gObj in geomInst.GetInstanceGeometry())
            {
                if (gObj is Autodesk.Revit.DB.Solid)
                {
                    GetPointCloud(gObj as Autodesk.Revit.DB.Solid, pts);
                }
                else if (gObj is GeometryInstance)
                {
                    GetPointCloud(gObj as GeometryInstance, pts);
                }
            }
        }

        /// <summary>
        /// Obtain a point collection outlining a Solid GeometryObject
        /// </summary>
        /// <param name="solid"></param>
        /// <param name="pts"></param>
        protected static void GetPointCloud(Autodesk.Revit.DB.Solid solid, List<XYZ> pts)
        {
            foreach (Edge gEdge in solid.Edges)
            {
                var c = gEdge.AsCurve();
                if (c is Line)
                {
                    pts.Add(c.Evaluate(0, true));
                    pts.Add(c.Evaluate(1, true));
                }
                else
                {
                    IList<XYZ> xyzArray = gEdge.Tessellate();
                    pts.AddRange(xyzArray);
                }
            }
        }


        /// <summary>
        /// Make a single element appear in a particular view
        /// </summary>
        /// <param name="view"></param>
        /// <param name="element"></param>
        protected static void IsolateInView(View3D view, Autodesk.Revit.DB.Element element)
        {
            var fec = GetVisibleElementFilter();

            view.CropBoxActive = true;
                
            var all = fec.ToElements();
            var toHide =
                fec.ToElements().Where(x => !x.IsHidden(view) && x.CanBeHidden(view) && x.Id != element.Id).Select(x => x.Id).ToList();

            if (toHide.Count > 0)
                view.HideElements(toHide);

            // (sic)
            Document.Regenerate();

            if (view.IsPerspective)
            {
                var farClip = view.get_Parameter("Far Clip Active");
                farClip.Set(0);
            }
            else
            {
                var pts = new List<XYZ>();

                GetPointCloud(element, pts);

                var bounding = view.CropBox;
                var transInverse = bounding.Transform.Inverse;
                var transPts = pts.Select(transInverse.OfPoint).ToList();

                //ingore the Z coordindates and find
                //the max X ,Y and Min X, Y in 3d view.
                double dMaxX = 0, dMaxY = 0, dMinX = 0, dMinY = 0;

                bool bFirstPt = true;
                foreach (var pt1 in transPts)
                {
                    if (true == bFirstPt)
                    {
                        dMaxX = pt1.X;
                        dMaxY = pt1.Y;
                        dMinX = pt1.X;
                        dMinY = pt1.Y;
                        bFirstPt = false;
                    }
                    else
                    {
                        if (dMaxX < pt1.X)
                            dMaxX = pt1.X;
                        if (dMaxY < pt1.Y)
                            dMaxY = pt1.Y;
                        if (dMinX > pt1.X)
                            dMinX = pt1.X;
                        if (dMinY > pt1.Y)
                            dMinY = pt1.Y;
                    }
                }

                bounding.Max = new XYZ(dMaxX, dMaxY, bounding.Max.Z);
                bounding.Min = new XYZ(dMinX, dMinY, bounding.Min.Z);
                view.CropBox = bounding;
            }

            view.CropBoxVisible = false;

        }

        /// <summary>
        /// Set the cropping for the current view
        /// </summary>
        /// <param name="view3D"></param>
        /// <param name="bbox"></param>
        private void IsolateInView(View3D view3D, BoundingBoxXYZ bbox)
        {
            view3D.CropBox = bbox;
        }

        /// <summary>
        /// Create a Revit 3D View
        /// </summary>
        /// <param name="orient"></param>
        /// <param name="name"></param>
        /// <param name="isPerspective"></param>
        /// <returns></returns>
        protected static View3D Create3DView(ViewOrientation3D orient, string name, bool isPerspective)
        {
            // (sic) From the Dynamo legacy implementation
            var viewFam = DocumentManager.Instance.ElementsOfType<ViewFamilyType>()
                .FirstOrDefault(x => x.ViewFamily == ViewFamily.ThreeDimensional);

            if (viewFam == null)
            {
                throw new Exception("There is no three dimensional view family int he document");
            }

            Autodesk.Revit.DB.View3D view;
            if (isPerspective)
            {
                view = View3D.CreatePerspective(Document, viewFam.Id);
            }
            else
            {
                view = View3D.CreateIsometric(Document, viewFam.Id);
            }

            view.SetOrientation(orient);
            view.SaveOrientationAndLock();

            try
            {
                //will fail if name is not unique
                view.Name = name;
            }
            catch
            {
                view.Name = CreateUniqueViewName(name);
            }


            return view;
        }

        /// <summary>
        /// Determines whether a view with the provided name already exists.
        /// If a view exists with the provided name, and new view is created with
        /// an incremented name. Otherwise, the original view name is returned.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string CreateUniqueViewName(string name)
        {
            string viewName = name;
            bool found = false;

            var collector = new FilteredElementCollector(Document);
            collector.OfClass(typeof(View));

            if (collector.ToElements().Count(x => x.Name == name) == 0)
                return name;

            int count = 0;
            while (!found)
            {
                string[] nameChunks = viewName.Split('_');

                viewName = string.Format("{0}_{1}", nameChunks[0], count.ToString(CultureInfo.InvariantCulture));

                if (collector.ToElements().ToList().Any(x => x.Name == viewName))
                    count++;
                else
                    found = true;
            }

            return viewName;
        }

        // (sic) From Dynamo legacy

        /// <summary>
        /// Utility method to create a filtered element collector which collects all elements in a view
        /// which Dynamo would like to view or on which Dynamo would like to operate.
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        protected static FilteredElementCollector GetVisibleElementFilter()
        {
            var fec = new FilteredElementCollector(Document);
            var filterList = new List<ElementFilter>();

            //Autodesk.Revit.DB.Analysis.AnalysisDisplayLegend;
            //Autodesk.Revit.DB.Analysis.AnalysisDisplayStyle;
            //Autodesk.Revit.DB.Analysis.MassEnergyAnalyticalModel;
            //Autodesk.Revit.DB.Analysis.MassLevelData;
            //Autodesk.Revit.DB.Analysis.MassSurfaceData;
            //Autodesk.Revit.DB.Analysis.MassZone;
            //Autodesk.Revit.DB.Analysis.SpatialFieldManager;
            //Autodesk.Revit.DB.AreaScheme;
            //Autodesk.Revit.DB.AppearanceAssetElement;
            var FContinuousRail = new ElementClassFilter(typeof(Autodesk.Revit.DB.Architecture.ContinuousRail));
            var FRailing = new ElementClassFilter(typeof(Autodesk.Revit.DB.Architecture.Railing));
            var FStairs = new ElementClassFilter(typeof(Autodesk.Revit.DB.Architecture.Stairs));
            var FStairsLanding = new ElementClassFilter(typeof(Autodesk.Revit.DB.Architecture.StairsLanding));
            //var FStairsPath = new ElementClassFilter(typeof(Autodesk.Revit.DB.Architecture.StairsPath));
            //var FStairsRun = new ElementClassFilter(typeof(Autodesk.Revit.DB.Architecture.StairsRun));
            var FTopographySurface = new ElementClassFilter(typeof(Autodesk.Revit.DB.Architecture.TopographySurface));
            //Autodesk.Revit.DB.AreaScheme;
            var FAssemblyInstance = new ElementClassFilter(typeof(Autodesk.Revit.DB.AssemblyInstance));
            var FBaseArray = new ElementClassFilter(typeof(Autodesk.Revit.DB.BaseArray));
            //ElementClassFilter FBasePoint = new ElementClassFilter(typeof(Autodesk.Revit.DB.BasePoint));
            var FBeamSystem = new ElementClassFilter(typeof(Autodesk.Revit.DB.BeamSystem));
            var FBoundaryConditions = new ElementClassFilter(typeof(Autodesk.Revit.DB.BoundaryConditions));
            //ElementClassFilter FCombinableElement = new ElementClassFilter(typeof(Autodesk.Revit.DB.CombinableElement));
            //Autodesk.Revit.DB..::..ComponentRepeater
            //Autodesk.Revit.DB..::..ComponentRepeaterSlot
            var FConnectorElement = new ElementClassFilter(typeof(Autodesk.Revit.DB.ConnectorElement));
            var FControl = new ElementClassFilter(typeof(Autodesk.Revit.DB.Control));
            var FCurveElement = new ElementClassFilter(typeof(Autodesk.Revit.DB.CurveElement));
            //Autodesk.Revit.DB.DesignOption;
            //Autodesk.Revit.DB.Dimension;
            //Autodesk.Revit.DB..::..DisplacementElement
            var FDividedSurface = new ElementClassFilter(typeof(Autodesk.Revit.DB.DividedSurface));
            var FCableTrayConduitRunBase = new ElementClassFilter(typeof(Autodesk.Revit.DB.Electrical.CableTrayConduitRunBase));
            //Autodesk.Revit.DB.Electrical.ElectricalDemandFactorDefinition;
            //Autodesk.Revit.DB.Electrical.ElectricalLoadClassification;
            //Autodesk.Revit.DB.Electrical.PanelScheduleSheetInstance;
            //Autodesk.Revit.DB.Electrical.PanelScheduleTemplate;
            var FElementType = new ElementClassFilter(typeof(Autodesk.Revit.DB.ElementType));
            //Autodesk.Revit.DB..::..ElevationMarker
            //ElementClassFilter FFamilyBase = new ElementClassFilter(typeof(Autodesk.Revit.DB.FamilyBase));
            //Autodesk.Revit.DB.FilledRegion;
            //Autodesk.Revit.DB.FillPatternElement;
            //Autodesk.Revit.DB.FilterElement;
            //Autodesk.Revit.DB.GraphicsStyle;
            //Autodesk.Revit.DB.Grid;
            //ElementClassFilter FGroup = new ElementClassFilter(typeof(Autodesk.Revit.DB.Group));
            var FHostObject = new ElementClassFilter(typeof(Autodesk.Revit.DB.HostObject));
            //Autodesk.Revit.DB.IndependentTag;
            var FInstance = new ElementClassFilter(typeof(Autodesk.Revit.DB.Instance));
            //Autodesk.Revit.DB.Level;
            //Autodesk.Revit.DB.LinePatternElement;
            //Autodesk.Revit.DB.Material;
            //Autodesk.Revit.DB.Mechanical.Zone;
            var FMEPSystem = new ElementClassFilter(typeof(Autodesk.Revit.DB.MEPSystem));
            var FModelText = new ElementClassFilter(typeof(Autodesk.Revit.DB.ModelText));
            //Autodesk.Revit.DB..::..MultiReferenceAnnotation
            var FOpening = new ElementClassFilter(typeof(Autodesk.Revit.DB.Opening));
            var FPart = new ElementClassFilter(typeof(Autodesk.Revit.DB.Part));
            var FPartMaker = new ElementClassFilter(typeof(Autodesk.Revit.DB.PartMaker));
            //Autodesk.Revit.DB.Phase;
            //Autodesk.Revit.DB..::..PhaseFilter
            //Autodesk.Revit.DB.PrintSetting;
            //Autodesk.Revit.DB.ProjectInfo;
            //Autodesk.Revit.DB.PropertyLine;
            //ElementClassFilter FPropertySetElement = new ElementClassFilter(typeof(Autodesk.Revit.DB.PropertySetElement));
            //Autodesk.Revit.DB.PropertySetLibrary;
            var FReferencePlane = new ElementClassFilter(typeof(Autodesk.Revit.DB.ReferencePlane));
            var FReferencePoint = new ElementClassFilter(typeof(Autodesk.Revit.DB.ReferencePoint));
            //Autodesk.Revit.DB..::..ScheduleSheetInstance
            //Autodesk.Revit.DB..::..Segment
            //ElementClassFilter FSketchBase = new ElementClassFilter(typeof(Autodesk.Revit.DB.SketchBase));
            //ElementClassFilter FSketchPlane = new ElementClassFilter(typeof(Autodesk.Revit.DB.SketchPlane));
            var FSpatialElement = new ElementClassFilter(typeof(Autodesk.Revit.DB.SpatialElement));
            //Autodesk.Revit.DB..::..SpatialElementCalculationLocation
            //ElementClassFilter FSpatialElementTag = new ElementClassFilter(typeof(Autodesk.Revit.DB.SpatialElementTag));
            //Autodesk.Revit.DB.Structure..::..AnalyticalLink
            //Autodesk.Revit.DB.Structure.AnalyticalModel;
            var FAreaReinforcement = new ElementClassFilter(typeof(Autodesk.Revit.DB.Structure.AreaReinforcement));
            //Autodesk.Revit.DB.Structure..::..FabricArea
            //Autodesk.Revit.DB.Structure..::..FabricReinSpanSymbolControl
            //Autodesk.Revit.DB.Structure..::..FabricSheet
            var FHub = new ElementClassFilter(typeof(Autodesk.Revit.DB.Structure.Hub));
            //Autodesk.Revit.DB.Structure.LoadBase;
            //Autodesk.Revit.DB.Structure.LoadCase;
            //Autodesk.Revit.DB.Structure.LoadCombination;
            //Autodesk.Revit.DB.Structure.LoadNature;
            //Autodesk.Revit.DB.Structure.LoadUsage;
            var FPathReinforcement = new ElementClassFilter(typeof(Autodesk.Revit.DB.Structure.PathReinforcement));
            var FRebar = new ElementClassFilter(typeof(Autodesk.Revit.DB.Structure.Rebar));
            //Autodesk.Revit.DB.Structure..::..RebarInSystem
            var FTruss = new ElementClassFilter(typeof(Autodesk.Revit.DB.Structure.Truss));
            //Autodesk.Revit.DB.SunAndShadowSettings;
            //Autodesk.Revit.DB.TextElement;
            //Autodesk.Revit.DB.View;
            //Autodesk.Revit.DB..::..Viewport
            //Autodesk.Revit.DB.ViewSheetSet;
            //Autodesk.Revit.DB.WorksharingDisplaySettings;

            filterList.Add(FContinuousRail);
            filterList.Add(FRailing);
            filterList.Add(FStairs);
            filterList.Add(FStairsLanding);
            filterList.Add(FTopographySurface);
            filterList.Add(FAssemblyInstance);
            filterList.Add(FBaseArray);
            filterList.Add(FBeamSystem);
            filterList.Add(FBoundaryConditions);
            filterList.Add(FConnectorElement);
            filterList.Add(FControl);
            filterList.Add(FCurveElement);
            filterList.Add(FDividedSurface);
            filterList.Add(FCableTrayConduitRunBase);
            filterList.Add(FHostObject);
            filterList.Add(FInstance);
            filterList.Add(FMEPSystem);
            filterList.Add(FModelText);
            filterList.Add(FOpening);
            filterList.Add(FPart);
            filterList.Add(FPartMaker);
            filterList.Add(FReferencePlane);
            filterList.Add(FReferencePoint);
            filterList.Add(FAreaReinforcement);
            filterList.Add(FHub);
            filterList.Add(FPathReinforcement);
            filterList.Add(FRebar);
            filterList.Add(FTruss);
            filterList.Add(FSpatialElement);

            //ElementCategoryFilter CRailings = new ElementCategoryFilter(BuiltInCategory.OST_StairsRailing);
            //ElementCategoryFilter CStairs = new ElementCategoryFilter(BuiltInCategory.OST_Stairs);

            var CRvtLinks = new ElementCategoryFilter(BuiltInCategory.OST_RvtLinks);
            filterList.Add(CRvtLinks);

            //List<ElementFilter> ignores = new List<ElementFilter>();
            //ElementCategoryFilter CLightFixtureSource = new ElementCategoryFilter(BuiltInCategory.OST_LightingFixtureSource, true);
            //ignores.Add(CLightFixtureSource);

            var filters = new LogicalOrFilter(filterList);
            //LogicalOrFilter exclusions = new LogicalOrFilter(ignores);

            fec.WherePasses(filters).WhereElementIsNotElementType();

            return fec;
        }

        #endregion

        #region Protected mutators

        /// <summary>
        /// Set the name of the current view
        /// </summary>
        /// <param name="name"></param>
        protected void InternalSetName(string name)
        {
            if (!this.InternalView3D.Name.Equals(name))
                this.InternalView3D.Name = CreateUniqueViewName(name);
        }

        /// <summary>
        /// Set the orientation of the view
        /// </summary>
        /// <param name="orient"></param>
        protected void InternalSetOrientation( ViewOrientation3D orient)
        {
            if (this.InternalView3D.ViewDirection.IsAlmostEqualTo(orient.ForwardDirection) &&
                this.InternalView3D.Origin.IsAlmostEqualTo(orient.EyePosition)) return;

            this.InternalView3D.Unlock();
            this.InternalView3D.SetOrientation(orient);
            this.InternalView3D.SaveOrientationAndLock();
        }

        /// <summary>
        /// Isolate the element in the current view by creating a mininum size crop box around it
        /// </summary>
        /// <param name="element"></param>
        protected void InternalIsolateInView(Autodesk.Revit.DB.Element element)
        {
            IsolateInView(this.InternalView3D, element);
        }
    
        /// <summary>
        /// Isolate the bounding box in the current view
        /// </summary>
        /// <param name="bbox"></param>
        protected void InternalIsolateInView(BoundingBoxXYZ bbox)
        {
            IsolateInView(this.InternalView3D, bbox);
        }

        /// <summary>
        /// Show all hiddent elements in the view
        /// </summary>
        protected void InternalRemoveIsolation()
        {
            InternalView3D.UnhideElements(GetVisibleElementFilter().ToElementIds());
            InternalView3D.CropBoxActive = false;
        }

        /// <summary>
        /// Set the InternalView3D property and the associated element id and unique id
        /// </summary>
        /// <param name="view"></param>
        protected void InternalSetView3D(View3D view)
        {
            this.InternalView3D = view;
            this.InternalElementId = view.Id;
            this.InternalUniqueId = view.UniqueId;
        }

        #endregion

    }
}

