using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Autodesk.DesignScript.Runtime;

using Dynamo.Utilities;

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
        ///     TODO
        /// </summary>
        public readonly Type Type;

        public TypeLoadData(Type typeIn)
        {
            Type = typeIn;

            AlsoKnownAs = Type.GetCustomAttributes<AlsoKnownAsAttribute>(false)
                .SelectMany(aka => aka.Values)
                .ToArray();

            IsDeprecated = Type.GetCustomAttributes<NodeDeprecatedAttribute>(true).Any();
            IsMetaNode = Type.GetCustomAttributes<IsMetaNodeAttribute>(false).Any();
            IsDSCompatible = Type.GetCustomAttributes<IsMetaNodeAttribute>(false).Any();
            IsHidden = Type.GetCustomAttributes<IsVisibleInDynamoLibraryAttribute>(true)
                .Any(attr => !attr.Visible);

            var attribs = Type.GetCustomAttributes<NodeNameAttribute>(false);
            if (attribs.Any() && !IsDeprecated && !IsMetaNode && IsDSCompatible && !IsHidden)
            {
                Name = attribs.First().Name;
            }
            else
                Name = Type.Name;
        }

        /// <summary>
        ///     TODO
        /// </summary>
        public readonly IEnumerable<string> AlsoKnownAs;

        /// <summary>
        ///     TODO
        /// </summary>
        public readonly bool IsDeprecated;

        /// <summary>
        /// TODO
        /// </summary>
        public readonly bool IsMetaNode;

        /// <summary>
        /// TODO
        /// </summary>
        public readonly bool IsDSCompatible;
        
        /// <summary>
        /// TODO
        /// </summary>
        public readonly bool IsHidden;
        
        /// <summary>
        /// TODO
        /// </summary>
        public readonly string Name;
    }
}
