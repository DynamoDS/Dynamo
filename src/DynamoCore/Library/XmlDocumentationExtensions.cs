using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Autodesk.DesignScript.Geometry;
using System.Windows.Media.Imaging;
using DynamoUtilities;
using System.Resources;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;

namespace Dynamo.DSEngine
{
    /// <summary>
    /// Provides extension methods for reading XML documentation from reflected members.
    /// </summary>
    public static class XmlDocumentationExtensions
    {
        private static Dictionary<string, BitmapImage> _cachedIcons =
            new Dictionary<string, BitmapImage>(StringComparer.OrdinalIgnoreCase);

        #region Public methods

        public static string GetSummary(this FunctionDescriptor member)
        {
            XDocument xml = null;

            if (member.Assembly != null)
                xml = DocumentationServices.GetForAssembly(member.Assembly);

            return member.GetSummary(xml);
        }
        public static BitmapImage GetIconSmall(this FunctionDescriptor member)
        {
            XDocument xml = null;
            string iconSmall = string.Empty;
            if (member.Assembly != null)
            {
                try
                {
                    xml = DocumentationServices.GetForAssembly(member.Assembly);
                    iconSmall = member.GetIconSmall(xml);
                    if (_cachedIcons.ContainsKey(iconSmall))
                        return _cachedIcons[iconSmall];
                    string documentationPath = string.Empty;
                    if (ResolveForAssembly(member.Assembly, ref documentationPath))
                    {
                        Assembly resourcesAssembly = Assembly.LoadFrom(documentationPath);
                        System.IO.Stream stream =
                            resourcesAssembly.GetManifestResourceStream
                            (resourcesAssembly.GetManifestResourceNames()[0]);

                        ResourceReader resReader = new ResourceReader(stream);
                        Dictionary<string, object> data = resReader
                            .OfType<DictionaryEntry>()
                            .Select(i => new { Key = i.Key.ToString(), value = i.Value })
                            .ToDictionary(i => i.Key, i => i.value);

                        foreach (var item in data)
                        {
                            MemoryStream memory = new MemoryStream();
                            var bitmap = item.Value as Bitmap;
                            bitmap.Save(memory, ImageFormat.Png);
                            memory.Position = 0;
                            BitmapImage bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();
                            bitmapImage.StreamSource = memory;
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.EndInit();
                            _cachedIcons.Add(item.Key, bitmapImage);
                        }
                        return _cachedIcons[iconSmall];
                    }
                }
                catch
                {
                    return null;
                }
            }
            
            return null;
        }

        public static bool ResolveForAssembly(string assemblyLocation, ref string documentationPath)
        {
            DynamoPathManager.Instance.ResolveLibraryPath(ref assemblyLocation);
            if (!File.Exists(assemblyLocation))
            {
                return false;
            }

            var qualifiedPath = Path.GetFullPath(assemblyLocation);
            var fn = Path.GetFileNameWithoutExtension(qualifiedPath);
            var dir = Path.GetDirectoryName(qualifiedPath);

            fn = fn + ".resources.dll";

            documentationPath = Path.Combine(dir, fn);

            return File.Exists(documentationPath);
        }
        public static string GetDescription(this TypedParameter member)
        {
            XDocument xml = null;

            if (member.Function != null && member.Function.Assembly != null)
                xml = DocumentationServices.GetForAssembly(member.Function.Assembly);

            return member.GetDescription(xml);
        }

        public static IEnumerable<string> GetSearchTags(this FunctionDescriptor member)
        {
            XDocument xml = null;

            if (member.Assembly != null)
                xml = DocumentationServices.GetForAssembly(member.Assembly);

            return member.GetSearchTags(xml);
        }

        #endregion

        #region Overloads specifying XDocument

        public static string GetDescription(this TypedParameter parameter, XDocument xml)
        {
            if (xml == null) return String.Empty;

            return GetMemberElement(parameter.Function, 
                String.Format("param[@name='{0}']", parameter.Name), xml).CleanUpDocString();
        }

        public static string GetSummary(this FunctionDescriptor member, XDocument xml)
        {
            if (xml == null) return String.Empty;

            return GetMemberElement(member, "summary", xml).CleanUpDocString();
        }

        public static string GetIconSmall(this FunctionDescriptor member, XDocument xml)
        {
            if (xml == null) return String.Empty;

            return GetMemberElement(member, "iconSmall", xml).CleanUpDocString();
        }

        public static IEnumerable<string> GetSearchTags(this FunctionDescriptor member, XDocument xml)
        {
            if (xml == null) return new List<string>();

            return GetMemberElement(member, "search", xml)
                .CleanUpDocString()
                .Split(',')
                .Select(x => x.Trim())
                .Where(x => x != String.Empty);
        }

        #endregion

        #region Helpers

        private static string CleanUpDocString(this string text)
        {
            if (String.IsNullOrEmpty(text)) return String.Empty;

            // trim, clean up new lines, and clean out excessive internal spaces
            var sb = new StringBuilder();
            var lastChar = ' ';

            foreach (var currentChar in text.Trim())
            {
                if (currentChar == ' ' && lastChar == ' ') continue;
                if (currentChar == '\n') continue;
                sb.Append(currentChar);
                lastChar = currentChar;
            }

            return sb.ToString();
        }

        private static string GetMemberElement(FunctionDescriptor function,
            string suffix, XDocument xml)
        {
            // Construct the entire function descriptor name, including CLR style names
            string clrMemberName = GetMemberElementName(function);

            // match clr member name
            var match = xml.XPathEvaluate(
                String.Format("string(/doc/members/member[@name='{0}']/{1})", clrMemberName, suffix));

            if (match is String && !string.IsNullOrEmpty((string)match))
            {
                return match as string;
            }

            // fallback, namespace qualified method name
            var methodName = function.QualifiedName;

            // match with fallback
            match = xml.XPathEvaluate(
                String.Format(
                    "string(/doc/members/member[contains(@name,'{0}')]/{1})", methodName, suffix));

            if (match is String && !string.IsNullOrEmpty((string)match))
            {
                return match as string;
            }

            return String.Empty;
        }

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

            string memberName = member.ClassName + "." + member.Name;

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
                        member.Parameters.Select(x => x.Type).Select(PrimitiveMap).ToArray()
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

                default:
                    throw new ArgumentException("Unknown member type", "member");
            }

            // elements are of the form "M:Namespace.Class.Method"
            return String.Format("{0}:{1}", prefixCode, memberName);
        }

        #endregion
    }

}