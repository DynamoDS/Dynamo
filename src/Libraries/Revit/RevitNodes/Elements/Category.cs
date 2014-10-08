using System;

using Autodesk.DesignScript.Runtime;
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
            get
            {
                return internalCategory.Name;
            }
        }

        public int Id
        {
            get
            {
                return InternalCategory.Id.IntegerValue;
            }
        }

        #endregion

        [IsVisibleInDynamoLibrary(false)]
        public Autodesk.Revit.DB.Category InternalCategory
        {
            get { return internalCategory; }
        }

        #region public static constructors

        public static Category ByName(string name)
        {
            Settings documentSettings = DocumentManager.Instance.CurrentDBDocument.Settings;
            var groups = documentSettings.Categories;
            var builtInCat = (BuiltInCategory)Enum.Parse(typeof(BuiltInCategory), name);
            var category = groups.get_Item(builtInCat);

            if (category == null)
            {
                throw new Exception("The selected category is not valid in this document.");
            }

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
