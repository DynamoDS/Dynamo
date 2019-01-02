using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Engine;
using Dynamo.Graph.Nodes.ZeroTouch;
using ProtoCore.Mirror;

namespace Dynamo.Graph.Nodes
{
    public static class NodeModelExtensions
    {
        internal static void VisibleUpstreamNodes(this NodeModel node, List<NodeModel> gathered)
        {
            var upstream = node.InPorts.SelectMany(p => p.Connectors.Select(c => c.Start.Owner));

            foreach (var n in upstream.Where(n => !gathered.Contains(n)))
            {
                gathered.Add(n);
                n.VisibleUpstreamNodes(gathered);
            }
        }

        internal static IEnumerable<NodeModel> UpstreamNodesMatchingPredicate(this NodeModel node, List<NodeModel> gathered, Predicate<NodeModel> match)
        {
            var upstream = node.InPorts.SelectMany(p => p.Connectors.Select(c => c.Start.Owner)).
                Where(n => match(n));

            foreach (var n in upstream.Where(n => !gathered.Contains(n)))
            {
                gathered.Add(n);
            }

            foreach (var n in upstream)
            {
                n.UpstreamNodesMatchingPredicate(gathered, match);
            }

            return gathered;
        }

        internal static IEnumerable<NodeModel> AllUpstreamNodes(this NodeModel node, List<NodeModel> gathered)
        {
            if(node == null)
            {
                return new List<NodeModel>();
            }

            var upstream = node.InPorts.SelectMany(p => p.Connectors.Select(c => c.Start.Owner));

            foreach (var n in upstream.Where(n => !gathered.Contains(n)))
            {
                gathered.Add(n);
            }

            foreach (var n in upstream)
            {
                n.AllUpstreamNodes(gathered);
            }

            return gathered;
        }

        internal static bool IsUpstreamOf(this NodeModel node, NodeModel otherNode)
        {
            var gathered = new List<NodeModel>();
            otherNode.AllUpstreamNodes(gathered);

            return gathered.Contains(node);
        }

        internal static List<IGraphicItem> GeneratedGraphicItems(this NodeModel node, EngineController engineController)
        {
            var ids = node.GetAllOutportAstIdentifiers();

            var results = new List<IGraphicItem>();

            foreach (var id in ids)
            {
                var mirror = engineController.GetMirror(id);
                if (mirror == null) continue;

                var mirrorData = mirror.GetData();
                if (mirrorData == null) continue;

                GetGraphicItemsFromMirrorData(mirrorData, results);
            }

            return results;
        }

        internal static string GetOriginalName(this NodeModel node)
        {
            if (node == null) return string.Empty;

            var function = node as DSFunctionBase;
            if (function != null)
                return function.Controller.Definition.DisplayName;

            var nodeType = node.GetType();
            var elNameAttrib = nodeType.GetCustomAttributes<NodeNameAttribute>(false).FirstOrDefault();
            if (elNameAttrib != null)
                return elNameAttrib.Name;

            return nodeType.FullName;
        }

        private static void GetGraphicItemsFromMirrorData(MirrorData mirrorData, List<IGraphicItem> graphicItems)
        {
            if (mirrorData == null) return;

            if (mirrorData.IsCollection)
            {
                foreach (var el in mirrorData.GetElements())
                {
                    GetGraphicItemsFromMirrorData(el, graphicItems);
                }
            }
            else
            {
                var graphicItem = mirrorData.Data as IGraphicItem;
                if (graphicItem == null) return;

                graphicItems.Add(graphicItem);
            }
        }

        private static IEnumerable<string> GetAllOutportAstIdentifiers(this NodeModel node)
        {
            var ids = new List<string>();
            for (var i = 0; i < node.OutPorts.Count; ++i)
            {
                var id = node.GetOneOutportAstIdentifier(i);
                if (!string.IsNullOrEmpty(id))
                    ids.Add(id);
            }

            return ids;
        }

        private static string GetOneOutportAstIdentifier(this NodeModel node, int outPortIndex)
        {
            var output = node.GetAstIdentifierForOutputIndex(outPortIndex);
            return output == null ? null : output.ToString();
        }
    }
}
