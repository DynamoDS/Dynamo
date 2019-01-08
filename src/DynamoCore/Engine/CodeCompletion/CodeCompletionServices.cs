using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dynamo.Graph.Nodes;
using Dynamo.Utilities;
using ProtoCore.DSDefinitions;
using ProtoCore.Mirror;
using ProtoCore.Namespace;
using Symbol = ProtoCore.Namespace.Symbol;

namespace Dynamo.Engine.CodeCompletion
{
    /// <summary>
    /// Interacts with the VM core to provide code completion data to the UI
    /// </summary>
    internal class CodeCompletionServices
    {
        private readonly ProtoCore.Core core = null;

        // TODO: Only those DS keywords are exposed currently that are supported in CBN's
        public static string[] KeywordList = {Keyword.Def, 
                                        Keyword.If, Keyword.Elseif, Keyword.Else, 
                                        Keyword.While, Keyword.For, Keyword.In, Keyword.Continue,  
                                        Keyword.True, Keyword.False, 
                                        Keyword.Static,
                                        Keyword.Associative, Keyword.Imperative};


        public CodeCompletionServices(ProtoCore.Core core)
        {
            this.core = core;
        }

        /// <summary>
        /// Determines if the completion string is a valid type and 
        /// enumerates the list of completion members on the type
        /// </summary>
        /// <param name="code"> code typed in the code block </param>
        /// <param name="stringToComplete"> Class name or declared variable </param>
        /// <param name="resolver"></param>
        /// <returns> list of method and property members of the type </returns>
        internal IEnumerable<CompletionData> GetCompletionsOnType(string code, string stringToComplete, ElementResolver resolver = null)
        {
            IEnumerable<StaticMirror> members = null;

            if (resolver != null)
            {
                stringToComplete = resolver.LookupResolvedName(stringToComplete) ?? stringToComplete;
            }

            // Determine if the string to be completed is a class
            var type = GetClassType(stringToComplete);
            if (type != null)
            {
                members = type.GetMembers();
            }
            // If not of class type
            else
            {
                // Check if the string to be completed is a declared variable
                string typeName = CodeCompletionParser.GetVariableType(code, stringToComplete);
                if (typeName != null)
                    type = GetClassType(typeName);

                if (type != null)
                {
                    members = type.GetInstanceMembers();
                }
            }

            return members.Select(x => CompletionData.ConvertMirrorToCompletionData(x));
        }

        /// <summary>
        /// Returns the list of names of classes loaded in the Core
        /// </summary>
        /// <returns> list of class names </returns>
        internal IEnumerable<string> GetClasses()
        {
            return StaticMirror.GetClasses(core).Select(x => x.Alias);
        }

        /// <summary>
        /// Returns the list of names of global methods and properties in Core
        /// </summary>
        /// <returns> list of names of global methods and properties </returns>
        internal IEnumerable<string> GetGlobals()
        {
            return StaticMirror.GetGlobals(core).Select(x => x.Name);
        }

        /// <summary>
        /// Matches the completion string with classes and
        /// global methods and properties loaded in the session
        /// </summary>
        /// <param name="stringToComplete"> current string being typed which is to be completed </param>
        /// <param name="guid"> code block node guid to identify current node being typed </param>
        /// <param name="resolver"></param>
        /// <returns> list of classes, global methods and properties that match with string being completed </returns>
        internal IEnumerable<CompletionData> SearchCompletions(string stringToComplete, Guid guid, ElementResolver resolver = null)
        {
            var completions = new List<CompletionData>();
            
            // Add matching DS keywords
            completions.AddRange(KeywordList.Where(x => x.ToLower().Contains(stringToComplete.ToLower())).
                Select(x => new CompletionData(x, CompletionData.CompletionType.Keyword)));

            AddTypesToCompletionData(stringToComplete, completions, resolver);

            // Add matching builtin methods
            completions.AddRange(StaticMirror.GetBuiltInMethods(core).
                GroupBy(x => x.Name).Select(y => y.First()).
                Where(x => x.MethodName.ToLower().Contains(stringToComplete.ToLower())).
                Select(x => CompletionData.ConvertMirrorToCompletionData(x)));

            return completions;
        }

        /// <summary>
        ///  Matches the completion string with classes, including primitive types.
        /// </summary>
        /// <param name="stringToComplete"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        internal IEnumerable<CompletionData> SearchTypes(string stringToComplete, ElementResolver resolver = null)
        {
            var completions = new List<CompletionData>();
            
            AddTypesToCompletionData(stringToComplete, completions, resolver);

            return completions;
        }

