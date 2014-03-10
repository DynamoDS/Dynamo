using Autodesk.Revit.DB;
using RevitServices.Persistence;

namespace Revit.Elements
{
    public class Category
    {
        #region private members

        private Autodesk.Revit.DB.Category internal_category;

        #endregion

        #region public properties

        public string Name
        {
            get { return internal_category.Name; }
        }

        #endregion

        #region public static constructors

        public static Category ByName(string name)
        {
            Settings documentSettings = DocumentManager.Instance.CurrentDBDocument.Settings;
            var groups = documentSettings.Categories;
            var builtInCat = (BuiltInCategory)System.Enum.Parse(typeof(BuiltInCategory), name);
            var category = groups.get_Item(builtInCat);
            return new Category(category);
        }

        #endregion

        #region private constructors

        private Category(Autodesk.Revit.DB.Category category)
        {
            internal_category = category;
        }

        #endregion

        public override string ToString()
        {
            return internal_category.Name;
        }
    }
}
