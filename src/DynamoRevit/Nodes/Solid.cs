using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Xml;
using Autodesk.Revit.DB;
using Dynamo.Controls;
using Dynamo.FSchemeInterop;
using Dynamo.Models;
using Dynamo.Revit;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;

namespace Dynamo.Nodes
{
    #region Solid Creation

    [NodeName("Loft Surface")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SURFACE_CREATE)]
    [NodeDescription("Creates a new loft surface.  Internally this is a LoftForm element")]
    public class LoftForm : RevitTransactionNodeWithOneOutput
    {
        public LoftForm()
        {
            InPortData.Add(new PortData("solid/void", "Indicates if the Form is Solid or Void. Use True for solid and false for void.", typeof(FScheme.Value.Number),FScheme.Value.NewNumber(1)));
            InPortData.Add(new PortData("list", "A list of profiles for the Loft Form. The recommended way is to use a list of planar Reference Curves, list of lists and list of curves are supported for legacy graphs.", typeof(FScheme.Value.List)));
            InPortData.Add(new PortData("surface?", "Create a single surface or an extrusion if one loop", typeof(FScheme.Value.Container), FScheme.Value.NewNumber(1)));

            OutPortData.Add(new PortData("form", "Loft Form", typeof(object)));

            RegisterAllPorts();
            if (_formId == null)
                _formId = ElementId.InvalidElementId;
        }

        Dictionary<ElementId, ElementId> _sformCurveToReferenceCurveMap;
        ElementId _formId;
        bool _preferSurfaceForOneLoop;

        protected override bool AcceptsListOfLists(FScheme.Value value)
        {
            if (Utils.IsListOfListsOfLists(value))
                return false;

            FSharpList<FScheme.Value> vals = ((FScheme.Value.List)value).Item;
            if (!vals.Any() || !(vals[0] is FScheme.Value.List))
                return true;
            FSharpList<FScheme.Value> firstListInList = ((FScheme.Value.List)vals[0]).Item;
            if (!firstListInList.Any() || !(firstListInList[0] is FScheme.Value.Container))
                return true;
            var var1 = ((FScheme.Value.Container)firstListInList[0]).Item;
            if (var1 is ModelCurveArray)
                return false;

            return true;
        }

        bool matchOrAddFormCurveToReferenceCurveMap(Form formElement, ReferenceArrayArray refArrArr, bool doMatch)
        {
            if (formElement.Id != _formId && doMatch)
            {
                return false;
            }
            else if (!doMatch)
                _formId = formElement.Id;

            if (doMatch && _sformCurveToReferenceCurveMap.Count == 0)
                return false;
            else if (!doMatch)
                _sformCurveToReferenceCurveMap = new Dictionary<ElementId, ElementId>();

            for (int indexRefArr = 0; indexRefArr < refArrArr.Size; indexRefArr++)
            {
                if (indexRefArr >= refArrArr.Size)
                {
                    if (!doMatch)
                        _sformCurveToReferenceCurveMap.Clear();
                    return false;
                }

                if (refArrArr.get_Item(indexRefArr).Size != formElement.get_CurveLoopReferencesOnProfile(indexRefArr, 0).Size)
                {
                    if (!doMatch)
                        _sformCurveToReferenceCurveMap.Clear();
                    return false;
                }
                for (int indexRef = 0; indexRef < refArrArr.get_Item(indexRefArr).Size; indexRef++)
                {
                    Reference oldRef = formElement.get_CurveLoopReferencesOnProfile(indexRefArr, 0).get_Item(indexRef);
                    Reference newRef = refArrArr.get_Item(indexRefArr).get_Item(indexRef);

                    if ((oldRef.ElementReferenceType != ElementReferenceType.REFERENCE_TYPE_NONE &&
                            oldRef.ElementReferenceType != ElementReferenceType.REFERENCE_TYPE_LINEAR) ||
                            (newRef.ElementReferenceType != ElementReferenceType.REFERENCE_TYPE_NONE &&
                             newRef.ElementReferenceType != ElementReferenceType.REFERENCE_TYPE_LINEAR) ||
                            oldRef.ElementReferenceType != newRef.ElementReferenceType)
                    {
                        if (!doMatch)
                            _sformCurveToReferenceCurveMap.Clear();
                        return false;
                    }
                    ElementId oldRefId = oldRef.ElementId;
                    ElementId newRefId = newRef.ElementId;

                    if (doMatch && (!_sformCurveToReferenceCurveMap.ContainsKey(newRefId) ||
                                    _sformCurveToReferenceCurveMap[newRefId] != oldRefId)
                       )
                    {
                        return false;
                    }
                    else if (!doMatch)
                        _sformCurveToReferenceCurveMap[newRefId] = oldRefId;
                }
            }
            return true;
        }


        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            //Solid argument
            bool isSolid = ((FScheme.Value.Number)args[0]).Item == 1;

            //Surface argument
            bool isSurface = ((FScheme.Value.Number)args[2]).Item == 1;

            //Build up our list of list of references for the form by...
            var curvesListList = (FScheme.Value.List)args[1];
            //Now we add all of those references into ReferenceArrays
            ReferenceArrayArray refArrArr = new ReferenceArrayArray();

            FSharpList<FScheme.Value> vals = ((FScheme.Value.List)curvesListList).Item;

            if (vals.Any() && (vals[0] is FScheme.Value.Container) && ((FScheme.Value.Container)vals[0]).Item is ModelCurveArray)
            {
                //Build a sequence that unwraps the input list from it's Value form.
                IEnumerable<ModelCurveArray> modelCurveArrays = ((FScheme.Value.List)args[1]).Item.Select(
                   x => (ModelCurveArray)((FScheme.Value.Container)x).Item
                );

                foreach (var modelCurveArray in modelCurveArrays)
                {
                    var refArr = new ReferenceArray();
                    foreach (Autodesk.Revit.DB.ModelCurve modelCurve in modelCurveArray)
                    {
                        refArr.Append(modelCurve.GeometryCurve.Reference);
                    }
                    refArrArr.Append(refArr);
                }
            }
            else
            {
                IEnumerable<IEnumerable<Reference>> refArrays = (curvesListList).Item.Select(
                    //...first selecting everything in the topmost list...
                   delegate(FScheme.Value x)
                   {
                       //If the element in the topmost list is a sub-list...
                       if (x.IsList)
                       {
                           //...then we return a new IEnumerable of References by converting the sub list.
                           return (x as FScheme.Value.List).Item.Select(
                              delegate(FScheme.Value y)
                              {
                                  //Since we're in a sub-list, we can assume it's a container.
                                  var item = ((FScheme.Value.Container)y).Item;
                                  if (item is CurveElement)
                                      return (item as CurveElement).GeometryCurve.Reference;
                                  else
                                      return (Reference)item;
                              }
                           );
                       }
                       //If the element is not a sub-list, then just assume it's a container.
                       else
                       {
                           var obj = ((FScheme.Value.Container)x).Item;
                           Reference r;
                           if (obj is CurveElement)
                           {
                               r = (obj as CurveElement).GeometryCurve.Reference;
                           }
                           else
                           {
                               r = (Reference)obj;
                           }
                           //We return a list here since it's expecting an IEnumerable<Reference>. In reality,
                           //just passing the element by itself instead of a sub-list is a shortcut for having
                           //a list with one element, so this is just performing that for the user.
                           return new List<Reference>() { r };
                       }
                   }
                );

                //Now we add all of those references into ReferenceArrays

                foreach (IEnumerable<Reference> refs in refArrays.Where(x => x.Any()))
                {
                    var refArr = new ReferenceArray();
                    foreach (Reference r in refs)
                        refArr.Append(r);
                    refArrArr.Append(refArr);
                }
            }
            //If we already have a form stored...
            if (this.Elements.Any())
            {
                Form oldF;
                //is this same element?
                if (dynUtils.TryGetElement(this.Elements[0], out oldF))
                {
                    if (oldF.IsSolid == isSolid &&
                        _preferSurfaceForOneLoop == isSurface
                        && matchOrAddFormCurveToReferenceCurveMap(oldF, refArrArr, true))
                    {
                        return FScheme.Value.NewContainer(oldF);
                    }
                }

                //Dissolve it, we will re-make it later.
                if (FormUtils.CanBeDissolved(this.UIDocument.Document, this.Elements.Take(1).ToList()))
                    FormUtils.DissolveForms(this.UIDocument.Document, this.Elements.Take(1).ToList());
                //And register the form for deletion. Since we've already deleted it here manually, we can 
                //pass "true" as the second argument.
                this.DeleteElement(this.Elements[0], true);

            }
            else if (this._formId != ElementId.InvalidElementId)
            {
                Form oldF;
                if (dynUtils.TryGetElement(this._formId, out oldF))
                {
                    if (oldF.IsSolid == isSolid
                        && _preferSurfaceForOneLoop == isSurface
                        && matchOrAddFormCurveToReferenceCurveMap(oldF, refArrArr, true))
                    {
                        return FScheme.Value.NewContainer(oldF);
                    }
                }
            }

            _preferSurfaceForOneLoop = isSurface;

            //We use the ReferenceArrayArray to make the form, and we store it for later runs.

            Form f;
            //if we only have a single refArr, we can make a capping surface or an extrusion
            if (refArrArr.Size == 1)
            {
                ReferenceArray refArr = refArrArr.get_Item(0);

                if (isSurface) // make a capping surface
                {
                    f = this.UIDocument.Document.FamilyCreate.NewFormByCap(true, refArr);
                }
                else  // make an extruded surface
                {
                    // The extrusion form direction
                    XYZ direction = new XYZ(0, 0, 50);
                    f = this.UIDocument.Document.FamilyCreate.NewExtrusionForm(true, refArr, direction);
                }
            }
            else // make a lofted surface
            {
                f = this.UIDocument.Document.FamilyCreate.NewLoftForm(isSolid, refArrArr);
            }

            matchOrAddFormCurveToReferenceCurveMap(f, refArrArr, false);
            this.Elements.Add(f.Id);

            return FScheme.Value.NewContainer(f);
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            nodeElement.SetAttribute("FormId", _formId.ToString());
            nodeElement.SetAttribute("PreferSurfaceForOneLoop", _preferSurfaceForOneLoop.ToString());

            System.String mapAsString = "";

            if (_sformCurveToReferenceCurveMap != null)
            {
                var enumMap = _sformCurveToReferenceCurveMap.GetEnumerator();
                for (; enumMap.MoveNext(); )
                {
                    ElementId keyId = enumMap.Current.Key;
                    ElementId valueId = enumMap.Current.Value;

                    mapAsString = mapAsString + keyId.ToString() + "=" + valueId.ToString() + ";";
                }
            }
            nodeElement.SetAttribute("FormCurveToReferenceCurveMap", mapAsString);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            try
            {
                _formId = new ElementId(Convert.ToInt32(nodeElement.Attributes["FormId"].Value));
                var thisIsSurface = nodeElement.Attributes["PreferSurfaceForOneLoop"];
                if (thisIsSurface != null)
                    _preferSurfaceForOneLoop = Convert.ToBoolean(thisIsSurface.Value);
                else //used to be able to make only surface, so init to more likely value
                    _preferSurfaceForOneLoop = true;

                string mapAsString = nodeElement.Attributes["FormCurveToReferenceCurveMap"].Value;
                _sformCurveToReferenceCurveMap = new Dictionary<ElementId, ElementId>();
                if (mapAsString != "")
                {

                    string[] curMap = mapAsString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    int mapSize = curMap.Length;
                    for (int iMap = 0; iMap < mapSize; iMap++)
                    {
                        string[] thisMap = curMap[iMap].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                        if (thisMap.Length != 2)
                        {
                            _sformCurveToReferenceCurveMap = new Dictionary<ElementId, ElementId>();
                            break;
                        }
                        ElementId keyId = new ElementId(Convert.ToInt32(thisMap[0]));
                        ElementId valueId = new ElementId(Convert.ToInt32(thisMap[1]));
                        _sformCurveToReferenceCurveMap[keyId] = valueId;
                    }
                }
            }
            catch
            {
                _sformCurveToReferenceCurveMap = new Dictionary<ElementId, ElementId>();
            }
        }
    }

    [NodeName("Revolve")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SOLID_CREATE)]
    [NodeDescription("Creates a solid by revolving closed curve loops lying in xy plane a given coordinate system.")]
    public class CreateRevolvedGeometry : GeometryBase
    {
        public CreateRevolvedGeometry()
        {
            InPortData.Add(new PortData("profile", "The Curve Loop or closed Curve to use as a profile", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("transform", "Coordinate system for revolve, loop should be in xy plane of this transform on the right side of z axis used for rotate.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("start angle", "start angle measured counter-clockwise from x-axis of transform", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("end angle", "end angle measured counter-clockwise from x-axis of transform", typeof(FScheme.Value.Number)));

            OutPortData.Add(new PortData("solid", "The revolved geometry.", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var cLoop = (Autodesk.Revit.DB.CurveLoop)((FScheme.Value.Container)args[0]).Item;
            var trf = (Transform)((FScheme.Value.Container)args[1]).Item;
            var start = ((FScheme.Value.Number)args[2]).Item;
            var end = ((FScheme.Value.Number)args[3]).Item;

            var loopList = new List<Autodesk.Revit.DB.CurveLoop> { cLoop };
            var thisFrame = new Autodesk.Revit.DB.Frame();
            thisFrame.Transform(trf);

            var result = GeometryCreationUtilities.CreateRevolvedGeometry(thisFrame, loopList, start, end);

            return FScheme.Value.NewContainer(result);
        }
    }

    [NodeName("Sweep")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SOLID_CREATE)]
    [NodeDescription("Creates a solid by sweeping curve loop along the path")]
    public class CreateSweptGeometry : GeometryBase
    {
        public CreateSweptGeometry()
        {
            InPortData.Add(new PortData("sweep path", "The curve loop to sweep along.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("attachment curve index", "index of the curve where profile loop is attached", typeof(FScheme.Value.Number), FScheme.Value.NewNumber(0)));
            InPortData.Add(new PortData("attachment parameter", "parameter of attachment point on its curve", typeof(FScheme.Value.Number), FScheme.Value.NewNumber(0)));
            InPortData.Add(new PortData("profile loop", "The curve loop to sweep to be put in orthogonal plane to path at attachment point.", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("geometry", "The swept geometry.", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public static Autodesk.Revit.DB.CurveLoop CurveLoopFromContainer(FScheme.Value.Container curveOrCurveLoop)
        {
            var pathLoopBoxed = curveOrCurveLoop.Item;
            Autodesk.Revit.DB.CurveLoop curveLoop;
            var loop = pathLoopBoxed as Autodesk.Revit.DB.CurveLoop;
            if (loop != null)
            {
                curveLoop = loop;
            }
            else
            {
                curveLoop = new Autodesk.Revit.DB.CurveLoop();
                curveLoop.Append((Autodesk.Revit.DB.Curve)pathLoopBoxed);
            }

            return curveLoop;
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var pathLoop = CurveLoopFromContainer((FScheme.Value.Container)args[0]);
            int attachementIndex = (int)((FScheme.Value.Number)args[1]).Item;
            double attachementPar = ((FScheme.Value.Number)args[2]).Item;
            Autodesk.Revit.DB.CurveLoop profileLoop = (Autodesk.Revit.DB.CurveLoop)((FScheme.Value.Container)args[3]).Item;
            List<Autodesk.Revit.DB.CurveLoop> loopList = new List<Autodesk.Revit.DB.CurveLoop>();

            if (profileLoop.HasPlane())
            {
                Autodesk.Revit.DB.Plane profileLoopPlane = profileLoop.GetPlane();
                Autodesk.Revit.DB.CurveLoopIterator CLiter = pathLoop.GetCurveLoopIterator();

                for (int indexCurve = 0; indexCurve < attachementIndex && CLiter.MoveNext(); indexCurve++)
                {
                    CLiter.MoveNext();
                }

                Autodesk.Revit.DB.Curve pathCurve = CLiter.Current;
                if (pathCurve != null)
                {
                    double angleTolerance = Math.PI / 1800.0;
                    Transform pathTrf = pathCurve.ComputeDerivatives(attachementPar, false);
                    XYZ pathDerivative = pathTrf.BasisX.Normalize();
                    double distAttachment = profileLoopPlane.Normal.DotProduct(profileLoopPlane.Origin - pathTrf.Origin);
                    if (Math.Abs(distAttachment) > 0.000001 ||
                         Math.Abs(profileLoopPlane.Normal.DotProduct(pathDerivative)) < 1.0 - angleTolerance * angleTolerance
                       )
                    {
                        //put profile at proper plane
                        double distOrigin = profileLoopPlane.Normal.DotProduct(profileLoopPlane.Origin);
                        XYZ fromPoint = pathTrf.Origin;
                        if (Math.Abs(distAttachment) > 0.000001 + Math.Abs(distOrigin))
                            fromPoint = (-distOrigin) * profileLoopPlane.Normal;
                        else
                            fromPoint = pathTrf.Origin - (pathTrf.Origin - profileLoopPlane.Origin).DotProduct(profileLoopPlane.Normal) * profileLoopPlane.Normal;
                        XYZ fromVecOne = profileLoopPlane.Normal;
                        if (fromVecOne.DotProduct(pathDerivative) < -angleTolerance)
                            fromVecOne = -fromVecOne;
                        XYZ toVecOne = pathDerivative;
                        XYZ fromVecTwo = XYZ.BasisZ.CrossProduct(fromVecOne);
                        if (fromVecTwo.IsZeroLength())
                            fromVecTwo = XYZ.BasisX;
                        else
                            fromVecTwo = fromVecTwo.Normalize();
                        XYZ toVecTwo = XYZ.BasisZ.CrossProduct(pathDerivative);
                        if (toVecTwo.IsZeroLength())
                            toVecTwo = XYZ.BasisX;
                        else
                            toVecTwo = toVecTwo.Normalize();
                        if (toVecTwo.DotProduct(fromVecTwo) < -angleTolerance)
                            toVecTwo = -toVecTwo;

                        Transform trfToAttach = Transform.CreateTranslation(pathTrf.Origin);
                        trfToAttach.BasisX = toVecOne;
                        trfToAttach.BasisY = toVecTwo;
                        trfToAttach.BasisZ = toVecOne.CrossProduct(toVecTwo);

                        Transform trfToProfile = Transform.CreateTranslation(fromPoint);
                        trfToProfile.BasisX = fromVecOne;
                        trfToProfile.BasisY = fromVecTwo;
                        trfToProfile.BasisZ = fromVecOne.CrossProduct(fromVecTwo);

                        Transform trfFromProfile = trfToProfile.Inverse;

                        Transform combineTrf = trfToAttach.Multiply(trfFromProfile);

                        //now get new curve loop
                        Autodesk.Revit.DB.CurveLoop transformedCurveLoop = new Autodesk.Revit.DB.CurveLoop();
                        Autodesk.Revit.DB.CurveLoopIterator CLiterT = profileLoop.GetCurveLoopIterator();
                        for (; CLiterT.MoveNext(); )
                        {
                            Curve curCurve = CLiterT.Current;
                            Curve curCurveTransformed = curCurve.CreateTransformed(combineTrf);

                            transformedCurveLoop.Append(curCurveTransformed);
                        }
                        profileLoop = transformedCurveLoop;
                    }
                }
            }
            loopList.Add(profileLoop);

            Solid result = GeometryCreationUtilities.CreateSweptGeometry(pathLoop, attachementIndex, attachementPar, loopList);

            return FScheme.Value.NewContainer(result);
        }
    }

    [NodeName("Extrude")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SOLID_CREATE)]
    [NodeDescription("Creates a solid by linearly extruding a closed curve.")]
    public class CreateExtrusionGeometry : GeometryBase
    {

        public CreateExtrusionGeometry()
        {
            InPortData.Add(new PortData("profile", "The profile CurveLoop.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("direction", "The direction in which to extrude the profile.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("distance", "The positive distance by which the loops are to be extruded.", typeof(FScheme.Value.Number)));

            OutPortData.Add(new PortData("solid", "The extrusion.", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var curveLoop = CreateSweptGeometry.CurveLoopFromContainer((FScheme.Value.Container)args[0]);
            var direction = (XYZ)((FScheme.Value.Container)args[1]).Item;
            var distance = ((FScheme.Value.Number)args[2]).Item;

            var result = GeometryCreationUtilities.CreateExtrusionGeometry(new List<Autodesk.Revit.DB.CurveLoop>() { curveLoop }, direction, distance);

            return FScheme.Value.NewContainer(result);
        }
    }

    [NodeName("Blend")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SOLID_CREATE)]
    [NodeDescription("Creates a solid by blending two closed curve loops lying in non-coincident planes.")]
    public class CreateBlendGeometry : GeometryBase
    {
        public CreateBlendGeometry()
        {
            InPortData.Add(new PortData("first loop", "The first curve loop. The loop must be a closed planar loop.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("second loop", "The second curve loop, which also must be a closed planar loop.", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("solid", "The blended geometry.", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var firstLoop = CreateSweptGeometry.CurveLoopFromContainer((FScheme.Value.Container)args[0]);
            var secondLoop = CreateSweptGeometry.CurveLoopFromContainer((FScheme.Value.Container)args[1]);

            List<VertexPair> vertPairs = null;

            if (dynRevitSettings.Revit.Application.VersionName.Contains("2013"))
            {
                vertPairs = new List<VertexPair>();

                int i = 0;
                int nCurves1 = firstLoop.Count();
                int nCurves2 = secondLoop.Count();
                for (; i < nCurves1 && i < nCurves2; i++)
                {
                    vertPairs.Add(new VertexPair(i, i));
                }
            }

            var result = GeometryCreationUtilities.CreateBlendGeometry(firstLoop, secondLoop, vertPairs);

            return FScheme.Value.NewContainer(result);
        }
    }

    [NodeName("Swept Blend")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SOLID_CREATE)]
    [NodeDescription("Creates a solid by sweeping and blending curve loop along a single curve")]
    public class CreateSweptBlendGeometry : GeometryBase
    {
        public CreateSweptBlendGeometry()
        {
            InPortData.Add(new PortData("profile loops", "Closed planar curve loops located along the path in orthogonal plane to the path.", typeof(FScheme.Value.List)));
            InPortData.Add(new PortData("sweep path", "The curve to sweep along.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("attachment parameters", "parameter of attachment point on its curve", typeof(FScheme.Value.List), FScheme.Value.NewList(FSharpList<FScheme.Value>.Empty)));

            OutPortData.Add(new PortData("solid", "The swept geometry.", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            Autodesk.Revit.DB.Curve pathCurve = (Autodesk.Revit.DB.Curve)((FScheme.Value.Container)args[1]).Item;

            List<Autodesk.Revit.DB.CurveLoop> profileLoops = new List<Autodesk.Revit.DB.CurveLoop>();
            var input = (args[0] as FScheme.Value.List).Item;
            foreach (FScheme.Value v in input)
            {
                Autodesk.Revit.DB.CurveLoop cL = ((FScheme.Value.Container)v).Item as Autodesk.Revit.DB.CurveLoop;
                profileLoops.Add(cL);
            }

            List<double> attachParams = new List<double>();
            var inputPar = (args[2] as FScheme.Value.List).Item;
            if (inputPar.Length < profileLoops.Count)
            {
                //intersect plane of each curve loop with the pathCurve
                double lastParam = pathCurve.ComputeRawParameter(0.0);
                foreach (Autodesk.Revit.DB.CurveLoop cLoop in profileLoops)
                {
                    Autodesk.Revit.DB.Plane planeOfCurveLoop = cLoop.GetPlane();
                    if (planeOfCurveLoop == null)
                        throw new Exception("Profile Curve Loop is not planar");
                    Face face = Dynamo.Nodes.CurveFaceIntersection.buildFaceOnPlaneByCurveExtensions(pathCurve, planeOfCurveLoop);
                    IntersectionResultArray xsects = null;
                    face.Intersect(pathCurve, out xsects);
                    if (xsects == null)
                        throw new Exception("Could not find attachment point on path curve");
                    if (xsects.Size > 1)
                    {
                        for (int indexInt = 0; indexInt < xsects.Size; indexInt++)
                        {
                            if (xsects.get_Item(indexInt).Parameter < lastParam + 0.0000001)
                                continue;
                            lastParam = xsects.get_Item(indexInt).Parameter;
                            break;
                        }
                    }
                    else
                        lastParam = xsects.get_Item(0).Parameter;
                    attachParams.Add(lastParam);
                }
            }
            else
            {
                foreach (FScheme.Value vPar in inputPar)
                {
                    double par = (double)((FScheme.Value.Container)vPar).Item;
                    attachParams.Add(par);
                }
            }
            //check the parameter and set it if not right or not defined
            List<ICollection<Autodesk.Revit.DB.VertexPair>> vertPairs = null;

            Solid result = GeometryCreationUtilities.CreateSweptBlendGeometry(pathCurve, attachParams, profileLoops, vertPairs);

            return FScheme.Value.NewContainer(result);
        }
    }

    [NodeName("Boolean Operation")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SOLID_BOOLEAN)]
    [NodeDescription("Creates a solid by union, intersection or difference of two solids.")]
    public class BooleanOperation : GeometryBase
    {
        ComboBox combo;
        int selectedItem = -1;

        public BooleanOperation()
        {
            InPortData.Add(new PortData("First Solid", "First solid input for boolean geometrical operation", typeof(object)));
            InPortData.Add(new PortData("Second Solid", "Second solid input for boolean geometrical operation", typeof(object)));

            OutPortData.Add(new PortData("solid in the element's geometry objects", "Solid", typeof(object)));
            selectedItem = 2;
            RegisterAllPorts();
        }
        
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            //add a drop down list to the window
            combo = new ComboBox();
            combo.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            combo.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            nodeUI.inputGrid.Children.Add(combo);
            System.Windows.Controls.Grid.SetColumn(combo, 0);
            System.Windows.Controls.Grid.SetRow(combo, 0);

            combo.DropDownOpened += new EventHandler(combo_DropDownOpened);
            combo.SelectionChanged += delegate
            {
                if (combo.SelectedIndex != -1)
                    this.RequiresRecalc = true;
            };
            if (selectedItem >= 0 && selectedItem <= 2)
            {
                PopulateComboBox();
                combo.SelectedIndex = selectedItem;
                selectedItem = -1;
            }
            if (combo.SelectedIndex < 0 || combo.SelectedIndex > 2)
                combo.SelectedIndex = 2;

            combo.SelectionChanged += combo_SelectionChanged;
        }

        void combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedItem = ((ComboBox) sender).SelectedIndex;
        }
        
        void combo_DropDownOpened(object sender, EventArgs e)
        {
            PopulateComboBox();
        }

        public enum BooleanOperationOptions { Union, Intersect, Difference };

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //nodeElement.SetAttribute("index", this.combo.SelectedIndex.ToString());
            nodeElement.SetAttribute("index", selectedItem.ToString());
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            try
            {
                selectedItem = Convert.ToInt32(nodeElement.Attributes["index"].Value);
                if (combo != null)
                    combo.SelectedIndex = selectedItem;
            }
            catch { }
        }

        private void PopulateComboBox()
        {

            combo.Items.Clear();
            ComboBoxItem cbiUnion = new ComboBoxItem();
            cbiUnion.Content = "Union";
            combo.Items.Add(cbiUnion);

            ComboBoxItem cbiIntersect = new ComboBoxItem();
            cbiIntersect.Content = "Intersect";
            combo.Items.Add(cbiIntersect);

            ComboBoxItem cbiDifference = new ComboBoxItem();
            cbiDifference.Content = "Difference";
            combo.Items.Add(cbiDifference);
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            Solid firstSolid = (Solid)((FScheme.Value.Container)args[0]).Item;
            Solid secondSolid = (Solid)((FScheme.Value.Container)args[1]).Item;

            int n = combo.SelectedIndex;


            BooleanOperationsType opType = (n == 0) ? BooleanOperationsType.Union :
                ((n == 2) ? BooleanOperationsType.Difference : BooleanOperationsType.Intersect);

            Solid result = BooleanOperationsUtils.ExecuteBooleanOperation(firstSolid, secondSolid, opType);

            return FScheme.Value.NewContainer(result);
        }
    }

    [NodeName("Boolean Difference")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SOLID_BOOLEAN)]
    [NodeDescription("Creates a solid by boolean difference of two solids")]
    public class SolidDifference : GeometryBase
    {

        public SolidDifference()
        {
            InPortData.Add(new PortData("First Solid", "First solid input for boolean geometrical operation", typeof(object)));
            InPortData.Add(new PortData("Second Solid", "Second solid input for boolean geometrical operation", typeof(object)));

            OutPortData.Add(new PortData("solid in the element's geometry objects", "Solid", typeof(object)));

            RegisterAllPorts();

        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var firstSolid = (Solid)((FScheme.Value.Container)args[0]).Item;
            var secondSolid = (Solid)((FScheme.Value.Container)args[1]).Item;

            var result = BooleanOperationsUtils.ExecuteBooleanOperation(firstSolid, secondSolid, BooleanOperationsType.Difference);

            return FScheme.Value.NewContainer(result);
        }
    }

    [NodeName("Boolean Union")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SOLID_BOOLEAN)]
    [NodeDescription("Creates a solid by boolean union of two solids")]
    public class SolidUnion : GeometryBase
    {

        public SolidUnion()
        {
            InPortData.Add(new PortData("First Solid", "First solid input for union", typeof(object)));
            InPortData.Add(new PortData("Second Solid", "Second solid input for union", typeof(object)));

            OutPortData.Add(new PortData("solid in the element's geometry objects", "Solid", typeof(object)));

            RegisterAllPorts();

        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var firstSolid = (Solid)((FScheme.Value.Container)args[0]).Item;
            var secondSolid = (Solid)((FScheme.Value.Container)args[1]).Item;

            var result = BooleanOperationsUtils.ExecuteBooleanOperation(firstSolid, secondSolid, BooleanOperationsType.Union);

            return FScheme.Value.NewContainer(result);
        }
    }

    [NodeName("Boolean Intersect")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SOLID_BOOLEAN)]
    [NodeDescription("Creates solid by boolean difference of two solids")]
    public class SolidIntersection : GeometryBase
    {

        public SolidIntersection()
        {
            InPortData.Add(new PortData("First Solid", "First solid input for intersection", typeof(object)));
            InPortData.Add(new PortData("Second Solid", "Second solid input for intersection", typeof(object)));

            OutPortData.Add(new PortData("solid in the element's geometry objects", "Solid", typeof(object)));

            RegisterAllPorts();

        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var firstSolid = (Solid)((FScheme.Value.Container)args[0]).Item;
            var secondSolid = (Solid)((FScheme.Value.Container)args[1]).Item;

            var result = BooleanOperationsUtils.ExecuteBooleanOperation(firstSolid, secondSolid, BooleanOperationsType.Intersect);

            return FScheme.Value.NewContainer(result);
        }
    }
    /*
    [NodeName("Solid from Element")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SOLID_QUERY)]
    [NodeDescription("Creates reference to the solid in the element's geometry objects.")]
    public class ElementSolid : GeometryBase
    {
        Dictionary<ElementId, List<GeometryObject>> instanceSolids;

        public ElementSolid()
        {
            InPortData.Add(new PortData("element", "element to create geometrical reference to", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("solid", "solid in the element's geometry objects", typeof(object)));

            RegisterAllPorts();

            instanceSolids = new Dictionary<ElementId, List<GeometryObject>>();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            Element thisElement = (Element)((FScheme.Value.Container)args[0]).Item;

            ElementId thisId = ElementId.InvalidElementId;

            if (thisElement != null)
            {
                thisId = thisElement.Id;
                instanceSolids[thisId] = new List<GeometryObject>();
            }

            Solid mySolid = null;

            //because of r2013 used GenericForm  which is superclass of FreeFromElement
            if ((thisElement is GenericForm) && (FreeForm.freeFormSolids != null &&
                  FreeForm.freeFormSolids.ContainsKey(thisElement.Id)))
            {
                mySolid = FreeForm.freeFormSolids[thisElement.Id];
            }
            else
            {
                bool bNotVisibleOption = false;
                if (thisElement is GenericForm)
                {
                    GenericForm gF = (GenericForm)thisElement;
                    if (!gF.Combinations.IsEmpty)
                        bNotVisibleOption = true;
                }
                int nTry = (bNotVisibleOption) ? 2 : 1;
                for (int iTry = 0; iTry < nTry && (mySolid == null); iTry++)
                {
                    Autodesk.Revit.DB.Options geoOptions = new Autodesk.Revit.DB.Options();
                    geoOptions.ComputeReferences = true;
                    if (bNotVisibleOption && (iTry == 1))
                        geoOptions.IncludeNonVisibleObjects = true;

                    GeometryObject geomObj = thisElement.get_Geometry(geoOptions);
                    GeometryElement geomElement = geomObj as GeometryElement;

                    if (geomElement != null)
                    {
                        foreach (GeometryObject geob in geomElement)
                        {
                            GeometryInstance ginsta = geob as GeometryInstance;
                            if (ginsta != null && thisId != ElementId.InvalidElementId)
                            {
                                GeometryElement instanceGeom = ginsta.GetInstanceGeometry();

                                instanceSolids[thisId].Add(instanceGeom);

                                foreach (GeometryObject geobInst in instanceGeom)
                                {
                                    mySolid = geobInst as Solid;
                                    if (mySolid != null)
                                    {
                                        FaceArray faceArr = mySolid.Faces;
                                        var thisEnum = faceArr.GetEnumerator();
                                        bool hasFace = false;
                                        for (; thisEnum.MoveNext(); )
                                        {
                                            hasFace = true;
                                            break;
                                        }
                                        if (!hasFace)
                                            mySolid = null;
                                        else
                                            break;
                                    }
                                }
                                if (mySolid != null)
                                    break;
                            }
                            else
                            {
                                mySolid = geob as Solid;
                                if (mySolid != null)
                                {
                                    FaceArray faceArr = mySolid.Faces;
                                    var thisEnum = faceArr.GetEnumerator();
                                    bool hasFace = false;
                                    for (; thisEnum.MoveNext(); )
                                    {
                                        hasFace = true;
                                        break;
                                    }
                                    if (!hasFace)
                                        mySolid = null;
                                    else
                                        break;
                                }

                            }
                        }
                    }
                }
            }

            return FScheme.Value.NewContainer(mySolid);
        }
    }
    */
    /*
    [NodeName("Cylinder")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SOLID_PRIMITIVES)]
    [NodeDescription("Create a cylinder from the axis, origin, radius, and height")]
    public class SolidCylinder : GeometryBase
    {
        public SolidCylinder()
        {
            InPortData.Add(new PortData("axis", "Axis of cylinder", typeof(FScheme.Value.Container), FScheme.Value.NewContainer(new XYZ(0, 0, 1))));
            InPortData.Add(new PortData("origin", "Base point of cylinder", typeof(FScheme.Value.Container), FScheme.Value.NewContainer(new XYZ(0, 0, 0))));
            InPortData.Add(new PortData("radius", "Radius of cylinder", typeof(FScheme.Value.Number), FScheme.Value.NewNumber(1)));
            InPortData.Add(new PortData("height", "Height of cylinder", typeof(FScheme.Value.Number), FScheme.Value.NewNumber(2)));

            OutPortData.Add(new PortData("cylinder", "Solid cylinder", typeof(object)));

            RegisterAllPorts();
        }

        public static Solid CylinderByAxisOriginRadiusHeight(XYZ axis, XYZ origin, double radius, double height)
        {
            // get axis that is perp to axis by first generating random vector
            var zaxis = axis.Normalize();
            var randXyz = new XYZ(1, 0, 0);
            if (axis.IsAlmostEqualTo(randXyz)) randXyz = new XYZ(0, 1, 0);
            var yaxis = zaxis.CrossProduct(randXyz).Normalize();

            // get second axis that is perp to axis
            var xaxis = yaxis.CrossProduct(zaxis);

            // create circle (this is ridiculous, but curve loop doesn't work with a circle - you need two arcs)
            var arc1 = dynRevitSettings.Doc.Application.Application.Create.NewEllipse(origin, radius, radius, xaxis, yaxis, 0, Circle.RevitPI);
            var arc2 = dynRevitSettings.Doc.Application.Application.Create.NewEllipse(origin, radius, radius, xaxis, yaxis, Circle.RevitPI, 2 * Circle.RevitPI);

            // create curve loop from cirle
            var circleLoop = Autodesk.Revit.DB.CurveLoop.Create(new List<Curve>() { arc1, arc2 });

            // extrude the curve and return
            return GeometryCreationUtilities.CreateExtrusionGeometry(new List<Autodesk.Revit.DB.CurveLoop>() { circleLoop }, zaxis, height);
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            // unpack input
            var axis = (XYZ)((FScheme.Value.Container)args[0]).Item;
            var origin = (XYZ)((FScheme.Value.Container)args[1]).Item;
            var radius = ((FScheme.Value.Number)args[2]).Item;
            var height = ((FScheme.Value.Number)args[3]).Item;

            // create and return geom
            return FScheme.Value.NewContainer(CylinderByAxisOriginRadiusHeight(axis, origin, radius, height));
        }
    }
    */
    [NodeName("Sphere")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SOLID_PRIMITIVES)]
    [NodeDescription("Creates sphere from a center point and axis")]
    public class SolidSphere : GeometryBase
    {

        public SolidSphere()
        {
            InPortData.Add(new PortData("center", "Center point of sphere", typeof(FScheme.Value.Container), FScheme.Value.NewContainer(new XYZ(0, 0, 0))));
            InPortData.Add(new PortData("radius", "Radius of sphere", typeof(FScheme.Value.Number), FScheme.Value.NewNumber(1)));

            OutPortData.Add(new PortData("sphere", "Solid sphere", typeof(object)));

            RegisterAllPorts();
        }

        public static Solid SphereByCenterRadius(XYZ center, double radius)
        {

            // create semicircular arc
            var semicircle = dynRevitSettings.Doc.Application.Application.Create.NewArc(center, radius, 0, Circle.RevitPI, XYZ.BasisZ, XYZ.BasisX);

            // create axis curve of cylinder - running from north to south pole
            var axisCurve = dynRevitSettings.Doc.Application.Application.Create.NewLineBound(new XYZ(0, 0, -radius),
                new XYZ(0, 0, radius));

            var circleLoop = Autodesk.Revit.DB.CurveLoop.Create(new List<Curve>() { semicircle, axisCurve });

            // revolve arc to form sphere
            return
                GeometryCreationUtilities.CreateRevolvedGeometry(
                    new Autodesk.Revit.DB.Frame(center, XYZ.BasisX, XYZ.BasisY, XYZ.BasisZ), new List<Autodesk.Revit.DB.CurveLoop>() { circleLoop }, 0,
                    2 * Circle.RevitPI);

        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            // unpack input
            var center = (XYZ)((FScheme.Value.Container)args[0]).Item;
            var radius = ((FScheme.Value.Number)args[1]).Item;

            return FScheme.Value.NewContainer(SphereByCenterRadius(center, radius));
        }
    }

    [NodeName("Torus")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SOLID_PRIMITIVES)]
    [NodeDescription("Creates torus from axis, radius, and outer radius")]
    public class SolidTorus : GeometryBase
    {
        public SolidTorus()
        {
            InPortData.Add(new PortData("axis", "Axis of torus", typeof(FScheme.Value.Container), FScheme.Value.NewContainer(new XYZ(0, 0, 1))));
            InPortData.Add(new PortData("center", "Center point of torus", typeof(object), FScheme.Value.NewContainer(new XYZ(0, 0, 1))));
            InPortData.Add(new PortData("radius", "The distance from the center to the cross-sectional center", typeof(FScheme.Value.Number),
                FScheme.Value.NewNumber(1)));
            InPortData.Add(new PortData("section radius", "The radius of the cross-section of the torus", typeof(FScheme.Value.Number),
                FScheme.Value.NewNumber(0.25)));

            OutPortData.Add(new PortData("torus", "Solid torus", typeof(object)));

            RegisterAllPorts();
        }

        public static Solid TorusByAxisOriginRadiusCrossSectionRadius(XYZ zAxis, XYZ center, double radius, double sectionRadius)
        {

            // get axis that is perp to axis by first generating random vector
            var zaxis = zAxis.Normalize();
            var randXyz = new XYZ(1, 0, 0);
            if (zaxis.IsAlmostEqualTo(randXyz)) randXyz = new XYZ(0, 1, 0);
            var yaxis = zaxis.CrossProduct(randXyz).Normalize();

            // get second axis that is perp to axis
            var xaxis = yaxis.CrossProduct(zaxis);

            // form origin of the arc
            var origin = center + xaxis * radius;

            // create circle (this is ridiculous but curve loop doesn't work with a circle
            var arc1 = dynRevitSettings.Doc.Application.Application.Create.NewEllipse(origin, sectionRadius, sectionRadius, xaxis, zaxis, 0, Circle.RevitPI);
            var arc2 = dynRevitSettings.Doc.Application.Application.Create.NewEllipse(origin, sectionRadius, sectionRadius, xaxis, zaxis, Circle.RevitPI, 2 * Circle.RevitPI);

            // create curve loop from cirle
            var circleLoop = Autodesk.Revit.DB.CurveLoop.Create(new List<Curve>() { arc1, arc2 });

            // extrude the curve and return
            return
                GeometryCreationUtilities.CreateRevolvedGeometry(
                    new Autodesk.Revit.DB.Frame(center, xaxis, yaxis, zaxis), new List<Autodesk.Revit.DB.CurveLoop>() { circleLoop }, 0,
                    2 * Circle.RevitPI);

        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            // unpack input
            var axis = (XYZ)((FScheme.Value.Container)args[0]).Item;
            var center = (XYZ)((FScheme.Value.Container)args[1]).Item;
            var radius = ((FScheme.Value.Number)args[2]).Item;
            var sectionradius = ((FScheme.Value.Number)args[3]).Item;

            // build and return geom
            return FScheme.Value.NewContainer(TorusByAxisOriginRadiusCrossSectionRadius(axis, center, radius, sectionradius));
        }
    }

    [NodeName("Box by Two Corners")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SOLID_PRIMITIVES)]
    [NodeDescription("Create solid box aligned with the world coordinate system given two corner points")]
    public class SolidBoxByTwoCorners : GeometryBase
    {
        public SolidBoxByTwoCorners()
        {
            InPortData.Add(new PortData("bottom", "Bottom point of box", typeof(FScheme.Value.Container), FScheme.Value.NewContainer(new XYZ(-1, -1, -1))));
            InPortData.Add(new PortData("top", "Top point of box", typeof(FScheme.Value.Container), FScheme.Value.NewContainer(new XYZ(1, 1, 1))));

            OutPortData.Add(new PortData("box", "Solid box", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public static Solid AlignedBoxByTwoCorners(XYZ bottomInput, XYZ topInput)
        {
            XYZ top, bottom;
            if (bottomInput.Z > topInput.Z)
            {
                top = bottomInput;
                bottom = topInput;
            }
            else
            {
                top = topInput;
                bottom = bottomInput;
            }

            // obtain coordinates of base rectangle
            var p0 = bottom;
            var p1 = p0 + new XYZ(top.X - bottom.X, 0, 0);
            var p2 = p1 + new XYZ(0, top.Y - bottom.Y, 0);
            var p3 = p2 - new XYZ(top.X - bottom.X, 0, 0);

            // form edges of base rect
            var l1 = dynRevitSettings.Doc.Application.Application.Create.NewLineBound(p0, p1);
            var l2 = dynRevitSettings.Doc.Application.Application.Create.NewLineBound(p1, p2);
            var l3 = dynRevitSettings.Doc.Application.Application.Create.NewLineBound(p2, p3);
            var l4 = dynRevitSettings.Doc.Application.Application.Create.NewLineBound(p3, p0);

            // form curve loop from lines of base rect
            var cl = new Autodesk.Revit.DB.CurveLoop();
            cl.Append(l1);
            cl.Append(l2);
            cl.Append(l3);
            cl.Append(l4);

            // get height of box
            var height = top.Z - bottom.Z;

            // extrude the curve and return
            return
                GeometryCreationUtilities.CreateExtrusionGeometry(new List<Autodesk.Revit.DB.CurveLoop>() { cl },
                    XYZ.BasisZ, height);

        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            // unpack input
            var bottom = (XYZ)((FScheme.Value.Container)args[0]).Item;
            var top = (XYZ)((FScheme.Value.Container)args[1]).Item;

            // build and return geom
            return FScheme.Value.NewContainer(AlignedBoxByTwoCorners(bottom, top));
        }
    }

    [NodeName("Box by Center and Dimensions")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SOLID_PRIMITIVES)]
    [NodeDescription("Create solid box aligned with the world coordinate system given the center of the box and the length of its axes")]
    public class SolidBoxByCenterAndDimensions : GeometryBase
    {
        public SolidBoxByCenterAndDimensions()
        {
            InPortData.Add(new PortData("center", "Center of box", typeof(FScheme.Value.Container), FScheme.Value.NewContainer(new XYZ(0, 0, 0))));
            InPortData.Add(new PortData("x", "Dimension of box in x direction", typeof(FScheme.Value.Number), FScheme.Value.NewNumber(1)));
            InPortData.Add(new PortData("y", "Dimension of box in y direction", typeof(FScheme.Value.Number), FScheme.Value.NewNumber(1)));
            InPortData.Add(new PortData("z", "Dimension of box in z direction", typeof(FScheme.Value.Number), FScheme.Value.NewNumber(1)));

            OutPortData.Add(new PortData("box", "Solid box", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public static Solid AlignedBoxByCenterAndDimensions(XYZ center, double x, double y, double z)
        {

            var bottom = center - new XYZ(x / 2, y / 2, z / 2);
            var top = center + new XYZ(x / 2, y / 2, z / 2);

            return SolidBoxByTwoCorners.AlignedBoxByTwoCorners(bottom, top);

        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            // unpack input
            var center = (XYZ)((FScheme.Value.Container)args[0]).Item;
            var x = ((FScheme.Value.Number)args[1]).Item;
            var y = ((FScheme.Value.Number)args[2]).Item;
            var z = ((FScheme.Value.Number)args[3]).Item;

            // build and return geom
            return FScheme.Value.NewContainer(AlignedBoxByCenterAndDimensions(center, x, y, z));
        }
    }

    #endregion

    #region Solid Manipulation

    [NodeName("Explode Solid")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SOLID_QUERY)]
    [NodeDescription("Creates list of faces of solid or edges of face")]
    public class GeometryObjectsFromRoot : NodeWithOneOutput
    {

        public GeometryObjectsFromRoot()
        {
            InPortData.Add(new PortData("Explode Geometry Object", "Solid to extract faces or face to extract edges", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("Exploded Geometry objects", "List", typeof(FScheme.Value.List)));

            RegisterAllPorts();

        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            Solid thisSolid = null;
            if (((FScheme.Value.Container)args[0]).Item is Solid)
                thisSolid = (Solid)((FScheme.Value.Container)args[0]).Item;

            Autodesk.Revit.DB.Face thisFace = thisSolid == null ? (Autodesk.Revit.DB.Face)(((FScheme.Value.Container)args[0]).Item) : null;

            var result = FSharpList<FScheme.Value>.Empty;

            if (thisSolid != null)
            {
                FaceArray faceArr = thisSolid.Faces;
                var thisEnum = faceArr.GetEnumerator();
                for (; thisEnum.MoveNext(); )
                {
                    Autodesk.Revit.DB.Face curFace = (Autodesk.Revit.DB.Face)thisEnum.Current;
                    if (curFace != null)
                        result = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewContainer(curFace), result);
                }
            }
            else if (thisFace != null)
            {
                EdgeArrayArray loops = thisFace.EdgeLoops;
                var loopsEnum = loops.GetEnumerator();
                for (; loopsEnum.MoveNext(); )
                {
                    EdgeArray thisArr = (EdgeArray)loopsEnum.Current;
                    if (thisArr == null)
                        continue;
                    var oneLoopEnum = thisArr.GetEnumerator();
                    for (; oneLoopEnum.MoveNext(); )
                    {
                        Edge curEdge = (Edge)oneLoopEnum.Current;
                        if (curEdge != null)
                            result = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewContainer(curEdge), result);
                    }
                }
            }

            return FScheme.Value.NewList(result);
        }
    }

    [NodeName("Transform Solid")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_TRANSFORM_APPLY)]
    [NodeDescription("Creates solid by transforming solid")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class TransformSolid : GeometryBase
    {
        public TransformSolid()
        {
            InPortData.Add(new PortData("Solid", "Solid to transform", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("Transform", "Transform to apply", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("Solid", "Resulting Solid", typeof(FScheme.Value.Container)));

            RegisterAllPorts();

        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            Solid thisSolid = (Solid)((FScheme.Value.Container)args[0]).Item;
            Transform transform = (Transform)((FScheme.Value.Container)args[1]).Item;

            Solid result = null;

            Type GeometryCreationUtilitiesType = typeof(Autodesk.Revit.DB.GeometryCreationUtilities);

            MethodInfo[] geometryCreationUtilitiesStaticMethods = GeometryCreationUtilitiesType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            System.String nameOfReplaceMethod = "CreateGeometryByFaceReplacement";

            foreach (MethodInfo ms in geometryCreationUtilitiesStaticMethods)
            {
                if (ms.Name == nameOfReplaceMethod)
                {
                    object[] argsM = new object[3];
                    argsM[0] = thisSolid;
                    argsM[1] = new List<GeometryObject>();
                    argsM[2] = new List<GeometryObject>();
                    result = (Solid)ms.Invoke(null, argsM);
                    break;
                }
            }
            if (result == null)
                throw new Exception(" could not copy solid or validation during copy failed");

            Type SolidType = typeof(Autodesk.Revit.DB.Solid);
            MethodInfo[] solidInstanceMethods = SolidType.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            System.String nameOfMethodTransform = "transform";

            foreach (MethodInfo m in solidInstanceMethods)
            {
                if (m.Name == nameOfMethodTransform)
                {
                    object[] argsM = new object[1];
                    argsM[0] = transform;

                    m.Invoke(result, argsM);

                    break;
                }
            }

            return FScheme.Value.NewContainer(result);
        }
    }

    [NodeName("Replace Solid Faces")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SOLID_MODIFY)]
    [NodeDescription("Build solid replacing faces of input solid by supplied faces")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class ReplaceFacesOfSolid : GeometryBase
    {
        public ReplaceFacesOfSolid()
        {
            InPortData.Add(new PortData("Solid", "Solid to transform", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("Faces", "Faces to be replaced", typeof(FScheme.Value.List)));
            InPortData.Add(new PortData("Faces", "Faces to use", typeof(FScheme.Value.List)));
            OutPortData.Add(new PortData("Solid", "Resulting Solid", typeof(FScheme.Value.Container)));

            RegisterAllPorts();

        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            Solid thisSolid = (Solid)((FScheme.Value.Container)args[0]).Item;

            var facesToBeReplaced = ((FScheme.Value.List)args[1]).Item.Select(
               x => ((GeometryObject)((FScheme.Value.Container)x).Item)).ToList();

            var facesToReplaceWith = ((FScheme.Value.List)args[2]).Item.Select(
               x => ((GeometryObject)((FScheme.Value.Container)x).Item)).ToList();

            Type GeometryCreationUtilitiesType = typeof(Autodesk.Revit.DB.GeometryCreationUtilities);

            MethodInfo[] geometryCreationUtilitiesStaticMethods = GeometryCreationUtilitiesType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            System.String nameOfReplaceMethod = "CreateGeometryByFaceReplacement";

            Solid result = null;

            foreach (MethodInfo ms in geometryCreationUtilitiesStaticMethods)
            {
                if (ms.Name == nameOfReplaceMethod)
                {
                    object[] argsM = new object[3];
                    argsM[0] = thisSolid;
                    argsM[1] = facesToBeReplaced;
                    argsM[2] = facesToReplaceWith;
                    result = (Solid)ms.Invoke(null, argsM);
                }
            }
            if (result == null)
                throw new Exception(" could not make solid by replacement of face or faces");

            return FScheme.Value.NewContainer(result);
        }
    }

    [NodeName("Fillet Solid Edges")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SOLID_MODIFY)]
    [NodeDescription("Build solid by replace edges with round blends")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class BlendEdges : GeometryBase
    {
        public BlendEdges()
        {
            InPortData.Add(new PortData("Solid", "Solid to transform", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("Edges", "Edges to be blends", typeof(FScheme.Value.List)));
            InPortData.Add(new PortData("Radius", "Radius of blend", typeof(FScheme.Value.Number)));
            OutPortData.Add(new PortData("Solid", "Resulting Solid", typeof(FScheme.Value.Container)));

            RegisterAllPorts();

        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            Solid thisSolid = (Solid)((FScheme.Value.Container)args[0]).Item;

            FSharpList<FScheme.Value> vals = ((FScheme.Value.List)args[1]).Item;
            List<GeometryObject> edgesToBeReplaced = new List<GeometryObject>();

            var doc = dynRevitSettings.Doc;

            for (int ii = 0; ii < vals.Count(); ii++)
            {
                var item = ((FScheme.Value.Container)vals[ii]).Item;

                if (item is Reference)
                {
                    Reference refEdge = (Reference)item;
                    Element selectedElement = doc.Document.GetElement(refEdge);

                    GeometryObject edge = selectedElement.GetGeometryObjectFromReference(refEdge);
                    if (edge is Edge)
                        edgesToBeReplaced.Add(edge);
                }
                else if (item is Edge)
                {
                    GeometryObject edge = (Edge)item;
                    edgesToBeReplaced.Add(edge);
                }
            }

            double radius = ((FScheme.Value.Number)args[2]).Item;

            System.Reflection.Assembly revitAPIAssembly = System.Reflection.Assembly.GetAssembly(typeof(GeometryCreationUtilities));
            Type SolidModificationUtilsType = revitAPIAssembly.GetType("Autodesk.Revit.DB.SolidModificationUtils", false);

            if (SolidModificationUtilsType == null)
                throw new Exception(" could not make edge chamfer");

            MethodInfo[] solidModificationUtilsStaticMethods = SolidModificationUtilsType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            System.String nameOfReplaceMethod = "ExecuteShapingOfEdges";

            Solid result = null;

            foreach (MethodInfo ms in solidModificationUtilsStaticMethods)
            {
                if (ms.Name == nameOfReplaceMethod)
                {
                    object[] argsM = new object[4];
                    bool isRound = true;
                    argsM[0] = thisSolid;
                    argsM[1] = isRound;
                    argsM[2] = radius;
                    argsM[3] = edgesToBeReplaced;
                    result = (Solid)ms.Invoke(null, argsM);
                    break;
                }
            }
            if (result == null)
                throw new Exception(" could not make solid by blending requested edges with given radius");

            return FScheme.Value.NewContainer(result);
        }
    }

    [NodeName("Chamfer Solid Edges")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SOLID_MODIFY)]
    [NodeDescription("Build solid by replace edges with chamfers")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class ChamferEdges : GeometryBase
    {
        public ChamferEdges()
        {
            InPortData.Add(new PortData("Solid", "Solid to transform", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("Edges", "Edges to be blends", typeof(FScheme.Value.List)));
            InPortData.Add(new PortData("Size", "Size of chamfer ", typeof(FScheme.Value.Number)));
            OutPortData.Add(new PortData("Solid", "Resulting Solid", typeof(FScheme.Value.Container)));

            RegisterAllPorts();

        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var thisSolid = (Solid)((FScheme.Value.Container)args[0]).Item;

            var vals = ((FScheme.Value.List)args[1]).Item;
            var edgesToBeReplaced = new List<GeometryObject>();
            var doc = dynRevitSettings.Doc;

            for (int ii = 0; ii < vals.Count(); ii++)
            {
                var item = ((FScheme.Value.Container)vals[ii]).Item;

                if (item is Reference)
                {
                    Reference refEdge = (Reference)item;
                    Element selectedElement = doc.Document.GetElement(refEdge);

                    GeometryObject edge = selectedElement.GetGeometryObjectFromReference(refEdge);
                    if (edge is Edge)
                        edgesToBeReplaced.Add(edge);
                }
                else if (item is Edge)
                {
                    GeometryObject edge = (Edge)item;
                    edgesToBeReplaced.Add(edge);
                }
            }

            double size = ((FScheme.Value.Number)args[2]).Item;

            System.Reflection.Assembly revitAPIAssembly = System.Reflection.Assembly.GetAssembly(typeof(GeometryCreationUtilities));
            Type SolidModificationUtilsType = revitAPIAssembly.GetType("Autodesk.Revit.DB.SolidModificationUtils", false);

            if (SolidModificationUtilsType == null)
                throw new Exception(" could not make edge chamfer");


            MethodInfo[] solidModificationUtilsStaticMethods = SolidModificationUtilsType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            System.String nameOfReplaceMethod = "ExecuteShapingOfEdges";

            Solid result = null;

            foreach (MethodInfo ms in solidModificationUtilsStaticMethods)
            {
                if (ms.Name == nameOfReplaceMethod)
                {
                    object[] argsM = new object[4];
                    bool isRound = false;
                    argsM[0] = thisSolid;
                    argsM[1] = isRound;
                    argsM[2] = size;
                    argsM[3] = edgesToBeReplaced;
                    result = (Solid)ms.Invoke(null, argsM);
                    break;
                }
            }
            if (result == null)
                throw new Exception(" could not make solid by chamfering requested edges with given chamfer size");

            return FScheme.Value.NewContainer(result);
        }
    }

    [NodeName("Find Missing Solid Face Boundaries")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SOLID_REPAIR)]
    [NodeDescription("List open faces of solid as CurveLoops")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class OnesidedEdgesAsCurveLoops : NodeWithOneOutput
    {

        public OnesidedEdgesAsCurveLoops()
        {
            InPortData.Add(new PortData("Incomplete Solid", "Geometry to check for being Solid", typeof(object)));
            InPortData.Add(new PortData("CurveLoops", "Additional curve loops ready for patching", typeof(FScheme.Value.List)));
            OutPortData.Add(new PortData("Onesided boundaries", "Onesided Edges as CurveLoops", typeof(FScheme.Value.List)));

            RegisterAllPorts();

        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var thisSolid = (Solid)((FScheme.Value.Container)args[0]).Item;
            var listIn = ((FScheme.Value.List)args[1]).Item.Select(
                    x => ((Autodesk.Revit.DB.CurveLoop)((FScheme.Value.Container)x).Item)
                       ).ToList();

            Type SolidType = typeof(Autodesk.Revit.DB.Solid);

            MethodInfo[] solidTypeMethods = SolidType.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            var nameOfMethodCreate = "oneSidedEdgesAsCurveLoops";
            List<Autodesk.Revit.DB.CurveLoop> oneSidedAsLoops = null;

            foreach (MethodInfo m in solidTypeMethods)
            {
                if (m.Name == nameOfMethodCreate)
                {
                    object[] argsM = new object[1];
                    argsM[0] = listIn;

                    oneSidedAsLoops = (List<Autodesk.Revit.DB.CurveLoop>)m.Invoke(thisSolid, argsM);

                    break;
                }
            }

            var result = FSharpList<FScheme.Value>.Empty;
            var thisEnum = oneSidedAsLoops.GetEnumerator();

            for (; thisEnum.MoveNext(); )
            {
                result = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewContainer((Autodesk.Revit.DB.CurveLoop)thisEnum.Current), result);
            }


            return FScheme.Value.NewList(result);
        }
    }

    [NodeName("Replace Missing Solid Faces")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SOLID_REPAIR)]
    [NodeDescription("Patch set of faces as Solid ")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class PatchSolid : GeometryBase
    {

        public PatchSolid()
        {
            InPortData.Add(new PortData("Incomplete Solid", "Geometry to check for being Solid", typeof(object)));
            InPortData.Add(new PortData("CurveLoops", "Additional curve loops ready for patching", typeof(FScheme.Value.List)));
            InPortData.Add(new PortData("Faces", "Faces to exclude", typeof(FScheme.Value.List)));

            OutPortData.Add(new PortData("Result", "Computed Solid", typeof(object)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var thisSolid = (Solid)((FScheme.Value.Container)args[0]).Item;

            var listInCurveLoops = ((FScheme.Value.List)args[1]).Item.Select(
                    x => ((Autodesk.Revit.DB.CurveLoop)((FScheme.Value.Container)x).Item)
                       ).ToList();

            var listInFacesToExclude = ((FScheme.Value.List)args[2]).Item.Select(
                    x => ((Autodesk.Revit.DB.Face)((FScheme.Value.Container)x).Item)
                       ).ToList();

            var SolidType = typeof(Autodesk.Revit.DB.Solid);

            var solidTypeMethods = SolidType.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            var nameOfMethodCreate = "patchSolid";
            Solid resultSolid = null;

            foreach (MethodInfo m in solidTypeMethods)
            {
                if (m.Name == nameOfMethodCreate)
                {
                    object[] argsM = new object[2];
                    argsM[0] = listInCurveLoops;
                    argsM[1] = listInFacesToExclude;

                    resultSolid = (Solid)m.Invoke(thisSolid, argsM);

                    break;
                }
            }
            if (resultSolid == null)
                throw new Exception("Could not make patched solid, list Onesided Edges to investigate");

            return FScheme.Value.NewContainer(resultSolid);
        }
    }

    [NodeName("Solid From Curve Loops")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SOLID_CREATE)]
    [NodeDescription("Build a solid from a collection of curve loops")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class SkinCurveLoops : GeometryBase
    {

        public SkinCurveLoops()
        {
            InPortData.Add(new PortData("CurveLoops", "Additional curve loops ready for patching", typeof(FScheme.Value.List)));
            OutPortData.Add(new PortData("Result", "Computed Solid", typeof(object)));

            RegisterAllPorts();
        }

        public static bool noSkinSolidMethod()
        {

            Type SolidType = typeof(Autodesk.Revit.DB.Solid);

            MethodInfo[] solidTypeMethods = SolidType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            System.String nameOfMethodCreate = "skinCurveLoopsIntoSolid";
            bool methodFound = false;

            foreach (MethodInfo m in solidTypeMethods)
            {
                if (m.Name == nameOfMethodCreate)
                {
                    methodFound = true;

                    break;
                }
            }

            return !methodFound;
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var listInCurveLoops = ((FScheme.Value.List)args[0]).Item.Select(
                    x => ((Autodesk.Revit.DB.CurveLoop)((FScheme.Value.Container)x).Item)
                       ).ToList();

            Type SolidType = typeof(Autodesk.Revit.DB.Solid);

            MethodInfo[] solidTypeMethods = SolidType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            System.String nameOfMethodCreate = "skinCurveLoopsIntoSolid";
            Solid resultSolid = null;
            bool methodFound = false;

            foreach (MethodInfo m in solidTypeMethods)
            {
                if (m.Name == nameOfMethodCreate)
                {
                    object[] argsM = new object[1];
                    argsM[0] = listInCurveLoops;

                    resultSolid = (Solid)m.Invoke(null, argsM);
                    methodFound = true;

                    break;
                }
            }

            if (!methodFound)
                throw new Exception("This method uses later version of RevitAPI.dll with skinCurveLoopsIntoSolid method. Please use Patch Solid node instead.");
            if (resultSolid == null)
                throw new Exception("Failed to make solid, please check the input.");

            return FScheme.Value.NewContainer(resultSolid);
        }
    }

    #endregion
}
