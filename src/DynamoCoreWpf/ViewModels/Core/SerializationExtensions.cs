using Dynamo.ViewModels;

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
        public static string ToJson(this WorkspaceViewModel viewModel)
        {
            return "test";
        }
    }
}
