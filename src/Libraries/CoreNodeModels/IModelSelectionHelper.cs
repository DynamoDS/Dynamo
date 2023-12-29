using System.Collections.Generic;
using Dynamo.Logging;

namespace CoreNodeModels
{
    public enum SelectionType
    {
        One,
        Many
    }

    public enum SelectionObjectType
    {
        Face,
        Edge,
        PointOnFace,
        Element,
        None
    };

    public interface IModelSelectionHelper<out T> : ILogSource
    {
        /// <summary>
        /// Request a selection filtered by a type.
        /// </summary>
        /// <param name="selectionMessage"></param>
        /// <param name="selectionType"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        IEnumerable<T> RequestSelectionOfType(
            string selectionMessage, SelectionType selectionType,
            SelectionObjectType objectType);
    }
}
