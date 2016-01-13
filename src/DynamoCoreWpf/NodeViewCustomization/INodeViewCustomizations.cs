using System;
using System.Collections.Generic;
using Dynamo.Logging;


namespace Dynamo.Wpf
{
    public interface INodeViewCustomizations
    {
        /// <summary>
        /// Get a dictionary of collections of INodeViewCustomization types.
        /// </summary>
        /// <param name="logger">An ILogger used for logging exceptions when loading INodeViewCustomizations from assemblies.</param>
        /// <returns>A dictionary of collections of INodeViewCustomization types.</returns>
        IDictionary<Type, IEnumerable<Type>> GetCustomizations(ILogger logger);
    }
}
