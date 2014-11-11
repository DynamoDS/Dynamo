using System;
using System.Collections.Generic;
using System.ComponentModel;
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

            SearchKeys = Type.GetCustomAttributes<NodeSearchTagsAttribute>(false).SelectMany(x => x.Tags);
            Category =
                Type.GetCustomAttributes<NodeCategoryAttribute>(false)
                    .Select(x => x.ElementCategory)
                    .FirstOrDefault();
            Description =
                Type.GetCustomAttributes<NodeDescriptionAttribute>(false)
                    .Select(x => x.ElementDescription)
                    .FirstOrDefault() ?? "";
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

        /// <summary>
        /// TODO
        /// </summary>
        public readonly IEnumerable<string> SearchKeys;

        /// <summary>
        /// TODO
        /// </summary>
        public string Category
        {
            get
            {
                if (string.IsNullOrWhiteSpace(category))
                    return Type.Namespace;
                return category;
            }
            private set { category = value; }
        }
        private string category;

        /// <summary>
        /// TODO
        /// </summary>
        public readonly string Description;
    }
}
