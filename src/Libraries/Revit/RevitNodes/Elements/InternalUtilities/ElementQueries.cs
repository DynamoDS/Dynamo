using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using RevitServices.Persistence;

namespace Revit.Elements.InternalUtilities
{
    [IsVisibleInDynamoLibrary(false)]
    public static class ElementQueries
    {
        public static IList<Element> OfFamilyType(FamilySymbol familyType)
        {
            var instanceFilter = new ElementClassFilter(typeof(Autodesk.Revit.DB.FamilyInstance));
            var fec = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);

            var familyInstances = fec.WherePasses(instanceFilter)
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<Autodesk.Revit.DB.FamilyInstance>()
                .Where(x => x.Symbol.IsSimilarType(familyType.InternalFamilySymbol.Id));

            var instances = familyInstances
                .Select(x => ElementSelector.ByElementId(x.Id.IntegerValue)).ToList();
            return instances;
        }

        public static IList<Element> OfCategory(Category category)
        {
            var catFilter = new ElementCategoryFilter(category.InternalCategory.Id);
            var fec = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);
            var instances = 
                fec.WherePasses(catFilter)
                    .WhereElementIsNotElementType()
                    .ToElementIds()
                    .Select(id => ElementSelector.ByElementId(id.IntegerValue))
                    .ToList();
            return instances;
        }

        public static IList<Element> AtLevel(Level arg)
        {
            var levFilter = new ElementLevelFilter(arg.InternalLevel.Id);
            var fec = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);
            var instances =
                fec.WherePasses(levFilter)
                    .WhereElementIsNotElementType()
                    .ToElementIds()
                    .Select(id => ElementSelector.ByElementId(id.IntegerValue))
                    .ToList();
            return instances;
        }
    }
}
