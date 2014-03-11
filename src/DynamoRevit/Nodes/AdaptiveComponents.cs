using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Dynamo.Models;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
using Dynamo.Revit;
using RevitServices.Persistence;

namespace Dynamo.Nodes
{
    [NodeName("Adaptive Component by XYZs")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILIES)]
    [NodeDescription("Given a list of XYZs and a family type, creates an adaptive component at that location.")]
    public class AdaptiveComponentByPoints : RevitTransactionNodeWithOneOutput
    {
        public override Value Evaluate(FSharpList<Value> args)
        {
            throw new NotImplementedException();
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll",
                "AdaptiveComponent.ByPoints", "AdaptiveComponent.ByPoints@Point[],FamilySymbol");
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
                        if (instance.Symbol.Id != symbol.Id)
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
            
            ICollection<ElementId> ids = new List<ElementId>();

            if (instData.Any())
            {
                var document = DocumentManager.Instance.CurrentUIDocument.Document;
                if (document.IsFamilyDocument)
                {
                    ids = document.FamilyCreate.NewFamilyInstances2(instData);
                }
                else
                {
                    ids = document.Create.NewFamilyInstances2(instData);
                }
                if (ids.Count > 0)
                {
                    //hack to force regeneration. There should be a better way!!
                    XYZ moveVec = new XYZ(0.0, 0.0, 0.0);
                    ElementTransformUtils.MoveElement(this.UIDocument.Document, ids.ElementAt(0), moveVec);

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
                foreach (var eId in ids)
                {
                    DeleteElement(eId);
                }
                throw new Exception("There are more adaptive component instances than there are points to adjust.");
            }
            try
            {
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
            }
            catch (Exception ex)
            {
                foreach (var eId in ids)
                {
                    DeleteElement(eId);
                }
                throw ex;
            }
            sw.Stop();
            Debug.WriteLine(string.Format("{0} elapsed for updating remaining instance locations.", sw.Elapsed));

            //add all of the instances
            results = Elements.Aggregate(results,
                (current, id) =>
                    FSharpList<Value>.Cons(Value.NewContainer(DocumentManager.Instance.CurrentUIDocument.Document.GetElement(id)), current));
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
                var document = DocumentManager.Instance.CurrentUIDocument.Document;
                var point = document.GetElement(id) as ReferencePoint;
                var pt = (XYZ) ((Value.Container) pts.ElementAt(i)).Item;
                point.Position = pt;
                i++;
            }
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll",
                "AdaptiveComponent.ByPoints", "AdaptiveComponent.ByPoints@Point[],FamilySymbol");
        }
    }

    [NodeName("Adaptive Component by UVs on Face")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILIES)]
    [NodeDescription("Given a list of XYZs and a family type, creates an adaptive component at that location on the face.")]
    public class AdaptiveComponentByUvsOnFace : RevitTransactionNodeWithOneOutput
    {
        public override Value Evaluate(FSharpList<Value> args)
        {
            throw new NotImplementedException();
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll",
                "AdaptiveComponent.ByPointsOnFace",
                "AdaptiveComponent.ByPointsOnFace@double[][],Face,FamilySymbol");
        }
    }

    [NodeName("Adaptive Component by Parameter on Curve")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILIES)]
    [NodeDescription("Given a list of parameters and a family type, creates an adaptive component at that location on the curve.")]
    public class AdaptiveComponentByParametersOnCurve : RevitTransactionNodeWithOneOutput
    {
        public override Value Evaluate(FSharpList<Value> args)
        {
            throw new NotImplementedException();
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll",
                "AdaptiveComponent.ByPointsOnCurve",
                "AdaptiveComponent.ByPointsOnCurve@double[],Curve,FamilySymbol");
        }
    }
}
