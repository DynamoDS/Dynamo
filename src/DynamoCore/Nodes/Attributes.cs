using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Autodesk.DesignScript.Runtime;

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
    /// PortAttribute is base class for all attibutes used for ports.
    /// PortAttribute can process resource file (.resx) and load strings from this file.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class PortAttribute : Attribute
    {
        /// <summary>
        /// Attribute is loaded from resource file.
        /// </summary>
        /// <param name="resourceType">type of resource</param>
        /// <param name="resourceNames">resource titles</param>
        protected PortAttribute(Type resourceType, params string[] resourceNames)
        {
            if ((resourceType == null) || (resourceNames == null))
                return;

            List<string> titles = new List<string>();
            foreach (var resourceName in resourceNames)
            {
                var property = resourceType.GetProperty(resourceName, BindingFlags.Public | BindingFlags.Static);

                if (property == null)
                {
                    throw new InvalidOperationException(string.Format("Resource Type Does Not Have Property"));
                }
                if (property.PropertyType != typeof(string))
                {
                    throw new InvalidOperationException(string.Format("Resource Property is Not String Type"));
                }
                titles.Add((string)property.GetValue(null, null));
            }

            PortTitles = titles;
        }

        /// <summary>
        /// Non-localized resource.
        /// </summary>
        /// <param name="portTitles">port titles</param>
        protected PortAttribute(params string[] portTitles)
        {
            PortTitles = portTitles;
        }

        protected IEnumerable<string> PortTitles { get; private set; }
    }

    /// <summary>
    /// Indicates ports' names.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class InPortNamesAttribute : PortAttribute
    {
        public InPortNamesAttribute(Type resourceType, params string[] resourceNames)
            : base(resourceType, resourceNames)
        { }

        public InPortNamesAttribute(params string[] names)
            : base(names)
        { }

        public IEnumerable<string> PortNames
        {
            get { return PortTitles; }
        }
    }

    /// <summary>
    /// Indicates ports' description
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class InPortDescriptionsAttribute : PortAttribute
    {
        public InPortDescriptionsAttribute(Type resourceType, params string[] resourceTypes)
            : base(resourceType, resourceTypes)
        { }

        public InPortDescriptionsAttribute(params string[] types)
            : base(types)
        { }

        public IEnumerable<string> PortDescriptions
        {
            get { return PortTitles; }
        }
    }

    /// <summary>
    /// Indicates ports' types
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class InPortTypesAttribute : PortAttribute
    {
        public InPortTypesAttribute(Type resourceType, params string[] resourceTypes)
            : base(resourceType, resourceTypes)
        { }

        public InPortTypesAttribute(params string[] types)
            : base(types)
        { }

        public IEnumerable<string> PortTypes
        {
            get { return PortTitles; }
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class OutPortNamesAttribute : Attribute
    {
        public OutPortNamesAttribute(params string[] values)
        {
            Values = values;
        }

        public string[] Values { get; private set; }
    }
}
