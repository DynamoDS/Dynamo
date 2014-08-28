using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Dynamo.Models;
using Dynamo.Utilities;

namespace Dynamo.Wpf
{
    internal interface INodeCustomizations
    {
        IDictionary<Type, IEnumerable<Type>> GetCustomizations();
    }
}
