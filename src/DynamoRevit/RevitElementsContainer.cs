using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Utilities;
using Dynamo.Core;

namespace Dynamo.Revit
{
    public class RevitElementsContainer : ElementsContainer<ElementId>
    {
        public override void DestroyElement(ElementId e)
        {
            try
            {
                dynRevitSettings.Doc.Document.Delete(e);
            }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException)
            {
                //TODO: Flesh out?
            } 
        }
    }
}
