using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using Dynamo.Graph.Workspaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dynamo.Graph.Nodes
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NodeOutputTypes
    {
        [EnumMember(Value = "integer")]
        integerOutput,
        [EnumMember(Value = "float")]
        floatOutput,
        [EnumMember(Value = "boolean")]
        booleanOutput,
        [EnumMember(Value = "string")]
        stringOutput,
        [EnumMember(Value = "unknown")]
        unknownOutput
    };


    /// <summary>
    /// Represents a node which acts as a UI output for the graph
    /// - may also hold a value for that output
    /// </summary>
    public class NodeOutputData
    {
        /// <summary>
        /// The id of the node.
        /// </summary>
        [JsonConverter(typeof(IdToGuidConverter))]
        public Guid Id { get; set; }
        /// <summary>
        /// Display name of the output node.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The type of output this node is.
        /// </summary>
        public NodeOutputTypes Type { get; set; }
        /// <summary>
        /// The value of the output when the graph was saved.
        /// This should always be a string for all types.
        /// </summary>
        public string InitialValue { get; set; }
        /// <summary>
        /// Obsolete property due to typo in API.  Please use InitialValue.
        /// </summary>
        [JsonIgnore]
        [Obsolete("Property will be deprecated in Dynamo 3.0, please use InitialValue")]
        public string IntitialValue
        {
            get { return InitialValue; }
            set { InitialValue = value; }
        }
        /// <summary>
        /// Description displayed to user of this output node.
        /// </summary>
        public string Description { get; set; }

        private static Dictionary<Type, NodeOutputTypes> dotNetTypeToNodeOutputType = new Dictionary<Type, NodeOutputTypes>
        {
            { typeof(String),NodeOutputTypes.stringOutput},
            { typeof(Boolean),NodeOutputTypes.booleanOutput},
            { typeof(double),NodeOutputTypes.floatOutput},
            { typeof(Int32),NodeOutputTypes.integerOutput},
            { typeof(Int64),NodeOutputTypes.integerOutput},
            { typeof(float),NodeOutputTypes.floatOutput},
        };
        public static NodeOutputTypes getNodeOutputTypeFromType(Type type)
        {
            NodeOutputTypes output;
            if (dotNetTypeToNodeOutputType.TryGetValue(type, out output))
            {
                return output;
            }
            else
            {
                return NodeOutputTypes.unknownOutput;
            }
        }

        public override bool Equals(object obj)
        {
            var converted = obj as NodeOutputData;

            var valNumberComparison = false;
            try
            {
                valNumberComparison = Math.Abs(Convert.ToDouble(this.InitialValue, CultureInfo.InvariantCulture) - Convert.ToDouble(converted.InitialValue, CultureInfo.InvariantCulture)) < .000001;
            }
            catch (Exception e)
            {
                //this just stays false.
                valNumberComparison = false;
            }

            return obj is NodeOutputData && this.Id == converted.Id &&
                this.Description == converted.Description &&
                //TODO don't check name for now as this requires a VIew.
                //and we only have model level tests.
                //this.Name == converted.Name &&
                this.Type == converted.Type &&
                //check if the value is the same or if the value is a number check is it similar
                ((this.InitialValue == converted.InitialValue) || valNumberComparison || this.InitialValue.ToString() == converted.InitialValue.ToString());
        }
    }

}
