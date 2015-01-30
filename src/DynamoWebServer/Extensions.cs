using System.Linq;
using System.Text;

using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;

namespace DynamoWebServer
{
    public static class Extensions
    {
        public static string GetExtraData(this CodeBlockNodeModel codeBlock)
        {
            var stringBuilder = new StringBuilder();
            var allDefs = CodeBlockUtils.GetDefinitionLineIndexMap(codeBlock.CodeStatements);
            var lineIndices = allDefs.Select(def => def.Value - 1).ToList();

            stringBuilder.Append("\"Code\":\"");
            stringBuilder.Append(codeBlock.Code.
                                 Replace("\n", "\\n").
                                 Replace("\"", "\\\""));
            stringBuilder.Append("\", ");
            stringBuilder.Append("\"LineIndices\": [");
            stringBuilder.Append(string.Join(", ", lineIndices.Select(x => x.ToString()).ToArray()));
            stringBuilder.Append("],");

            return stringBuilder.ToString();
        }

        public static string GetExtraData(this VariableInputNode varNode)
        {
            var stringBuilder = new StringBuilder();
            var type = varNode.GetType();

            if (type.Name == "PythonNode")
            {
                var script = type.GetProperty("Script").GetValue(varNode, null).ToString();

                stringBuilder.Append("\"Script\":\"");
                stringBuilder.Append(script.
                                     Replace("\n", "\\n").
                                     Replace("\"", "\\\""));
                stringBuilder.Append("\", ");
            }

            return stringBuilder.ToString();
        }

        public static string GetInOutPortsData(this NodeModel node, bool isVarInputNode)
        {
            var stringBuilder = new StringBuilder();

            var pattern = node is VariableInputNode ? "{{ \"name\":\"{0}\", \"type\": \"{1}\" }}" : "\"{0}\"";
            var inPorts = node.InPorts.Select(port => string.Format(pattern, port.PortName, port.ToolTipContent)).ToList();
            var outPorts = node.OutPorts.Select(port => "\"" + port.ToolTipContent + "\"").ToList();

            stringBuilder.Append(isVarInputNode ? "\"varInputs\": [" : "\"InPorts\": [");
            stringBuilder.Append(inPorts.Any() ? inPorts.Aggregate((i, j) => i + "," + j) : "");
            stringBuilder.Append("], \"OutPorts\": [");
            stringBuilder.Append(outPorts.Any() ? outPorts.Aggregate((i, j) => i + "," + j) : "");
            stringBuilder.Append("]");

            return stringBuilder.ToString();
        }
    }
}
