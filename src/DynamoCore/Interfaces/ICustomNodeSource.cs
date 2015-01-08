using System;
using Dynamo.Nodes;

namespace Dynamo.Interfaces
{
    public interface ICustomNodeSource
    {
        /// <summary>
        ///     Creates a new Custom Node Instance.
        /// </summary>
        /// <param name="id">Identifier referring to a custom node definition.</param>
        /// <param name="nickname"></param>
        /// <param name="isTestMode"></param>
        Function CreateCustomNodeInstance(
            Guid id, string nickname = null, bool isTestMode = false);
    }
}