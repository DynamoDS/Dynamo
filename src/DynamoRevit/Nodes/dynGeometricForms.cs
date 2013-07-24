using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Connectors;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
using Dynamo.FSchemeInterop;
using Dynamo.Revit;
using System.Reflection;
using System.Xml;

namespace Dynamo.Nodes
{
    [NodeName("Loft Form")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SURFACE)]
    [NodeDescription("Creates a new loft form <doc.FamilyCreate.NewLoftForm>")]
    public class dynLoftForm : dynRevitTransactionNodeWithOneOutput
    {
        public dynLoftForm()
        {
            InPortData.Add(new PortData("solid/void", "Indicates if the Form is Solid or Void. Use True for solid and false for void.", typeof(Value.Number)));
            InPortData.Add(new PortData("list", "A list of profiles for the Loft Form. The recommended way is to use list of Planar Ref Curve Chains, list of lists and list of curves are supported for legacy graphs.", typeof(Value.List)));
            InPortData.Add(new PortData("surface?", "Create a single surface or an extrusion if one loop", typeof(Value.Container)));

            OutPortData.Add(new PortData("form", "Loft Form", typeof(object)));

            RegisterAllPorts();
            if (formId == null)
                formId = ElementId.InvalidElementId;
        }

        Dictionary<ElementId, ElementId> sformCurveToReferenceCurveMap;
        ElementId formId;
        bool preferSurfaceForOneLoop;

        public override bool acceptsListOfLists(FScheme.Value value)
        {
            if (Utils.IsListOfListsOfLists(value))
                return false;

            FSharpList<Value> vals = ((Value.List)value).Item;
            if (!vals.Any() || !(vals[0] is Value.List))
                return true;
            FSharpList<Value> firstListInList = ((Value.List)vals[0]).Item;
            if (!firstListInList.Any() || !(firstListInList[0] is Value.Container))
                return true;
            var var1 = ((Value.Container)firstListInList[0]).Item;
            if (var1 is ModelCurveArray)
                return false;

            return true;
        }

        bool matchOrAddFormCurveToReferenceCurveMap(Form formElement, ReferenceArrayArray refArrArr, bool doMatch)
        {
            if (formElement.Id != formId && doMatch)
            {
                return false;
            }
            else if (!doMatch)
                formId = formElement.Id;

            if (doMatch && sformCurveToReferenceCurveMap.Count == 0)
                return false;
            else if (!doMatch)
                sformCurveToReferenceCurveMap = new Dictionary<ElementId, ElementId>();

            for (int indexRefArr = 0; indexRefArr < refArrArr.Size; indexRefArr++)
            {
                if (indexRefArr >= refArrArr.Size)
                {
                    if (!doMatch)
                        sformCurveToReferenceCurveMap.Clear();
                    return false;
                }

                if (refArrArr.get_Item(indexRefArr).Size != formElement.get_CurveLoopReferencesOnProfile(indexRefArr, 0).Size)
                {
                    if (!doMatch)
                        sformCurveToReferenceCurveMap.Clear();
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
                            sformCurveToReferenceCurveMap.Clear();
                        return false;
                    }
                    ElementId oldRefId = oldRef.ElementId;
                    ElementId newRefId = newRef.ElementId;

                    if (doMatch && (!sformCurveToReferenceCurveMap.ContainsKey(newRefId) ||
                                    sformCurveToReferenceCurveMap[newRefId] != oldRefId)
                       )
                    {
                        return false;
                    }
                    else if (!doMatch)
                        sformCurveToReferenceCurveMap[newRefId] = oldRefId;
                }
            }
            return true;
        }


        public override Value Evaluate(FSharpList<Value> args)
        {

            //Solid argument
            bool isSolid = ((Value.Number)args[0]).Item == 1;

            //Surface argument
            bool isSurface = ((Value.Number)args[2]).Item == 1;

            //Build up our list of list of references for the form by...
            var curvesListList = (Value.List)args[1];
            //Now we add all of those references into ReferenceArrays
            ReferenceArrayArray refArrArr = new ReferenceArrayArray();

            FSharpList<Value> vals = ((Value.List)curvesListList).Item;

            if (vals.Any() && (vals[0] is Value.Container) && ((Value.Container)vals[0]).Item is ModelCurveArray)  
            {
                //Build a sequence that unwraps the input list from it's Value form.
                IEnumerable<ModelCurveArray> modelCurveArrays = ((Value.List)args[1]).Item.Select(
                   x => (ModelCurveArray)((Value.Container)x).Item
                );

                foreach (var modelCurveArray in modelCurveArrays)
                {
                    var refArr = new ReferenceArray();
                    foreach (ModelCurve modelCurve in modelCurveArray)
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
                   delegate(Value x)
                   {
                       //If the element in the topmost list is a sub-list...
                       if (x.IsList)
                       {
                           //...then we return a new IEnumerable of References by converting the sub list.
                           return (x as Value.List).Item.Select(
                              delegate(Value y)
                              {
                                  //Since we're in a sub-list, we can assume it's a container.
                                  var item = ((Value.Container)y).Item;
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
                           var obj = ((Value.Container)x).Item;
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
                //is this same element?
                Element e = null;
                if (dynUtils.TryGetElement(this.Elements[0], typeof(Form), out e) && e != null &&
                    e is Form)
                {
                    Form oldF = (Form)e;
                    if (oldF.IsSolid == isSolid  &&
                        preferSurfaceForOneLoop == isSurface 
                        && matchOrAddFormCurveToReferenceCurveMap(oldF, refArrArr, true))
                    {
                            return Value.NewContainer(oldF);
                    }
                }

                //Dissolve it, we will re-make it later.
                if (FormUtils.CanBeDissolved(this.UIDocument.Document, this.Elements.Take(1).ToList()))
                   FormUtils.DissolveForms(this.UIDocument.Document, this.Elements.Take(1).ToList());
                //And register the form for deletion. Since we've already deleted it here manually, we can 
                //pass "true" as the second argument.
                this.DeleteElement(this.Elements[0], true);

            }
            else if (this.formId != ElementId.InvalidElementId)
            {
                Element e = null;
                if (dynUtils.TryGetElement(this.formId, typeof(Form), out e) && e != null &&
                    e is Form)
                {
                    Form oldF = (Form)e;
                    if (oldF.IsSolid == isSolid  &&
                        preferSurfaceForOneLoop == isSurface 
                        && matchOrAddFormCurveToReferenceCurveMap(oldF, refArrArr, true))
                    {
                        return Value.NewContainer(oldF);
                    }
                }
            }

            preferSurfaceForOneLoop = isSurface;

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

            return Value.NewContainer(f);
        }

        public override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            dynEl.SetAttribute("FormId", formId.ToString());
            dynEl.SetAttribute("PreferSurfaceForOneLoop", preferSurfaceForOneLoop.ToString());

            String mapAsString = "";

            if (sformCurveToReferenceCurveMap != null)
            {
                var enumMap = sformCurveToReferenceCurveMap.GetEnumerator();
                for (; enumMap.MoveNext(); )
                {
                    ElementId keyId = enumMap.Current.Key;
                    ElementId valueId = enumMap.Current.Value;

                    mapAsString = mapAsString + keyId.ToString() + "=" + valueId.ToString() + ";";
                }
            }
            dynEl.SetAttribute("FormCurveToReferenceCurveMap", mapAsString);
        }

        public override void LoadNode(XmlNode elNode)
        {
            try
            {
                formId = new ElementId(Convert.ToInt32(elNode.Attributes["FormId"].Value));
                var thisIsSurface = elNode.Attributes["PreferSurfaceForOneLoop"];
                if (thisIsSurface != null)
                   preferSurfaceForOneLoop = Convert.ToBoolean(thisIsSurface.Value);
                else //used to be able to make only surface, so init to more likely value
                   preferSurfaceForOneLoop = true;

                string mapAsString = elNode.Attributes["FormCurveToReferenceCurveMap"].Value;
                sformCurveToReferenceCurveMap = new Dictionary<ElementId,ElementId>();
                if (mapAsString != "")
                {

                    string[] curMap = mapAsString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    int mapSize = curMap.Length;
                    for (int iMap = 0; iMap < mapSize; iMap++)
                    {
                        string[] thisMap = curMap[iMap].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                        if (thisMap.Length != 2)
                        {
                            sformCurveToReferenceCurveMap = new Dictionary<ElementId, ElementId>();
                            break;
                        }
                        ElementId keyId = new ElementId(Convert.ToInt32(thisMap[0]));
                        ElementId valueId = new ElementId(Convert.ToInt32(thisMap[1]));
                        sformCurveToReferenceCurveMap[keyId] = valueId;
                    }
                }
            }
            catch 
            {
                sformCurveToReferenceCurveMap = new Dictionary<ElementId, ElementId>();
            }
        }
    }

    [NodeName("Free Form")]
    [NodeCategory(BuiltinNodeCategories.REVIT)]
    [NodeDescription("Creates a free form <FreeFormElement.Create>")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    public class dynFreeForm : dynRevitTransactionNodeWithOneOutput
    {
        public static Dictionary <ElementId, Solid> freeFormSolids = null;
        public static Dictionary <ElementId, ElementId> previouslyDeletedFreeForms = null;
        public dynFreeForm()
        {
            InPortData.Add(new PortData("solid", "solid to use for Freeform", typeof(object)));

            OutPortData.Add(new PortData("form", "Free Form", typeof(object)));

            RegisterAllPorts();
            if (freeFormSolids == null)
                freeFormSolids = new Dictionary<ElementId, Solid>();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            ElementId deleteId = ElementId.InvalidElementId;

            //If we already have a form stored...
            if (this.Elements.Any())
            {
                //And register the form for deletion. Since we've already deleted it here manually, we can 
                //pass "true" as the second argument.
                this.DeleteElement(this.Elements[0], true);
            }

            //Surface argument
            Solid mySolid = (Solid)((Value.Container)args[0]).Item;

            GenericForm ffe = null;
            //use reflection to check for the method

            System.Reflection.Assembly revitAPIAssembly = System.Reflection.Assembly.GetAssembly(typeof(GenericForm));
            Type FreeFormType = revitAPIAssembly.GetType("Autodesk.Revit.DB.FreeFormElement", true);
            bool methodCalled = false;

            if (FreeFormType != null)
            {
                MethodInfo[] freeFormMethods = FreeFormType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                String nameOfMethodCreate = "Create";
                foreach (MethodInfo m in freeFormMethods)
                {
                    if (m.Name == nameOfMethodCreate)
                    {
                        object[] argsM = new object[2];
                        argsM[0] = this.UIDocument.Document;
                        argsM[1] = mySolid;

                        methodCalled = true;

                        ffe = (GenericForm)m.Invoke(null, argsM);
                        break;
                    }
                }
            }
            if (ffe != null)
            {
                this.Elements.Add(ffe.Id);
                freeFormSolids[ffe.Id] = mySolid;
                if (deleteId != ElementId.InvalidElementId)
                {
                    if (previouslyDeletedFreeForms == null)
                        previouslyDeletedFreeForms = new Dictionary<ElementId, ElementId>();
                    previouslyDeletedFreeForms[ffe.Id] = deleteId;
                    if (previouslyDeletedFreeForms.ContainsKey(deleteId))
                    {
                        ElementId previouslyDeletedId = previouslyDeletedFreeForms[deleteId];
                        if (previouslyDeletedId != ElementId.InvalidElementId)
                           freeFormSolids.Remove(previouslyDeletedFreeForms[deleteId]);
                        previouslyDeletedFreeForms.Remove(deleteId);
                    }
                }
            }
            else if (!methodCalled)
                throw new Exception("This method is not available before 2014 release.");

            return Value.NewContainer(ffe);
        }
    }

    [NodeName("Ref Curve Chain")]
    [NodeCategory(BuiltinNodeCategories.REVIT_BAKE)]
    [NodeDescription("Creates continuous chain of reference curves ")]
    public class dynPlanarRefCurveChain : dynRevitTransactionNodeWithOneOutput
    {
        public dynPlanarRefCurveChain()
        {
            InPortData.Add(new PortData("list", "A list of ref curves to make one planar chain", typeof(Value.List)));

            OutPortData.Add(new PortData("Chain", "Chain of ref. curves ready to be one profile for nodes like Loft Form", typeof(ModelCurveArray)));

            RegisterAllPorts();
        }
        public override Value Evaluate(FSharpList<Value> args)
        {
            var doc = dynRevitSettings.Doc;
            var refCurveList = ((Value.List)args[0]).Item.Select(
               x => ( ((Value.Container)x).Item is ModelCurve ?
                   ((ModelCurve)((Value.Container)x).Item)
                   : (ModelCurve)(
                                      doc.Document.GetElement( 
                                             ((Reference) ((Value.Container)x).Item).ElementId)
                                                             )
                                 )
                   ).ToList();

            ModelCurveArray myModelCurves = new ModelCurveArray();
      
            //Plane thisPlane = null;
            //Line oneLine = null;

            List<ElementId> refIds = new List<ElementId>();
            XYZ loopStart = new XYZ();
            XYZ otherEnd = new XYZ();
            int index = 0;
            double tolerance = 0.000000001;
            foreach( var refCurve in refCurveList)
            {
                if (index == 0)
                {
                    loopStart = refCurve.GeometryCurve.Evaluate(0.0, true);
                    otherEnd = refCurve.GeometryCurve.Evaluate(1.0, true);
                }
                else //if (index > 0)
                {
                    XYZ startXYZ = refCurve.GeometryCurve.Evaluate(0.0, true);
                    XYZ endXYZ = refCurve.GeometryCurve.Evaluate(1.0, true);
                    if (index == 1)
                    {
                        if (startXYZ.DistanceTo(otherEnd) > tolerance && endXYZ.DistanceTo(otherEnd) > tolerance &&
                            (startXYZ.DistanceTo(loopStart) > tolerance || endXYZ.DistanceTo(loopStart) > tolerance))
                        {
                            XYZ temp = loopStart;
                            loopStart = otherEnd;
                            otherEnd = temp;
                        }
                        if (startXYZ.DistanceTo(otherEnd) > tolerance && endXYZ.DistanceTo(otherEnd) < tolerance)
                            otherEnd = startXYZ;
                        else if (startXYZ.DistanceTo(otherEnd) <tolerance && endXYZ.DistanceTo(otherEnd) >tolerance)
                            otherEnd = endXYZ;
                        else
                            throw new Exception("Gap between curves in chain of reference curves.");
                    }                 
                }
                /* not needed check
                if (refCurve.GeometryCurve is Line)
                {
                    Line thisLine = refCurve.GeometryCurve as Line;
                    if (thisPlane != null)
                    {
                        if (Math.Abs(thisPlane.Normal.DotProduct(thisLine.Direction)) > tolerance)
                            throw new Exception(" Planar Ref Curve Chain fails: not planar");
                        if (Math.Abs(thisPlane.Normal.DotProduct(thisLine.Origin - thisPlane.Origin)) > tolerance)
                            throw new Exception(" Planar Ref Curve Chain fails: not planar");
                    }
                    else if (oneLine == null)
                        oneLine = thisLine;
                    else
                    {
                        if (Math.Abs(oneLine.Direction.DotProduct(thisLine.Direction)) > 1.0 - tolerance)
                        {
                            double projAdjust = oneLine.Direction.DotProduct(oneLine.Origin - thisLine.Origin);
                            XYZ adjustedOrigin = thisLine.Origin + projAdjust * oneLine.Direction;
                            if (adjustedOrigin.DistanceTo(oneLine.Origin) > tolerance)
                                throw new Exception(" Planar Ref Curve Chain fails: not planar");
                        }
                        else
                        {
                            XYZ norm = oneLine.Direction.CrossProduct(thisLine.Direction);
                            norm = norm.Normalize();
                            thisPlane = new Plane(norm, oneLine.Origin);
                            if (Math.Abs(thisPlane.Normal.DotProduct(thisLine.Origin - thisPlane.Origin)) > tolerance)
                                throw new Exception(" Planar Ref Curve Chain fails: not planar");
                        }

                    }
                }
                else
                {
                    CurveLoop curveLoop = new CurveLoop();
                    curveLoop.Append(refCurve.GeometryCurve);
                    if (!curveLoop.HasPlane())
                        throw new Exception(" Planar Ref Curve Chain fails: curve is not planar.");
                    Plane curvePlane = curveLoop.GetPlane();
                    if (thisPlane == null && oneLine == null)
                        thisPlane = curveLoop.GetPlane();
                    else if (thisPlane != null)
                    {
                        if (Math.Abs(thisPlane.Normal.DotProduct(curvePlane.Normal)) < 1.0 - tolerance)
                            throw new Exception(" Planar Ref Curve Chain fails: not planar");
                        if (Math.Abs(thisPlane.Normal.DotProduct(curvePlane.Origin - thisPlane.Origin)) > tolerance)
                            throw new Exception(" Planar Ref Curve Chain fails: not planar");
                    }
                    else if (oneLine != null)
                    {
                        thisPlane = curvePlane;
                        if (Math.Abs(thisPlane.Normal.DotProduct(oneLine.Direction)) > tolerance)
                            throw new Exception(" Planar Ref Curve Chain fails: not planar");
                        if (Math.Abs(thisPlane.Normal.DotProduct(oneLine.Origin - thisPlane.Origin)) > tolerance)
                            throw new Exception(" Planar Ref Curve Chain fails: not planar");
                    }
                }
                */

                refIds.Add(refCurve.Id);
                myModelCurves.Append(refCurve);
                index++;
            }

            List<ElementId> removeIds = new List<ElementId>();
            foreach (ElementId oldId in this.Elements)
            {
                if (!refIds.Contains(oldId))
                {
                    removeIds.Add(oldId);
                }
            }

            foreach (ElementId removeId in removeIds)
            {
                    this.Elements.Remove(removeId);
            }
            foreach (ElementId newId in refIds)
            {
                if (!this.Elements.Contains(newId))
                    this.Elements.Add(newId);
            }
            //if (!curveLoop.HasPlane())
            //    throw new Exception(" Planar Ref Curve Chain fails: not planar");
            return Value.NewContainer(myModelCurves);
        }
    }
}
