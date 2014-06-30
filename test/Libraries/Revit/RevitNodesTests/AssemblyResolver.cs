﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DSRevitNodesTests
{
    public static class AssemblyResolver
    {
        private static bool resolverSetup;
        public static void Setup()
        {
            if (resolverSetup) return;
            AppDomain.CurrentDomain.AssemblyResolve += ResolveProtoGeometry;
            AppDomain.CurrentDomain.AssemblyResolve += ResolveDynamoUnits;
            AppDomain.CurrentDomain.AssemblyResolve += ResolveProtoInterface;
            AppDomain.CurrentDomain.AssemblyResolve += ResolveDSNodeServices;
            resolverSetup = true;
        }

        private static string GetDynamoRootDirectory()
        {
            var assemPath = Assembly.GetExecutingAssembly().Location;
            var assemDir = new DirectoryInfo(Path.GetDirectoryName(assemPath));
            return assemDir.Parent.FullName;
        }

        private static Assembly ResolveDSNodeServices(object sender, ResolveEventArgs args)
        {
            if (!args.Name.Contains("DSNodeServices")) return null;

            var assemPath = Path.Combine(GetDynamoRootDirectory(), "DSNodeServices.dll");

            return File.Exists(assemPath) ? Assembly.LoadFrom(assemPath) : null;
        }

        private static Assembly ResolveProtoInterface(object sender, ResolveEventArgs args)
        {
            if (!args.Name.Contains("ProtoInterface")) return null;

            var assemPath = Path.Combine(GetDynamoRootDirectory(), "ProtoInterface.dll");

            return File.Exists(assemPath) ? Assembly.LoadFrom(assemPath) : null;
        }

        private static Assembly  ResolveProtoGeometry(object sender, ResolveEventArgs args)
        {
            if (!args.Name.Contains("ProtoGeometry")) return null;

            var assemPath = Path.Combine(GetDynamoRootDirectory(), "ProtoGeometry.dll");

            return File.Exists(assemPath) ? Assembly.LoadFrom(assemPath) : null;
        }

        private static Assembly ResolveDynamoUnits(object sender, ResolveEventArgs args)
        {
            if (!args.Name.Contains("DynamoUnits")) return null;

            var assemPath = Path.Combine(GetDynamoRootDirectory(), "DynamoUnits.dll");

            return File.Exists(assemPath) ? Assembly.LoadFrom(assemPath) : null;
        }
    }
    
}
 