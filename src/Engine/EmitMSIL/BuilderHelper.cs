using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;

namespace EmitMSIL
{
    public static class BuilderHelper
    {
        public static DynamicMethod CreateDynamicMethod(string methodName, Type returnType, Type[] parameterTypes)
        {
            return new DynamicMethod(methodName, returnType, parameterTypes);
        }

        public static AssemblyBuilder CreateAssemblyBuilder(String assemblyName, bool bDebug,AssemblyBuilderAccess access)
        {
            AssemblyName aname = new AssemblyName(assemblyName);
            AppDomain currentDomain = AppDomain.CurrentDomain; // Thread.GetDomain();

            //Type daType = typeof(DebuggableAttribute);
            //ConstructorInfo daCtor = daType.GetConstructor(new Type[] { typeof(DebuggableAttribute.DebuggingModes) });

            //CustomAttributeBuilder daBuilder = new CustomAttributeBuilder(daCtor, new object[] {
            //    DebuggableAttribute.DebuggingModes.DisableOptimizations |
            //    DebuggableAttribute.DebuggingModes.Default }
            //);

            string assemblyPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

#if NET6_0_OR_GREATER
            AssemblyBuilder builder = AssemblyBuilder.DefineDynamicAssembly(aname, AssemblyBuilderAccess.RunAndCollect);
#else
            //AssemblyBuilder builder = currentDomain.DefineDynamicAssembly(aname, bDebug ? 
            //    AssemblyBuilderAccess.RunAndSave : AssemblyBuilderAccess.RunAndCollect, assemblyPath); // CPHCORE
            AssemblyBuilder builder = currentDomain.DefineDynamicAssembly(aname,
                access, assemblyPath);
#endif

            //if (bDebug)
            //    builder.SetCustomAttribute(daBuilder);

            return builder;
        }



        public static ModuleBuilder CreateEXEModuleBuilder(AssemblyBuilder asmBuilder, String moduleName)
        {

#if NET6_0_OR_GREATER
            ModuleBuilder builder = asmBuilder.DefineDynamicModule(moduleName);
#else
            ModuleBuilder builder = asmBuilder.DefineDynamicModule(moduleName, moduleName + ".exe", true); // CPHCORE
#endif
            return builder;
        }


        public static ModuleBuilder CreateDLLModuleBuilder(AssemblyBuilder asmBuilder, String moduleName)
        {
#if NET6_0_OR_GREATER
            ModuleBuilder builder = asmBuilder.DefineDynamicModule(moduleName);
#else
            ModuleBuilder builder = asmBuilder.DefineDynamicModule(moduleName, moduleName + ".dll", true); 
#endif
            return builder;
        }

        public static TypeBuilder CreateType(ModuleBuilder modBuilder, String className)
        {
            TypeBuilder builder = modBuilder.DefineType(className,
                TypeAttributes.Public |
                TypeAttributes.Class |
                TypeAttributes.SequentialLayout | // Sequential layout
                TypeAttributes.AnsiClass,
                typeof(object), // TODO - parent!!!
                PackingSize.Size1
            );
            return builder;
        }

        public static EnumBuilder CreateEnum(ModuleBuilder modBuilder, String enumName)
        {
            // TODO - attributes...
            EnumBuilder builder = modBuilder.DefineEnum(enumName, TypeAttributes.Public, typeof(int));
            return builder;
        }

        public static TypeBuilder CreateType(ModuleBuilder modBuilder, String className, string[] genericparameters)
        {
            // TODO - attributes...
            TypeBuilder builder = modBuilder.DefineType(className, TypeAttributes.Public | TypeAttributes.Class);

            GenericTypeParameterBuilder[] genBuilders = builder.DefineGenericParameters(genericparameters);

            foreach (GenericTypeParameterBuilder genBuilder in genBuilders)
            // We take each generic type T : class, new()
            {
                genBuilder.SetGenericParameterAttributes(
                    GenericParameterAttributes.ReferenceTypeConstraint
                    | GenericParameterAttributes.DefaultConstructorConstraint
                );

                //genBuilder.SetInterfaceConstraints(interfaces);
            }

            return builder;
        }


        public static MethodBuilder CreateMethod(TypeBuilder typBuilder, String methodName)
        {
            MethodBuilder builder = typBuilder.DefineMethod(methodName, MethodAttributes.Public | MethodAttributes.HideBySig);
            return builder;
        }


        public static MethodBuilder CreateMethod(TypeBuilder typBuilder, String methodName, MethodAttributes attributes, 
            Type returnType, Type[] parameterTypes)
        {
            attributes = attributes | MethodAttributes.Public | MethodAttributes.HideBySig;

            MethodBuilder builder = typBuilder.DefineMethod(
                methodName,
                attributes,
                CallingConventions.Standard,  // TODO  calling conventions here!!  Standard = static...HasThis = virtual / instance ,etc.
                returnType,
                parameterTypes
            );

            // TODO - We need a parameter which indicates that we want to mark the array as unmanaged...once this works, that is
            // The whole thing seems broken - the attribute doesn't seem to get applied correctly...

            //for (int i = 0; i < parameterTypes.Length; i++)
            //{
            //    if (parameterTypes[i].IsArray)
            //    {
            //        ParameterBuilder pb = builder.DefineParameter(i + 1, ParameterAttributes.HasFieldMarshal, "_array");

            //        // Get the MarshalAsAttribute constructor that takes an argument of type UnmanagedType.
            //        ConstructorInfo ci = typeof(MarshalAsAttribute).GetConstructor(new Type[] { typeof(UnmanagedType) });

            //        // We need the field information of "SizeConst" of the MarshalAs attribute 
            //        FieldInfo[] sizeConstField = new FieldInfo[] { typeof(MarshalAsAttribute).GetField("SizeConst"), typeof(MarshalAsAttribute).GetField("ArraySubType") };
            //        object[] sizeConstValue = new object[] { 5, UnmanagedType.R8 };

            //        CustomAttributeBuilder cabuilder =
            //            new CustomAttributeBuilder(ci, new object[] { UnmanagedType.LPArray }, sizeConstField, sizeConstValue);

            //        // Apply the attribute to the parameter.
            //        //
            //        pb.SetCustomAttribute(cabuilder);
            //    }
            //}

#if NET6_0_OR_GREATER
#else
            if (methodName.ToLower() == "main") // CPHCORE - and this is crap anyway
                (builder.DeclaringType.Assembly as AssemblyBuilder).SetEntryPoint(builder);
#endif

            return builder;
        }

        public static MethodBuilder CreateMethod(TypeBuilder typBuilder, String methodName, Type returnType, String[] genericParameters, Type[] parameterTypes)
        {
            MethodBuilder builder = typBuilder.DefineMethod(
                methodName,
                MethodAttributes.Public | MethodAttributes.HideBySig,
                CallingConventions.HasThis,
                returnType,
                parameterTypes

            );

            GenericTypeParameterBuilder[] genBuilders = builder.DefineGenericParameters(genericParameters);

            foreach (GenericTypeParameterBuilder genBuilder in genBuilders) // We take each generic type T : class, new()
            {
                //genBuilder.SetInterfaceConstraints(interfaces);

                genBuilder.SetGenericParameterAttributes(
                    GenericParameterAttributes.ReferenceTypeConstraint |
                    GenericParameterAttributes.DefaultConstructorConstraint
                );
            }
            return builder;
        }
    }
}
