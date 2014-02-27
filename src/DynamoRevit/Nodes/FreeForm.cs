using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.DB;
using Dynamo.Models;
using Dynamo.Revit;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;

namespace Dynamo.Nodes
{
    [NodeName("Bake Solid as Revit Element")]
    [NodeCategory(BuiltinNodeCategories.REVIT_BAKE)]
    [NodeDescription("Turn a solid into a revit element.  Internally this is a FreeForm element")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    public class FreeForm : RevitTransactionNodeWithOneOutput
    {
        public static Dictionary<ElementId, Solid> freeFormSolids = null;
        public static Dictionary<ElementId, ElementId> previouslyDeletedFreeForms = null;
        public FreeForm()
        {
            InPortData.Add(new PortData("solid", "solid to use for Freeform", typeof(object)));

            OutPortData.Add(new PortData("form", "Free Form", typeof(object)));

            RegisterAllPorts();
            if (freeFormSolids == null)
                freeFormSolids = new Dictionary<ElementId, Solid>();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            ElementId deleteId = ElementId.InvalidElementId;


            //Surface argument
            Solid mySolid = (Solid)((FScheme.Value.Container)args[0]).Item;

            GenericForm ffe = null;
            //use reflection to check for the method

            System.Reflection.Assembly revitAPIAssembly = System.Reflection.Assembly.GetAssembly(typeof(GenericForm));
            Type FreeFormType = revitAPIAssembly.GetType("Autodesk.Revit.DB.FreeFormElement", true);
            bool methodCalled = false;

            bool usedUpdateMethod = false;

            if (FreeFormType != null)
            {
                if (this.Elements.Any())
                {
                    MethodInfo[] freeFormInstanceMethods = FreeFormType.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    System.String nameOfMethodUpdate = "UpdateToSolidGeometry";
                    System.String nameOfMethodUpdateAlt = "UpdateSolidGeometry";

                    foreach (MethodInfo mInst in freeFormInstanceMethods)
                    {
                        if (mInst.Name == nameOfMethodUpdate || mInst.Name == nameOfMethodUpdateAlt)
                        {
                            object[] argsM = new object[1];
                            argsM[0] = mySolid;
                            Element e;
                            if (dynUtils.TryGetElement(this.Elements[0], out e) && e is GenericForm)
                            {
                                ffe = e as GenericForm;
                                object[] argsMInst = new object[1];
                                argsMInst[0] = mySolid;
                                usedUpdateMethod = true;
                                mInst.Invoke(e, argsMInst);
                                break;
                            }
                        }
                    }
                }

                if (!usedUpdateMethod)
                {
                    MethodInfo[] freeFormMethods = FreeFormType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                    System.String nameOfMethodCreate = "Create";
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
            }

            //If we already have a form stored...
            if (!usedUpdateMethod && this.Elements.Any())
            {
                //And register the form for deletion. Since we've already deleted it here manually, we can 
                //pass "true" as the second argument.
                deleteId = this.Elements[0];
                this.DeleteElement(this.Elements[0], false);
            }

            if (ffe != null)
            {
                if (!usedUpdateMethod)
                    this.Elements.Add(ffe.Id);
                freeFormSolids[ffe.Id] = mySolid;
                if (deleteId != ElementId.InvalidElementId && !usedUpdateMethod)
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

            return FScheme.Value.NewContainer(ffe);
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSRevitNodes.dll",
                "FreeForm.BySolid", "FreeForm.BySolid@Solid");
        }
    }
}
