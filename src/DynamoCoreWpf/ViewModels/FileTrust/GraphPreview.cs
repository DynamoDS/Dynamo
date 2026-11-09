using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dynamo.Wpf.ViewModels.FileTrust
{
    /// <summary>
    /// A lightweight, read-only description of a .dyn graph's layout, built by reading the
    /// file's JSON directly. No NodeModels are created, no types are resolved and no
    /// libraries, packages or scripts are loaded, so it is safe to build for a file from
    /// an untrusted location before the user has accepted the trust warning.
    /// </summary>
    internal sealed class GraphPreview
    {
        internal sealed class PreviewNode
        {
            internal string Name;
            internal double X;
            internal double Y;
            internal int InputCount;
            internal int OutputCount;
        }

        internal sealed class PreviewConnector
        {
            internal int StartNode;
            internal int StartPortIndex;
            internal int EndNode;
            internal int EndPortIndex;
        }

        internal sealed class PreviewGroup
        {
            internal string Title;
            internal double Left;
            internal double Top;
            internal double Width;
            internal double Height;
            internal string Background;
        }

        internal sealed class PreviewNote
        {
            internal string Text;
            internal double X;
            internal double Y;
        }

        internal List<PreviewNode> Nodes { get; } = new List<PreviewNode>();
        internal List<PreviewConnector> Connectors { get; } = new List<PreviewConnector>();
        internal List<PreviewGroup> Groups { get; } = new List<PreviewGroup>();
        internal List<PreviewNote> Notes { get; } = new List<PreviewNote>();

        /// <summary>
        /// True when the graph has nothing that can be drawn.
        /// </summary>
        internal bool IsEmpty => Nodes.Count == 0 && Groups.Count == 0 && Notes.Count == 0;

        /// <summary>
        /// Builds a preview of a .dyn graph's layout from the file's text.
        /// </summary>
        /// <param name="fileText">The .dyn file content, as read from disk.</param>
        /// <returns>The preview, or null when the text is not a JSON graph.</returns>
        internal static GraphPreview FromText(string fileText)
        {
            using (var stringReader = new StringReader(fileText ?? string.Empty))
            using (var jsonReader = new JsonTextReader(stringReader) { DateParseHandling = DateParseHandling.None })
            {
                JObject root;
                try
                {
                    root = JObject.Load(jsonReader);
                }
                catch (JsonReaderException)
                {
                    // Legacy XML graphs or corrupt files have no JSON preview.
                    return null;
                }
                return FromJson(root);
            }
        }

        /// <summary>
        /// Builds a preview from a parsed .dyn JSON object.
        /// </summary>
        internal static GraphPreview FromJson(JObject root)
        {
            var preview = new GraphPreview();
            if (root == null) return preview;

            // Map every port id to its owning node and its index on that node, from the model section.
            var portLookup = new Dictionary<string, (string nodeId, int index)>(StringComparer.OrdinalIgnoreCase);
            var portCounts = new Dictionary<string, (int inputs, int outputs)>(StringComparer.OrdinalIgnoreCase);
            if (root["Nodes"] is JArray modelNodes)
            {
                foreach (var node in modelNodes.OfType<JObject>())
                {
                    var nodeId = (string)node["Id"];
                    if (string.IsNullOrEmpty(nodeId)) continue;

                    var inputs = AddPorts(node["Inputs"] as JArray, nodeId, portLookup);
                    var outputs = AddPorts(node["Outputs"] as JArray, nodeId, portLookup);
                    portCounts[nodeId] = (inputs, outputs);
                }
            }

            var view = root["View"] as JObject;

            // Node positions and names live in the view section.
            var nodeIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (view?["NodeViews"] is JArray nodeViews)
            {
                foreach (var nodeView in nodeViews.OfType<JObject>())
                {
                    var nodeId = (string)nodeView["Id"];
                    if (string.IsNullOrEmpty(nodeId) || nodeIndex.ContainsKey(nodeId)) continue;

                    portCounts.TryGetValue(nodeId, out var counts);
                    nodeIndex[nodeId] = preview.Nodes.Count;
                    preview.Nodes.Add(new PreviewNode
                    {
                        Name = (string)nodeView["Name"] ?? string.Empty,
                        X = ToDouble(nodeView["X"]),
                        Y = ToDouble(nodeView["Y"]),
                        InputCount = counts.inputs,
                        OutputCount = counts.outputs
                    });
                }
            }

            if (root["Connectors"] is JArray connectors)
            {
                foreach (var connector in connectors.OfType<JObject>())
                {
                    var start = (string)connector["Start"];
                    var end = (string)connector["End"];
                    if (start == null || end == null) continue;
                    if (!portLookup.TryGetValue(start, out var startPort) || !portLookup.TryGetValue(end, out var endPort)) continue;
                    if (!nodeIndex.TryGetValue(startPort.nodeId, out var startNode) || !nodeIndex.TryGetValue(endPort.nodeId, out var endNode)) continue;

                    preview.Connectors.Add(new PreviewConnector
                    {
                        StartNode = startNode,
                        StartPortIndex = startPort.index,
                        EndNode = endNode,
                        EndPortIndex = endPort.index
                    });
                }
            }

            if (view?["Annotations"] is JArray annotations)
            {
                foreach (var annotation in annotations.OfType<JObject>())
                {
                    var width = ToDouble(annotation["Width"]);
                    var height = ToDouble(annotation["Height"]);
                    var hasMembers = annotation["Nodes"] is JArray members && members.Count > 0;

                    // An annotation with no members and no size is a note saved in the annotation list.
                    if (!hasMembers && width <= 0 && height <= 0)
                    {
                        preview.Notes.Add(new PreviewNote
                        {
                            Text = (string)annotation["Title"] ?? string.Empty,
                            X = ToDouble(annotation["Left"]),
                            Y = ToDouble(annotation["Top"])
                        });
                        continue;
                    }

                    preview.Groups.Add(new PreviewGroup
                    {
                        Title = (string)annotation["Title"] ?? string.Empty,
                        Left = ToDouble(annotation["Left"]),
                        Top = ToDouble(annotation["Top"]),
                        Width = width,
                        Height = height,
                        Background = (string)annotation["Background"]
                    });
                }
            }

            if (view?["Notes"] is JArray notes)
            {
                foreach (var note in notes.OfType<JObject>())
                {
                    preview.Notes.Add(new PreviewNote
                    {
                        Text = (string)note["Text"] ?? string.Empty,
                        X = ToDouble(note["X"]),
                        Y = ToDouble(note["Y"])
                    });
                }
            }

            return preview;
        }

        private static int AddPorts(JArray ports, string nodeId, Dictionary<string, (string nodeId, int index)> portLookup)
        {
            if (ports == null) return 0;

            var index = 0;
            foreach (var portId in ports.OfType<JObject>().Select(port => (string)port["Id"]))
            {
                if (!string.IsNullOrEmpty(portId))
                {
                    portLookup[portId] = (nodeId, index);
                }
                index++;
            }
            return index;
        }

        private static double ToDouble(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null) return 0;
            if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
            {
                var value = token.Value<double>();
                return double.IsNaN(value) || double.IsInfinity(value) ? 0 : value;
            }
            return 0;
        }
    }
}
