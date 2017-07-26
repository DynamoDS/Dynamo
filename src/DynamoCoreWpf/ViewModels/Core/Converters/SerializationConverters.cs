using System;
using System.Linq;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Notes;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Type = System.Type;

namespace Dynamo.Wpf.ViewModels.Core.Converters
{
    /// <summary>
    /// The AnnotationConverter is used to serialize and deserialize AnnotationModels.
    /// The SelectedModels property on AnnotationModel is a list of references
    /// to ModelBase objects. During serialization we want to refer to these objects
    /// by their ids. During deserialization, we use the ReferenceResolver to
    /// find the correct ModelBase instances to reference.
    /// </summary>
    public class AnnotationViewModelConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(AnnotationViewModel);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var title = obj["Title"].Value<string>();
            //if the id is not a guid, makes a guid based on the id of the model
            Guid annotationId = GuidUtility.tryParseOrCreateGuid(obj["Id"].Value<string>());

            // This is a string id, which
            // should be accessible in the ReferenceResolver.
            var models = obj["Nodes"].Values<JValue>();

            var existing = models.Select(m =>
            {
                Guid modelId = GuidUtility.tryParseOrCreateGuid(m.Value<string>());

                return serializer.ReferenceResolver.ResolveReference(serializer.Context, modelId.ToString());
            });

            var nodes = existing.Where(m => typeof(NodeModel).IsAssignableFrom(m.GetType())).Cast<NodeModel>();
            var notes = existing.Where(m => typeof(NoteModel).IsAssignableFrom(m.GetType())).Cast<NoteModel>();

            var anno = new AnnotationModel(nodes, notes);
            anno.AnnotationText = title;
            anno.GUID = annotationId;
            anno.X = obj["Left"].Value<double>();
            anno.Y = obj["Top"].Value<double>();
            anno.Width = obj["Width"].Value<double>();
            anno.Height = obj["Height"].Value<double>();
            anno.FontSize = obj["FontSize"].Value<double>();
            anno.InitialTop = obj["InitialTop"].Value<double>();
            anno.InitialHeight = obj["InitialHeight"].Value<double>();
            anno.TextBlockHeight = obj["TextblockHeight"].Value<double>();
            anno.Background = obj["Background"].Value<string>();
            return anno;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var annoVM = (AnnotationViewModel)value;
            var anno = annoVM.AnnotationModel;


            writer.WriteStartObject();

            writer.WritePropertyName("Id");
            writer.WriteValue(anno.GUID.ToString());
            writer.WritePropertyName("Title");
            writer.WriteValue(anno.AnnotationText);
            writer.WritePropertyName("Nodes");
            writer.WriteStartArray();
            foreach (var m in anno.Nodes)
            {
                writer.WriteValue(m.GUID.ToString());
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

    /// <summary>
    /// Serialize NoteViewModel.
    /// </summary>
    public class NoteViewModelConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(NoteViewModel);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var noteViewModel = (NoteViewModel)value;
            var notes = noteViewModel.Model;

            // For each noteViewModel, start a new object
            writer.WriteStartObject();
            writer.WritePropertyName("Id");
            writer.WriteValue(notes.GUID);
            writer.WritePropertyName("X");
            writer.WriteValue(notes.X);
            writer.WritePropertyName("Y");
            writer.WriteValue(notes.Y);
            writer.WritePropertyName("Text");
            writer.WriteValue(notes.Text);
            writer.WriteEndObject();
        }
    }
}
