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
        List<string> RequestElementSelection<T>(
            string selectionMessage, SelectionType selectionType,
            SelectionObjectType objectType, ILogger logger);

        Dictionary<string,List<string>> RequestReferenceSelection(
            string selectionMessage, SelectionType selectionType,
            SelectionObjectType objectType, ILogger logger);
    }

}
