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
            InPortData.Add(new PortData("list", "A list of curves. The profile set of the newly created loft. Each profile should consist of only one curve loop. The input profile must be in one plane.", typeof(Value.List)));
            InPortData.Add(new PortData("surface?", "Create a single surface or an extrusion if one loop", typeof(Value.Container)));

            OutPortData.Add(new PortData("form", "Loft Form", typeof(object)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            //If we already have a form stored...
            if (this.Elements.Any())
            {
                //Dissolve it, we will re-make it later.
                FormUtils.DissolveForms(this.UIDocument.Document, this.Elements.Take(1).ToList());
                //And register the form for deletion. Since we've already deleted it here manually, we can 
                //pass "true" as the second argument.
                this.DeleteElement(this.Elements[0], true);
            }

            //Solid argument
            bool isSolid = ((Value.Number)args[0]).Item == 1;

            //Surface argument
            bool isSurface = ((Value.Number)args[2]).Item == 1;

            //Build up our list of list of references for the form by...
            IEnumerable<IEnumerable<Reference>> refArrays = ((Value.List)args[1]).Item.Select(
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
            ReferenceArrayArray refArrArr = new ReferenceArrayArray();
            foreach (IEnumerable<Reference> refs in refArrays.Where(x => x.Any()))
            {
                var refArr = new ReferenceArray();
                foreach (Reference r in refs)
                    refArr.Append(r);
                refArrArr.Append(refArr);
            }

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

            this.Elements.Add(f.Id);

            return Value.NewContainer(f);
        }
    }

    [NodeName("Free Form")]
    [NodeCategory(BuiltinNodeCategories.REVIT)]
    [NodeDescription("Creates a free form <FreeFormElement.Create>")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    public class dynFreeForm : dynRevitTransactionNodeWithOneOutput
    {
        public static Dictionary <ElementId, Solid> freeFormSolids = null;
        public dynFreeForm()
        {
            InPortData.Add(new PortData("solid", "solid to use for Freeform", typeof(Value.List)));

            OutPortData.Add(new PortData("form", "Free Form", typeof(object)));

            RegisterAllPorts();
            if (freeFormSolids == null)
                freeFormSolids = new Dictionary<ElementId, Solid>();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
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
            }
            else if (!methodCalled)
                throw new Exception("This method is not available before 2014 release.");

            return Value.NewContainer(ffe);
        }
    }
}
