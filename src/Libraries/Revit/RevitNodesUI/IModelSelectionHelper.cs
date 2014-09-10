using System.Collections.Generic;

namespace Dynamo.Interfaces
{
    public enum SelectionType
    {
        Face,
        Edge,
        PointOnFace,
        MultipleReferences,
        Element,
        MultipleElements,
    };

    public interface IModelSelectionHelper
    {
        List<string> RequestElementSelection<T>(
            string selectionMessage, out object selectionTarget, SelectionType selectionType,
            ILogger logger);

        List<string> RequestReferenceSelection(
            string selectionMessage, out object selectionTarget, SelectionType selectionType,
            ILogger logger);
    }

}
