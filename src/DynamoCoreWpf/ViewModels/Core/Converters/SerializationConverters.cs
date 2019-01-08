using System;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Notes;
using Dynamo.ViewModels;
using Newtonsoft.Json;
using Type = System.Type;

namespace Dynamo.Wpf.ViewModels.Core.Converters
{
    /// <summary>
    /// WorkspaceWriteConverter is used for serializing Workspaces to JSON.
    /// </summary>
    public class WorkspaceViewWriteConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(WorkspaceViewModel).IsAssignableFrom(objectType);
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var workspaceView = (WorkspaceViewModel)value;

            writer.WriteStartObject();

            writer.WritePropertyName("Dynamo");
            serializer.Serialize(writer, workspaceView.DynamoPreferences);

            writer.WritePropertyName("Camera");
            serializer.Serialize(writer, workspaceView.Camera);

            writer.WritePropertyName("NodeViews");
            writer.WriteStartArray();
            foreach (var nodeView in workspaceView.Nodes)
            {
              serializer.Serialize(writer, nodeView);
            }
            writer.WriteEndArray();

            writer.WritePropertyName("Annotations");
            writer.WriteStartArray();
            foreach (var annotation in workspaceView.Annotations)
                serializer.Serialize(writer, annotation);
            foreach (var note in workspaceView.Notes)
            {
                AnnotationModel convertedNote = new AnnotationModel(new NodeModel[0], new NoteModel[0]);
                convertedNote.GUID = note.Model.GUID;
                convertedNote.X = note.Left;
                convertedNote.Y = note.Top;
                convertedNote.AnnotationText = note.Text;

                serializer.Serialize(writer, new AnnotationViewModel(workspaceView, convertedNote));
            }
            writer.WriteEndArray();

            writer.WritePropertyName("X");
            writer.WriteValue(workspaceView.X);

            writer.WritePropertyName("Y");
            writer.WriteValue(workspaceView.Y);

            writer.WritePropertyName("Zoom");
            writer.WriteValue(workspaceView.Zoom);

            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

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
