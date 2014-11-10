using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dynamo.Models
{
    public class TypeLoadData
    {
        /// <summary>
        ///     Assembly containing the type.
        /// </summary>
        public Assembly Assembly
        {
            get { return Type.Assembly; }
        }

        /// <summary>
        ///     
        /// </summary>
        public readonly Type Type;

        public TypeLoadData(Type typeIn)
        {
            Type = typeIn;
        }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> AlsoKnownAs
        {
            get
            {
                return
                    Type.GetCustomAttributes(false)
                        .OfType<AlsoKnownAsAttribute>()
                        .SelectMany(aka => aka.Values);
            }
        }
    }
}
