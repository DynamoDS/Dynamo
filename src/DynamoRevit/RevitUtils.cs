using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;

using System.Windows.Media.Media3D;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Dynamo.Models;
using Autodesk.Revit.Creation;
using Dynamo.Nodes;
using Dynamo.Revit;

using Autodesk.Revit.DB;
using Dynamo.FSchemeInterop;

using Microsoft.FSharp.Collections;
using RevitServices.Persistence;
using CurveLoop = Autodesk.Revit.DB.CurveLoop;
using DividedSurface = Autodesk.Revit.DB.DividedSurface;
using Document = Autodesk.Revit.Creation.Document;
using Face = Autodesk.Revit.DB.Face;
using HermiteSpline = Autodesk.Revit.DB.HermiteSpline;
using ModelCurve = Autodesk.Revit.DB.ModelCurve;
using ModelText = Autodesk.Revit.DB.ModelText;
using Plane = Autodesk.Revit.DB.Plane;
using ReferencePlane = Autodesk.Revit.DB.ReferencePlane;
using SketchPlane = Autodesk.Revit.DB.SketchPlane;
using Value = Dynamo.FScheme.Value;

namespace Dynamo.Utilities
{
    public static class dynRevitUtils
    {

        /// <summary>
        /// Utility method used with auto-generated assets for storing ElementIds generated during evaluate.
        /// Handles conversion of el
        /// </summary>
        /// <param name="node"></param>
        /// <param name="result"></param>
        public static void StoreElements(RevitTransactionNode node, List<object> results)
        {
            foreach (object result in results)
            {
                if (typeof(Element).IsAssignableFrom(result.GetType()))
                {
                    node.Elements.Add(((Element)result).Id);
                }
                else if (typeof(ElementId).IsAssignableFrom(result.GetType()))
                {
                    node.Elements.Add((ElementId)result);
                }
                else if (typeof(List<Element>).IsAssignableFrom(result.GetType()))
                {
                    ((List<Element>)result).ForEach(x => node.Elements.Add(((Element)x).Id));
                }
                else if (typeof(List<ElementId>).IsAssignableFrom(result.GetType()))
                {
                    ((List<ElementId>)result).ForEach(x => node.Elements.Add((ElementId)x));
                }
            }
        }

        public static MethodBase GetAPIMethodInfo(Type base_type, string methodName, bool isConstructor, Type[] types, out Type returnType)
        {
            MethodBase result = null;
            returnType = base_type;

            if (isConstructor)
            {
                result = base_type.GetConstructor(types);
            }
            else
            {
                try
                {

                    //http://stackoverflow.com/questions/11443707/getproperty-reflection-results-in-ambiguous-match-found-on-new-property
                    result = base_type.
                            GetMethods().
                            Where(x => x.Name == methodName && x.GetParameters().
                                Select(y => y.ParameterType).
                                Except(types).Count() == 0).
                                First();

                }
                catch (Exception e)
                {
                    throw new Exception("There was an error finding the appropriate API method to call.", e);
                }
            }

            if (result == typeof(MethodInfo))
            {
                returnType = ((MethodInfo)result).ReturnType;
            }

            return result;
        }

