using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;

namespace ProtoFFI.Reflection
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
                var baseTypeMethods = baseType.GetMethods()
                    .Where(x => x.Name == methodInfo.Name);

                if (!baseTypeMethods.Any())
                {
                    return methodInfo;
                }

                MethodInfo baseTypeMethod = null;

                // If there are more methods in the Base type with the same name,
                // we need to find the one where the parameter types matches.
                if (baseTypeMethods.Count() > 1)
                {
                    Type[] inputMethodArgs;

                    inputMethodArgs = methodInfo
                        .GetParameters()
                        .Select(x => x.ParameterType)
                        .ToArray();

                    foreach (var method in baseTypeMethods)
                    {
                        var methodArgs = method
                            .GetParameters()
                            .Select(x => x.ParameterType)
                            .ToArray();

                        if (methodArgs.SequenceEqual(inputMethodArgs))
                        {
                            baseTypeMethod = method;
                        }
                    }
                }

                else
                {
                    baseTypeMethod = baseTypeMethods.FirstOrDefault();
                }

                // Need to return the original methodInfo if the method is hiding the inherited member using the new keyword
                // This is probably not a supported way of doing this... but getting desperate
                if (baseTypeMethod is null || 
                    baseTypeMethod.IsVirtual != true && 
                    baseTypeMethod.GetMethodBody() != null)
                {
                    return methodInfo;
                }

                return baseTypeMethod;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return null;
            }

        }

        /// <summary>
        /// Gets all custom attributes of this MemberInfo.
        /// Compared to CustomAttributeData.GetCustomAttributes() which is usually used to get attributes 
        /// from reflection only loaded members, this will recreate the actual Attribute object so the correct values can easily be extracted from them
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        internal static IEnumerable<Attribute> GetAttributesFromReflectionContext(this MemberInfo member)
        {
            var customAttributes = CustomAttributeData.GetCustomAttributes(member);
            return GetReflectionAttributes(customAttributes);
        }

        /// <summary>
        /// Gets all custom attributes of this ParameterInfo.
        /// Compared to CustomAttributeData.GetCustomAttributes() which is usually used to get attributes 
        /// from reflection only loaded members, this will recreate the actual Attribute object so the correct values can easily be extracted from them
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        internal static IEnumerable<Attribute> GetAttributesFromReflectionContext(this ParameterInfo parameter)
        {
            var customAttributes = CustomAttributeData.GetCustomAttributes(parameter);
            return GetReflectionAttributes(customAttributes);
        }

        /// <summary>
        /// Gets all custom attributes of this ParameterInfo.
        /// Compared to CustomAttributeData.GetCustomAttributes() which is usually used to get attributes 
        /// from reflection only loaded members, this will recreate the actual Attribute object so the correct values can easily be extracted from them
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        internal static IEnumerable<Attribute> GetAttributesFromReflectionContext(this System.Type type)
        {
            var customAttributes = CustomAttributeData.GetCustomAttributes(type);
            return GetReflectionAttributes(customAttributes);
        }

        /// <summary>
        /// Recreates the actual attribute object from a CustomAttributeData. This is used to easily qurey attributes
        /// when they are coming from reflection only types, where it is only possible to retrieve CustomAttributeData from
        /// </summary>
        /// <param name="customAttributes"></param>
        /// <returns></returns>
        private static IEnumerable<Attribute> GetReflectionAttributes(IList<CustomAttributeData> customAttributes)
        {
            var attributes = new List<Attribute>();

            foreach (var ca in customAttributes)
            {
                try
                {
                    var type = ca.AttributeType;

                    var paramValues = new List<object>();
                    for (int i = 0; i < ca.ConstructorArguments.Count; i++)
                    {
                        var item = ca.ConstructorArguments[i];
                        if (item.ArgumentType.IsArray)
                        {
                            var valueColl = item.Value as ICollection;
                            var argsArray = new object[valueColl.Count];
                            var idx = 0;
                            foreach (var e in item.Value as ICollection)
                            {
                                if (e is CustomAttributeTypedArgument cs)
                                    argsArray[idx] = cs.Value.CreateInstanceFromObject(cs.ArgumentType);
                                else
                                    argsArray[idx] = e;
                                idx++;
                            }
                            if (TryConvertToTypeArray(argsArray, out Array typeArray))
                            {
                                paramValues.Add(typeArray);
                                continue;
                            }
                            paramValues.Add(argsArray);
                            continue;
                        }

                        if (item.Value is ICollection enumerable)
                        {
                            foreach (var e in enumerable)
                            {
                                if (e is CustomAttributeTypedArgument cs)
                                    paramValues.Add(cs.Value.CreateInstanceFromObject(cs.ArgumentType));
                                else
                                    paramValues.Add(e);
                            }

                            continue;
                        }
                        paramValues.Add(item.Value.CreateInstanceFromObject(item.ArgumentType));
                    }


                    object ctorParam = null;
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
                catch(Exception e)
                {
                    Console.WriteLine($"Could not create instance of {ca.AttributeType.Name} attribute: {e.InnerException?.Message ?? string.Empty}");
                }
            }

            return attributes;
        }

        private static Attribute CreateInstance(System.Type type, object args)
        {
            var loadedAssembly = Assembly.LoadFrom(type.Assembly.Location);
            var loadedType = loadedAssembly.GetType(type.FullName);

            if (!loadedType.GetConstructors().Any()) return null;

            if (args is null)
            {
                return (Attribute)Activator.CreateInstance(loadedType);
            }

            var attr = args is ICollection ?
                (Attribute)Activator.CreateInstance(loadedType, (object[])args) :
                (Attribute)Activator.CreateInstance(loadedType, args);

            return attr;
        }

        private static object CreateInstanceFromObject(this object obj, System.Type type)
        {
            if (type.IsPrimitive || type.Name == typeof(string).Name)
            {
                return obj;
            }

            if (type.IsEnum)
            {
                return InstanceFromEnum(type, (int)obj);
            }

            var typedObj = CreateInstance(type, obj);
            if (typedObj is null) return obj;

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
