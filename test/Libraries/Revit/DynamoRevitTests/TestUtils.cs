using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Utilities;
using RevitServices.Persistence;

namespace Dynamo.Tests
{
    class TestUtils
    {
        /// <summary>
        /// Retrieves all family instances of the named type from the active document.
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static IEnumerable<FamilyInstance> GetAllFamilyInstancesWithTypeName(string typeName)
        {
            FilteredElementCollector fec = new FilteredElementCollector(DocumentManager.GetInstance().CurrentUIDocument.Document);
            fec.OfClass(typeof(FamilyInstance));
            return fec.ToElements().Where(x => ((FamilyInstance)x).Symbol.Name == typeName).Cast<FamilyInstance>();
        }
    }
}
