using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Dynamo.Engine;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;

namespace Dynamo.Graph.Workspaces
{
    /// <summary>
    /// Contains methods for serializing a WorkspaceModel to json.
    /// </summary>
    public static class SerializationExtensions
    {
        /// <summary>
        /// Save a Workspace to json.
        /// </summary>
        /// <returns>A string representing the serialized WorkspaceModel.</returns>
        internal static string ToJson(this WorkspaceModel workspace, EngineController engine)
        {
            var logger = engine != null ? engine.AsLogger() : null;

            var settings = new JsonSerializerSettings
            {
                Error = (sender, args) =>
                {
                    args.ErrorContext.Handled = true;
                    Console.WriteLine(args.ErrorContext.Error);
                },
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
                Culture = CultureInfo.InvariantCulture,
                Converters = new List<JsonConverter>{
                        new ConnectorConverter(logger),                        
                        new WorkspaceWriteConverter(engine),
                        new DummyNodeWriteConverter(),
                        new TypedParameterConverter(),
                        new NodeLibraryDependencyConverter(logger),
                        new LinterManagerConverter(logger)
                    },
                ReferenceResolverProvider = () => { return new IdReferenceResolver(); }
            };

            var json = JsonConvert.SerializeObject(workspace, settings);
            var result = ReplaceTypeDeclarations(json);

            result = SerializeIntegerSliderAs32BitType(result);
            result = customOrderNodes(result);

            return result;
        }

        /// <summary>
        /// Strips $type references from the generated json, replacing them with 
        /// type names matching those expected by the server.
        /// </summary>
        /// <param name="json">The json to parse.</param>
        /// <param name="fromServer">A flag indicating whether this json is coming from the server, and thus
        /// needs to be converted back to its Json.net friendly format.</param>
        /// <returns></returns>
        internal static string ReplaceTypeDeclarations(string json, bool fromServer = false)
        {
            var result = json;

            if (fromServer)
            {
                var rgx2 = new Regex(@"ConcreteType");
                result = rgx2.Replace(result, "$type");
            }
            else
            {
                var rgx2 = new Regex(@"\$type");
                result = rgx2.Replace(result, "ConcreteType");
            }

            return result;
        }

        [Obsolete("Remove method after obsoleting IntegerSlider and replacing it with IntegerSlider64Bit")]
        internal static string DeserializeIntegerSliderTo64BitType(string json)
        {
            var result = json;

            var rgx2 = new Regex(@"\bCoreNodeModels.Input.IntegerSlider\b");

            return rgx2.Replace(result, "CoreNodeModels.Input.IntegerSlider64Bit");
        }

        [Obsolete("Remove method after obsoleting IntegerSlider and replacing it with IntegerSlider64Bit")]
        internal static string SerializeIntegerSliderAs32BitType(string json)
        {
            var result = json;

            var rgx2 = new Regex(@"\bCoreNodeModels.Input.IntegerSlider64Bit\b");

            return rgx2.Replace(result, "CoreNodeModels.Input.IntegerSlider");
        }

        internal static string keyProperty = "ConcreteType";

        /// <summary>
        /// Orders the properties of each Node according to the specify in the NodePropertiesOrder function
        /// </summary>
        /// <param name="metaContent"></param>
        /// <returns></returns>
        internal static string customOrderNodes(string metaContent)
        {
            try
            {
                string metaNodesStartKey = "\"Nodes\": [\r\n   ";
                string metaNodesEndKey = "\r\n  ]";
                int flatNodesStartIndex = metaContent.IndexOf(metaNodesStartKey) + metaNodesStartKey.Length;
                int flatNodesEndIndex = metaContent.IndexOf(metaNodesEndKey, flatNodesStartIndex);

                string flatNodes = metaContent.Substring(flatNodesStartIndex, flatNodesEndIndex - flatNodesStartIndex);

                string metaContentBegin = metaContent.Substring(0, flatNodesStartIndex - metaNodesStartKey.Length);
                string metaContentEnd = metaContent.Substring(flatNodesEndIndex + metaNodesEndKey.Length);

                List<string> splittedFlatNodes = splitFlatNodes(flatNodes);

                StringBuilder rebuiltFlatNodes = new StringBuilder();
                foreach (var flatNode in splittedFlatNodes)
                {
                    string rebuiltFlatNode = orderingFlatNode(flatNode);
                    rebuiltFlatNodes.Append(rebuiltFlatNode).Append(",");
                }

                rebuiltFlatNodes = rebuiltFlatNodes.Remove(rebuiltFlatNodes.Length - 1, 1);

                return metaContentBegin + metaNodesStartKey + rebuiltFlatNodes.ToString() + metaNodesEndKey + metaContentEnd;
            }
            catch (Exception)
            {
                return metaContent;
            }            
        }

        internal static List<string> splitFlatNodes(string flatNodes)
        {
            List<string> metaProperties = new List<string>();

            var rxKeyProperty = new Regex(keyProperty);
            Match match = rxKeyProperty.Match(flatNodes);

            List<int> coincidenciasIndex = new List<int>();
            while (match.Success)
            {
                coincidenciasIndex.Add(match.Index);
                match = match.NextMatch();
            }

            int closeBracketSpace = 23;

            List<int> commas = new List<int>();

            for (int i = 0; i < coincidenciasIndex.Count; i++)
            {
                int closeBracket = i == coincidenciasIndex.Count - 1 ? flatNodes.LastIndexOf('}') : coincidenciasIndex[i + 1] - closeBracketSpace;
                if (i < coincidenciasIndex.Count - 1)
                {
                    commas.Add(flatNodes.IndexOf(',', closeBracket));
                }
            }

            for (int i = 0; i < commas.Count + 1; i++)
            {
                int metaPropertiesStart = i == 0 ? 0 : i == commas.Count ? commas[commas.Count - 1] + 1 : commas[i - 1] + 1;
                int metaPropertiesLength = i == 0 ? commas[i] : i == commas.Count ? flatNodes.Length - commas[commas.Count - 1] - 1 : commas[i] - commas[i - 1] - 1;
                metaProperties.Add(flatNodes.Substring(metaPropertiesStart, metaPropertiesLength));
            }

            return metaProperties;
        }

