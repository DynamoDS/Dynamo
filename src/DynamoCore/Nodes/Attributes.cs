using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Autodesk.DesignScript.Runtime;

using Dynamo.Utilities;

namespace Dynamo.Nodes
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

    [AttributeUsage(AttributeTargets.All)]
    public class NodeCategoryAttribute : Attribute
    {
        public NodeCategoryAttribute(string category)
        {
            ElementCategory = category;
        }

        public string ElementCategory { get; set; }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class NodeSearchTagsAttribute : Attribute
    {
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
        public NodeSearchTagsAttribute(params string[] tags)
        {
            Tags = tags.ToList();
        }

        public List<string> Tags { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class NotSearchableInHomeWorkspace : Attribute
    { }

    [AttributeUsage(AttributeTargets.Class)]
    public class NotSearchableInCustomNodeWorkspace : Attribute
    { }

    [AttributeUsage(AttributeTargets.All)]
    public class IsInteractiveAttribute : Attribute
    {
        public IsInteractiveAttribute(bool isInteractive)
        {
            IsInteractive = isInteractive;
        }

        public bool IsInteractive { get; set; }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class NodeDescriptionAttribute : Attribute
    {
        public NodeDescriptionAttribute(string description)
        {
            ElementDescription = description;
        }

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

        public string ElementDescription { get; set; }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class NodeSearchableAttribute : Attribute
    {
        public NodeSearchableAttribute(bool isSearchable)
        {
            IsSearchable = isSearchable;
        }

        public bool IsSearchable { get; set; }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class NodeTypeIdAttribute : Attribute
    {
        public NodeTypeIdAttribute(string description)
        {
            Id = description;
        }

        public string Id { get; set; }
    }

    /// <summary>
    ///     The DoNotLoadOnPlatforms attribute allows the node implementor
    ///     to define an array of contexts in which the node will not
    ///     be loaded.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class DoNotLoadOnPlatformsAttribute : Attribute
    {
        public DoNotLoadOnPlatformsAttribute(params string[] values)
        {
            Values = values;
        }

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
        public AlsoKnownAsAttribute(params string[] values)
        {
            Values = values;
        }

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
    ///    The NodeDescriptionAttribute indicates this node is obsolete
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
