﻿// Copyright (c) 2010 Joe Moorhouse

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Scripting.Hosting.Shell;
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
        CommandLine commandLine;
        internal volatile bool AutocompletionInProgress = false;

        public PythonConsoleCompletionDataProvider(CommandLine commandLine)//IMemberProvider memberProvider)
        {
            this.commandLine = commandLine;
        }

        /// <summary>
        /// Generates completion data for the specified text. The text should be everything before
        /// the dot character that triggered the completion. The text can contain the command line prompt
        /// '>>>' as this will be ignored.
        /// </summary>
        public ICompletionData[] GenerateCompletionData(string line)
        {
            var items = new List<DynamoCompletionData>(); //DefaultCompletionData

            string name = GetName(line);
            if (!String.IsNullOrEmpty(name))
            {
                System.IO.Stream stream = commandLine.ScriptScope.Engine.Runtime.IO.OutputStream;
                try
                {
                    AutocompletionInProgress = true;
                    // Another possibility:
                    //commandLine.ScriptScope.Engine.Runtime.IO.SetOutput(new System.IO.MemoryStream(), Encoding.UTF8);
                    //object value = commandLine.ScriptScope.Engine.CreateScriptSourceFromString(name, SourceCodeKind.Expression).Execute(commandLine.ScriptScope);
                    //IList<string> members = commandLine.ScriptScope.Engine.Operations.GetMemberNames(value);

                    Type type = TryGetType(name);
                    if (type != null && type.Namespace != "IronPython.Runtime")
                    {
                        PopulateFromCLRType(items, type, name);
                    }
                    else
                    {
                        string dirCommand = "dir(" + name + ")";
                        object value = commandLine.ScriptScope.Engine.CreateScriptSourceFromString(dirCommand, SourceCodeKind.Expression).Execute(commandLine.ScriptScope);
                        AutocompletionInProgress = false;
                        foreach (object member in (value as IronPython.Runtime.List))
                        {
                            items.Add(new DynamoCompletionData((string)member, name, commandLine, false));
                        }
                    }
                }
                catch (ThreadAbortException tae)
                {
                    if (tae.ExceptionState is Microsoft.Scripting.KeyboardInterruptException) Thread.ResetAbort();
                }
                catch
                {
                    // Do nothing.
                }
                commandLine.ScriptScope.Engine.Runtime.IO.SetOutput(stream, Encoding.UTF8);
                AutocompletionInProgress = false;
            }
            return items.ToArray();
        }

        protected Type TryGetType(string name)
        {
            string tryGetType = name + ".GetType()";
            object type = null;
            try
            {
                type = commandLine.ScriptScope.Engine.CreateScriptSourceFromString(tryGetType, SourceCodeKind.Expression).Execute(commandLine.ScriptScope);
            }
            catch (ThreadAbortException tae)
            {
                if (tae.ExceptionState is Microsoft.Scripting.KeyboardInterruptException) Thread.ResetAbort();
            }
            catch
            {
                // Do nothing.
            }
            return type as Type;
        }

        protected void PopulateFromCLRType(List<DynamoCompletionData> items, Type type, string name)
        {
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
                items.Add(new DynamoCompletionData(completion, name, commandLine, true));
            }
        }

        ///// <summary>
        ///// Generates completion data for the specified text. The text should be everything before
        ///// the dot character that triggered the completion. The text can contain the command line prompt
        ///// '>>>' as this will be ignored.
        ///// </summary>
        //public void GenerateDescription(string stub, string item, DescriptionUpdateDelegate updateDescription, bool isInstance)
        //{
        //    System.IO.Stream stream = commandLine.ScriptScope.Engine.Runtime.IO.OutputStream;
        //    string description = "";
        //    if (!String.IsNullOrEmpty(item))
        //    {
        //        try
        //        {
        //            AutocompletionInProgress = true;
        //            // Another possibility:
        //            //commandLine.ScriptScope.Engine.Runtime.IO.SetOutput(new System.IO.MemoryStream(), Encoding.UTF8);
        //            //object value = commandLine.ScriptScope.Engine.CreateScriptSourceFromString(item, SourceCodeKind.Expression).Execute(commandLine.ScriptScope);
        //            //description = commandLine.ScriptScope.Engine.Operations.GetDocumentation(value);
        //            string docCommand = "";
        //            if (isInstance) docCommand = "type(" + stub + ")" + "." + item + ".__doc__";
        //            else docCommand = stub + "." + item + ".__doc__";
        //            object value = commandLine.ScriptScope.Engine.CreateScriptSourceFromString(docCommand, SourceCodeKind.Expression).Execute(commandLine.ScriptScope);
        //            description = (string)value;
        //            AutocompletionInProgress = false;
        //        }
        //        catch (ThreadAbortException tae)
        //        {
        //            if (tae.ExceptionState is Microsoft.Scripting.KeyboardInterruptException) Thread.ResetAbort();
        //            AutocompletionInProgress = false;
        //        }
        //        catch
        //        {
        //            AutocompletionInProgress = false;
        //            // Do nothing.
        //        }
        //        commandLine.ScriptScope.Engine.Runtime.IO.SetOutput(stream, Encoding.UTF8);
        //        updateDescription(description);
        //    }
        //}


        string GetName(string text)
        {
            text = text.Replace("\t", "   ");
            int startIndex = text.LastIndexOf(' ');
            return text.Substring(startIndex + 1).Trim('.');
        }

    }
}