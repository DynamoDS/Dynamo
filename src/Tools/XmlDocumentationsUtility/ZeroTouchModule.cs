﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Dynamo.Engine;
using ProtoCore;
using ProtoCore.Mirror;
using ProtoCore.Utils;
using ProtoFFI;

namespace NodeDocumentationUtility
{
    class ZeroTouchModule
    {
        private readonly List<ClassMirror> types;

        /// <summary>
        /// Loads zero touch library module from given assembly.
        /// </summary>
        /// <param name="assembly"></param>
        public ZeroTouchModule(string assembly)
        {
            var core = CreateCore();
            if (!CompilerUtils.TryLoadAssemblyIntoCore(core, assembly))
                throw new InvalidOperationException("Failed to load : " + assembly);

            var library = new LibraryMirror(core, assembly, core.ClassTable.ClassNodes);
            types = library.GetClasses();
        }

        /// <summary>
        /// Checks if the given type exists in this module and it is imported
        /// into the engine.
        /// </summary>
        /// <param name="typename">Fully qualified class name</param>
        /// <returns>true if the given type exists</returns>
        public bool TypeExists(string typename)
        {
            return null != GetClass(typename);
        }

        /// <summary>
        /// Checks if the given method exists on the given type, by looking into
        /// functions as well as constructor of the class of given type. This
        /// doesn't care about method overloads.
        /// </summary>
        /// <param name="typename">Fully qualified name of the class</param>
        /// <param name="methodname">Name of the method to look for.</param>
        /// <returns>true if the given method on given type exists.</returns>
        public bool MethodExists(string typename, string methodname)
        {
            var classmirror = GetClass(typename);
            if (null == classmirror)
                return false;

            var constructors = classmirror.GetConstructors();
            var methods = classmirror.GetFunctions();

            Func<MethodMirror, bool> match =
                mirror => string.Compare(mirror.MethodName, methodname, StringComparison.Ordinal) == 0;

            return constructors.Any(match) || methods.Any(match);
        }

        /// <summary>
        /// Checks if the given property exists on the given type.
        /// </summary>
        /// <param name="typename">Fully qualified name of the class</param>
        /// <param name="propertyname">Name of the property to look for</param>
        /// <returns>true if the given property on given type exists.</returns>
        public bool PropertyExists(string typename, string propertyname)
        {
            var classmirror = GetClass(typename);
            if (null == classmirror)
                return false;

            var properties = classmirror.GetProperties();
            return properties.Any(property => string.Compare(property.PropertyName, propertyname, StringComparison.Ordinal) == 0);
        }

        /// <summary>
        /// Gets the given type if exists in this module and is imported
        /// into the engine.
        /// </summary>
        /// <param name="typename">Fully qualified class name</param>
        /// <returns>ClassMirror of the given type if exists, else null</returns>
        internal ClassMirror GetClass(string typename)
        {
            return (from type in types where string.Compare(type.ClassName, typename, StringComparison.Ordinal) == 0 select type).FirstOrDefault();
        }

        /// <summary>
        /// Creates a default core
        /// </summary>
        /// <returns>ProtoCore.Core</returns>
        private static Core CreateCore()
        {
            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
            CLRModuleType.ClearTypes();
            var core = new Core(new Options { RootCustomPropertyFilterPathName = string.Empty });
            core.Compilers.Add(Language.Associative, new ProtoAssociative.Compiler(core));
            core.ParsingMode = ParseMode.AllowNonAssignment;
            return core;
        }
    }
}
