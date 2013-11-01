using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Models;
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
}
