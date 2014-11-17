using System;
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
            if (familyType == null) return null;

            var instanceFilter = new ElementClassFilter(typeof(Autodesk.Revit.DB.FamilyInstance));
            var fec = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);

            var familyInstances = fec.WherePasses(instanceFilter)
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<Autodesk.Revit.DB.FamilyInstance>();

            var matches = familyInstances.Where(x => x.Symbol.Id == familyType.InternalFamilySymbol.Id);

            var instances = matches
                .Select(x => ElementSelector.ByElementId(x.Id.IntegerValue)).ToList();
            return instances;
        }

        public static IList<Element> OfElementType(Type elementType)
        {
            if (elementType == null) return null;

            var elFilter = new ElementClassFilter(elementType);
            var fec = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);
            fec.WherePasses(elFilter);

            var instances = fec.ToElements()
                .Select(x => ElementSelector.ByElementId(x.Id.IntegerValue)).ToList();
            return instances;
        }

        public static IList<Element> OfCategory(Category category)
        {
            if (category == null) return null;

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
            if (arg == null) return null;

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

        internal static IEnumerable<Autodesk.Revit.DB.Level> GetAllLevels()
        {
            var collector = new Autodesk.Revit.DB.FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);
            collector.OfClass(typeof(Autodesk.Revit.DB.Level));
            return collector.ToElements().Cast<Autodesk.Revit.DB.Level>();
        }
    }
}
