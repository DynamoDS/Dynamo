using Dynamo.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json.Linq;

namespace Dynamo.Wpf.ViewModels.Core
{
    /// <summary>
    /// SerializationExtensions contains methods for serializing a WorkspaceViewModel to json.
    /// </summary>
    public static class SerializationExtensions
    {
        /// <summary>
        /// Serialize the WorkspaceViewModel to JSON.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns>A JSON string representing the WorkspaceViewModel</returns>
        internal static string ToJson(this WorkspaceViewModel viewModel)
        {
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
                Converters = new List<JsonConverter>
                {
                    new NodeViewModelConverter(),
                }
            };

            var json = JsonConvert.SerializeObject(viewModel, settings);

            return json;
        }
    }

    public class NodeViewModelConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(NodeViewModel);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var nodeViewModel = (NodeViewModel) value;
            // For each nodeViewModel, start a new object
            writer.WriteStartObject();
            // Serialize Node Id as property name
            writer.WritePropertyName(nodeViewModel.Id.ToString());

            // Start a new object for all the nodeViewModel properties
            writer.WriteStartObject();
            writer.WritePropertyName("Name");
            writer.WriteValue(nodeViewModel.Name);
            writer.WritePropertyName("X");
            writer.WriteValue(nodeViewModel.X);
            writer.WritePropertyName("Y");
            writer.WriteValue(nodeViewModel.Y);
            writer.WritePropertyName("isVisible");
            writer.WriteValue(nodeViewModel.IsVisible);
            writer.WritePropertyName("IsUpstreamVisible");
            writer.WriteValue(nodeViewModel.IsUpstreamVisible);
            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
