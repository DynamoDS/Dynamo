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

    [AttributeUsage(AttributeTargets.Class)]
    public class InputParametersAttribute : Attribute
    {
        public InputParametersAttribute(params string[] values)
        {
            Values = new List<Tuple<string, string>>();

            foreach (var value in values)
            {
                var parameters = value.Split(',');
                if (parameters.Length != 2)
                    continue;

                var name = parameters[0];
                var type = parameters[1];

                Values.Add(Tuple.Create(name, type));
            }
        }

        public List<Tuple<string, string>> Values { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class OutputParametersAttribute : Attribute
    {
        public OutputParametersAttribute(params string[] values)
        {
            Values = values;
        }

        public string[] Values { get; private set; }
    }
}
