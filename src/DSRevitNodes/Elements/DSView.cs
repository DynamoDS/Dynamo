using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSRevitNodes
{
    class DSView
    {
    }
}

/*  [NodeName("Drafting View")]
    [NodeCategory(BuiltinNodeCategories.REVIT_VIEW)]
    [NodeDescription("Creates a drafting view.")]
    public class DraftingView: RevitTransactionNodeWithOneOutput
    {
        public DraftingView()
        {
            InPortData.Add(new PortData("name", "Name", typeof(Value.String)));
            OutPortData.Add(new PortData("v", "Drafting View", typeof(Value.Container)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            ViewDrafting vd = null;
            string viewName = ((Value.String)args[0]).Item;

            if (this.Elements.Any())
            {
                if (!dynUtils.TryGetElement(this.Elements[0], out vd))
                {
                    vd = dynRevitSettings.Doc.Document.Create.NewViewDrafting();
                    this.Elements[0] = vd.Id;
                }
            }
            else
            {
                vd = dynRevitSettings.Doc.Document.Create.NewViewDrafting();
                this.Elements.Add(vd.Id);
            }

            //rename the view
            if(!vd.Name.Equals(viewName))
                 vd.Name = ViewBase.CreateUniqueViewName(viewName);

            return Value.NewContainer(vd);
        }
    }

    public delegate View3D View3DCreationDelegate(ViewOrientation3D orient, string name, bool isPerspective);

    public abstract class ViewBase:RevitTransactionNodeWithOneOutput
    {
        protected bool isPerspective = false;

        protected ViewBase()
        {
            InPortData.Add(new PortData("eye", "The eye position point.", typeof(Value.Container)));
            InPortData.Add(new PortData("target", "The location where the view is pointing.", typeof(Value.Container)));
            InPortData.Add(new PortData("name", "The name of the view.", typeof(Value.String)));
            InPortData.Add(new PortData("extents", "Pass in a bounding box or an element to define the 3D crop of the view.", typeof(Value.String)));
            InPortData.Add(new PortData("isolate", "If an element is supplied in 'extents', it will be isolated in the view.", typeof(Value.String)));

            OutPortData.Add(new PortData("view", "The newly created 3D view.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            View3D view = null;
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
        }

        private static void ParseElementGeometry(Element e, List<XYZ> pts)
        {
            foreach (GeometryObject gObj in e.get_Geometry(dynRevitSettings.GeometryOptions))
            {
                if (gObj is Solid)
                {
                    ParseSolid(gObj, pts);
                }
                else if (gObj is GeometryInstance)
                {
                    ParseInstanceGeometry(gObj, pts);
                }
            }
        }

        private static void ParseInstanceGeometry(GeometryObject obj, List<XYZ> pts)
        {
            var geomInst = obj as GeometryInstance;
            foreach (GeometryObject gObj in geomInst.GetInstanceGeometry())
            {
                if (gObj is Solid)
                {
                    ParseSolid(gObj, pts);
                }
                else if (gObj is GeometryInstance)
                {
                    ParseInstanceGeometry(gObj, pts);
                }
            }
        }

        private static void ParseSolid(GeometryObject obj, List<XYZ> pts)
        {
            var solid = obj as Solid;
            foreach (Edge gEdge in solid.Edges)
            {
                Curve c = gEdge.AsCurve();
                if (c is Line)
                {
                    pts.Add(c.Evaluate(0, true));
                    pts.Add(c.Evaluate(1,true));
                }
                else
                {
                    IList<XYZ> xyzArray = gEdge.Tessellate();
                    pts.AddRange(xyzArray);
                }
            }
        }

        public static View3D Create3DView(ViewOrientation3D orient, string name, bool isPerspective)
        {
            //http://adndevblog.typepad.com/aec/2012/05/viewplancreate-method.html

            IEnumerable<ViewFamilyType> viewFamilyTypes = from elem in new
              FilteredElementCollector(dynRevitSettings.Doc.Document).OfClass(typeof(ViewFamilyType))
                                                          let type = elem as ViewFamilyType
                                                          where type.ViewFamily == ViewFamily.ThreeDimensional
                                                          select type;

            //create a new view
            View3D view = isPerspective ?
                              View3D.CreatePerspective(dynRevitSettings.Doc.Document, viewFamilyTypes.First().Id) :
                              View3D.CreateIsometric(dynRevitSettings.Doc.Document, viewFamilyTypes.First().Id);

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

            var collector = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            collector.OfClass(typeof(View));

            if (collector.ToElements().Count(x=>x.Name == name) == 0)
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
    }

    [NodeName("Axonometric View")]
    [NodeCategory(BuiltinNodeCategories.REVIT_VIEW)]
    [NodeDescription("Creates an axonometric view.")]
    public class IsometricView : ViewBase
    {
        public IsometricView ()
        {
            isPerspective = false;
        }
    }

    [NodeName("Perspective View")]
    [NodeCategory(BuiltinNodeCategories.REVIT_VIEW)]
    [NodeDescription("Creates a perspective view.")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    public class PerspectiveView : ViewBase
    {
        public PerspectiveView()
        {
            isPerspective = true;
        }
    }

    [NodeName("Bounding Box XYZ")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_MEASURE)]
    [NodeDescription("Create a bounding box.")]
    public class BoundingBoxXyz : NodeWithOneOutput
    {
        public BoundingBoxXyz()
        {
            InPortData.Add(new PortData("trans", "The coordinate system of the box.", typeof(Value.Container)));
            InPortData.Add(new PortData("x size", "The size of the bounding box in the x direction of the local coordinate system.", typeof(Value.Number)));
            InPortData.Add(new PortData("y size", "The size of the bounding box in the y direction of the local coordinate system.", typeof(Value.Number)));
            InPortData.Add(new PortData("z size", "The size of the bounding box in the z direction of the local coordinate system.", typeof(Value.Number)));
            OutPortData.Add(new PortData("bbox", "The bounding box.", typeof(Value.Container)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            BoundingBoxXYZ bbox = new BoundingBoxXYZ();
            
            Transform t = (Transform)((Value.Container)args[0]).Item;
            double x = (double)((Value.Number)args[1]).Item;
            double y = (double)((Value.Number)args[2]).Item;
            double z = (double)((Value.Number)args[3]).Item;

            bbox.Transform = t;
            bbox.Min = new XYZ(0, 0, 0);
            bbox.Max = new XYZ(x, y, z);
            return Value.NewContainer(bbox);
        }

    }

    [NodeName("Section View")]
    [NodeCategory(BuiltinNodeCategories.REVIT_VIEW)]
    [NodeDescription("Creates a section view.")]
    public class SectionView : RevitTransactionNodeWithOneOutput
    {
        public SectionView()
        {
            InPortData.Add(new PortData("bbox", "The bounding box of the view.", typeof(Value.Container)));
            OutPortData.Add(new PortData("v", "The newly created section view.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            ViewSection view = null;
            BoundingBoxXYZ bbox = (BoundingBoxXYZ)((Value.Container)args[0]).Item;

            //recreate the view. it does not seem possible to update a section view's orientation
            if (this.Elements.Any())
            {
                //create a new view
                view = CreateSectionView(bbox);
                Elements[0] = view.Id;
            }
            else
            {
                view = CreateSectionView(bbox);
                Elements.Add(view.Id);
            }

            return Value.NewContainer(view);
        }

        private static ViewSection CreateSectionView(BoundingBoxXYZ bbox)
        {
            //http://adndevblog.typepad.com/aec/2012/05/viewplancreate-method.html

            IEnumerable<ViewFamilyType> viewFamilyTypes = from elem in new
              FilteredElementCollector(dynRevitSettings.Doc.Document).OfClass(typeof(ViewFamilyType))
                                                          let type = elem as ViewFamilyType
                                                          where type.ViewFamily == ViewFamily.Section
                                                          select type;

            //create a new view
            ViewSection view = ViewSection.CreateSection(dynRevitSettings.Doc.Document, viewFamilyTypes.First().Id, bbox);
            return view;
        }
    }

    [NodeName("Get Active View")]
    [NodeCategory(BuiltinNodeCategories.REVIT_VIEW)]
    [NodeDescription("Gets the active Revit view.")]
    public class ActiveRevitView : RevitTransactionNodeWithOneOutput
    {
        public ActiveRevitView()
        {
            OutPortData.Add(new PortData("v", "The active revit view.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {

            return Value.NewContainer(dynRevitSettings.Doc.Document.ActiveView);
        }

    }*/