using System;
using System.Collections.Generic;
using Dynamo;
using ICSharpCode.AvalonEdit.CodeCompletion;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting;
using System.Threading;
using System.Reflection;

namespace DynamoPython
{
    /// <summary>
    /// Provides code completion for the Python Console window.
    /// </summary>
    public class PythonConsoleCompletionDataProvider
    {
        private ScriptEngine engine;
        private ScriptScope scope;

        internal volatile bool AutocompletionInProgress = false;

        public PythonConsoleCompletionDataProvider()
        {
            engine = Python.CreateEngine();
            scope = engine.CreateScope();

            var defaultImports =
                "import clr\nimport System\n";

            scope.Engine.CreateScriptSourceFromString(defaultImports, SourceCodeKind.Statements).Execute(scope);

            try
            {
                var revitImports =
                    "clr.AddReference('RevitAPI')\nclr.AddReference('RevitAPIUI')\nfrom Autodesk.Revit.DB import *\nimport Autodesk\n";

                scope.Engine.CreateScriptSourceFromString(defaultImports, SourceCodeKind.Statements).Execute(scope);
            }
            catch
            {
                DynamoLogger.Instance.Log("Failed to load Revit types for Autocomplete.  Autocomplete will not see Autodesk namespace types.");
            }
        }

        /// <summary>
        /// Generates completion data for the specified text. 
        /// </summary>
        public ICompletionData[] GenerateCompletionData(string line)
        {
            var items = new List<DynamoCompletionData>();

            string name = GetName(line);
            if (!String.IsNullOrEmpty(name))
            {
                try
                {
                    AutocompletionInProgress = true;

                    Type type = TryGetType(name);
                    if (type != null && type.Namespace != "IronPython.Runtime")
                    {
                        PopulateFromCLRType(items, type, name);
                    }
                    else
                    {
                        string dirCommand = "dir(" + name + ")";
                        object value = scope.Engine.CreateScriptSourceFromString(dirCommand, SourceCodeKind.Expression).Execute(scope);

                        foreach (object member in (value as IronPython.Runtime.List))
                        {
                            items.Add(new DynamoCompletionData((string)member, name, false));
                        }
                    }
                }
                catch
                {
                    // Do nothing.
                }
                AutocompletionInProgress = false;
            }
            return items.ToArray();
        }

        /// <summary>
        ///     Get a type from a name.  For example: System.Collections or System.Collections.ArrayList
        /// </summary>
        /// <param name="name">The name</param>
        /// <returns>The type or null if its not a valid type</returns>
        protected Type TryGetType(string name)
        {
            string tryGetType = name + ".GetType()";
            dynamic type = null;
            try
            {
                type = scope.Engine.CreateScriptSourceFromString(tryGetType, SourceCodeKind.Expression).Execute(scope);
            }
            catch (ThreadAbortException tae)
            {
                if (tae.ExceptionState is Microsoft.Scripting.KeyboardInterruptException) Thread.ResetAbort();
            }
            catch
            {
                Console.WriteLine();
            }
            return type as Type;
        }


        protected void PopulateFromCLRType(List<DynamoCompletionData> items, Type type, string name)
        {
            DynamoLogger.Instance.Log(type.ToString());
            DynamoLogger.Instance.Log(name);

            List<string> completionsList = new List<string>();
            MethodInfo[] methodInfo = type.GetMethods();
            PropertyInfo[] propertyInfo = type.GetProperties();
            FieldInfo[] fieldInfo = type.GetFields();
            foreach (MethodInfo methodInfoItem in methodInfo)
            {
                if ((methodInfoItem.IsPublic)
                    && (methodInfoItem.Name.IndexOf("get_") != 0) && (methodInfoItem.Name.IndexOf("set_") != 0)
                    && (methodInfoItem.Name.IndexOf("add_") != 0) && (methodInfoItem.Name.IndexOf("remove_") != 0)
                    && (methodInfoItem.Name.IndexOf("__") != 0))
                    completionsList.Add(methodInfoItem.Name);
            }
            foreach (PropertyInfo propertyInfoItem in propertyInfo)
            {
                completionsList.Add(propertyInfoItem.Name);
            }
            foreach (FieldInfo fieldInfoItem in fieldInfo)
            {
                completionsList.Add(fieldInfoItem.Name);
            }
            completionsList.Sort();
            string last = "";
            for (int i = completionsList.Count - 1; i > 0; --i)
            {
                if (completionsList[i] == last) completionsList.RemoveAt(i);
                else last = completionsList[i];
            }
            foreach (string completion in completionsList)
            {
                items.Add(new DynamoCompletionData(completion, name, true));
            }
        }

        string GetName(string text)
        {
            text = text.Replace("\t", "   ");
            text = text.Replace("\n", " ");
            text = text.Replace("\r", " ");
            int startIndex = text.LastIndexOf(' ');
            return text.Substring(startIndex + 1).Trim('.');
        }

        // TODO: traverse source and process import statements
        // TODO: variable autocompletion?  we want to get types without executing!

    }
}