        /// <summary>
        /// Invoke an API method, using the node's lacing strategy to build lists of arguments
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="args">The incoming Values on the node.</param>
        /// <param name="api_base_type">The API's base type whose method we will invoke.</param>
        /// <param name="pi">An array of parameter info for the method.</param>
        /// <param name="mi">The method info for the method.</param>
        /// <param name="return_type">The expected return type from the method.</param>
        /// <returns></returns>
        public static Value InvokeAPIMethod(RevitTransactionNode node, FSharpList<Value> args, Type api_base_type, ParameterInfo[] pi, MethodBase mi, Type return_type)
        {
            //if any argument are a list, honor the lacing strategy
            //compile a list of parameter lists to be used in our method invocation
            List<List<object>> parameters = null;

            switch (node.ArgumentLacing)
            {
                case LacingStrategy.First:
                    parameters = GetSingleArguments(args, pi);
                    break;
                case LacingStrategy.Shortest:
                    parameters = GetShortestArguments(args, pi);
                    break;
                case LacingStrategy.Longest:
                    parameters = GetLongestArguments(args, pi);
                    break;
                default:
                    parameters = GetSingleArguments(args, pi);
                    break;
            }

            var invocationTargetList = new List<object>();

            if (api_base_type == typeof(Document) ||
                api_base_type == typeof(FamilyItemFactory) ||
                api_base_type == typeof(ItemFactoryBase))
            {
                if (DocumentManager.Instance.CurrentUIDocument.Document.IsFamilyDocument)
                {
                    invocationTargetList.Add(DocumentManager.Instance.CurrentUIDocument.Document.FamilyCreate);
                }
                else
                {
                    invocationTargetList.Add(DocumentManager.Instance.CurrentUIDocument.Document.Create);
                }
            }
            else if (api_base_type == typeof(Application))
            {
                invocationTargetList.Add(DocumentManager.Instance.CurrentUIApplication.Application.Create);
            }
            else
            {
                if (!mi.IsStatic && !mi.IsConstructor)
                {
                    if (args[0].IsList)
                    {
                        invocationTargetList.AddRange(((Value.List)args[0]).Item.Select(x => DynamoTypeConverter.ConvertInput(x, api_base_type)));
                    }
                    else
                    {
                        //the first input will always hold the instance
                        //whose methods you want to invoke
                        invocationTargetList.Add(DynamoTypeConverter.ConvertInput(args[0], api_base_type));
                    }
                }
            }

            //object result = null;
            List<object> results = null;

            //if the method info is for a constructor, then
            //call the constructor for each set of parameters
            //if it's an instance method, then invoke the method for
            //each instance passed in.
            results = mi.IsConstructor ?
                parameters.Select(x => ((ConstructorInfo)mi).Invoke(x.ToArray())).ToList() :
                invocationTargetList.SelectMany(x => parameters.Select(y => mi.Invoke(x, y.ToArray())).ToList()).ToList();

            StoreElements(node, results);

            return ConvertAllResults(results);
        }

        /// <summary>
        /// Get a property value from a member
        /// </summary>
        /// <param name="args">The incoming arguments</param>
        /// <param name="api_base_type">The base type</param>
        /// <param name="pi">The property info object for which you will return the value.</param>
        /// <param name="return_type">The expected return type.</param>
        /// <returns></returns>
        public static Value GetAPIPropertyValue(FSharpList<Value> args,
                                                 Type api_base_type, PropertyInfo pi, Type return_type)
        {
            //var arg0 = (Autodesk.Revit.DB.HermiteFace)DynamoTypeConverter.ConvertInput(args[0], typeof(Autodesk.Revit.DB.HermiteFace));
            //var result = arg0.Points;

            var results = new List<object>();

            //there should only be one argument
            foreach (var arg in args)
            {
                if (arg.IsList)
                {
                    FSharpList<Value> lst = FSharpList<Value>.Empty;

                    //the values here are the items whose properties
                    //you want to query. nothing fancy, just get the
                    //property for each of the items.
                    results.AddRange(((Value.List)arg).Item.
                        Select(v => DynamoTypeConverter.ConvertInput(v, api_base_type)).
                        Select(invocationTarget => pi.GetValue(invocationTarget, null)));
                }
                else
                {
                    var invocationTarget = DynamoTypeConverter.ConvertInput(args[0], api_base_type);
                    results.Add(pi.GetValue(invocationTarget, null));
                }
            }

            return ConvertAllResults(results);

        }

        private static Value ConvertAllResults(List<object> results)
        {
            //if there are multiple items in the results list
            //return a list type
            if (results.Count > 1)
            {
                FSharpList<Value> lst = FSharpList<Value>.Empty;

                //reverse the results list so our CONs list isn't backwards
                results.Reverse();

                lst = results.Aggregate(lst,
                                        (current, result) =>
                                        FSharpList<Value>.Cons(DynamoTypeConverter.ConvertToValue(result), current));

                //the result will be a list of objects if any lists
                return Value.NewList(lst);
            }
            //otherwise, return a single value
            else
            {
                return DynamoTypeConverter.ConvertToValue(results.First());
            }
        }

        /// <summary>
        /// Get a straight list of matching arguments and parameters
        /// For a parameter set like p1, p2, p3 and argument lists like {a} {b1,b2,b3} {c1,c2}
        /// This will return a List of Lists of objects like:
        /// {a,b1,c1}
        /// </summary>
        /// <param name="args">The incoming arguments.</param>
        /// <param name="pi">The parameter information.</param>
        /// <returns></returns>
        private static List<List<object>> GetSingleArguments(FSharpList<Value> args, ParameterInfo[] pi)
        {
            var parameters = new List<List<object>>();

            BuildParameterList(args, pi, 1, parameters);

            return parameters;
        }

