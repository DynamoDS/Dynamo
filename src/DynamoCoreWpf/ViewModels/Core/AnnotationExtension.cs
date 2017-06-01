using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Graph;
using Dynamo.Graph.Annotations;
using Dynamo.Models;

namespace Dynamo.Wpf.ViewModels.Core
{
    /// <summary>
    /// Extension method to check if a model exists in a group
    /// </summary>
    internal static class AnnotationExtensions
    {
        public static bool ContainsModel(this IEnumerable<AnnotationModel> groups, Guid nodeGuid)
        {
            return (groups.SelectMany(m => m.Nodes).Any(m => m.GUID == nodeGuid));
        }
    }
}
