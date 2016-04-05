using System.Linq;
using System.Xml;
using Dynamo.Configuration;
using Dynamo.Graph.Nodes;
using Dynamo.Search.SearchElements;
using DynamoUtilities;

namespace Dynamo.Search
{
    /// <summary>
    ///     Searchable library of NodeSearchElements that can produce NodeModels.
    /// </summary>
    public class NodeSearchModel : SearchLibrary<NodeSearchElement, NodeModel>
    {
        internal override void Add(NodeSearchElement entry)
        {
            SearchElementGroup group = SearchElementGroup.None;

            entry.FullCategoryName = ProcessNodeCategory(entry.FullCategoryName, ref group);
            entry.Group = group;

            base.Add(entry);
        }

        /// <summary>
        ///     Dumps the contents of search into an Xml file.
        /// </summary>
        /// <param name="fileName"></param>
        internal void DumpLibraryToXml(string fileName)
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
        internal XmlDocument ComposeXmlForLibrary()
        {
            var document = XmlHelper.CreateDocument("LibraryTree");

            var root = SearchCategoryUtil.CategorizeSearchEntries(
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
            XmlHelper.AddNode(element, "Group", entry.Group.ToString());
            XmlHelper.AddNode(element, "Description", entry.Description);

            // If entry has input parameters.
            if (entry.InputParameters.First().Item2 != Properties.Resources.NoneString)
            {
                var inputNode = XmlHelper.AddNode(element, "Inputs");
                foreach (var parameter in entry.InputParameters)
                {
                    var parameterNode = XmlHelper.AddNode(inputNode, "InputParameter");
                    XmlHelper.AddAttribute(parameterNode, "Name", parameter.Item1);
                    XmlHelper.AddAttribute(parameterNode, "Type", parameter.Item2);
                }
            }

            // If entry has output parameters.
            if (entry.OutputParameters.First() != Properties.Resources.NoneString)
            {
                var inputNode = XmlHelper.AddNode(element, "Outputs");
                foreach (var parameter in entry.OutputParameters)
                {
                    var parameterNode = XmlHelper.AddNode(inputNode, "OutputParameter");
                    XmlHelper.AddAttribute(parameterNode, "Type", parameter);
                }
            }

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

        /// <summary>
        /// Call this method to assign a default grouping information if a given category 
        /// does not have any. A node category's group can either be "Create", "Query" or
        /// "Actions". If none of the group names above is assigned to the category, it 
        /// will be assigned a default one that is "Actions".
        /// 
        /// For examples:
        /// 
        ///     "Core.Evaluate" will be renamed as "Core.Evaluate.Actions"
        ///     "Core.List.Create" will remain as "Core.List.Create"
        /// 
        /// </summary>
        internal string ProcessNodeCategory(string category, ref SearchElementGroup group)
        {
            if (string.IsNullOrEmpty(category))
                return category;

            int index = category.LastIndexOf(Configurations.CategoryDelimiterString);

            // If "index" is "-1", then the whole "category" will be used as-is.            
            switch (category.Substring(index + 1))
            {
                case Configurations.CategoryGroupAction:
                    group = SearchElementGroup.Action;
                    break;
                case Configurations.CategoryGroupCreate:
                    group = SearchElementGroup.Create;
                    break;
                case Configurations.CategoryGroupQuery:
                    group = SearchElementGroup.Query;
                    break;
                default:
                    group = SearchElementGroup.Action;
                    return category;
            }

            return category.Substring(0, index);
        }
    }
}