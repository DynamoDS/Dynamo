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

            Face f = dynRevitSettings.Doc.Document.GetElement(faceRef).GetGeometryObjectFromReference(faceRef) as Face;
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

            Face f = dynRevitSettings.Doc.Document.GetElement(faceRef).GetGeometryObjectFromReference(faceRef) as Face;
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

}
