//Copyright 2013 Ian Keough

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Dynamo.Connectors;
using Value = Dynamo.FScheme.Value;
using Dynamo.FSchemeInterop;
using Microsoft.FSharp.Collections;
using Dynamo.Utilities;
using Dynamo.Revit;

namespace Dynamo.Nodes
{
    [NodeName("Surface Area")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_MEASURE)]
    [NodeDescription("An element which measures the surface area of a face (f)")]
    public class dynSurfaceArea: dynNodeWithOneOutput
    {
        public dynSurfaceArea()
        {
            InPortData.Add(new PortData("f", "The face whose surface area you wish to calculate (Reference).", typeof(Value.Container)));//Ref to a face of a form
            OutPortData.Add(new PortData("a", "The surface area of the face (Number).", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            double area = 0.0;

            object arg0 = ((Value.Container)args[0]).Item;
            if (arg0 is Reference)
            {
                Reference faceRef = arg0 as Reference;
                Face f = dynRevitSettings.Doc.Document.GetElement(faceRef.ElementId).GetGeometryObjectFromReference(faceRef) as Face;
                if (f != null)
                {
                    area = f.Area;
                }
            }
            else
            {
                throw new Exception("Cannot cast first argument to Face.");
            }

            //Fin
            return Value.NewNumber(area);
        }
    }

    [NodeName("Surface Domain")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_MEASURE)]
    [NodeDescription("An element which measures the domain of a surface in U and V.")]
    public class dynSurfaceDomain : dynRevitTransactionNodeWithOneOutput
    {
        public dynSurfaceDomain()
        {
            InPortData.Add(new PortData("f", "The surface whose domain you wish to calculate (Reference).", typeof(Value.Container)));//Ref to a face of a form
            OutPortData.Add(new PortData("d", "The min, max, and dimensions of the surface domain. (List)", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            FSharpList<Value> result = FSharpList<Value>.Empty;
            BoundingBoxUV bbox = null;

            object arg0 = ((Value.Container)args[0]).Item;
            if (arg0 is Reference)
            {
                Reference faceRef = arg0 as Reference;
                Face f = this.UIDocument.Document.GetElement(faceRef.ElementId).GetGeometryObjectFromReference(faceRef) as Face;
                if (f != null)
                {
                    bbox = f.GetBoundingBox();
                }
            }
            else
            {
                throw new Exception("Cannot cast first argument to Face.");
            }

            result = FSharpList<Value>.Cons(
                           Value.NewNumber(bbox.Max.V - bbox.Min.V),
                           result);
            result = FSharpList<Value>.Cons(
                           Value.NewNumber(bbox.Max.U - bbox.Min.U),
                           result);
            result = FSharpList<Value>.Cons(
                           Value.NewContainer(bbox.Max),
                           result);
            result = FSharpList<Value>.Cons(
                           Value.NewContainer(bbox.Min),
                           result);
            
            //Fin
            return Value.NewList(result);
        }
    }

    [NodeName("XYZ Distance")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_MEASURE)]
    [NodeDescription("Returns the distance between a(XYZ) and b(XYZ).")]
    public class dynXYZDistance: dynNodeWithOneOutput
    {
        public dynXYZDistance()
        {
            InPortData.Add(new PortData("a", "Start (XYZ).", typeof(Value.Container)));//Ref to a face of a form
            InPortData.Add(new PortData("b", "End (XYZ)", typeof(Value.Container)));//Ref to a face of a form
            OutPortData.Add(new PortData("d", "The distance between the two XYZs (Number).", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var a = (XYZ)((Value.Container)args[0]).Item;
            var b = (XYZ)((Value.Container)args[1]).Item;

            return Value.NewNumber(a.DistanceTo(b));
        }
    }

    [NodeName("Height")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_MEASURE)]
    [NodeDescription("Returns the height in z of an element.")]
    public class dynHeight: dynNodeWithOneOutput
    {
        public dynHeight()
        {
            InPortData.Add(new PortData("elem", "Level, Family Instance, RefPoint, XYZ", typeof(Value.Container)));//add elements here when adding switch statements 
            OutPortData.Add(new PortData("h", "The height of an element in z relative to project 0.", typeof(Value.Number)));

            RegisterAllPorts();
        }

        private static double getHeight(object elem)
        {
            double h = 0;

            if (elem is Level)
            {
                h = ((Level)elem).Elevation;
                return h;
            }
            else if (elem is ReferencePoint)
            {
                h = ((ReferencePoint)elem).Position.Z;
                return h;
            }
            else if (elem is FamilyInstance)
            {
                LocationPoint loc =  (LocationPoint)((FamilyInstance)elem).Location;
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

        public override Value Evaluate(FSharpList<Value> args)
        {
            var a = ((Value.Container)args[0]).Item;

            return Value.NewNumber(getHeight(a));
        }
    }
}


