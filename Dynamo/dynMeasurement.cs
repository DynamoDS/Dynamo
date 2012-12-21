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
    [ElementDescription("An element which measures the surface area of a face")]
    [RequiresTransaction(true)]
    public class dynSurfaceArea : dynNode
    {
        public dynSurfaceArea()
        {
            InPortData.Add(new PortData("face", "Ref", typeof(Reference)));//Ref to a face of a form
            OutPortData = new PortData("area", "The surface area of the face.", typeof(object));

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
            InPortData.Add(new PortData("face", "Ref", typeof(Reference)));//Ref to a face of a form
            OutPortData = new PortData("dom", "The min, max, and dimensions of the surface domain.", typeof(object));

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
}