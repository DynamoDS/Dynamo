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

using Autodesk.Revit.DB;

using Dynamo.Connectors;

using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
using Dynamo.Utilities;
using System.Windows.Media.Media3D;

namespace Dynamo.Nodes
{
    [NodeName("Evaluate Normal")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_SURFACE)]
    [NodeDescription("Evaluate a point on a face to find the normal.")]
    class dynNormalEvaluate: dynXYZBase
    {
        public dynNormalEvaluate()
        {
            InPortData.Add(new PortData("uv", "The point to evaluate.", typeof(Value.Container)));
            InPortData.Add(new PortData("face", "The face to evaluate.", typeof(Value.Container)));
            OutPortData.Add(new PortData("XYZ", "The normal.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Reference faceRef = (args[1] as Value.Container).Item as Reference;
            Autodesk.Revit.DB.Face f = (faceRef == null)? ((args[1] as Value.Container).Item as Autodesk.Revit.DB.Face) : dynRevitSettings.Doc.Document.GetElement(faceRef).GetGeometryObjectFromReference(faceRef) as Autodesk.Revit.DB.Face;

            XYZ norm = null;
            
            if (f != null)
            {
                //each item in the list will be a reference point
                UV uv = (UV)(args[0] as Value.Container).Item;
                norm = f.ComputeNormal(uv);
            }

            pts.Add(norm);

            return Value.NewContainer(norm);
        }
    }

    [NodeName("Evaluate UV")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_SURFACE)]
    [NodeDescription("Evaluate a parameter(UV) on a face to find the XYZ location.")]
    class dynXYZEvaluate : dynXYZBase
    {
        public dynXYZEvaluate()
        {
            InPortData.Add(new PortData("uv", "The point to evaluate.", typeof(Value.Container)));
            InPortData.Add(new PortData("face", "The face to evaluate.", typeof(Value.Container)));
            OutPortData.Add(new PortData("XYZ", "The location.", typeof(Value.Container)));
            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Reference faceRef = (args[1] as Value.Container).Item as Reference;
            Autodesk.Revit.DB.Face f = (faceRef == null) ? ((args[1] as Value.Container).Item as Autodesk.Revit.DB.Face) : dynRevitSettings.Doc.Document.GetElement(faceRef).GetGeometryObjectFromReference(faceRef) as Autodesk.Revit.DB.Face;

            XYZ face_point = null;

            if (f != null)
            {
                //each item in the list will be a reference point
                UV param = (UV)(args[0] as Value.Container).Item;
                face_point = f.Evaluate(param);
            }

            pts.Add(face_point);

            return Value.NewContainer(face_point);
        }
    }

    [NodeName("Compute Face Derivatives")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_SURFACE)]
    [NodeDescription("Returns a transform describing the face (f) at the parameter (uv).")]
    public class dynComputeFaceDerivatives : dynTransformBase
    {
        public dynComputeFaceDerivatives()
        {
            InPortData.Add(new PortData("f", "The face to evaluate(Face)", typeof(Value.Container)));
            InPortData.Add(new PortData("uv", "The parameter to evaluate(UV)", typeof(Value.Container)));
            OutPortData.Add(new PortData("t", "Transform describing the face at the parameter(Transform)", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var faceRef = ((Value.Container)args[0]).Item as Reference;
            var uv = (UV)((Value.Container)args[1]).Item;

            Transform t = Transform.Identity;

            Autodesk.Revit.DB.Face f = (faceRef == null) ? 
                ((Autodesk.Revit.DB.Face)((Value.Container)args[0]).Item) : 
                (dynRevitSettings.Doc.Document.GetElement(faceRef.ElementId).GetGeometryObjectFromReference(faceRef) as Autodesk.Revit.DB.Face);

            if (f != null)
            {
                t = f.ComputeDerivatives(uv);
                t.BasisX = t.BasisX.Normalize();
                t.BasisZ = t.BasisZ.Normalize();
                t.BasisY = t.BasisX.CrossProduct(t.BasisZ);
            }

            transforms.Add(t);

            return Value.NewContainer(t);
        }

    }

    [NodeName("Compute Curve Derivatives")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_SURFACE)]
    [NodeDescription("Returns a transform describing the face (f) at the parameter (uv).")]
    public class dynComputeCurveDerivatives : dynTransformBase
    {
        public dynComputeCurveDerivatives()
        {
            InPortData.Add(new PortData("crv", "The curve to evaluate", typeof(Value.Container)));
            InPortData.Add(new PortData("p", "The parameter to evaluate", typeof(Value.Container)));
            OutPortData.Add(new PortData("t", "Transform describing the curve at the parameter(Transform)", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Curve crv = (Curve)((Value.Container)args[0]).Item;
            double p = (double)((Value.Number)args[1]).Item;

            Transform t = Transform.Identity;

            if (crv != null)
            {
                t = crv.ComputeDerivatives(p,true);
                t.BasisX = t.BasisX.Normalize();
                t.BasisZ = t.BasisZ.Normalize();
                t.BasisY = t.BasisX.CrossProduct(t.BasisZ);
            }

            transforms.Add(t);

            return Value.NewContainer(t);
        }

    }

}