        static string orderingFlatNode(string flatNode)
        {
            List<string> nodeRows = splitFlatNodeInRows(flatNode);
            string firstRow = nodeRows[0];
            string lastRow = nodeRows[nodeRows.Count() - 1];

            nodeRows.RemoveAt(0);
            nodeRows.RemoveAt(nodeRows.Count() - 1);

            List<string> orderedNodeRows = new List<string>();
            List<NodeRowForOrdering> markedNodeRows = new List<NodeRowForOrdering>();

            foreach (var nodeRow in nodeRows)
            {
                markedNodeRows.Add(new NodeRowForOrdering()
                {
                    metaNodeRow = nodeRow,
                    propertyName = getPropertyNameFromMetaProperty(nodeRow),
                    isMarked = false
                });
            }

            foreach (string orderedProperty in NodeModel.PropertiesSerializationOrder())
            {
                if (markedNodeRows.Exists(mrow => mrow.propertyName == orderedProperty))
                {
                    string metaNodeRow = markedNodeRows.Where(mkNode => mkNode.propertyName == orderedProperty).FirstOrDefault().metaNodeRow;
                    if (metaNodeRow.Substring(0, 1) != ",")
                    {
                        metaNodeRow = "," + metaNodeRow;
                    }
                    orderedNodeRows.Add(metaNodeRow);
                    markedNodeRows.FirstOrDefault(mkNode => mkNode.propertyName == orderedProperty).isMarked = true;
                }
            }

            foreach (var markedNodeRow in markedNodeRows.Where(mkNode => mkNode.isMarked == false))
            {
                orderedNodeRows.Add(markedNodeRow.metaNodeRow);
            }

            if (orderedNodeRows.Count > 0)
            {
                if (orderedNodeRows[0].Substring(0, 1) == ",")
                {
                    orderedNodeRows[0] = orderedNodeRows[0].Substring(1);
                }
            }

            orderedNodeRows.Insert(0, firstRow);
            orderedNodeRows.Add(lastRow);

            StringBuilder flatNodeBuilder = new StringBuilder();
            foreach (var row in orderedNodeRows)
            {
                flatNodeBuilder.Append(row);
            }
            return flatNodeBuilder.ToString();
        }

        internal static List<string> splitFlatNodeInRows(string flatNode)
        {
            List<string> rows = new List<string>();

            int firstPropertyIndex = flatNode.IndexOf("{");

            var lastPropertyIndicator = '"';
            int lastPropertyIndex = flatNode.LastIndexOf(lastPropertyIndicator);

            string firstLine = flatNode.Substring(0, flatNode.Length - (flatNode.Length - firstPropertyIndex) + 1);
            string lastLine = flatNode.Substring(lastPropertyIndex + 1);
            string flatProperties = flatNode.Substring(firstPropertyIndex + 1, lastPropertyIndex - firstPropertyIndex);

            StringBuilder flatNodeBuilder = new StringBuilder();
            flatNodeBuilder.Append(firstLine).Append(flatProperties).Append(lastLine);
            string builtFlatNode = flatNodeBuilder.ToString();

            string propertySeparator = ",\r\n      \"";

            var rxPropertySeparator = new Regex(propertySeparator);
            Match matchProperties = rxPropertySeparator.Match(flatProperties);

            List<int> propertiesIndex = new List<int>();

            while (matchProperties.Success)
            {
                propertiesIndex.Add(matchProperties.Index);
                matchProperties = matchProperties.NextMatch();
            }

            List<string> metaProperties = new List<string>();
            for (int i = 0; i < propertiesIndex.Count + 1; i++)
            {
                int startMetaProperty = i == 0 ? 0 : propertiesIndex[i - 1];
                int endMetaProperty = i == 0 ? propertiesIndex[i] : i == propertiesIndex.Count ? flatProperties.Length - propertiesIndex[i - 1] : propertiesIndex[i] - propertiesIndex[i - 1]; // 15; // ACA ME QUEDE
                string metaProperty = flatProperties.Substring(startMetaProperty, endMetaProperty);
                metaProperties.Add(metaProperty);
            }

            rows.Add(firstLine);
            foreach (var metaProperty in metaProperties)
            {
                rows.Add(metaProperty);
            }
            rows.Add(lastLine);

            return rows;
        }

        internal static string getPropertyNameFromMetaProperty(string metaProperty)
        {
            string result = string.Empty;

            int equalIndex = metaProperty.IndexOf(':');
            int quoteIndex = metaProperty.IndexOf('"');

            if (equalIndex != -1)
            {
                result = metaProperty.Substring(0, equalIndex - 1);
                result = result.Substring(quoteIndex + 1);
            }

            return result;
        }
    }

    class NodeRowForOrdering
    {
        public string metaNodeRow { get; set; }
        public string propertyName { get; set; }
        public Boolean isMarked { get; set; }
    }
}
