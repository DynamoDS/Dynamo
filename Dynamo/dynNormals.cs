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

using Autodesk.Revit.DB;

using Dynamo.Connectors;

using Microsoft.FSharp.Collections;
using Expression = Dynamo.FScheme.Expression;
using Dynamo.Utilities;

namespace Dynamo.Nodes
{
    [ElementName("Evaluate Normal")]
    [ElementCategory(BuiltinElementCategories.MISC)]
    [ElementDescription("Evaluate a point on a face to find the normal.")]
    class dynNormalEvaluate : dynNode
    {
        public dynNormalEvaluate()
        {
            InPortData.Add(new PortData("pt", "The point to evaluate.", typeof(object)));
            InPortData.Add(new PortData("face", "The face to evaluate.", typeof(object)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("XYZ", "The normal.", typeof(string));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            Reference faceRef = (args[1] as Expression.Container).Item as Reference;

            Face f = dynSettings.Instance.Doc.Document.GetElement(faceRef).GetGeometryObjectFromReference(faceRef) as Face;
            XYZ norm = null;

            if (f != null)
            {
                //each item in the list will be a reference point
                ReferencePoint rp = (args[0] as Expression.Container).Item as ReferencePoint;

                if (rp != null)
                {
                    PointOnFace pof = rp.GetPointElementReference() as PointOnFace;

                    if (pof != null)
                    {
                        norm = f.ComputeNormal(pof.UV);
                    }
                }
            }

            return Expression.NewContainer(norm);
        }
    }

    [ElementName("Evaluate XYZ")]
    [ElementCategory(BuiltinElementCategories.MISC)]
    [ElementDescription("Evaluate a point on a face to find the XYZ location.")]
    class dynXYZEvaluate : dynNode
    {
        public dynXYZEvaluate()
        {
            InPortData.Add(new PortData("pt", "The point to evaluate.", typeof(object)));
            InPortData.Add(new PortData("face", "The face to evaluate.", typeof(object)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("XYZ", "The location.", typeof(string));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            Reference faceRef = (args[1] as Expression.Container).Item as Reference;

            Face f = dynSettings.Instance.Doc.Document.GetElement(faceRef).GetGeometryObjectFromReference(faceRef) as Face;
            XYZ loc = null;

            if (f != null)
            {
                //each item in the list will be a reference point
                ReferencePoint rp = (args[0] as Expression.Container).Item as ReferencePoint;

                if (rp != null)
                {
                    PointOnFace pof = rp.GetPointElementReference() as PointOnFace;

                    if (pof != null)
                    {
                        loc = f.Evaluate(pof.UV);
                    }
                }
            }

            return Expression.NewContainer(loc);
        }
    }

}