        /// <summary>
        /// Returns the list of function signatures of all overloads of a given method
        /// </summary>
        /// <param name="code"> code being typed in code block </param>
        /// <param name="functionName"> given method name for which signature is queried </param>
        /// <param name="functionPrefix"> class name in case of constructor or static method, OR
        /// declared instance variable on which method is invoked </param>
        /// <param name="resolver"></param>
        /// <returns> list of method overload signatures </returns>
        internal IEnumerable<CompletionData> GetFunctionSignatures(string code, string functionName, string functionPrefix,
            ElementResolver resolver = null)
        {
            IEnumerable<MethodMirror> candidates = null;

            // if function is global, search for function in Built-ins
            if (string.IsNullOrEmpty(functionPrefix))
            {
                return StaticMirror.GetOverloadsOnBuiltIns(core, functionName).
                    Select(x =>
                    {
                        return new CompletionData(x.MethodName, CompletionData.CompletionType.Method)
                        {
                            Stub = x.ToString()
                        };
                    });
            }

            // Determine if the function prefix is a class name
            if (resolver != null)
            {
                functionPrefix = resolver.LookupResolvedName(functionPrefix) ?? functionPrefix;
            }

            var type = GetClassType(functionPrefix);
            if (type != null)
            {
                candidates = type.GetOverloadsOnType(functionName);
            }
            // If not of class type
            else
            {
                // Check if the function prefix is a typed identifier
                string typeName = CodeCompletionParser.GetVariableType(code, functionPrefix);
                if (typeName != null)
                    type = GetClassType(typeName);

                if (type != null)
                {
                    candidates = type.GetOverloadsOnInstance(functionName);
                }
            }
            return candidates.Select(x => CompletionData.ConvertMirrorToCompletionData(x));
        }

        private void AddTypesToCompletionData(string stringToComplete, List<CompletionData> completions, ElementResolver resolver)
        {
            var partialName = stringToComplete.ToLower();
            // Add matching Classes
            var classMirrorGroups = StaticMirror.GetAllTypes(core).
                Where(x => !x.IsHiddenInLibrary && x.Alias.ToLower().Contains(partialName)).
                    GroupBy(x => x.Alias);

            foreach (var classMirrorGroup in classMirrorGroups)
            {
                // For those class names that have collisions, use shorter names
                bool useShorterName = classMirrorGroup.Count() > 1;
                if (useShorterName)
                {
                    // For colliding namespaces, compute shortest unique names for each
                    var namespaces = classMirrorGroup.Select(x => new Symbol(x.ClassName)).ToList();
                    var shortNames = Symbol.GetShortestUniqueNames(namespaces);
                    Debug.Assert(shortNames.Count() == classMirrorGroup.Count());

                    // Update class mirror (group) Alias with short name computed above
                    for (int i = 0; i < classMirrorGroup.Count(); i++)
                    {
                        var cm = classMirrorGroup.ElementAt(i);
                        cm.Alias = shortNames.ElementAt(i).Value;
                    }
                }
                completions.AddRange(classMirrorGroup.
                    Where(x => !x.IsEmpty).
                        Select(x =>
                                CompletionData.ConvertMirrorToCompletionData(x, useShorterName,
                                    resolver: resolver)));
            }
        }

        private ClassMirror GetClassType(string className)
        {
            try
            {
                return new ClassMirror(className, core);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Code completion data that typically gets displayed on a list as part of 
    /// the auto-completion feature. This class represents a common currency that
    /// is view-independent. 
    /// </summary>
    internal class CompletionData
    {
        public enum CompletionType
        {
            Namespace,
            Method,
            Constructor,
            Class,
            Property,
            Keyword,
        };

        /// <summary>
        /// Displayed text in completion list
        /// Class name or fully qualified name
        /// Method, property name or keyword
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Method signatures or any stub description for classes etc.
        /// </summary>
        public string Stub { get; set; }

        /// <summary>
        /// Description of completion item - class, method or property
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Type of completion item
        /// </summary>
        public CompletionType Type { get; private set; }

        internal CompletionData(string text, CompletionType type)
        {
            this.Text = text;
            this.Type = type;
        }


        internal static CompletionData ConvertMirrorToCompletionData(StaticMirror mirror, bool useShorterName = false, 
            ElementResolver resolver = null)
        {
            var method = mirror as MethodMirror;
            if (method != null)
            {
                string methodName = method.MethodName;
                string signature = method.ToString();
                CompletionType type = method.IsConstructor ? CompletionType.Constructor : CompletionType.Method;
                return new CompletionData(methodName, type)
                {
                    Stub = signature
                };
            }
            var property = mirror as PropertyMirror;
            if (property != null)
            {
                string propertyName = property.PropertyName;
                return new CompletionData(propertyName, CompletionType.Property);
            }
            var classMirror = mirror as ClassMirror;
            if (classMirror != null)
            {
                string className = useShorterName ? GetShortClassName(classMirror, resolver) : classMirror.Alias;
                return new CompletionData(className, CompletionType.Class);
            }
            else
                throw new ArgumentException("Invalid argument");
        }

        private static string GetShortClassName(ClassMirror mirror, ElementResolver resolver)
        {
            var shortName = string.Empty;
            if (resolver != null)
            {
                shortName = resolver.LookupShortName(mirror.ClassName);
            }

            return string.IsNullOrEmpty(shortName) ? mirror.Alias : shortName;
        }
    }
}
