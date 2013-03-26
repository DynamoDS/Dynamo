using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.Revit.DB;

using Microsoft.FSharp.Collections;
using Expression = Dynamo.FScheme.Expression;
using Value = Dynamo.FScheme.Value;

namespace Dynamo.Utilities
{
    public static class dynRevitUtils
    {
        public static ReferenceArrayArray ConvertFSharpListListToReferenceArrayArray(FSharpList<Value> lstlst)
        {
            ReferenceArrayArray refArrArr = new ReferenceArrayArray();
            foreach (Value v in lstlst)
            {
                ReferenceArray refArr = new ReferenceArray();
                FSharpList<Value> lst = (v as Value.List).Item;

                AddReferencesToArray(refArr, lst);

                refArrArr.Append(refArr);
            }

            return refArrArr;
        }

        private static void AddReferencesToArray(ReferenceArray refArr, FSharpList<Value> lst)
        {
            dynRevitSettings.Doc.RefreshActiveView();

            foreach (Value vInner in lst)
            {
                var mc = (vInner as Value.Container).Item as ModelCurve;
                var f = (vInner as Value.Container).Item as Face;
                var p = (vInner as Value.Container).Item as Point;
                var c = (vInner as Value.Container).Item as Curve;
                var rp = (vInner as Value.Container).Item as ReferencePlane;
                
                if (mc != null)
                    refArr.Append(mc.GeometryCurve.Reference);
                else if (f != null)
                    refArr.Append(f.Reference);
                else if (p != null)
                    refArr.Append(p.Reference);
                else if (c != null)
                    refArr.Append(c.Reference);
                else if (c != null)
                    refArr.Append(rp.Reference);
            }
            
        }

        public static ReferenceArray ConvertFSharpListListToReferenceArray(FSharpList<Value> lstlst)
        {
            ReferenceArray refArr = new ReferenceArray();

            AddReferencesToArray(refArr, lstlst);
            
            return refArr;

        }

        public static CurveArrArray ConvertFSharpListListToCurveArrayArray(FSharpList<Value> lstlst)
        {
            CurveArrArray crvArrArr = new CurveArrArray();
            foreach (Value v in lstlst)
            {
                CurveArray crvArr = new CurveArray();
                FSharpList<Value> lst = (v as Value.List).Item;

                AddCurvesToArray(crvArr, lst);

                crvArrArr.Append(crvArr);
            }

            return crvArrArr;
        }

        public static CurveArray ConvertFSharpListListToCurveArray(FSharpList<Value> lstlst)
        {
            CurveArray crvArr = new CurveArray();

            AddCurvesToArray(crvArr, lstlst);

            return crvArr;

        }

        private static void AddCurvesToArray(CurveArray crvArr, FSharpList<Value> lst)
        {
            dynRevitSettings.Doc.RefreshActiveView();

            foreach (Value vInner in lst)
            {
                var c = (vInner as Value.Container).Item as Curve;
                crvArr.Append(c);
            }

        }

    }

    /// <summary>
    /// Used with the Auto-generator. Allows automatic conversion between input types.
    /// </summary>
    public static class DynamoRevitTypeConverter
    {
        public static object Convert(object input, object output)
        {
            #region ModelCurve
            if (input.GetType() ==  typeof(ModelCurve))
            {
                if (output.GetType() == typeof(Curve))
                {
                    return ((ModelCurve)input).GeometryCurve;
                }
            }
            #endregion

            #region SketchPlane
            if (input.GetType() == typeof(SketchPlane))
            {
                SketchPlane a = (SketchPlane)input;

                if (output.GetType() == typeof(Plane))
                {
                    return a.Plane;
                }
                else if (output.GetType() == typeof(ReferencePlane))
                {
                    return a.Plane; 
                }
                else if (output.GetType() == typeof(string))
                {
                    return string.Format("{0},{1},{2},{3},{4},{5}", a.Plane.Origin.X, a.Plane.Origin.Y, a.Plane.Origin.Z, 
                        a.Plane.Normal.X, a.Plane.Normal.Y, a.Plane.Normal.Z);
                }
            }
            #endregion

            #region Point
            if (input.GetType() == typeof(Point))
            {
                Point a = (Point)input;

                if (output.GetType() == typeof(XYZ))
                {
                    return a.Coord;
                }
                else if (output.GetType() == typeof(string))
                {
                    return string.Format("{0},{1},{2}", a.Coord.X, a.Coord.Y, a.Coord.Z);
                }
            }
            #endregion

            #region ReferencePoint
            if (input.GetType() == typeof(ReferencePoint))
            {
                ReferencePoint a = (ReferencePoint)input;

                if (output == typeof(XYZ))
                {
                    return a.Position;
                }
                else if (output.GetType() == typeof(Reference))
                {
                    return a.GetCoordinatePlaneReferenceXY();
                }
                else if (output.GetType() == typeof(Transform))
                {
                    return a.GetCoordinateSystem();
                }
                else if (output.GetType() == typeof(string))
                {
                    return string.Format("{0},{1},{2}", a.Position.X, a.Position.Y, a.Position.Z);
                }

            }
            #endregion

            //return the input by default
            return input;
        }
    }

    /// <summary>
    /// Used with the Auto-generator. Allows automatic conversion of outputs to FScheme.Value Types
    /// </summary>
    public static class DynamoOutputTypeConverter
    {
        public static Value Convert(object input)
        {
            if (input.GetType() == typeof(double))
            {
                return Value.NewNumber(System.Convert.ToDouble(input));
            }
            else if (input.GetType() == typeof(int))
            {
                return Value.NewNumber(System.Convert.ToDouble(input));
            }
            else if (input.GetType() == typeof(string))
            {
                return Value.NewString(System.Convert.ToString(input));
            }
            else if (input.GetType() == typeof(bool))
            {
                return Value.NewNumber(System.Convert.ToInt16(input));
            }
            else
            {
                return Value.NewContainer(input);
            }
        }
    }
}
