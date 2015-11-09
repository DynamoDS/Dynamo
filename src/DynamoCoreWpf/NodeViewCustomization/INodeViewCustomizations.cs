using System;
using System.Collections.Generic;
using Dynamo.Logging;


namespace Dynamo.Wpf
{
    public interface INodeViewCustomizations
    {
        IDictionary<Type, IEnumerable<Type>> GetCustomizations(ILogger logger);
    }
}
