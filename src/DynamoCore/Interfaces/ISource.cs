using System;

namespace Dynamo.Interfaces
{
    /// <summary>
    ///     Has an event that produces items.
    /// </summary>
    /// <typeparam name="TItem">Type of items produced.</typeparam>
    public interface ISource<out TItem>
    {
        /// <summary>
        ///     Produces items, potentially asynchronously.
        /// </summary>
        event Action<TItem> ItemProduced;
    }
}