using System;
using System.Globalization;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels.Core.Converters;
using Newtonsoft.Json;
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
            return ToJsonJObject(viewModel).ToString();
        }

        /// <summary>
        /// Convert the WorkspaceViewModel to a JObject.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns>A JObject representing the WorkspaceViewModel</returns>
        internal static JObject ToJsonJObject(this WorkspaceViewModel viewModel)
        {
            var serializer = new JsonSerializer {
                SerializationBinder = Graph.Workspaces.SerializationExtensions.Binder,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
                Culture = CultureInfo.InvariantCulture
            };
            serializer.Converters.Add(new WorkspaceViewWriteConverter());
            serializer.Converters.Add(new AnnotationViewModelConverter());
            serializer.Error += (sender, args) =>
                {
                    args.ErrorContext.Handled = true;
                    Console.WriteLine(args.ErrorContext.Error);
                };

            return JObject.FromObject(viewModel, serializer);
        }
    }
}
