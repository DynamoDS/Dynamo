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

    public interface IModelSelectionHelper
    {
        /// <summary>
        /// Request a selection.
        /// </summary>
        /// <param name="selectionMessage"></param>
        /// <param name="selectionType"></param>
        /// <param name="objectType"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        Dictionary<string, List<string>> RequestSelection(
            string selectionMessage, SelectionType selectionType,
            SelectionObjectType objectType, ILogger logger);

        /// <summary>
        /// Request a selection filtered by a type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectionMessage"></param>
        /// <param name="selectionType"></param>
        /// <param name="objectType"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        List<string> RequestSelectionOfType<T>(
            string selectionMessage, SelectionType selectionType,
            SelectionObjectType objectType, ILogger logger);

        /// <summary>
        /// Request a sub selection of an initial selection 
        /// filtered by type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectionMessage"></param>
        /// <param name="selectionType"></param>
        /// <param name="objectType"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        Dictionary<string, List<string>> RequestSubSelectionOfType<T>(
            string selectionMessage, SelectionType selectionType,
            SelectionObjectType objectType, ILogger logger);
    }

}
