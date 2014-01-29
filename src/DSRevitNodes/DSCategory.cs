using Autodesk.Revit.DB;
using RevitServices.Persistence;

namespace Revit
{
    public class DSCategory
    {
        #region private members

        private Autodesk.Revit.DB.Category internal_category;

        #endregion

        #region public static constructors

        public static DSCategory ByName(string name)
        {
            Settings documentSettings = DocumentManager.GetInstance().CurrentDBDocument.Settings;
            var groups = documentSettings.Categories;
            var category = (BuiltInCategory)System.Enum.Parse(typeof(BuiltInCategory), name);
            return new DSCategory(groups.get_Item(category));
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
