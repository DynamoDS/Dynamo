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
using Dynamo.Models;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
using Dynamo.Revit;

namespace Dynamo.Nodes
{
    [NodeName("To Revit Element")]
    [NodeDescription("Converts ASM Geometry to a Revit Element.")]
    [NodeCategory(BuiltinNodeCategories.CORE_EXPERIMENTAL_GEOMETRY)]
    [NodeSearchable(false)]
    public class dynASMToRevitNode : dynRevitTransactionNodeWithOneOutput
    {
        ReferencePoint _revitPoint = null;
        CurveByPoints _curveByPoints = null;

        ReferencePoint _startPoint = null;
        ReferencePoint _endPoint = null;

        public dynASMToRevitNode()
        {
            InPortData.Add(new PortData("Geometry", "Input Geometry. Should have type of Autodesk.LibG...", typeof(Value.Container)));
            OutPortData.Add(new PortData("Element", "Created Revit Element.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        private ReferencePoint ReferencePointFromPoint(Autodesk.LibG.Point point)
        {
            XYZ xyz = new XYZ(point.x(), point.y(), point.z());
            return this.UIDocument.Document.FamilyCreate.NewReferencePoint(xyz);
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Autodesk.LibG.Geometry geometry = (Autodesk.LibG.Geometry)((Value.Container)args[0]).Item;

            Autodesk.LibG.Point point = geometry as Autodesk.LibG.Point;

            if (point != null)
            {
                XYZ xyz = new XYZ(point.x(), point.y(), point.z());

                if (_revitPoint == null)
                {
                    _revitPoint = this.UIDocument.Document.FamilyCreate.NewReferencePoint(xyz);
                    this.Elements.Add(_revitPoint.Id);
                }
                else
                {
                    _revitPoint.Position = xyz;
                }

                return Value.NewContainer(_revitPoint);
            }

            Autodesk.LibG.Line line = geometry as Autodesk.LibG.Line;

            if (line != null)
            {
                ReferencePointArray refPoints = new ReferencePointArray();

                Autodesk.LibG.Point start_point = line.start_point();
                Autodesk.LibG.Point end_point = line.end_point();

                if (_curveByPoints == null)
                {
                    _startPoint = ReferencePointFromPoint(start_point);
                    _endPoint = ReferencePointFromPoint(end_point);

                    refPoints.Append(_startPoint);
                    refPoints.Append(_endPoint);

                    _curveByPoints = this.UIDocument.Document.FamilyCreate.NewCurveByPoints(refPoints);
                    this.Elements.Add(_curveByPoints.Id);
                }
                else
                {
                    _startPoint.Position = new XYZ(start_point.x(), start_point.y(), start_point.z());
                    _endPoint.Position = new XYZ(end_point.x(), end_point.y(), end_point.z());
                }

                return Value.Container.NewContainer(_curveByPoints);
            }

            //Autodesk.LibG.BSplineCurve bspline = geometry as Autodesk.LibG.BSplineCurve;
            
            //if (bspline != null)
            //{
            //    ReferencePointArray refPoints = new ReferencePointArray();

            //    for (int i = 0; i <= 15; ++i)
            //    {
            //        double param = (double)i / 15.0;
            //        Autodesk.LibG.Point p = bspline.point_at_parameter(param);
            //        ReferencePoint refPoint = ReferencePointFromPoint(p);
            //        refPoints.Append(refPoint);
            //    }

            //    if (_curveByPoints == null)
            //    {
            //        _curveByPoints = this.UIDocument.Document.FamilyCreate.NewCurveByPoints(refPoints);
            //        this.Elements.Add(_curveByPoints.Id);
            //    }
            //    else
            //    {
            //        _curveByPoints.SetPoints(refPoints);
            //    }

            //    foreach (ReferencePoint refPoint in refPoints)
            //    {
            //        refPoint.Visible = false;
            //    }

            //    return Value.Container.NewContainer(_curveByPoints);
            //}

            return Value.Container.NewContainer(null);

            // Surface

            // Solid
        }
    }

}