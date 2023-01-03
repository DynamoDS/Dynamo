using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.DesignScript.Runtime;
using Dynamo.Utilities;

namespace Dynamo.Graph.Nodes
{
    [AttributeUsage(AttributeTargets.All)]
    public class NodeNameAttribute : Attribute
    {
        public NodeNameAttribute(string elementName)
        {
            Name = elementName;
        }

        public string Name { get; set; }
    }

    /// <summary>
    ///     The NodeCategoryAttribute attribute allows the node implementer
    ///     to define in which category node will appear. 
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class NodeCategoryAttribute : Attribute
    {
        /// <summary>
        /// Creates NodeCategoryAttribute.
        /// </summary>
        /// <param name="category">Full name of the category. E.g. Core.List.Create</param>
        public NodeCategoryAttribute(string category)
        {
            ElementCategory = category;
        }

        /// <summary>
        /// Full name of the category. E.g. Core.List.Create
        /// </summary>
        public string ElementCategory { get; set; }
    }

    /// <summary>
    ///     The NodeCategoryAttribute attribute allows the node implementer
    ///     to define search tags.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class NodeSearchTagsAttribute : Attribute
    {
        /// <summary>
        /// Creates NodeSearchTagsAttribute with tags from resx file. 
        /// </summary>
        /// <param name="tagsID">Tag name</param>
        /// <param name="resourceType">Type of Resource. E.g. typeof(Resource)</param>
        public NodeSearchTagsAttribute(string tagsID, Type resourceType)
        {
            if (resourceType == null)
                throw new ArgumentNullException("resourceType");

            //Sometimes resources are made internal so that they don't appear in 
            //node library, hence we also need to query non public properties.
            var prop = resourceType.GetProperty(tagsID, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
            if (prop != null && prop.PropertyType == typeof(String))
            {
                var tagString = (string)prop.GetValue(null, null);
                Tags = tagString.Split(';').ToList();
            }
            else
            {
                Tags = new List<String> { tagsID };
            }
        }

        /// <summary>
        /// Creates NodeSearchTagsAttribute with string tags.
        /// </summary>
        /// <param name="tags"></param>
        public NodeSearchTagsAttribute(params string[] tags)
        {
            Tags = tags.ToList();
        }

        /// <summary>
        /// Search tags used in library search.
        /// </summary>
        public List<string> Tags { get; set; }
    }

    /// <summary>
    ///     The NodeDescriptionAttribute attribute allows the node implementer
    ///     to define node description.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class NodeDescriptionAttribute : Attribute
    {
        /// <summary>
        /// Creates NodeDescriptionAttribute with string.
        /// </summary>
        /// <param name="description">String description. E.g. "Get a color given a color range." for ColorRange node.</param>
        public NodeDescriptionAttribute(string description)
        {
            ElementDescription = description;
        }

        /// <summary>
        /// Creates NodeDescriptionAttribute with description from resx file.
        /// </summary>
        /// <param name="descriptionResourceID">Resource name</param>
        /// <param name="resourceType">Type of Resource. E.g. typeof(Resource)</param>
        public NodeDescriptionAttribute(string descriptionResourceID, Type resourceType)
        {
            if (resourceType == null)
                throw new ArgumentNullException("resourceType");

            //Sometimes resources are made internal so that they don't appear in 
            //node library, hence we also need to query non public properties.
            var prop = resourceType.GetProperty(descriptionResourceID, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
            if (prop != null && prop.PropertyType == typeof(String))
            {
                ElementDescription = (string)prop.GetValue(null, null);
            }
            else
            {
                ElementDescription = descriptionResourceID;
            }
        }

        /// <summary>
        /// Description of the node.
        /// E.g. "Get a color given a color range." for ColorRange node.
        /// </summary>
        public string ElementDescription { get; set; }
    }

    /// <summary>
    ///     The DoNotLoadOnPlatforms attribute allows the node implementer
    ///     to define an array of contexts.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class DoNotLoadOnPlatformsAttribute : Attribute
    {
        /// <summary>
        /// Creates DoNotLoadOnPlatformsAttribute with restricted contexts.
        /// </summary>
        /// <param name="values"></param>
        public DoNotLoadOnPlatformsAttribute(params string[] values)
        {
            Values = values;
        }

        /// <summary>
        /// Restricted contexts, i.e. contexts in which node won't be loaded.
        /// E.g. Revit 2014
        /// </summary>
        public string[] Values { get; set; }
    }


    /// <summary>
    ///     Flag to hide deprecated nodes in search, but allow in workflows
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class NodeDeprecatedAttribute : Attribute { }

    /// <summary>
    ///     The AlsoKnownAs attribute allows the node implementor to
    ///     define an array of names that this node might have had
    ///     in the past.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class AlsoKnownAsAttribute : Attribute
    {
        /// <summary>
        /// Constructs AlsoKnownAsAttribute with defined old names.
        /// </summary>
        /// <param name="values">Old names, that node used to have.</param>
        public AlsoKnownAsAttribute(params string[] values)
        {
            Values = values;
        }

        /// <summary>
        /// Old names, that node used to have.
        /// </summary>
        public string[] Values { get; set; }
    }

    /// <summary>
    ///     The MetaNode attribute means this node shouldn't be added to the category,
    ///     only its instances are allowed
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    public class IsMetaNodeAttribute : Attribute { }

    /// <summary>
    ///     The IsDesignScriptCompatibleAttribute indicates if the node is able
    ///     to work with DesignScript evaluation engine.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    public class IsDesignScriptCompatibleAttribute : Attribute { }

    /// <summary>
    ///    The NodeObsoleteAttribute indicates this node is obsolete
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public sealed class NodeObsoleteAttribute : IsObsoleteAttribute
    {
        public NodeObsoleteAttribute(string message)
            : base(message)
        {
        }

        public NodeObsoleteAttribute(string descriptionResourceID, Type resourceType)
        {
            if (resourceType == null)
                throw new ArgumentNullException("resourceType");

            var prop = resourceType.GetProperty(descriptionResourceID, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
            if (prop != null && prop.PropertyType == typeof(String))
            {
                Message = (string)prop.GetValue(null, null);
            }
            else
            {
                Message = descriptionResourceID;
            }
        }
    }

    /// <summary>
    /// Indicates input ports' names.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class InPortNamesAttribute : Attribute
    {
        /// <summary>
        /// Loads ports names from resource.
        /// </summary>
        /// <param name="resourceType">resource type, i.e. type of resource class specified in .resx file</param>
        /// <param name="resourceNames">resource names</param>
        public InPortNamesAttribute(Type resourceType, params string[] resourceNames)
        {
            portNames = ResourceLoader.Load(resourceType, resourceNames);
        }

        /// <summary>
        /// Loads ports names, that are specified directly in Attribute.
        /// </summary>
        /// <param name="names"></param>
        public InPortNamesAttribute(params string[] names)
        {
            portNames = names;
        }

        private readonly IEnumerable<string> portNames;

        /// <summary>
        /// Port titles, that are shown in Dynamo.
        /// </summary>
        public IEnumerable<string> PortNames
        {
            get { return portNames; }
        }
    }

    /// <summary>
    /// Indicates input ports' description
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class InPortDescriptionsAttribute : Attribute
    {
        /// <summary>
        /// Loads ports descriptions from resource.
        /// </summary>
        /// <param name="resourceType">resource type, i.e. type of resource class specified in .resx file</param>
        /// <param name="resourceNames">resource names</param>
        public InPortDescriptionsAttribute(Type resourceType, params string[] resourceNames)
        {
            portDescriptions = ResourceLoader.Load(resourceType, resourceNames);
        }

        /// <summary>
        /// Loads ports descriptions, that are specified directly in Attribute.
        /// </summary>
        /// <param name="descriptions"></param>
        public InPortDescriptionsAttribute(params string[] descriptions)
        {
            portDescriptions = descriptions;
        }

        private readonly IEnumerable<string> portDescriptions;

        /// <summary>
        /// Port descriptions, that are shown in Dynamo.
        /// </summary>
        public IEnumerable<string> PortDescriptions
        {
            get { return portDescriptions; }
        }
    }

    /// <summary>
    /// Indicates input ports' types
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class InPortTypesAttribute : Attribute
    {
        /// <summary>
        /// Loads ports types from resource.
        /// </summary>
        /// <param name="resourceType">resource type, i.e. type of resource class specified in .resx file</param>
        /// <param name="resourceNames">resource names</param>
        public InPortTypesAttribute(Type resourceType, params string[] resourceNames)
        {
            portTypes = ResourceLoader.Load(resourceType, resourceNames);
        }

        /// <summary>
        /// Loads ports types, that are specified directly in Attribute.
        /// </summary>
        /// <param name="types"></param>
        public InPortTypesAttribute(params string[] types)
        {
            portTypes = types;
        }

        private readonly IEnumerable<string> portTypes;

        /// <summary>
        /// Port types, that are shown in Dynamo tooltips.
        /// </summary>
        public IEnumerable<string> PortTypes
        {
            get { return portTypes; }
        }
    }


    /// <summary>
    /// Indicates output ports' names.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class OutPortNamesAttribute : Attribute
    {
        /// <summary>
        /// Loads ports names from resource.
        /// </summary>
        /// <param name="resourceType">resource type, i.e. type of resource class specified in .resx file</param>
        /// <param name="resourceNames">resource names</param>
        public OutPortNamesAttribute(Type resourceType, params string[] resourceNames)
        {
            portNames = ResourceLoader.Load(resourceType, resourceNames);
        }

        /// <summary>
        /// Loads ports names, that are specified directly in Attribute.
        /// </summary>
        /// <param name="names"></param>
        public OutPortNamesAttribute(params string[] names)
        {
            portNames = names;
        }

        private readonly IEnumerable<string> portNames;

        /// <summary>
        /// Port titles, that are shown in Dynamo.
        /// </summary>
        public IEnumerable<string> PortNames
        {
            get { return portNames; }
        }
    }

    /// <summary>
    /// Indicates output ports' description
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class OutPortDescriptionsAttribute : Attribute
    {
        /// <summary>
        /// Loads ports descriptions from resource.
        /// </summary>
        /// <param name="resourceType">resource type, i.e. type of resource class specified in .resx file</param>
        /// <param name="resourceNames">resource names</param>
        public OutPortDescriptionsAttribute(Type resourceType, params string[] resourceNames)
        {
            portDescriptions = ResourceLoader.Load(resourceType, resourceNames);
        }

        /// <summary>
        /// Loads ports descriptions, that are specified directly in Attribute.
        /// </summary>
        /// <param name="descriptions"></param>
        public OutPortDescriptionsAttribute(params string[] descriptions)
        {
            portDescriptions = descriptions;
        }

        private readonly IEnumerable<string> portDescriptions;

        /// <summary>
        /// Port descriptions, that are shown in Dynamo.
        /// </summary>
        public IEnumerable<string> PortDescriptions
        {
            get { return portDescriptions; }
        }
    }

    /// <summary>
    /// Indicates output ports' types
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class OutPortTypesAttribute : Attribute
    {
        /// <summary>
        /// Loads ports types from resource.
        /// </summary>
        /// <param name="resourceType">resource type, i.e. type of resource class specified in .resx file</param>
        /// <param name="resourceNames">resource names</param>
        public OutPortTypesAttribute(Type resourceType, params string[] resourceNames)
        {
            portTypes = ResourceLoader.Load(resourceType, resourceNames);
        }

        /// <summary>
        /// Loads ports types, that are specified directly in Attribute.
        /// </summary>
        /// <param name="types"></param>
        public OutPortTypesAttribute(params string[] types)
        {
            portTypes = types;
        }

        private readonly IEnumerable<string> portTypes;

        /// <summary>
        /// Port types, that are shown in Dynamo tooltips.
        /// </summary>
        public IEnumerable<string> PortTypes
        {
            get { return portTypes; }
        }
    }
}
