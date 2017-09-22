using System;
using System.Linq;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Notes;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Type = System.Type;

namespace Dynamo.Wpf.ViewModels.Core.Converters
{
    /// <summary>
    /// The AnnotationViewModelConverter is used to serialize AnnotationViewModels.
    /// </summary>
    public class AnnotationViewModelConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(AnnotationViewModel);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var annoVM = (AnnotationViewModel)value;
            var anno = annoVM.AnnotationModel;

            writer.WriteStartObject();

            writer.WritePropertyName("Id");
            writer.WriteValue(anno.GUID.ToString("N"));
            writer.WritePropertyName("Title");
            writer.WriteValue(anno.AnnotationText);
            writer.WritePropertyName("Nodes");
            writer.WriteStartArray();
            foreach (var m in anno.Nodes)
            {
                writer.WriteValue(m.GUID.ToString("N"));
            }
            writer.WriteEndArray();
            writer.WritePropertyName("Left");
            writer.WriteValue(anno.X);
            writer.WritePropertyName("Top");
            writer.WriteValue(anno.Y);
            writer.WritePropertyName("Width");
            writer.WriteValue(anno.Width);
            writer.WritePropertyName("Height");
            writer.WriteValue(anno.Height);
            writer.WritePropertyName("FontSize");
            writer.WriteValue(anno.FontSize);
            writer.WritePropertyName("InitialTop");
            writer.WriteValue(anno.InitialTop);
            writer.WritePropertyName("InitialHeight");
            writer.WriteValue(anno.InitialHeight);
            writer.WritePropertyName("TextblockHeight");
            writer.WriteValue(anno.TextBlockHeight);
            writer.WritePropertyName("Background");
            writer.WriteValue(anno.Background != null ? anno.Background : "");
            writer.WriteEndObject();
        }
    }
}
