using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;
using Dynamo.Interfaces;
using DynamoUtilities;

namespace Dynamo.DSEngine
{
    public static class DocumentationServices
    {
        private static Dictionary<string, bool> _triedPaths = new Dictionary<string, bool>();

        private static Dictionary<string, XmlReader> _cached =
            new Dictionary<string, XmlReader>(StringComparer.OrdinalIgnoreCase);

        private static Dictionary<string, MemberDocumentNode> documentNodes =
            new Dictionary<string, MemberDocumentNode>();

        public static void DestroyCachedData()
        {
            if (_triedPaths != null) // Release references for collection.
                _triedPaths.Clear();

            if (_cached != null) // Release references for collection.
                _cached.Clear();
        }

        public static string GetSummary(this FunctionDescriptor member)
        {
            var assemblyName = member.Assembly;
            var fullyQualifiedName = GetMemberElementName(member);

            if (documentNodes.ContainsKey(fullyQualifiedName))
                return documentNodes[fullyQualifiedName].Summary;
            else
                LoadDataFromXml(member);

            return String.Empty;
        }

        private static void LoadDataFromXml(FunctionDescriptor member)
        {
            if (member.Assembly != null)
                GetForAssembly(member.Assembly, member.PathManager);
        }

        private static XmlReader GetForAssembly(string assemblyPath, IPathManager pathManager)
        {
            if (_triedPaths.ContainsKey(assemblyPath))
            {
                return _triedPaths[assemblyPath] ? _cached[assemblyPath] : null;
            }

            var documentationPath = "";
            if (ResolveForAssembly(assemblyPath, pathManager, ref documentationPath))
            {
                var c = XmlReader.Create(documentationPath);
                _triedPaths.Add(assemblyPath, true);
                _cached.Add(assemblyPath, c);
                return c;
            }

            _triedPaths.Add(assemblyPath, false);
            return null;
        }

        private static bool ResolveForAssembly(string assemblyLocation,
            IPathManager pathManager, ref string documentationPath)
        {
            if (pathManager != null)
                pathManager.ResolveLibraryPath(ref assemblyLocation);

            if (!File.Exists(assemblyLocation))
            {
                return false;
            }

            var assemblyPath = Path.GetFullPath(assemblyLocation);

            var baseDir = Path.GetDirectoryName(assemblyPath);
            var xmlFileName = Path.GetFileNameWithoutExtension(assemblyPath) + ".xml";

            var language = System.Threading.Thread.CurrentThread.CurrentUICulture.ToString();
            var localizedResPath = Path.Combine(baseDir, language);
            documentationPath = Path.Combine(localizedResPath, xmlFileName);

            if (File.Exists(documentationPath))
                return true;

            localizedResPath = Path.Combine(baseDir, UI.Configurations.FallbackUiCulture);
            documentationPath = Path.Combine(localizedResPath, xmlFileName);
            if (File.Exists(documentationPath))
                return true;

            documentationPath = Path.Combine(baseDir, xmlFileName);
            return File.Exists(documentationPath);
        }

        private static void CreateMemberDocumentNode(string assembly, string fullyQualifiedName)
        {
            var memberDocNode = new MemberDocumentNode(assembly, fullyQualifiedName);
            documentNodes.Add(memberDocNode.FullyQualifiedName, memberDocNode);
        }

        #region Fully qualified name creation

        private static string PrimitiveMap(string s)
        {
            switch (s)
            {
                case "[]":
                    return "System.Collections.IList";
                case "var[]..[]":
                    return "System.Collections.IList";
                case "var":
                    return "System.Object";
                case "double":
                    return "System.Double";
                case "int":
                    return "System.Int32";
                case "bool":
                    return "System.Boolean";
                case "string":
                    return "System.String";
                default:
                    return s;
            }
        }

        private static string GetMemberElementName(FunctionDescriptor member)
        {
            char prefixCode;

            string memberName = member.ClassName + "." + member.FunctionName;

            switch (member.Type)
            {
                case FunctionType.Constructor:
                    // XML documentation uses slightly different constructor names
                    memberName = memberName.Replace(".ctor", "#ctor");
                    goto case FunctionType.InstanceMethod;

                case FunctionType.InstanceMethod:
                    prefixCode = 'M';

                    // parameters are listed according to their type, not their name
                    string paramTypesList = String.Join(
                        ",",
                        member.Parameters.Select(x => x.Type.ToShortString()).Select(PrimitiveMap).ToArray()
                        );

                    if (!String.IsNullOrEmpty(paramTypesList)) memberName += "(" + paramTypesList + ")";
                    break;

                case FunctionType.StaticMethod:
                    goto case FunctionType.InstanceMethod;
                    break;

                case FunctionType.InstanceProperty:
                    prefixCode = 'P';
                    break;

                case FunctionType.StaticProperty:
                    goto case FunctionType.InstanceProperty;
                    break;

                case FunctionType.GenericFunction:
                    return String.Empty;

                default:
                    throw new ArgumentException("Unknown member type", "member");
            }

            // elements are of the form "M:Namespace.Class.Method"
            return String.Format("{0}:{1}", prefixCode, memberName);
        }

        #endregion
    }

}
