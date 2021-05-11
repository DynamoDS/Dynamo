using System;
using System.Collections;
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

            foreach (var ca in customAttributes)
            {
                try
                {
                    var type = ca.AttributeType;

                    var paramValues = new List<dynamic>();
                    for (int i = 0; i < ca.ConstructorArguments.Count; i++)
                    {
                        var item = ca.ConstructorArguments[i];
                        if (item.Value is ICollection enumerable)
                        {
                            foreach (var e in enumerable)
                            {
                                if (e is CustomAttributeTypedArgument cs)
                                    paramValues.Add(cs.Value.InstanceFromObject(cs.ArgumentType));
                                else
                                    paramValues.Add(e);
                            }

                            continue;
                        }
                        paramValues.Add(item.Value.InstanceFromObject(item.ArgumentType));
                    }


                    dynamic ctorParam = null;
                    if (paramValues.Count > 1)
                    {
                        var array = paramValues.ToArray<object>();
                        if (TryConvertToTypeArray(array, out Array newArray))
                        {
                            ctorParam = newArray;
                        }

                        else
                        {
                            ctorParam = array;
                        }
                    }
                    else
                    {
                        ctorParam = paramValues.FirstOrDefault();
                    }  

                    var a = CreateInstance(type, ctorParam);
                    if (a is null) continue;

                    attributes.Add(a);
                }
                catch (Exception e)
                {
                }
            }

            return attributes.ToArray();
        }

        private static Attribute CreateInstance(System.Type type, object args)
        {
            var loadedAssembly = Assembly.LoadFrom(type.Assembly.Location);
            // Check if in right assembly before laoding...
            var loadedType = loadedAssembly.GetType(type.FullName);

            if (!loadedType.GetConstructors().Any())
                return null;

            if (args is null)
                return (Attribute)Activator.CreateInstance(loadedType);


            return (Attribute)Activator.CreateInstance(loadedType, args);
        }

        private static object InstanceFromObject(this object obj, System.Type type)
        {
            if (type.IsPrimitive || type.Name == typeof(string).Name)
                return obj;

            if (type.IsEnum)
                return InstanceFromEnum(type, (int)obj);

            var typedObj = CreateInstance(type, obj);
            if (typedObj is null)
                return obj;

            return typedObj;
        }

        private static object InstanceFromEnum(System.Type type, int value)
        {
            var loadedAssembly = Assembly.LoadFrom(type.Assembly.Location);
            var loadedType = loadedAssembly.GetType(type.FullName);
            if (!loadedType.IsEnum) return null;

            return Enum.ToObject(loadedType, value);
        }

        private static bool TryConvertToTypeArray(object[] array, out Array typeArray)
        {
            typeArray = array;
            var typesInArray = array.Select(x => x.GetType()).ToList();
            if (!typesInArray
                .All(x => x.Equals(typesInArray.FirstOrDefault())))
            {
                return false;
            }

            typeArray = Array.CreateInstance(typesInArray.FirstOrDefault(), array.Length);
            Array.Copy(array, typeArray, typeArray.Length);
            return true;

        }
    }
}
