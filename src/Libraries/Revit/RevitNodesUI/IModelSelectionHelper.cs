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
            string selectionMessage, out object selectionTarget, SelectionType selectionType,
            SelectionObjectType objectType, ILogger logger);

        List<string> RequestReferenceSelection(
            string selectionMessage, out object selectionTarget, SelectionType selectionType,
            SelectionObjectType objectType, ILogger logger);
    }

}
