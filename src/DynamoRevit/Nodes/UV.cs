using System.Linq;
using System.Xml;
using Autodesk.DesignScript.Geometry;
using Dynamo.Models;
using Microsoft.FSharp.Collections;
using RevitGeometryObjects = Revit.Geometry;

namespace Dynamo.Nodes
{
    [NodeName("UV")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SURFACE_UV)]
    [NodeDescription("Creates a UV from two double values.")]
    public class Uv : GeometryBase
    {
        public Uv()
        {
            InPortData.Add(new PortData("U", "U", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("V", "V", typeof(FScheme.Value.Number)));
            OutPortData.Add(new PortData("uv", "UV", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            double u, v;
            u = ((FScheme.Value.Number)args[0]).Item;
            v = ((FScheme.Value.Number)args[1]).Item;

            return FScheme.Value.NewContainer(new Autodesk.Revit.DB.UV(u, v));
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "UV.ByCoordinates",
                "UV.ByCoordinates@double,double");
        }
    }

    [NodeName("UV Domain")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SURFACE_UV)]
    [NodeDescription("Create a two dimensional domain specifying the Minimum and Maximum UVs.")]
    public class Domain2D : NodeWithOneOutput
    {
        public Domain2D()
        {
            InPortData.Add(new PortData("min", "The minimum UV of the domain.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("max", "The maximum UV of the domain.", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("domain", "A domain.", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var min = (Autodesk.Revit.DB.UV)((FScheme.Value.Container)args[0]).Item;
            var max = (Autodesk.Revit.DB.UV)((FScheme.Value.Container)args[1]).Item;

            var vmax = Vector.ByCoordinates(max.U, max.V,0);
            var vmin = Vector.ByCoordinates(min.U, min.V,0);

            return FScheme.Value.NewContainer(RevitGeometryObjects.Domain2D.ByMinimumAndMaximum(vmin, vmax));
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll",
                "Domain2D.ByMinimumAndMaximum", "Domain2D.ByMinimumAndMaximum@Vector,Vector");
        }
    }

    [NodeName("UV Grid")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SURFACE_UV)]
    [NodeDescription("Creates a grid of UVs from a domain.")]
    [NodeSearchTags("points", "array", "collection", "field", "uv")]
    public class UvGrid : NodeWithOneOutput
    {
        public UvGrid()
        {
            InPortData.Add(new PortData("domain", "A two dimensional domain.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("U-count", "Number in the U direction.", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("V-count", "Number in the V direction.", typeof(FScheme.Value.Number)));
            OutPortData.Add(new PortData("UVs", "List of UVs in the grid", typeof(FScheme.Value.List)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var domain = (RevitGeometryObjects.Domain2D)((FScheme.Value.Container)args[0]).Item;
            double ui = ((FScheme.Value.Number)args[1]).Item;
            double vi = ((FScheme.Value.Number)args[2]).Item;
            double us = domain.USpan / ui;
            double vs = domain.VSpan / vi;

            FSharpList<FScheme.Value> result = FSharpList<FScheme.Value>.Empty;

            for (int i = 0; i <= ui; i++)
            {
                double u = domain.Min.X + i * us;

                for (int j = 0; j <= vi; j++)
                {
                    double v = domain.Min.Y + j * vs;

                    result = FSharpList<FScheme.Value>.Cons(
                        FScheme.Value.NewContainer(new Autodesk.Revit.DB.UV(u, v)),
                        result
                    );
                }
            }

            return FScheme.Value.NewList(
               ListModule.Reverse(result)
            );
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement dummyNode = MigrationManager.CreateDummyNode(oldNode, 3, 1);
            migrationData.AppendNode(dummyNode);

            return migrationData;
        }
    }

    [NodeName("UV Random")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SURFACE_UV)]
    [NodeDescription("Creates a random collection of UVs from a domain.")]
    [NodeSearchTags("field")]
    public class UvRandom : NodeWithOneOutput
    {
        public UvRandom()
        {
            InPortData.Add(new PortData("dom", "A domain.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("U-count", "Number in the U direction.", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("V-count", "Number in the V direction.", typeof(FScheme.Value.Number)));
            OutPortData.Add(new PortData("UVs", "List of UVs in the grid", typeof(FScheme.Value.List)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            double ui, vi;

            var domain = (RevitGeometryObjects.Domain2D)((FScheme.Value.Container)args[0]).Item;
            ui = ((FScheme.Value.Number)args[1]).Item;
            vi = ((FScheme.Value.Number)args[2]).Item;

            FSharpList<FScheme.Value> result = FSharpList<FScheme.Value>.Empty;

            //UV min = ((Value.Container)domain[0]).Item as UV;
            //UV max = ((Value.Container)domain[1]).Item as UV;

            var min = new Autodesk.Revit.DB.UV(domain.Min.X, domain.Min.Y);
            var max = new Autodesk.Revit.DB.UV(domain.Max.X, domain.Max.Y);

            var r = new System.Random();
            double uSpan = max.U - min.U;
            double vSpan = max.V - min.V;

            for (int i = 0; i < ui; i++)
            {
                for (int j = 0; j < vi; j++)
                {
                    result = FSharpList<FScheme.Value>.Cons(
                        FScheme.Value.NewContainer(new Autodesk.Revit.DB.UV(min.U + r.NextDouble() * uSpan, min.V + r.NextDouble() * vSpan)),
                        result
                    );
                }
            }

            return FScheme.Value.NewList(
               ListModule.Reverse(result)
            );
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement dummyNode = MigrationManager.CreateDummyNode(oldNode, 3, 1);
            migrationData.AppendNode(dummyNode);

            return migrationData;
        }
    }
}
