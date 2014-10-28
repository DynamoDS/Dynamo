using Dynamo.Utilities;
using ProtoCore.DSDefinitions;
using ProtoCore.Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamo.DSEngine
{
    public class CodeCompletionServices
    {
        // TODO: Only those DS keywords are exposed currently that are supported in CBN's
        public static string[] KeywordList = {Keyword.Def, 
                                        Keyword.If, Keyword.Elseif, Keyword.Else, 
                                        Keyword.While, Keyword.For, Keyword.In, Keyword.Continue,  
                                        Keyword.Int, Keyword.Double, Keyword.String, Keyword.Bool, Keyword.Char, 
                                        Keyword.Void, Keyword.Null, Keyword.Var, 
                                        Keyword.True, Keyword.False, 
                                        Keyword.Return, Keyword.Static,
                                        Keyword.Associative, Keyword.Imperative};

        public struct CompletionData
        {
            private readonly string text;
            private readonly string stub;
            private readonly string description;
            private readonly double priority;
            private readonly CompletionType type;

            internal CompletionData(string text, string stub, CompletionType type, string description = "", double priority = 0)
            {
                this.text = text;
                this.stub = stub;
                this.description = description;
                this.type = type;
                this.priority = priority;
            }

            public enum CompletionType
            {
                Namespace,
                Method,
                Constructor,
                Class,
                Property,
                Keyword,
            };

            public string Text { get { return text; } }
            public string Stub { get { return stub; } }
            public string Description { get { return description; } }
            public double Priority { get { return priority; } }
            public CompletionType Type { get { return type; } }

            internal static CompletionData ConvertMirrorToCompletionData(StaticMirror mirror, bool useFullyQualifiedName = false)
            {
                MethodMirror method = mirror as MethodMirror;
                if (method != null)
                {
                    string methodName = method.MethodName;
                    string signature = method.ToString();
                    CompletionType type = method.IsConstructor ? CompletionType.Constructor : CompletionType.Method;
                    return new CompletionData(methodName, signature, type);
                }
                PropertyMirror property = mirror as PropertyMirror;
                if (property != null)
                {
                    string propertyName = property.PropertyName;
                    string stub = "";
                    return new CompletionData(propertyName, stub, CompletionType.Property);
                }
                ClassMirror classMirror = mirror as ClassMirror;
                if (classMirror != null)
                {
                    string className;
                    if (useFullyQualifiedName)
                        className = classMirror.ClassName;
                    else
                        className = classMirror.Alias;
                    string signature = "";
                    return new CompletionData(className, signature, CompletionType.Class);
                }
                else
                    throw new ArgumentException("Invalid argument");
            }
        }


        private readonly ProtoCore.Core core = null;

        public CodeCompletionServices(ProtoCore.Core core)
        {
            this.core = core;
        }

        internal IEnumerable<CompletionData> GetCompletionsOnType(string code, string stringToComplete)
        {
            IEnumerable<StaticMirror> members = null;

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

        internal IEnumerable<CompletionData> SearchCompletions(string stringToComplete, Guid guid)
        {
            List<CompletionData> completions = new List<CompletionData>();

            // Add matching DS keywords
            completions.AddRange(KeywordList.Where(x => x.ToLower().Contains(stringToComplete.ToLower())).
                Select(x => new CompletionData(x, "", CompletionData.CompletionType.Keyword)));

            // Add matching Classes
            var groups = StaticMirror.GetClasses(core).
                Where(x => x.Alias.ToLower().Contains(stringToComplete.ToLower())).
                    GroupBy(x => x.Alias);

            // For those class names that have collisions, list their fully qualified names in completion window
            foreach (var group in groups)
            {
                if (group.Count() > 1)
                {
                    completions.AddRange(group.Select(x =>
                    {
                        return CompletionData.ConvertMirrorToCompletionData(x, useFullyQualifiedName: true);
                    }));
                }
                else
                    completions.AddRange(group.Select(x => CompletionData.ConvertMirrorToCompletionData(x)));
            }

            // Add matching builtin methods
            completions.AddRange(StaticMirror.GetBuiltInMethods(core).
                GroupBy(x => x.Name).Select(y => y.First()).
                Where(x => x.MethodName.ToLower().Contains(stringToComplete.ToLower())).
                Select(x => CompletionData.ConvertMirrorToCompletionData(x)));

            return completions;
        }

        internal IEnumerable<CompletionData> GetFunctionSignatures(string code, string functionName, string functionPrefix)
        {
            IEnumerable<MethodMirror> candidates = null;

            // if function is global, search for function in Built-ins
            if (string.IsNullOrEmpty(functionPrefix))
            {
                return StaticMirror.GetOverloadsOnBuiltIns(core, functionName).
                    Select(x => new CompletionData(x.MethodName, x.ToString(), CompletionData.CompletionType.Method));
            }

            // Determine if the function prefix is a class name
            var type = GetClassType(functionPrefix);
            if (type != null)
            {
                candidates = type.GetOverloadsOnType(functionName);
            }
            // If not of class type
            else
            {
                // Check if the function prefix is a declared identifier
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
}
