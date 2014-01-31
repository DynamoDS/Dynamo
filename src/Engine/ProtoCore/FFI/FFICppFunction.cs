using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;

namespace ProtoCore.Lang
{
    class FFICppFunction2
    {
        private readonly string Name;
        private readonly System.Type ReturnType;
        private readonly Dictionary<string, MethodInfo> MethodGroup;
        private readonly ModuleBuilder ModuleBuilder;
        private readonly AssemblyName AssemblyName;
        private readonly AssemblyBuilder AssemblyBuilder;

        public FFICppFunction2(ModuleBuilder moduleBuilder, AssemblyName assemblyName, 
            AssemblyBuilder assemblyBuilder, string name) 
        {
            ModuleBuilder = moduleBuilder;
            AssemblyBuilder = assemblyBuilder;
            AssemblyName = assemblyName;
            Name = name;
            MethodGroup = new Dictionary<string, MethodInfo>();
        }

        public FFICppFunction2(ModuleBuilder moduleBuilder,AssemblyName assemblyName, 
            AssemblyBuilder assemblyBuilder, string name, System.Type returnType) : 
            this(moduleBuilder, assemblyName, assemblyBuilder, name)
        {
            ReturnType = returnType;
        }

        public object Execute(object[] parameters)
        {
            System.Type[] parameterTypes = GetParameterTypes(parameters);
            MethodInfo m = FindMethod(parameterTypes);
            object retVal = null;

            #region INVOKE_METHOD
            try
            {
                retVal = m.Invoke(null, parameters);
            }
            catch (DllNotFoundException ex)
            {
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);
                }
                Console.WriteLine(ex.Message);
            }
            catch (System.Reflection.TargetException ex)
            {
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);
                }
                Console.WriteLine(ex.Message);
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);
                }
                Console.WriteLine(ex.Message);
            }
            catch (System.Reflection.TargetParameterCountException ex)
            {
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);
                }
                Console.WriteLine(ex.Message);
            }
            catch (System.MethodAccessException ex)
            {
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);
                }
                Console.WriteLine(ex.Message);
            }
            catch (System.InvalidOperationException ex)
            {
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);
                }
                Console.WriteLine(ex.Message);
            }
            catch (System.NotSupportedException ex)
            {
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);
                }
                Console.WriteLine(ex.Message);
            }
            catch (System.ArgumentException ex)
            {
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);
                }
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);
                }
                Console.WriteLine(ex.Message);
            }
            #endregion
            
            return retVal;
        }

        private string MassageReturnType(System.Type type)
        {
            if (type.IsArray)
            {
                return typeof(IntPtr).ToString();
            }
            return type.ToString();
        }

        private MethodInfo FindMethod(System.Type[] parameterTypes)
        { 
            StringBuilder b = new StringBuilder();
            
            b.Append(AssemblyName.Name).Append(".").Append(Name).Append("(");
            if(parameterTypes.Length > 0)
            {
                b.Append(parameterTypes[0]);
            }

            for(int i = 1; i < parameterTypes.Length; ++i)
            {
                b.Append(parameterTypes[i].FullName);
            }
            b.Append(")");
            b.Append(MassageReturnType(ReturnType));


            string fullname = b.ToString();

            if (MethodGroup.ContainsKey(fullname))
            {
                return MethodGroup[fullname];
            }
            
            TypeBuilder typeBuilder = ModuleBuilder.DefineType(fullname, TypeAttributes.Public);
            MethodBuilder methodBuilder = typeBuilder.DefinePInvokeMethod(
                Name,
                AssemblyName.Name,
                MethodAttributes.PinvokeImpl | MethodAttributes.Static | MethodAttributes.Public,
                CallingConventions.Standard, ReturnType, parameterTypes, CallingConvention.Cdecl,
                CharSet.Ansi);

            methodBuilder.SetImplementationFlags(methodBuilder.GetMethodImplementationFlags() | MethodImplAttributes.PreserveSig);
            MethodInfo methodInfo = typeBuilder.CreateType().GetMethod(Name);
            MethodGroup.Add(fullname, methodInfo);
            return methodInfo;
        }

        private static System.Type[] GetParameterTypes(object[] parameters)
        {
            System.Type[] parameterTypes = new System.Type[parameters.Length];
            for (int i = 0; i < parameters.Length; ++i)
                parameterTypes[i] = GetMarshalType(parameters[i].GetType());
            return parameterTypes;
        }

        private static System.Type GetMarshalType(System.Type t)
        {
            return t;
        }

    }
}
