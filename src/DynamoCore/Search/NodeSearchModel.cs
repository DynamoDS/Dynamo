using System.Xml;
using Dynamo.Models;
using Dynamo.Search.Interfaces;
using Dynamo.Search.SearchElements;
using DynamoUtilities;

namespace Dynamo.Search
{
    /// <summary>
    ///     Searchable library of NodeSearchElements that can produce NodeModels.
    /// </summary>
    public class NodeSearchModel : SearchLibrary<NodeSearchElement, NodeModel>
    {
        /// <summary>
        ///     Dumps the contents of search into an Xml file.
        /// </summary>
        /// <param name="fileName"></param>
        public void DumpLibraryToXml(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            var document = ComposeXmlForLibrary();
            document.Save(fileName);
        }

        /// <summary>
        ///     Serializes the contents of search into Xml.
        /// </summary>
        /// <returns></returns>
        public XmlDocument ComposeXmlForLibrary()
        {
            var document = XmlHelper.CreateDocument("LibraryTree");

            var root = SearchCategory.CategorizeSearchEntries(
                SearchEntries,
                entry => entry.Categories);

            foreach (var category in root.SubCategories)
                AddCategoryToXml(document.DocumentElement, category);

            foreach (var entry in root.Entries)
                AddEntryToXml(document.DocumentElement, entry);

            return document;
        }

        private static void AddEntryToXml(XmlNode parent, NodeSearchElement entry)
        {
            var element = XmlHelper.AddNode(parent, entry.GetType().ToString());
            XmlHelper.AddNode(element, "FullCategoryName", entry.FullCategoryName);
            XmlHelper.AddNode(element, "Name", entry.Name);
            XmlHelper.AddNode(element, "Description", entry.Description);
        }

        private static void AddCategoryToXml(
            XmlNode parent, ISearchCategory<NodeSearchElement> category)
        {
            var element = XmlHelper.AddNode(parent, "Category");
            XmlHelper.AddAttribute(element, "Name", category.Name);

            foreach (var subCategory in category.SubCategories)
                AddCategoryToXml(element, subCategory);

            foreach (var entry in category.Entries)
                AddEntryToXml(element, entry);
        }
    }
}