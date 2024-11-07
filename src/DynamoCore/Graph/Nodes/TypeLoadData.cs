using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.DesignScript.Runtime;
using Dynamo.Utilities;

namespace Dynamo.Graph.Nodes
{
    /// <summary>
    /// This class represents data loaded from assembly for each type.
    /// Based on this info node model is created.
    /// </summary>
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
        ///     The type this data is associated with.
        /// </summary>
        public readonly Type Type;

        /// <summary>
        ///     Information about why this type is obsolete, if it is actually obsolete.
        /// </summary>
        public readonly string ObsoleteMessage;

        /// <summary>
        ///     Specifies whether or not this Type is obsolete.
        /// </summary>
        public bool IsObsolete { get { return !string.IsNullOrEmpty(ObsoleteMessage); } }

        /// <summary>
        /// Creates TypeLoadData.
        /// </summary>
        /// <param name="typeIn">Type</param>
        public TypeLoadData(Type typeIn)
        {
            Type = typeIn;

            ObsoleteMessage = string.Join(
                "\n",
                Type.GetCustomAttributes<ObsoleteAttribute>(true)
                    .Select(x => string.IsNullOrEmpty(x.Message) ? "Obsolete" : x.Message));

            IsDeprecated = Type.GetCustomAttributes<NodeDeprecatedAttribute>(true).Any();
            IsMetaNode = Type.GetCustomAttributes<IsMetaNodeAttribute>(false).Any();
            IsDSCompatible = Type.GetCustomAttributes<IsDesignScriptCompatibleAttribute>(false).Any();
            IsHidden = Type.GetCustomAttributes<IsVisibleInDynamoLibraryAttribute>(true)
                .Any(attr => !attr.Visible);

            var attribs = Type.GetCustomAttributes<NodeNameAttribute>(false);
            if (attribs.Any() && !IsDeprecated && !IsMetaNode && IsDSCompatible && !IsHidden)
            {
                Name = attribs.First().Name;
            }
            else
                Name = Type.Name;

            AlsoKnownAs =
                Type.GetCustomAttributes<AlsoKnownAsAttribute>(false)
                    .SelectMany(aka => aka.Values)
                    .Concat(Name.AsSingleton());

            SearchKeys = Type.GetCustomAttributes<NodeSearchTagsAttribute>(false).SelectMany(x => x.Tags);
            Category =
                Type.GetCustomAttributes<NodeCategoryAttribute>(false)
                    .Select(x => x.ElementCategory)
                    .FirstOrDefault();
            Description =
                Type.GetCustomAttributes<NodeDescriptionAttribute>(false)
                    .Select(x => x.ElementDescription)
                    .FirstOrDefault() ?? "";

            var inputNames = Type.GetCustomAttributes<InPortNamesAttribute>(false)
                .SelectMany(x => x.PortNames).ToList();
            var inputTypes = Type.GetCustomAttributes<InPortTypesAttribute>(false)
                .SelectMany(x => x.PortTypes).ToList();

            if (inputNames.Any() && (inputNames.Count == inputTypes.Count))
            {
                InputParameters = inputNames.Zip(inputTypes, (name, type) => new Tuple<string, string>(name, type));
            }
            else
            {
                InputParameters = new List<Tuple<string, string>>();
            }


            OutputParameters = Type.GetCustomAttributes<OutPortTypesAttribute>(false)
                .SelectMany(x => x.PortTypes);
            IsExperimental = CheckExperimentalFromAttribute(Type);
        }

        internal static bool CheckExperimentalFromAttribute(System.Type type)
        {
            return type.GetCustomAttributes<System.Diagnostics.CodeAnalysis.ExperimentalAttribute>(false).Any();
        }

        /// <summary>
        ///     Other names this type might go by inside of Dynamo.
        /// </summary>
        public readonly IEnumerable<string> AlsoKnownAs;

        /// <summary>
        ///     Specifies whether or not this type is deprecated by Dynamo.
        /// </summary>
        public readonly bool IsDeprecated;

        /// <summary>
        ///     Specifies whether or not this type is considered a "meta node"
        /// </summary>
        public readonly bool IsMetaNode;

        /// <summary>
        ///     Specifies whether or not this type is considered "DesignScript compatible"
        /// </summary>
        public readonly bool IsDSCompatible;
        
        /// <summary>
        ///     Specifies whether or not this type should be hidden from users in search.
        /// </summary>
        public readonly bool IsHidden;
        
        /// <summary>
        ///     The Name associated with this type.
        /// </summary>
        public readonly string Name;

        /// <summary>
        ///     Search keys that can be used to search for this type.
        /// </summary>
        public readonly IEnumerable<string> SearchKeys;

        /// <summary>
        ///     The category of this type, used in search.
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
        ///     The description of this type.
        /// </summary>
        public readonly string Description;

        /// <summary>
        ///     Indicates if the type is loaded from a package.
        /// </summary>
        public bool IsPackageMember;

        /// <summary>
        /// Indicates input parameters.
        /// </summary>
        public readonly IEnumerable<Tuple<string, string>> InputParameters;

        /// <summary>
        /// Indicates output parameters.
        /// </summary>
        public readonly IEnumerable<string> OutputParameters;
        /// <summary>
        /// Is this type experimental/unstable.
        /// </summary>
        internal bool IsExperimental;
    }
}
