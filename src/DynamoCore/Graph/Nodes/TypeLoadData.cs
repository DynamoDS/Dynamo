using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.DesignScript.Runtime;
using Dynamo.Utilities;
using ProtoCore.Reflection;

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

        public TypeLoadData(Type type, string obsoleteMessage, IEnumerable<string> alsoKnownAs, bool isDeprecated, bool isMetaNode, 
            bool isDSCompatible, bool isHidden, string name, IEnumerable<string> searchKeys, string description, 
            IEnumerable<Tuple<string, string>> inputParameters, IEnumerable<string> outputParameters)
        {
            Type = type;
            ObsoleteMessage = obsoleteMessage;
            AlsoKnownAs = alsoKnownAs;
            IsDeprecated = isDeprecated;
            IsMetaNode = isMetaNode;
            IsDSCompatible = isDSCompatible;
            IsHidden = isHidden;
            Name = name;
            SearchKeys = searchKeys;
            Description = description;
            InputParameters = inputParameters;
            OutputParameters = outputParameters;
        }


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
        }

        internal static TypeLoadData FromReflectionType(Type type)
        {
            var attributes = type.AttributesFromReflectionContext();
            var customAttributes = CustomAttributeData.GetCustomAttributes(type).ToList();
            var customAttributeTypes = customAttributes.Select(x => x.AttributeType).ToList();

            if (customAttributes is null || customAttributes.Count == 0)
                return null;

            var obsoleteMessage = string.Join(
                "\n",
                customAttributes.
                    Where(x => x.AttributeType.Name == nameof(ObsoleteAttribute)).
                    Select(x => x.ConstructorArguments.
                    Select(arg => string.IsNullOrEmpty(arg.Value as string) ? "Obsolete" : arg.Value as string)));

            var isDeprecated = customAttributes.
                Where(x => x.AttributeType.Name == nameof(NodeDeprecatedAttribute)).
                Any();
            var isMetaNode = customAttributes.
                Where(x => x.AttributeType.Name == nameof(IsMetaNodeAttribute)).
                Any();
            var isDSCompatible = customAttributes.
                Where(x => x.AttributeType.Name == nameof(IsDesignScriptCompatibleAttribute)).
                Any();

            var isHidden = customAttributes.
                Where(x => x.AttributeType.Name == nameof(IsVisibleInDynamoLibraryAttribute)).
                Any(attr => attr.ConstructorArguments.Any(v => (bool)v.Value == true));


            var attribs = customAttributes.
                Where(x => x.AttributeType.Name == nameof(NodeNameAttribute)).
                Select(x => x.ConstructorArguments).
                FirstOrDefault();

            string name = null;
            if (!(attribs is null) && attribs.Any() && !isDeprecated && !isMetaNode && isDSCompatible && !isHidden)
            {
                name = attribs.First().Value as string;
            }
            else
                name = type.Name;

            var alsoKnownAs = customAttributes.
                Where(x => x.AttributeType.Name == nameof(AlsoKnownAsAttribute))
                    .SelectMany(aka => aka.ConstructorArguments.Select(x=>x.Value as string)
                    .Concat(name.AsSingleton()));

            var searchKeys = customAttributes.
                Where(x => x.AttributeType.Name == nameof(NodeSearchTagsAttribute)).
                SelectMany(tag => tag.ConstructorArguments.Select(v => v.Value as string));


            var category = customAttributes.
                Where(x => x.AttributeType.Name == nameof(NodeCategoryAttribute)).
                SelectMany(x => x.ConstructorArguments).
                FirstOrDefault().Value as string;

            var description =  customAttributes.
                Where(x => x.AttributeType.Name == nameof(NodeDescriptionAttribute)).
                SelectMany(x => x.ConstructorArguments).
                FirstOrDefault().Value as string;


            var inputNames = customAttributes.
                Where(x => x.AttributeType.Name == nameof(InPortNamesAttribute)).
                SelectMany(x => x.ConstructorArguments).
                Select(x => x.Value as string).
                ToList();                

            var inputTypes = customAttributes.
                Where(x => x.AttributeType.Name == nameof(InPortTypesAttribute)).
                SelectMany(x => x.ConstructorArguments).
                Select(x => x.Value as string).
                ToList();

            IEnumerable<Tuple<string, string>> inputParameters = null;
            if (inputNames.Any() && (inputNames.Count == inputTypes.Count))
            {
                inputParameters = inputNames.Zip(inputTypes, (n, t) => new Tuple<string, string>(name, t));
            }
            else
            {
                inputParameters = new List<Tuple<string, string>>();
            }

            var outputParameters = customAttributes.
                Where(x => x.AttributeType.Name == nameof(OutPortTypesAttribute)).
                SelectMany(x => x.ConstructorArguments).
                Select(x => x.Value as string).
                ToList();

            return new TypeLoadData(type,obsoleteMessage,alsoKnownAs,isDeprecated,isMetaNode,isDSCompatible,isHidden,name,searchKeys,description,inputParameters,outputParameters);
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
    }
}
