using Autodesk.Revit.DB;
using RevitServices.Persistence;

namespace Revit.Elements
{
    public class Category
    {
        #region private members

        private readonly Autodesk.Revit.DB.Category internalCategory;

        #endregion

        #region public properties

        public string Name
        {
            get { return internalCategory.Name; }
        }

        #endregion

        internal Autodesk.Revit.DB.Category InternalCategory
        {
            get { return internalCategory; }
        }

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
            internalCategory = category;
        }

        #endregion

        public override string ToString()
        {
            return internalCategory != null ? internalCategory.Name : string.Empty;
        }
    }
}
