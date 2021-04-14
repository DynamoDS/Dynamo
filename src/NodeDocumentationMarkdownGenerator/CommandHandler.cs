using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Dynamo.Applications;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Dynamo.PackageManager;
using NodeDocumentationMarkdownGenerator.Commands;
using NodeDocumentationMarkdownGenerator.Verbs;

namespace NodeDocumentationMarkdownGenerator
{
    internal class CommandHandler
    {
        //private DynamoModel core;

        internal CommandHandler()
        {
            //core = Dynamo.Applications.StartupUtils.MakeModel(true);
            var list = AppDomain.CurrentDomain.GetAssemblies().OrderByDescending(a => a.FullName).Select(a => a).ToList();
            //list.ForEach(x => Assembly.ReflectionOnlyLoadFrom(GetAssemblyPath(x)));
            var reflection = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies().ToList();

        }



        internal string HandleFromPackage(FromPackageOptions opts)
        {
            var command = new FromPackageFolderCommand();
            return command.HandlePackageDocumentation(opts);
        }

        internal string HandleFromDirectory(FromDirectoryOptions opts)
        {
            return "..";
        }
    }
}