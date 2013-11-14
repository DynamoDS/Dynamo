using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Dynamo.Models;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
using Dynamo.Revit;

namespace Dynamo.Nodes
{
    [NodeName("Adaptive Component by XYZs")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILIES)]
    [NodeDescription("Given a list of XYZs and a family type, creates an adaptive component at that location.")]
    public class AdaptiveComponentByPoints : RevitTransactionNodeWithOneOutput
    {
        public AdaptiveComponentByPoints()
        {
            InPortData.Add(new PortData("xyzs", "The XYZs that define the locations of your adaptive points.", typeof(Value.List)));
            InPortData.Add(new PortData("family type", "The family type to create the adaptive component.", typeof(Value.Container)));
            OutPortData.Add(new PortData("adaptive component", "The adaptive component.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            FSharpList<Value> pts = ((Value.List)args[0]).Item;
            var fs = (FamilySymbol)((Value.Container)args[1]).Item;

            FamilyInstance ac;

            var instData = new List<FamilyInstanceCreationData>();

            var sw = new Stopwatch();

            //if the adapative component already exists, then move the points
            if (Elements.Any())
            {
                //mutate
                //...we attempt to fetch it from the document...
                if (dynUtils.TryGetElement(Elements[0], out ac))
                {
                    sw.Start();
                    ac.Symbol = fs;
                    sw.Stop();
                    Debug.WriteLine(string.Format("{0} elapsed for updating family type on AC.", sw.Elapsed));
                    sw.Reset();
                }
                else
                {
                    sw.Start();
                    //create
                    ac = AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance(dynRevitSettings.Doc.Document, fs);
                    Elements[0] = ac.Id;
                    sw.Stop();
                    Debug.WriteLine(string.Format("{0} elapsed for creating an AC.", sw.Elapsed));
                    sw.Reset();
                }
            }
            else
            {
                sw.Start();
                //create
                ac = AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance(dynRevitSettings.Doc.Document, fs);
                Elements.Add(ac.Id);
                sw.Stop();
                Debug.WriteLine(string.Format("{0} elapsed for creating an AC.", sw.Elapsed));
                sw.Reset();
            }

            if (ac == null)
                throw new Exception("An adaptive component could not be found or created.");

            IList<ElementId> placePointIds = AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(ac);

            if (placePointIds.Count() != pts.Count())
                throw new Exception("The input list of points does not have the same number of values required by the adaptive component.");

            sw.Start();
            // Set the position of each placement point
            int i = 0;
            foreach (ElementId id in placePointIds)
            {
                var point = dynRevitSettings.Doc.Document.GetElement(id) as ReferencePoint;
                var pt = (XYZ)((Value.Container)pts.ElementAt(i)).Item;
                point.Position = pt;
                i++;
            }
            sw.Stop();
            Debug.WriteLine(string.Format("{0} elapsed for updating placement points of the AC.", sw.Elapsed));
            sw.Reset();


            return Value.NewContainer(ac);
        }

    }

    [NodeName("Adaptive Component Batch by XYZs")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILIES)]
    [NodeDescription("Given sets of points, create adaptive components at those location.")]
    public class AdaptiveComponentBatchByPoints : RevitTransactionNodeWithOneOutput
    {
        public AdaptiveComponentBatchByPoints()
        {
            InPortData.Add(new PortData("xyzs", "The XYZs that define the locations of your adaptive points.", typeof(Value.List)));
            InPortData.Add(new PortData("family type", "The family type to create the adaptive component.", typeof(Value.Container)));
            OutPortData.Add(new PortData("adaptive component", "The adaptive component.", typeof(Value.Container)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var xyzs = ((Value.List)args[0]).Item;
            var symbol = (FamilySymbol) ((Value.Container) args[1]).Item;

            var instData = new List<FamilyInstanceCreationData>();
            var updated = new HashSet<ElementId>();

            int count = 0;

            #region batch creation data

            var sw = new Stopwatch();
            sw.Start();
            foreach (var pts in xyzs)
            {
                FamilyInstance instance = null;
                if (Elements.Count > count)
                {
                    if (dynUtils.TryGetElement(this.Elements[count], out instance))
                    {
                        //if we've found an instance, change its symbol if it needs
                        //changing
                        if (instance.Symbol != symbol)
                            instance.Symbol = symbol;

                        //update the placement points and add the 
                        //id to the list of updated acs
                        UpdatePlacementPoints(instance, xyzs, count);
                        updated.Add(Elements[count]);
                    }
                    else
                    {
                        var instanceData = new FamilyInstanceCreationData(XYZ.Zero, symbol, StructuralType.NonStructural);
                        instData.Add(instanceData);
                    }
                }
                else
                {
                    var instanceData = new FamilyInstanceCreationData(XYZ.Zero, symbol, StructuralType.NonStructural);
                    instData.Add(instanceData);
                }

                count++;
            }
            sw.Stop();
            Debug.WriteLine(string.Format("{0} elapsed for updating existing or generating family creation data.", sw.Elapsed));
            sw.Reset();

            #endregion

            //trim the elements collection
            foreach (var e in this.Elements.Skip(count))
            {
                DeleteElement(e);
            }

            FSharpList<Value> results = FSharpList<Value>.Empty;

            sw.Start();
            if (instData.Any())
            {
                ICollection<ElementId> ids;
                if (dynRevitSettings.Doc.Document.IsFamilyDocument)
                {
                    ids = dynRevitSettings.Doc.Document.FamilyCreate.NewFamilyInstances2(instData);
                }
                else
                {
                    ids = dynRevitSettings.Doc.Document.Create.NewFamilyInstances2(instData);
                }

                //add our batch-created instances ids'
                //to the elements collection
                ids.ToList().ForEach(x => Elements.Add(x));
            }
            sw.Stop();
            Debug.WriteLine(string.Format("{0} elapsed for creating instances from creation data.", sw.Elapsed));
            sw.Reset();

            sw.Start();
            //make sure the ids list and the XYZ sets list
            //have the same length
            if (count != xyzs.Count())
            {
                throw new Exception("There are more adaptive component instances than there are points to adjust.");
            }

            for (var j = 0; j < Elements.Count; j++)
            {
                if (updated.Contains(Elements[j]))
                {
                    continue;
                }

                FamilyInstance ac;
                if (!dynUtils.TryGetElement(Elements[j], out ac))
                {
                    continue;
                }

                UpdatePlacementPoints(ac, xyzs, j);
            }
            sw.Stop();
            Debug.WriteLine(string.Format("{0} elapsed for updating remaining instance locations.", sw.Elapsed));

            //add all of the instances
            results = Elements.Aggregate(results,
                (current, id) =>
                    FSharpList<Value>.Cons(Value.NewContainer(dynRevitSettings.Doc.Document.GetElement(id)), current));
            results.Reverse();

            return Value.NewList(results);
        }

        private static void UpdatePlacementPoints(FamilyInstance ac, FSharpList<Value> xyzs, int j)
        {
            IList<ElementId> placePointIds =
                AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(ac);

            var pts = ((Value.List) xyzs[j]).Item;

            if (placePointIds.Count() != pts.Count())
            {
                return;
            }

            // Set the position of each placement point
            int i = 0;
            foreach (ElementId id in placePointIds)
            {
                var point = dynRevitSettings.Doc.Document.GetElement(id) as ReferencePoint;
                var pt = (XYZ) ((Value.Container) pts.ElementAt(i)).Item;
                point.Position = pt;
                i++;
            }
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
                         ? (Face) ((Value.Container) args[1]).Item
                         : (Face)dynRevitSettings.Doc.Document.GetElement(faceRef.ElementId).GetGeometryObjectFromReference(faceRef);

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
                         ? (Curve)((Value.Container)args[1]).Item
                         : (Curve)dynRevitSettings.Doc.Document.GetElement(curveRef.ElementId).GetGeometryObjectFromReference(curveRef);

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
