using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Connectors;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Expression = Dynamo.FScheme.Expression;
using Dynamo.FSchemeInterop;

namespace Dynamo.Elements
{
    [ElementName("Loft Form")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Creates a new loft form <doc.FamilyCreate.NewLoftForm>")]
    [RequiresTransaction(true)]
    public class dynLoftForm : dynNode
    {
        public dynLoftForm()
        {
            InPortData.Add(new PortData("solid/void", "True creates a solid, false a void", typeof(object)));
            InPortData.Add(new PortData("refListList", "ReferenceArrayArray", typeof(object)));
            InPortData.Add(new PortData("surface?", "Create a single surface or an extrusion if one loop", typeof(object)));

            OutPortData = new PortData("form", "Loft Form", typeof(object));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
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
            bool isSolid = ((Expression.Number)args[0]).Item == 1;

            //Surface argument
            bool isSurface = ((Expression.Number)args[2]).Item == 1;

            //Build up our list of list of references for the form by...
            IEnumerable<IEnumerable<Reference>> refArrays = ((Expression.List)args[1]).Item.Select(
                //...first selecting everything in the topmost list...
               delegate(Expression x)
               {
                   //If the element in the topmost list is a sub-list...
                   if (x.IsList)
                   {
                       //...then we return a new IEnumerable of References by converting the sub list.
                       return (x as Expression.List).Item.Select(
                          delegate(Expression y)
                          {
                              //Since we're in a sub-list, we can assume it's a container.
                              var item = ((Expression.Container)y).Item;
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
                       var obj = ((Expression.Container)x).Item;
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

            return Expression.NewContainer(f);
        }
    }
}