        /// <summary>
        /// Get the shortest list of arguments.
        /// For a parameter set like p1, p2, p3 and argument lists like {a} {b1,b2,b3} {c1,c2}
        /// This will return a List of Lists of objects like:
        /// {a,b1,c1} {a,b2,c2}
        /// </summary>
        /// <param name="args"></param>
        /// <param name="pi"></param>
        /// <returns></returns>
        private static List<List<object>> GetShortestArguments(FSharpList<Value> args, ParameterInfo[] pi)
        {
            var parameters = new List<List<object>>();

            //find the SMALLEST list in the inputs
            int end = args.Where(arg => arg.IsList).Select(arg => ((Value.List)arg).Item.Count()).Concat(new[] { 1000000 }).Min();

            BuildParameterList(args, pi, end, parameters);

            return parameters;
        }

        /// <summary>
        /// Get the longest list of arguments.
        /// For a parameter set like p1, p2, p3 and argument lists like {a} {b1,b2,b3} {c1,c2}
        /// This will return a List of Lists of objects like:
        /// {a,b1,c1} {a,b2,c2} {a,b3,c2}
        /// </summary>
        /// <param name="args"></param>
        /// <param name="pi"></param>
        /// <returns></returns>
        private static List<List<object>> GetLongestArguments(FSharpList<Value> args, ParameterInfo[] pi)
        {
            var parameters = new List<List<object>>();

            //find the LARGEST list in the inputs
            int end = args.Where(arg => arg.IsList).Select(arg => ((Value.List)arg).Item.Count()).Concat(new[] { 1 }).Max();

            BuildParameterList(args, pi, end, parameters);

            return parameters;
        }

        /// <summary>
        /// Builds a parameter list given a list of arguments and a list of parameter info
        /// </summary>
        /// <param name="args">A list of Values which will be distributed to the output lists</param>
        /// <param name="pi">An array of parameter info for the method</param>
        /// <param name="end">The end count</param>
        /// <param name="parameters">A parameters List to add to.</param>
        private static void BuildParameterList(FSharpList<Value> args, ParameterInfo[] pi, int end, List<List<object>> parameters)
        {
            //for a static method, the number of parameters
            //will equal the number of arguments on the node
            if (args.Count() == pi.Count())
            {
                //ARGUMENT LOOP
                for (int j = 0; j < end; j++)
                {
                    //create a list to hold each set of arguments
                    var currParams = new List<object>();

                    //PARAMETER LOOP
                    for (int i = 0; i < pi.Count(); i++)
                    {
                        var arg = args[i];

                        //if the value is a list, add the jth item converted
                        //or the last item if i exceeds the count of the list
                        if (arg.IsList)
                        {
                            var lst = (Value.List)arg;
                            var argItem = (j < lst.Item.Count() ? lst.Item[j] : lst.Item.Last());

                            currParams.Add(DynamoTypeConverter.ConvertInput(argItem, pi[i].ParameterType));
                        }
                        else
                            //if the value is not a list,
                            //just add the value
                            currParams.Add(DynamoTypeConverter.ConvertInput(arg, pi[i].ParameterType));
                    }

                    parameters.Add(currParams);
                }
            }
            //for instance methods, the first argument will be the 
            //item or list of items which will be the target of invocation
            //in this case, skip parsing the first argument
            else
            {
                //ARGUMENT LOOP
                for (int j = 0; j < end; j++)
                {
                    //create a list to hold each set of arguments
                    var currParams = new List<object>();

                    //PARAMETER LOOP
                    for (int i = 0; i < pi.Count(); i++)
                    {
                        var arg = args[i + 1];

                        //if the value is a list, add the jth item converted
                        //or the last item if i exceeds the count of the list
                        if (arg.IsList)
                        {
                            //var argItem = ((Value.List)arg).Item.Count() < end ? args.Last() : args[j];

                            var lst = (Value.List)arg;
                            var argItem = (j < lst.Item.Count() ? lst.Item[j] : lst.Item.Last());

                            currParams.Add(DynamoTypeConverter.ConvertInput(argItem, pi[i].ParameterType));
                        }
                        else
                            //if the value is not a list,
                            //just add the value
                            currParams.Add(DynamoTypeConverter.ConvertInput(arg, pi[i].ParameterType));
                    }

                    parameters.Add(currParams);
                }
            }
        }

    }

