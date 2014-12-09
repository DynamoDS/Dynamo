using System;
using System.Reflection;

namespace Dynamo.Models
{
    public class TypeLoadData
    {
        public readonly Assembly Assembly;
        public readonly Type Type;
        public readonly string ObsoleteMessage;

        public bool IsObsolete { get { return !string.IsNullOrEmpty(ObsoleteMessage); } }

        public TypeLoadData(Assembly assemblyIn, Type typeIn, string obsoleteMsg)
        {
            Assembly = assemblyIn;
            Type = typeIn;
            ObsoleteMessage = obsoleteMsg;
        }

        public TypeLoadData(Assembly assemblyIn, Type typeIn) : this(assemblyIn, typeIn, "") { }
    }
}
