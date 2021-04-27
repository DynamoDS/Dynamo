using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;

namespace ProtoCore.Reflection
{
    internal static class MemberInfoReflectionExtensions
    {
        internal static MethodInfo GetBaseDefinitionReflectionContext(this MethodInfo methodInfo)
        {
            try
            {
                // if overriding object 
                var baseType = methodInfo.DeclaringType.BaseType;
                if (baseType is null)
                {
                    return methodInfo;
                }

                // This will fail if there are overloads of the method
                var baseTypeMethod = baseType.GetMethod(methodInfo.Name);

                if (baseTypeMethod is null)
                {
                    return methodInfo;
                }

                // Need to return the original methodInfo if the method is hiding the inherited member using the new keyword
                // This is probably not a supported way of doing this... but getting desperate
                if (baseTypeMethod.IsVirtual != true && baseTypeMethod.GetMethodBody() != null)
                {
                    return methodInfo;
                }

                return baseTypeMethod;
            }
            catch (Exception)
            {
                return null;
            }

        }


        internal static Attribute[] AttributesFromReflectionContext(this MemberInfo member)
        {
            var customAttributes = CustomAttributeData.GetCustomAttributes(member);
            return GetReflectionAttributes(customAttributes);

        }

        internal static Attribute[] AttributesFromReflectionContext(this ParameterInfo parameter)
        {
            var customAttributes = CustomAttributeData.GetCustomAttributes(parameter);
            return GetReflectionAttributes(customAttributes);
        }

        internal static Attribute[] AttributesFromReflectionContext(this System.Type type)
        {
            var customAttributes = CustomAttributeData.GetCustomAttributes(type);
            return GetReflectionAttributes(customAttributes);
        }

        private static Attribute[] GetReflectionAttributes(IList<CustomAttributeData> customAttributes)
        {
            var attributes = new List<Attribute>();
            try
            {
                foreach (var ca in customAttributes)
                {
                    var type = ca.AttributeType;
                    var args = ca.ConstructorArguments.Select(x => x.Value).ToArray();
                    var loadedAssembly = Assembly.LoadFrom(type.Assembly.Location);
                    // Check if in right assembly before laoding...
                    var loadedType = loadedAssembly.GetType(type.FullName);

                    var a = Activator.CreateInstance(loadedType, args) as Attribute;

                    attributes.Add(a);
                }
            }
            catch (Exception e)
            {
                var t = e.GetType();
            }

            return attributes.ToArray();
        }
    }

    //internal class ReflectionAttribute : Attribute
    //{
    //    public Attribute AttributeType { get; set; }

    //    public ReflectionAttribute(Attribute type)
    //    {
    //        AttributeType = type;
    //    }
    //}
}
