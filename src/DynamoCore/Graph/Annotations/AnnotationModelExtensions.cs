using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Graph.Annotations
{
    internal static class AnnotationModelExtensions
    {
        /// <summary>
        /// Checks if any of the provided groups contains the provided
        /// ModelBase.
        /// </summary>
        /// <param name="groups"></param>
        /// <param name="modelBase"></param>
        /// <returns></returns>
        internal static bool ContainsModel(this IEnumerable<AnnotationModel> groups, ModelBase modelBase)
        {
            return groups.Any(g => g.ContainsModel(modelBase));
        }
    }
}
