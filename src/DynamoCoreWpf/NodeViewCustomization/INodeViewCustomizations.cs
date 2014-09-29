using System;
using System.Collections.Generic;


namespace Dynamo.Wpf
{
    public interface INodeViewCustomizations
    {
        IDictionary<Type, IEnumerable<Type>> GetCustomizations();
    }
}
