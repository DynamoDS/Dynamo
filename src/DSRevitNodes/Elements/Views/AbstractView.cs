using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using DSRevitNodes.Elements;
using RevitServices.Persistence;

namespace DSRevitNodes
{
    public abstract class AbstractView : AbstractElement
    {
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
        public static FilteredElementCollector GetVisibleElementFilter()
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
    }
}



    //public delegate View3D View3DCreationDelegate(ViewOrientation3D orient, string name, bool isPerspective);

    //        InPortData.Add(new PortData("eye", "The eye position point.", typeof(Value.Container)));
    //        InPortData.Add(new PortData("target", "The location where the view is pointing.", typeof(Value.Container)));
    //        InPortData.Add(new PortData("name", "The name of the view.", typeof(Value.String)));
    //        InPortData.Add(new PortData("extents", "Pass in a bounding box or an element to define the 3D crop of the view.", typeof(Value.String)));
    //        InPortData.Add(new PortData("isolate", "If an element is supplied in 'extents', it will be isolated in the view.", typeof(Value.String)));

    //        OutPortData.Add(new PortData("view", "The newly created 3D view.", typeof(Value.Container)));


  

/*View3D view = null;
            var eye = (XYZ)((Value.Container)args[0]).Item;
            var target = (XYZ)((Value.Container)args[1]).Item;
            var name = ((Value.String)args[2]).Item;
            var extents = ((Value.Container)args[3]).Item;
            var isolate = Convert.ToBoolean(((Value.Number)args[4]).Item);

            var globalUp = XYZ.BasisZ;
            var direction = target.Subtract(eye);
            var up = direction.CrossProduct(globalUp).CrossProduct(direction);
            var orient = new ViewOrientation3D(eye, up, direction);

            if (this.Elements.Any())
            {
                if (dynUtils.TryGetElement(this.Elements[0], out view))
                {
                    if (!view.ViewDirection.IsAlmostEqualTo(direction) || !view.Origin.IsAlmostEqualTo(eye))
                    {
                        view.Unlock();
                        view.SetOrientation(orient);
                        view.SaveOrientationAndLock();
                    }

                    if (!view.Name.Equals(name))
                        view.Name = ViewBase.CreateUniqueViewName(name);
                }
                else
                {
                    //create a new view
                    view = ViewBase.Create3DView(orient, name, isPerspective);
                    Elements[0] = view.Id;
                }
            }
            else
            {
                view = Create3DView(orient, name, isPerspective);
                Elements.Add(view.Id);
            }

            var fec = dynRevitUtils.SetupFilters(dynRevitSettings.Doc.Document);

            if (isolate)
            {
                view.CropBoxActive = true;

                var element = extents as Element;
                if (element != null)
                {
                    var e = element;

                    var all = fec.ToElements();
                    var toHide =
                        fec.ToElements().Where(x => !x.IsHidden(view) && x.CanBeHidden(view) && x.Id != e.Id).Select(x => x.Id).ToList();
                    
                    if (toHide.Count > 0)
                        view.HideElements(toHide);

                    dynRevitSettings.Doc.Document.Regenerate();

                    Debug.WriteLine(string.Format("Eye:{0},Origin{1}, BBox_Origin{2}, Element{3}",
                        eye.ToString(), view.Origin.ToString(), view.CropBox.Transform.Origin.ToString(), (element.Location as LocationPoint).Point.ToString()));

                    //http://wikihelp.autodesk.com/Revit/fra/2013/Help/0000-API_Deve0/0039-Basic_In39/0067-Views67/0069-The_View69
                    if (isPerspective)
                    {
                        var farClip = view.get_Parameter("Far Clip Active");
                        farClip.Set(0);
                    }
                    else
                    {
                        //http://adndevblog.typepad.com/aec/2012/05/set-crop-box-of-3d-view-that-exactly-fits-an-element.html
                        var pts = new List<XYZ>();

                        ParseElementGeometry(element, pts);

                        var bounding = view.CropBox;
                        var transInverse = bounding.Transform.Inverse;
                        var transPts = pts.Select(transInverse.OfPoint).ToList();

                        //ingore the Z coordindates and find
                        //the max X ,Y and Min X, Y in 3d view.
                        double dMaxX = 0, dMaxY = 0, dMinX = 0, dMinY = 0;

                        //geom.XYZ ptMaxX, ptMaxY, ptMinX,ptMInY; 
                        //coorresponding point.
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
                }
                else
                {
                    var xyz = extents as BoundingBoxXYZ;
                    if (xyz != null)
                    {
                        view.CropBox = xyz;
                    }
                }

                view.CropBoxVisible = false;
            }
            else
            {
                view.UnhideElements(fec.ToElementIds());
                view.CropBoxActive = false;
            }

            return Value.NewContainer(view);
 * */

    //[NodeName("Bounding Box XYZ")]
    //[NodeCategory(BuiltinNodeCategories.ANALYZE_MEASURE)]
    //[NodeDescription("Create a bounding box.")]
    //public class BoundingBoxXyz : NodeWithOneOutput
    //{
    //    public BoundingBoxXyz()
    //    {
    //        InPortData.Add(new PortData("trans", "The coordinate system of the box.", typeof(Value.Container)));
    //        InPortData.Add(new PortData("x size", "The size of the bounding box in the x direction of the local coordinate system.", typeof(Value.Number)));
    //        InPortData.Add(new PortData("y size", "The size of the bounding box in the y direction of the local coordinate system.", typeof(Value.Number)));
    //        InPortData.Add(new PortData("z size", "The size of the bounding box in the z direction of the local coordinate system.", typeof(Value.Number)));
    //        OutPortData.Add(new PortData("bbox", "The bounding box.", typeof(Value.Container)));

    //        RegisterAllPorts();

    //        ArgumentLacing = LacingStrategy.Longest;
    //    }

    //    public override Value Evaluate(FSharpList<Value> args)
    //    {
    //        BoundingBoxXYZ bbox = new BoundingBoxXYZ();
            
    //        Transform t = (Transform)((Value.Container)args[0]).Item;
    //        double x = (double)((Value.Number)args[1]).Item;
    //        double y = (double)((Value.Number)args[2]).Item;
    //        double z = (double)((Value.Number)args[3]).Item;

    //        bbox.Transform = t;
    //        bbox.Min = new XYZ(0, 0, 0);
    //        bbox.Max = new XYZ(x, y, z);
    //        return Value.NewContainer(bbox);
    //    }

    //}


    //[NodeName("Get Active View")]
    //[NodeCategory(BuiltinNodeCategories.REVIT_VIEW)]
    //[NodeDescription("Gets the active Revit view.")]
    //public class ActiveRevitView : RevitTransactionNodeWithOneOutput
    //{
    //    public ActiveRevitView()
    //    {
    //        OutPortData.Add(new PortData("v", "The active revit view.", typeof(Value.Container)));

    //        RegisterAllPorts();
    //    }

    //    public override Value Evaluate(FSharpList<Value> args)
    //    {

    //        return Value.NewContainer(dynRevitSettings.Doc.Document.ActiveView);
    //    }

    //}