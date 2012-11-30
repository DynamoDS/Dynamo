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

using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Dynamo.Connectors;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;

using Expression = Dynamo.FScheme.Expression;
using Dynamo.FSchemeInterop;

namespace Dynamo.Elements
{
    [ElementName("Drafting View")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Creates a drafting view.")]
    [RequiresTransaction(true)]
    public class dynDraftingView : dynNode
    {
        public dynDraftingView()
        {
            InPortData.Add(new PortData("name", "Name", typeof(string)));
            OutPortData = new PortData("v", "Drafting View", typeof(dynDraftingView));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {

            ViewDrafting vd = null;
            string viewName = ((Expression.String)args[0]).Item;

            if (!string.IsNullOrEmpty(viewName))
            {
                //if we've already found the view
                //and it's the same one, get out
                if (vd != null && vd.Name == viewName)
                {
                    return Expression.NewContainer(vd);
                }

                FilteredElementCollector fec = new FilteredElementCollector(dynElementSettings.SharedInstance.Doc.Document);
                fec.OfClass(typeof(ViewDrafting));

                IList<Element> els = fec.ToElements();

                var vds = from v in els
                            where ((ViewDrafting)v).Name == viewName
                            select v;

                if (vds.Count() == 0)
                {
                    try
                    {
                        //create the view
                        vd = dynElementSettings.SharedInstance.Doc.Document.Create.NewViewDrafting();
                        if (vd != null)
                        {
                            vd.Name = viewName;
                        }
                    }
                    catch
                    {
                        this.Bench.Log(string.Format("Could not create view: {0}", viewName));
                    }
                }
                else
                {
                    vd = vds.First() as ViewDrafting;
                }
            }

            return Expression.NewContainer(vd);
        }
    }
}
