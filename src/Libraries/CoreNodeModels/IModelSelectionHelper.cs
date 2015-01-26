using System.Collections.Generic;

namespace Dynamo.Interfaces
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
        /// <typeparam name="T"></typeparam>
        /// <param name="selectionMessage"></param>
        /// <param name="selectionType"></param>
        /// <param name="objectType"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        IEnumerable<T> RequestSelectionOfType(
            string selectionMessage, SelectionType selectionType,
            SelectionObjectType objectType);
    }
}
