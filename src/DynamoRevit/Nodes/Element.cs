using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Models;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;

namespace Dynamo.Nodes
{
    [NodeName("Element Geometry Objects")]
    [NodeCategory(BuiltinNodeCategories.REVIT_BAKE)]
    [NodeDescription("Creates list of geometry object references in the element.")]
    public class ElementGeometryObjects : NodeWithOneOutput
    {
        List<GeometryObject> instanceGeometryObjects;

        public ElementGeometryObjects()
        {
            InPortData.Add(new PortData("element", "element to create geometrical references to", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("list", "Geometry objects of the element", typeof(FScheme.Value.List)));

            RegisterAllPorts();

            instanceGeometryObjects = null;
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            Element thisElement = (Element)((FScheme.Value.Container)args[0]).Item;

            instanceGeometryObjects = new List<GeometryObject>();

            var result = FSharpList<FScheme.Value>.Empty;

            Autodesk.Revit.DB.Options geoOptionsOne = new Autodesk.Revit.DB.Options();
            geoOptionsOne.ComputeReferences = true;

            GeometryObject geomObj = thisElement.get_Geometry(geoOptionsOne);
            GeometryElement geomElement = geomObj as GeometryElement;

            if ((thisElement is GenericForm) && (geomElement.Count() < 1))
            {
                GenericForm gF = (GenericForm)thisElement;
                if (!gF.Combinations.IsEmpty)
                {
                    Autodesk.Revit.DB.Options geoOptionsTwo = new Autodesk.Revit.DB.Options();
                    geoOptionsTwo.IncludeNonVisibleObjects = true;
                    geoOptionsTwo.ComputeReferences = true;
                    geomObj = thisElement.get_Geometry(geoOptionsTwo);
                    geomElement = geomObj as GeometryElement;
                }
            }

            foreach (GeometryObject geob in geomElement)
            {
                GeometryInstance ginsta = geob as GeometryInstance;
                if (ginsta != null)
                {
                    GeometryElement instanceGeom = ginsta.GetInstanceGeometry();
                    instanceGeometryObjects.Add(instanceGeom);
                    foreach (GeometryObject geobInst in instanceGeom)
                    {
                        result = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewContainer(geobInst), result);
                    }
                }
                else
                {
                    result = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewContainer(geob), result);
                }
            }

            return FScheme.Value.NewList(result);
        }
    }

    [NodeName("Height")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_MEASURE)]
    [NodeDescription("Returns the height in z of an element.")]
    public class Height : MeasurementBase
    {
        public Height()
        {
            InPortData.Add(new PortData("elem", "Level, Family Instance, RefPoint, XYZ", typeof(FScheme.Value.Container)));//add elements here when adding switch statements 
            OutPortData.Add(new PortData("h", "The height of an element in z relative to project 0.", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        private static double getHeight(object elem)
        {
            double h = 0;

            if (elem is Autodesk.Revit.DB.Level)
            {
                h = ((Autodesk.Revit.DB.Level)elem).Elevation;
                return h;
            }
            else if (elem is ReferencePoint)
            {
                h = ((ReferencePoint)elem).Position.Z;
                return h;
            }
            else if (elem is FamilyInstance)
            {
                LocationPoint loc = (LocationPoint)((FamilyInstance)elem).Location;
                h = loc.Point.Z;
                return h;
            }
            else if (elem is XYZ)
            {
                h = ((XYZ)elem).Z;
                return h;
            }
            else
            {
                return h;
            }

        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var a = ((FScheme.Value.Container)args[0]).Item;

            return FScheme.Value.NewContainer(Units.Length.FromFeet(getHeight(a)));
        }
    }
}
