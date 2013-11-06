using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using Curve = Autodesk.DesignScript.Geometry.Curve;
using Face = Autodesk.DesignScript.Geometry.Face;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace DSRevitNodes
{
    public class AdaptiveComponent
    {
        /// <summary>
        /// Create an adaptive component from a list of points.
        /// </summary>
        /// <param name="pts"></param>
        /// <returns></returns>
        static AdaptiveComponent ByPoints(List<Point> pts, FamilySymbol fs)
        {
            FamilyInstance ac;

            //if the adapative component already exists, then move the points
            if (Elements.Any())
            {
                //mutate
                //...we attempt to fetch it from the document...
                if (dynUtils.TryGetElement(Elements[0], out ac))
                {
                    ac.Symbol = fs;
                }
                else
                {
                    //create
                    ac = AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance(dynRevitSettings.Doc.Document, fs);
                    Elements[0] = ac.Id;
                }
            }
            else
            {
                //create
                ac = AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance(dynRevitSettings.Doc.Document, fs);
                Elements.Add(ac.Id);
            }

            if (ac == null)
                throw new Exception("An adaptive component could not be found or created.");

            IList<ElementId> placePointIds = AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(ac);

            if (placePointIds.Count() != pts.Count())
                throw new Exception("The input list of points does not have the same number of values required by the adaptive component.");

            // Set the position of each placement point
            int i = 0;
            foreach (ElementId id in placePointIds)
            {
                var point = dynRevitSettings.Doc.Document.GetElement(id) as ReferencePoint;
                var pt = (XYZ)((Value.Container)pts.ElementAt(i)).Item;
                point.Position = pt;
                i++;
            }

            return Value.NewContainer(ac);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Create an adaptive component by uv points on a face.
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        static AdaptiveComponent ByPointsOnFace(List<Vector> pts, Face f)
        {
            throw new NotImplementedException();
        }
    }
}


namespace Dynamo.Nodes
{
    [NodeName("Adaptive Component by XYZs")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILIES)]
    [NodeDescription("Given a list of XYZs and a family type, creates an adaptive component at that location.")]
    public class AdaptiveComponentByPoints : RevitTransactionNodeWithOneOutput
    {
        public AdaptiveComponentByPoints()
        {
            InPortData.Add(new PortData("xyzs", "The XYZs that define the locations of your adaptive points.", typeof(Value.Container)));
            InPortData.Add(new PortData("fs", "The family type to create the adaptive component.", typeof(Value.Container)));
            OutPortData.Add(new PortData("ac", "The adaptive component.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            FSharpList<Value> pts = ((Value.List)args[0]).Item;
            var fs = (FamilySymbol)((Value.Container)args[1]).Item;

            FamilyInstance ac;

            //if the adapative component already exists, then move the points
            if (Elements.Any())
            {
                //mutate
                //...we attempt to fetch it from the document...
                if (dynUtils.TryGetElement(Elements[0], out ac))
                {
                    ac.Symbol = fs;
                }
                else
                {
                    //create
                    ac = AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance(dynRevitSettings.Doc.Document, fs);
                    Elements[0] = ac.Id;
                }
            }
            else
            {
                //create
                ac = AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance(dynRevitSettings.Doc.Document, fs);
                Elements.Add(ac.Id);
            }

            if (ac == null)
                throw new Exception("An adaptive component could not be found or created.");

            IList<ElementId> placePointIds = AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(ac);

            if (placePointIds.Count() != pts.Count())
                throw new Exception("The input list of points does not have the same number of values required by the adaptive component.");

            // Set the position of each placement point
            int i = 0;
            foreach (ElementId id in placePointIds)
            {
                var point = dynRevitSettings.Doc.Document.GetElement(id) as ReferencePoint;
                var pt = (XYZ)((Value.Container)pts.ElementAt(i)).Item;
                point.Position = pt;
                i++;
            }

            return Value.NewContainer(ac);
        }

    }

    [NodeName("Adaptive Component by UVs on Face")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILIES)]
    [NodeDescription("Given a list of XYZs and a family type, creates an adaptive component at that location on the face.")]
    public class AdaptiveComponentByUvsOnFace : RevitTransactionNodeWithOneOutput
    {
        public AdaptiveComponentByUvsOnFace()
        {
            InPortData.Add(new PortData("uvs", "The UVs that define the locations of your adaptive points on the face.", typeof(Value.List)));
            InPortData.Add(new PortData("face", "The face on which to host your Adaptive Component instance.", typeof(Value.Container)));
            InPortData.Add(new PortData("fs", "The family type to create the adaptive component.", typeof(Value.Container)));
            OutPortData.Add(new PortData("ac", "The adaptive component.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if(!args[0].IsList)
                throw new Exception("A list of UVs is required to place the Adaptive Component.");

            FSharpList<Value> uvs = ((Value.List)args[0]).Item;

            var faceRef = ((Value.Container) args[1]).Item as Reference;
            var f = faceRef == null
                         ? (Autodesk.Revit.DB.Face) ((Value.Container) args[1]).Item
                         : (Autodesk.Revit.DB.Face)dynRevitSettings.Doc.Document.GetElement(faceRef.ElementId).GetGeometryObjectFromReference(faceRef);

            var fs = (FamilySymbol)((Value.Container)args[2]).Item;

            FamilyInstance ac = null;

            //if the adapative component already exists, then move the points
            if (Elements.Any())
            {
                //mutate
                //...we attempt to fetch it from the document...
                if (dynUtils.TryGetElement(this.Elements[0], out ac))
                {
                    ac.Symbol = fs;
                }
                else
                {
                    //create
                    ac = AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance(dynRevitSettings.Doc.Document, fs);
                    Elements[0] = ac.Id;
                }
            }
            else
            {
                //create
                ac = AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance(dynRevitSettings.Doc.Document, fs);
                Elements.Add(ac.Id);
            }

            if (ac == null)
                throw new Exception("An adaptive component could not be found or created.");

            IList<ElementId> placePointIds = new List<ElementId>();
            placePointIds = AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(ac);

            if (placePointIds.Count() != uvs.Count())
                throw new Exception("The input list of UVs does not have the same number of values required by the adaptive component.");

            // Set the position of each placement point
            int i = 0;
            foreach (ElementId id in placePointIds)
            {
                var uv = (UV)((Value.Container)uvs.ElementAt(i)).Item;
                var point = dynRevitSettings.Doc.Document.GetElement(id) as ReferencePoint;
                var peref = dynRevitSettings.Revit.Application.Create.NewPointOnFace(f.Reference, uv);
                point.SetPointElementReference(peref);
                i++;
            }

            return Value.NewContainer(ac);
        }

    }

    [NodeName("Adaptive Component by Parameter on Curve")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILIES)]
    [NodeDescription("Given a list of parameters and a family type, creates an adaptive component at that location on the curve.")]
    public class AdaptiveComponentByParametersOnCurve : RevitTransactionNodeWithOneOutput
    {
        public AdaptiveComponentByParametersOnCurve()
        {
            InPortData.Add(new PortData("params", "The parameters that define the locations of your adaptive points on the curve.", typeof(Value.List)));
            InPortData.Add(new PortData("curve", "The curve on which to host your Adaptive Component instance.", typeof(Value.Container)));
            InPortData.Add(new PortData("fs", "The family type to create the adaptive component.", typeof(Value.Container)));
            OutPortData.Add(new PortData("ac", "The adaptive component.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (!args[0].IsList)
                throw new Exception("A list of UVs is required to place the Adaptive Component.");

            FSharpList<Value> parameters = ((Value.List)args[0]).Item;

            var curveRef = ((Value.Container)args[1]).Item as Reference;
            var c = curveRef == null
                         ? (Autodesk.Revit.DB.Curve)((Value.Container)args[1]).Item
                         : (Autodesk.Revit.DB.Curve)dynRevitSettings.Doc.Document.GetElement(curveRef.ElementId).GetGeometryObjectFromReference(curveRef);

            var fs = (FamilySymbol)((Value.Container)args[2]).Item;

            FamilyInstance ac = null;

            //if the adapative component already exists, then move the points
            if (Elements.Any())
            {
                //...we attempt to fetch it from the document...
                if (dynUtils.TryGetElement(this.Elements[0], out ac))
                {
                    ac.Symbol = fs;
                }
                else
                {
                    //create
                    ac = AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance(dynRevitSettings.Doc.Document, fs);
                    Elements[0] = ac.Id;
                }
            }
            else
            {
                //create
                ac = AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance(dynRevitSettings.Doc.Document, fs);
                Elements.Add(ac.Id);
            }

            if (ac == null)
                throw new Exception("An adaptive component could not be found or created.");

            IList<ElementId> placePointIds = new List<ElementId>();
            placePointIds = AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(ac);

            if (placePointIds.Count() != parameters.Count())
                throw new Exception("The input list of UVs does not have the same number of values required by the adaptive component.");

            // Set the position of each placement point
            int i = 0;
            foreach (ElementId id in placePointIds)
            {
                var t = ((Value.Number)parameters.ElementAt(i)).Item;
                var point = dynRevitSettings.Doc.Document.GetElement(id) as ReferencePoint;
                var ploc = new PointLocationOnCurve(PointOnCurveMeasurementType.NonNormalizedCurveParameter, t,
                                                    PointOnCurveMeasureFrom.Beginning);
                var peref = dynRevitSettings.Revit.Application.Create.NewPointOnEdge(c.Reference, ploc);
                point.SetPointElementReference(peref);
                i++;
            }

            return Value.NewContainer(ac);
        }

    }
}
