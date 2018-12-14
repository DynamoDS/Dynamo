using System;
using System.IO;
using System.Linq;
using System.Xml;
using Dynamo.Configuration;
using Dynamo.Graph.Nodes;
using Dynamo.Search.SearchElements;
using DynamoUtilities;
using Dynamo.Logging;

namespace Dynamo.Search
{
    /// <summary>
    ///     Searchable library of NodeSearchElements that can produce NodeModels.
    /// </summary>
    public class NodeSearchModel : SearchLibrary<NodeSearchElement, NodeModel>
    {
        /// <summary>
        ///     Construct a NodeSearchModel object
        /// </summary>
        /// <param name="logger"> (Optional) A logger to pass through to SearchLibrary for logging search data</param>
        internal NodeSearchModel(ILogger logger = null) : base(logger)
        {
        }

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
        internal void DumpLibraryToXml(string fileName, string dynamoPath)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            var document = ComposeXmlForLibrary(dynamoPath);
            document.Save(fileName);
        }

        /// <summary>
        ///     Serializes the contents of search into Xml.
        /// </summary>
        /// <returns></returns>
        internal XmlDocument ComposeXmlForLibrary(string dynamoPath)
        {
            var document = XmlHelper.CreateDocument("LibraryTree");

            var root = SearchCategoryUtil.CategorizeSearchEntries(
                SearchEntries,
                entry => entry.Categories);

            foreach (var category in root.SubCategories)
                AddCategoryToXml(document.DocumentElement, category, dynamoPath);

            foreach (var entry in root.Entries)
                AddEntryToXml(document.DocumentElement, entry, dynamoPath);

            return document;
        }

        private static void AddEntryToXml(XmlNode parent, NodeSearchElement entry, string dynamoPath)
        {
            var element = XmlHelper.AddNode(parent, entry.GetType().ToString());
            XmlHelper.AddNode(element, "FullCategoryName", entry.FullCategoryName);
            XmlHelper.AddNode(element, "Name", entry.Name);
            XmlHelper.AddNode(element, "Group", entry.Group.ToString());
            XmlHelper.AddNode(element, "Description", entry.Description);

            // Add search tags, joined by ",".
            // E.g. <SearchTags>bounding,bound,bymaxmin,max,min,bypoints</SearchTags>
            XmlHelper.AddNode(element, "SearchTags",
                String.Join(",", entry.SearchKeywords.Where(word => !String.IsNullOrWhiteSpace(word))));

            var dynamoNode = entry.CreateNode();

            // If entry has input parameters.
            if (dynamoNode.InPorts.Count != 0)
            {
                var inputNode = XmlHelper.AddNode(element, "Inputs");
                for (int i = 0; i < dynamoNode.InPorts.Count; i++)
                {
                    var parameterNode = XmlHelper.AddNode(inputNode, "InputParameter");

                    XmlHelper.AddAttribute(parameterNode, "Name", dynamoNode.InPorts[i].Name);

                    // Case for UI nodes as ColorRange, List.Create etc.
                    // UI nodes  do not have incoming ports in NodeSearchElement, but do have incoming ports in NodeModel.
                    // UI node initializes its incoming ports, when its constructor is called.
                    // So, here we check if NodeModel has input ports and NodeSearchElement also has the same input ports.
                    // UI node node will have several incoming ports on NodeModel,
                    // but 0 incoming ports on NodeSearchElement 
                    // (when there is no incoming port, it returns none 1st port by default).
                    if (dynamoNode.InPorts.Count == entry.InputParameters.Count()
                        && entry.InputParameters.First().Item2 != Properties.Resources.NoneString)
                    {
                        XmlHelper.AddAttribute(parameterNode, "Type", entry.InputParameters.ElementAt(i).Item2);
                    }

                    // Add default value, if it's not null.
                    if (dynamoNode.InPorts[i].DefaultValue != null)
                    {
                        XmlHelper.AddAttribute(parameterNode, "DefaultValue",
                            dynamoNode.InPorts[i].DefaultValue.ToString());
                    }
                    XmlHelper.AddAttribute(parameterNode, "Tooltip", dynamoNode.InPorts[i].ToolTip);
                }
            }

            // If entry has output parameters.
            if (dynamoNode.OutPorts.Count != 0)
            {
                var outputNode = XmlHelper.AddNode(element, "Outputs");
                for (int i = 0; i < dynamoNode.OutPorts.Count; i++)
                {
                    var parameterNode = XmlHelper.AddNode(outputNode, "OutputParameter");
                    XmlHelper.AddAttribute(parameterNode, "Name", dynamoNode.OutPorts[i].Name);

                    // Case for UI nodes as ColorRange.
                    if (dynamoNode.OutPorts.Count == entry.OutputParameters.Count()
                        && entry.OutputParameters.First() != Properties.Resources.NoneString)
                    {
                        XmlHelper.AddAttribute(parameterNode, "Type", entry.OutputParameters.ElementAt(i));
                    }

                    XmlHelper.AddAttribute(parameterNode, "Tooltip", dynamoNode.OutPorts[i].ToolTip);
                }
            }

            var assemblyName = Path.GetFileNameWithoutExtension(entry.Assembly);
            const string resourcesPath = @"src\Resources\";

            // Get icon paths.
            string pathToSmallIcon = Path.Combine(
                resourcesPath,
                assemblyName,
                "SmallIcons", entry.IconName + ".Small.png");

            string pathToLargeIcon = Path.Combine(
               resourcesPath,
               assemblyName,
               "LargeIcons", entry.IconName + ".Large.png");

            if (!File.Exists(Path.Combine(dynamoPath, @"..\..\..\", pathToSmallIcon)))
            {
                // Try DynamoCore path.
                pathToSmallIcon = Path.Combine(
                   resourcesPath,
                    "DynamoCore",
                    "SmallIcons", entry.IconName + ".Small.png");
            }

            if (!File.Exists(Path.Combine(dynamoPath, @"..\..\..\", pathToLargeIcon)))
            {
                // Try DynamoCore path.
                pathToLargeIcon = Path.Combine(
                    resourcesPath,
                    "DynamoCore",
                    "LargeIcons", entry.IconName + ".Large.png");
            }

            // Dump icons.
            XmlHelper.AddNode(element, "SmallIcon", File.Exists(Path.Combine(dynamoPath, @"..\..\..\", pathToSmallIcon)) ? pathToSmallIcon : "Not found");
            XmlHelper.AddNode(element, "LargeIcon", File.Exists(Path.Combine(dynamoPath, @"..\..\..\", pathToLargeIcon)) ? pathToLargeIcon : "Not found");
        }

        private static void AddCategoryToXml(
            XmlNode parent, ISearchCategory<NodeSearchElement> category, string dynamoPath)
        {
            var element = XmlHelper.AddNode(parent, "Category");
            XmlHelper.AddAttribute(element, "Name", category.Name);

            foreach (var subCategory in category.SubCategories)
                AddCategoryToXml(element, subCategory, dynamoPath);

            foreach (var entry in category.Entries)
                AddEntryToXml(element, entry, dynamoPath);
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