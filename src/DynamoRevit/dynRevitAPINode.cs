using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.DB;
using Autodesk.Revit;
using Dynamo.Controls;
using Dynamo.Utilities;
using Dynamo.Connectors;
using Dynamo.Revit;

using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

using Dynamo.FSchemeInterop;
using Dynamo.FSchemeInterop.Node;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;

namespace Dynamo.Nodes
{
    /// <summary>
    /// Base class for all auto-generated Revit API nodes.
    /// </summary>
    public abstract class dynRevitAPINode : dynRevitTransactionNodeWithOneOutput, IDrawable
    {
        protected Type base_type;
        protected Type return_type;
        protected MethodBase mi;
        protected ParameterInfo[] pi;

        ///<summary>
        ///Default constructor
        ///</summary>
        public dynRevitAPINode()
        {

        }

        ///<summary>
        ///Auto-generated evaulate method for Dynamo node wrapping Autodesk.Revit.Creation.FamilyItemFactory.NewRadialDimension
        ///</summary>
        public override Value Evaluate(FSharpList<Value> args)
        {
            foreach (var e in this.Elements)
            {
                this.DeleteElement(e);
            }

            Value result = dynRevitUtils.InvokeAPIMethod(this, args, base_type, pi, mi, return_type);

            if (result.IsContainer)
            {
                RenderDescription rd = Draw((result as Value.Container).Item);

                if (dynSettings.Controller.CurrentSpace.RenderData.ContainsKey(this.NodeUI.GUID))
                {
                    dynSettings.Controller.CurrentSpace.RenderData[this.NodeUI.GUID].Add(rd);
                }
                else
                {
                    dynSettings.Controller.CurrentSpace.RenderData.Add(this.NodeUI.GUID, new List<RenderDescription>{rd});
                }
                
            }

            return result;
        }

        private RenderDescription DrawUndrawable(object obj)
        {
            return new RenderDescription();
        }

        private RenderDescription DrawReferencePoint(object obj)
        {
            RenderDescription description = new RenderDescription();
            ReferencePoint point = obj as ReferencePoint;
            description.points.Add(new Point3D(point.GetCoordinateSystem().Origin.X,
                point.GetCoordinateSystem().Origin.Y,
                point.GetCoordinateSystem().Origin.Z));

            return description;
        }

        private RenderDescription DrawXYZ(object obj)
        {
            RenderDescription description = new RenderDescription();
            XYZ point = obj as XYZ;
            description.points.Add(new Point3D(point.X, point.Y, point.Z));

            return description;
        }

        private RenderDescription DrawCurve(object obj)
        {
            Autodesk.Revit.DB.Curve curve = obj as Autodesk.Revit.DB.Curve;

            IList<XYZ> points = curve.Tessellate();

            RenderDescription description = new RenderDescription();

            foreach (XYZ xyz in points)
            {
                description.lines.Add(new Point3D(xyz.X, xyz.Y, xyz.Z));
            }

            return description;
        }

        private RenderDescription DrawCurveElement(object obj)
        {
            Autodesk.Revit.DB.CurveElement elem = obj as Autodesk.Revit.DB.CurveElement;

            return DrawCurve(elem.GeometryCurve);
        }

        public RenderDescription Draw(object obj)
        {
            if (typeof(Autodesk.Revit.DB.Curve).IsAssignableFrom(obj.GetType()))
            {
                return DrawCurve(obj);
            }
            else if (typeof(Autodesk.Revit.DB.CurveElement).IsAssignableFrom(obj.GetType()))
            {
                return DrawCurveElement(obj);
            }
            else if (typeof(Autodesk.Revit.DB.ReferencePoint).IsAssignableFrom(obj.GetType()))
            {
                return DrawReferencePoint(obj);
            }
            else if (typeof(Autodesk.Revit.DB.XYZ).IsAssignableFrom(obj.GetType()))
            {
                return DrawXYZ(obj);
            }
            else
            {
                return DrawUndrawable(obj);
            }
        }

    }

    /// <summary>
    /// Revit Document node. Returns the active Revit Document.
    /// </summary>
    [NodeName("Revit Document")]
    [NodeSearchTags("document", "active")]
    [NodeCategory(BuiltinNodeCategories.REVIT)]
    [NodeDescription("Gets the active Revit document.")]
    public class dynRevitDocument : dynRevitTransactionNodeWithOneOutput
    {
        public dynRevitDocument()
        {
            OutPortData.Add(new PortData("doc", "The active Revit doc.", typeof(Autodesk.Revit.DB.Document)));
            NodeUI.RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewContainer(dynRevitSettings.Doc.Document);
        }
    }
}
