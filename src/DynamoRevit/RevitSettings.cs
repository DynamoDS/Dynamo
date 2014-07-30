#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Dynamo.Revit;
using Dynamo.ViewModels;

using RevitServices.Persistence;

#endregion

namespace Dynamo.Utilities
{
    internal class dynUtils
    {
        /// <summary>
        ///     Utility function to determine if an Element of the given ID exists in the document.
        /// </summary>
        /// <returns>True if exists, false otherwise.</returns>
        public static bool TryGetElement<T>(ElementId id, out T e) where T : Element
        {
            try
            {
                e = DocumentManager.Instance.CurrentUIDocument.Document.GetElement(id) as T;
                return e != null && e.Id != null;
            }
            catch
            {
                e = null;
                return false;
            }
        }

    }

    public static class dynRevitSettings
    {
        private static Options geometryOptions;

        public static Stack<ElementsContainer> ElementsContainers =
            new Stack<ElementsContainer>(new[] { new ElementsContainer() });

        public static Level DefaultLevel { get; set; }
        public static DynamoWarningSwallower WarningSwallower { get; set; }

        public static Options GeometryOptions
        {
            get
            {
                if (geometryOptions == null)
                {
                    geometryOptions = new Options
                    {
                        ComputeReferences = true,
                        DetailLevel = ViewDetailLevel.Medium,
                        IncludeNonVisibleObjects = false
                    };
                }

                return geometryOptions;
            }
        }

        public static DynamoViewModel Controller { get; internal set; }


    }
}