    /// <summary>
    /// Used with the Auto-generator. Allows automatic conversion of inputs and outputs
    /// </summary>
    public static class DynamoTypeConverter
    {
        private static ReferenceArrayArray ConvertFSharpListListToReferenceArrayArray(FSharpList<Value> lstlst)
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
            DocumentManager.Instance.CurrentUIDocument.RefreshActiveView();

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

        private static ReferenceArray ConvertFSharpListListToReferenceArray(FSharpList<Value> lstlst)
        {
            ReferenceArray refArr = new ReferenceArray();

            AddReferencesToArray(refArr, lstlst);

            return refArr;

        }

        private static CurveArrArray ConvertFSharpListListToCurveArrayArray(FSharpList<Value> lstlst)
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

        private static CurveArray ConvertFSharpListListToCurveArray(FSharpList<Value> lstlst)
        {
            CurveArray crvArr = new CurveArray();

            AddCurvesToArray(crvArr, lstlst);

            return crvArr;

        }

        private static void AddCurvesToArray(CurveArray crvArr, FSharpList<Value> lst)
        {
            DocumentManager.Instance.CurrentUIDocument.RefreshActiveView();

            foreach (Value vInner in lst)
            {
                var c = (vInner as Value.Container).Item as Curve;
                crvArr.Append(c);
            }

        }
        
        /// <summary>
        /// Convert an input Value into and expected type if possible.
        /// Ex. If a node expects an XYZ, a user can pass in a ReferencePoint object
        /// and the position (XYZ) of that object will be returned.
        /// </summary>
        /// <param name="input">The value of the input.</param>
        /// <param name="output">The desired output type.</param>
        /// <returns></returns>
        public static object ConvertInput(Value input, Type output)
        {
            if (input.IsContainer)
            {
                object item = ((Value.Container)input).Item;

                #region ModelCurve
                if (item.GetType() == typeof(ModelCurve))
                {
                    ModelCurve a = (ModelCurve)item;

                    if (output == typeof(Curve))
                    {
                        return ((ModelCurve)item).GeometryCurve;
                    }
                }
                #endregion

                #region SketchPlane
                else if (item.GetType() == typeof(SketchPlane))
                {
                    SketchPlane a = (SketchPlane)item;

                    if (output == typeof(Plane))
                    {
                        return a.Plane;
                    }
                    else if (output == typeof(ReferencePlane))
                    {
                        return a.Plane;
                    }
                    else if (output == typeof(string))
                    {
                        return string.Format("{0},{1},{2},{3},{4},{5}", a.Plane.Origin.X, a.Plane.Origin.Y, a.Plane.Origin.Z,
                            a.Plane.Normal.X, a.Plane.Normal.Y, a.Plane.Normal.Z);
                    }
                }
                #endregion

                #region Point
                else if (item.GetType() == typeof(Point))
                {
                    Point a = (Point)item;

                    if (output == typeof(XYZ))
                    {
                        return a.Coord;
                    }
                    else if (output == typeof(string))
                    {
                        return string.Format("{0},{1},{2}", a.Coord.X, a.Coord.Y, a.Coord.Z);
                    }
                }
                #endregion

                #region ReferencePoint
                else if (item.GetType() == typeof(Autodesk.Revit.DB.ReferencePoint))
                {
                    Autodesk.Revit.DB.ReferencePoint a = (Autodesk.Revit.DB.ReferencePoint)item;

                    if (output == typeof(XYZ))
                    {
                        return a.Position;
                    }
                    else if (output == typeof(Reference))
                    {
                        return a.GetCoordinatePlaneReferenceXY();
                    }
                    else if (output == typeof(Transform))
                    {
                        return a.GetCoordinateSystem();
                    }
                    else if (output == typeof(string))
                    {
                        return string.Format("{0},{1},{2}", a.Position.X, a.Position.Y, a.Position.Z);
                    }
                    else if (output == typeof(ElementId))
                    {
                        return a.Id;
                    }
                }
                #endregion

                #region ElementId
                else if (item.GetType() == typeof(ElementId))
                {
                    if (output == typeof(ReferencePoint))
                    {
                        ElementId a = (ElementId)item;
                        Element el = DocumentManager.Instance.CurrentUIDocument.Document.GetElement(a);
                        ReferencePoint rp = (ReferencePoint)el;
                        return rp;
                    }
                }
                #endregion

                #region Reference
                else if (item.GetType() == typeof(Reference))
                {
                    Reference a = (Reference)item;
                    if(output.IsAssignableFrom(typeof(Element)))
                    {
                        Element e = DocumentManager.Instance.CurrentUIDocument.Document.GetElement(a);
                    }
                    else if (output == typeof(Face))
                    {
                        Face f = (Face)DocumentManager.Instance.CurrentUIDocument.Document.GetElement(a.ElementId).GetGeometryObjectFromReference(a);
                        return f;
                    }
                }
                #endregion

                #region XYZ
                if (item.GetType() == typeof(XYZ))
                {
                    XYZ a = (XYZ)item;
                    if (output == typeof(Transform))
                    {
                        Transform t = Transform.get_Translation(a);
                        return t;
                    }
                }
                #endregion

                return item;
            }
            else if (input.IsNumber)
            {
                double a = (double)((Value.Number)input).Item;

                if (output == typeof(bool))
                {
                    return Convert.ToBoolean(a);
                }
                else if (output == typeof(Int32))
                {
                    return Convert.ToInt32(a);
                }

                return a;
            }
            else if(input.IsString)
            {
                string a = ((Value.String)input).Item.ToString();
                return a;
            }
            else if (input.IsList)
            {
                FSharpList<Value> a = ((Value.List)input).Item;

                if (output == typeof(ReferenceArrayArray))
                {
                    return DynamoTypeConverter.ConvertFSharpListListToReferenceArrayArray(a);
                }
                else if (output == typeof(ReferenceArray))
                {
                    return DynamoTypeConverter.ConvertFSharpListListToReferenceArray(a);
                }
                else if (output == typeof(CurveArrArray))
                {
                    return DynamoTypeConverter.ConvertFSharpListListToCurveArray(a);
                }
                else if (output == typeof(CurveArray))
                {
                    return DynamoTypeConverter.ConvertFSharpListListToCurveArray(a);
                }

                return a;
            }

