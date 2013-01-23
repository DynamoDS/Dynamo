//Copyright 2012 Ian Keough

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
using Expression = Dynamo.FScheme.Expression;
using Dynamo.FSchemeInterop;
using Microsoft.FSharp.Collections;

namespace Dynamo.Elements
{
    [ElementName("Surface Area")]
    [ElementCategory(BuiltinElementCategories.MEASUREMENT)]
    [ElementDescription("An element which measures the surface area of a face (f)")]
    [RequiresTransaction(true)]
    public class dynSurfaceArea : dynNode
    {
        public dynSurfaceArea()
        {
            InPortData.Add(new PortData("f", "The face whose surface area you wish to calculate (Reference).", typeof(Reference)));//Ref to a face of a form
            OutPortData = new PortData("a", "The surface area of the face (Number).", typeof(object));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            double area = 0.0;

            object arg0 = ((Expression.Container)args[0]).Item;
            if (arg0 is Reference)
            {
                Reference faceRef = arg0 as Reference;
                Face f = this.UIDocument.Document.GetElement(faceRef.ElementId).GetGeometryObjectFromReference(faceRef) as Face;
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
            return Expression.NewNumber(area);
        }
    }

    [ElementName("Surface Domain")]
    [ElementCategory(BuiltinElementCategories.MEASUREMENT)]
    [ElementDescription("An element which measures the domain of a surface in U and V.")]
    [RequiresTransaction(true)]
    public class dynSurfaceDomain : dynNode
    {
        public dynSurfaceDomain()
        {
            InPortData.Add(new PortData("f", "The surface whose domain you wish to calculate (Reference).", typeof(Reference)));//Ref to a face of a form
            OutPortData = new PortData("d", "The min, max, and dimensions of the surface domain. (List)", typeof(object));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {

            FSharpList<Expression> result = FSharpList<Expression>.Empty;
            BoundingBoxUV bbox = null;

            object arg0 = ((Expression.Container)args[0]).Item;
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

            result = FSharpList<Expression>.Cons(
                           Expression.NewNumber(bbox.Max.V - bbox.Min.V),
                           result);
            result = FSharpList<Expression>.Cons(
                           Expression.NewNumber(bbox.Max.U - bbox.Min.U),
                           result);
            result = FSharpList<Expression>.Cons(
                           Expression.NewContainer(bbox.Max),
                           result);
            result = FSharpList<Expression>.Cons(
                           Expression.NewContainer(bbox.Min),
                           result);
            

            //Fin
            return Expression.NewList(result);
        }
    }

    [ElementName("XYZ Distance")]
    [ElementCategory(BuiltinElementCategories.MEASUREMENT)]
    [ElementDescription("Returns the distance between a(XYZ) and b(XYZ).")]
    [RequiresTransaction(false)]
    public class dynXYZDistance : dynNode
    {
        public dynXYZDistance()
        {
            InPortData.Add(new PortData("a", "Start (XYZ).", typeof(object)));//Ref to a face of a form
            InPortData.Add(new PortData("b", "End (XYZ)", typeof(object)));//Ref to a face of a form
            OutPortData = new PortData("d", "The distance between the two XYZs (Number).", typeof(object));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var a = (XYZ)((Expression.Container)args[0]).Item;
            var b = (XYZ)((Expression.Container)args[1]).Item;

            return Expression.NewNumber(a.DistanceTo(b));
        }
    }


    [ElementName("Height")]
    [ElementCategory(BuiltinElementCategories.MEASUREMENT)]
    [ElementDescription("Returns the height in z of an element.")]
    [RequiresTransaction(false)]
    public class dynHeight : dynNode
    {
        public dynHeight()
        {
            InPortData.Add(new PortData("elem", "Level, Family Instance, RefPoint, XYZ", typeof(object)));//add elements here when adding switch statements 
            OutPortData = new PortData("h", "The height of an element in z relative to project 0.", typeof(object));

            base.RegisterInputsAndOutputs();
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

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var a = ((Expression.Container)args[0]).Item;

            return Expression.NewNumber(getHeight(a));
        }
    }


}


