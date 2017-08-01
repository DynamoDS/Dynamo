using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Graph.Nodes
{
    public enum NodeInputTypes
    {
        numberInput, booleanInput, stringInput, colorInput, dateInput, selectionInput
    };



    /// <summary>
    /// Represents a node which acts as a UI input for the graph
    /// - may also hold a value for that input
    /// </summary>
    public class NodeInputData
    {
        /// <summary>
        /// The id of the node.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Display name of the input node.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The type of input this node is.
        /// </summary>
        [JsonConverter(typeof(NodeInputTypeConverter))]
        public NodeInputTypes Type { get; set; }
        /// <summary>
        /// The value of the input when the graph was saved.
        /// </summary>
        public string Value { get; set; }

        //optional properties, might be null
        /// <summary>
        /// If this input is a selection type a list of choices a user can select.
        /// </summary>
        public List<string> Choices { get; set; }
        /// <summary>
        /// if this input is a Number, the max value of that number.
        /// </summary>
        public double? MaximumValue { get; set; }
        /// <summary>
        /// if this input is a Number, the min value of that number.
        /// </summary>
        public double? MinimumValue { get; set; }
        /// <summary>
        /// if this input is a Number, the step value of that number when it acts as a slider.
        /// </summary>
        public double? StepValue { get; set; }
        /// <summary>
        /// if this input is a Number, the number type Double or Integer that the number returns.
        /// </summary>
        public string NumberType { get; set; }
        /// <summary>
        /// Description displayed to user of this input node.
        /// </summary>
        public string Description { get; set; }

        private static Dictionary<Type, NodeInputTypes> dotNetTypeToNodeInputType = new Dictionary<Type, NodeInputTypes>
        {
            {typeof(String),NodeInputTypes.stringInput},
            { typeof(Boolean),NodeInputTypes.booleanInput},
            { typeof(DateTime),NodeInputTypes.dateInput},
            { typeof(double),NodeInputTypes.numberInput},
            { typeof(int),NodeInputTypes.numberInput},
            {typeof(float),NodeInputTypes.numberInput},
        };

        private static Dictionary<NodeInputTypes, string> enumToStringMap = new Dictionary<NodeInputTypes, string>
        {
            {NodeInputTypes.stringInput,"string"},
            {NodeInputTypes.selectionInput,"selection"},
            {NodeInputTypes.colorInput,"color"},
            {NodeInputTypes.booleanInput,"boolean"},
            {NodeInputTypes.numberInput,"number"},
            {NodeInputTypes.dateInput,"date"}
        };


        public static NodeInputTypes getNodeInputTypeFromType(Type type)
        {
            NodeInputTypes output;
            if (dotNetTypeToNodeInputType.TryGetValue(type, out output))
            {
                return output;
            }
            else
            {
                throw new ArgumentException("could not find an inputType for this type");
            }
        }
        public static string getStringName(NodeInputTypes type)
        {
            string output;
            if (enumToStringMap.TryGetValue(type, out output))
            {
                return output;
            }
            else
            {
                throw new ArgumentException("could not find a string name for this type");
            }
        }
    }

    public class NodeInputTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(NodeInputTypes));
        }
        public override bool CanRead
        {
            get { return false; }
        }
        public override bool CanWrite
        {
            get { return true; }
        }


        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var name = NodeInputData.getStringName((NodeInputTypes)value);
            serializer.Serialize(writer, name);
        }
    }

}
