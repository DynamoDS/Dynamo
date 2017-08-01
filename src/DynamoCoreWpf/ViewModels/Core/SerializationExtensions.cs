using Dynamo.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Dynamo.Wpf.ViewModels.Core.Converters;

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
                Converters = new List<JsonConverter>{
                    new AnnotationViewModelConverter(),
                    new NoteViewModelConverter(),
                },
            };

            var json = JsonConvert.SerializeObject(viewModel, settings);

            return json;
        }
    }
}
