using Autodesk.Revit.DB;
using RevitServices.Persistence;

namespace Revit
{
    public class DSCategory
    {
        #region private members

        private Category internal_category;

        #endregion

        #region public properties

        public string Name
        {
            get { return internal_category.Name; }
        }

        #endregion

        #region public static constructors

        public static DSCategory ByName(string name)
        {
            Settings documentSettings = DocumentManager.GetInstance().CurrentDBDocument.Settings;
            var groups = documentSettings.Categories;
            var builtInCat = (BuiltInCategory)System.Enum.Parse(typeof(BuiltInCategory), name);
            var category = groups.get_Item(builtInCat);
            return new DSCategory(category);
        }

        #endregion

        #region private constructors

        private DSCategory(Autodesk.Revit.DB.Category category)
        {
            internal_category = category;
        }

        #endregion
    }
}
