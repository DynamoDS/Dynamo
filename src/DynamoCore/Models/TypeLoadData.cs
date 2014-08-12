using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dynamo.Models
{
    public class TypeLoadData
    {
        public Assembly Assembly;
        public Type Type;

        public TypeLoadData(Assembly assemblyIn, Type typeIn)
        {
            Assembly = assemblyIn;
            Type = typeIn;
        }
    }

}