            //return the input by default
            return input;
        }

        /// <summary>
        /// Convert the result of a wrapped Revit API method or property to it's correct Dynamo Value type.
        /// </summary>
        /// <param name="input">The result of the Revit API method.</param>
        /// <returns></returns>
        public static Value ConvertToValue(object input)
        {
            if (input == null)
            {
                return Value.NewNumber(0);
            }

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
            else if (input.GetType() == typeof(List<ElementId>))
            {
                List<Value> vals = new List<Value>();
                foreach (ElementId id in (List<ElementId>)input)
                {
                    vals.Add(Value.NewContainer(id));
                }

                return Value.NewList(Utils.SequenceToFSharpList(vals));
            }
            else if (input.GetType() == typeof(IntersectionResultArray))
            {
                // for interesection results, send out two lists
                // a list for the XYZs and one for the UVs
                List<Value> xyzs = new List<Value>();
                List<Value> uvs = new List<Value>();
                
                foreach (IntersectionResult ir in (IntersectionResultArray)input)
                {
                    xyzs.Add(Value.NewContainer(ir.XYZPoint));
                    uvs.Add(Value.NewContainer(ir.UVPoint));
                }

                FSharpList<Value> result = FSharpList<Value>.Empty;
                result = FSharpList<Value>.Cons(
                           Value.NewList(Utils.SequenceToFSharpList(uvs)),
                           result);
                result = FSharpList<Value>.Cons(
                           Value.NewList(Utils.SequenceToFSharpList(xyzs)),
                           result);
                if (xyzs.Count > 0 || uvs.Count > 0)
                {
                    return Value.NewList(result);
                }
                else
                {
                    //TODO: if we don't have any XYZs or UVs, chances are
                    //we have just created an intersection result array to
                    //catch some values. in this case, don't convert.
                    return Value.NewContainer(input);
                }
            }
            else
            {
                return Value.NewContainer(input);
            }
        }
    }
}
