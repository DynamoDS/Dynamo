using System;

namespace Dynamo.Graph.Nodes.CustomNodes
{
    public interface ICustomNodeSource
    {
        /// <summary>
        ///     Creates a new Custom Node Instance.
        /// </summary>
        /// <param name="id">Identifier referring to a custom node definition.</param>
        /// <param name="name"></param>
        /// <param name="isTestMode"></param>
        /// <param name="libraryServices">LibraryServices used for code block node initialization.</param>
        Function CreateCustomNodeInstance(
            Guid id, string name = null, bool isTestMode = false, Engine.LibraryServices libraryServices = null);
    }
}