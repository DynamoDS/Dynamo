using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Dynamo.Engine;
using Dynamo.Exceptions;
using Dynamo.Graph.Nodes.ZeroTouch;
using ProtoCore.Namespace;

namespace Dynamo.Graph.Nodes.NodeLoaders
{
    /// <summary>
    ///     Xml Loader for ZeroTouch nodes.
    /// </summary>
    internal class ZeroTouchNodeLoader : INodeLoader<NodeModel>
    {
        private readonly LibraryServices libraryServices;

        public ZeroTouchNodeLoader(LibraryServices libraryServices)
        {
            this.libraryServices = libraryServices;
        }

        public NodeModel CreateNodeFromXml(XmlElement nodeElement, SaveContext context, ElementResolver resolver)
        {
            string assembly = "";
            string function;
            var name = nodeElement.Attributes["nickname"].Value;

            FunctionDescriptor descriptor;

            Trace.Assert(nodeElement.Attributes != null, "nodeElement.Attributes != null");

            if (nodeElement.Attributes["assembly"] == null)
            {
                assembly = DetermineAssemblyName(nodeElement);
                function = name.Replace(".get", ".");
            }
            else
            {
                string xmlSignature = nodeElement.Attributes["function"].Value;

                string hintedSigniture =
                    libraryServices.FunctionSignatureFromFunctionSignatureHint(xmlSignature);

                if (hintedSigniture != null)
                {
                    nodeElement.Attributes["nickname"].Value =
                        libraryServices.NameFromFunctionSignature(xmlSignature);
                    function = hintedSigniture;

                    // if the node needs additional parameters, add them here
                    libraryServices.AddAdditionalAttributesToNode(xmlSignature, nodeElement);
                    libraryServices.AddAdditionalElementsToNode(xmlSignature, nodeElement);
                }
                else
                {
                    function = xmlSignature;
                }

                var xmlAttribute = nodeElement.Attributes["assembly"];
                if (xmlAttribute != null)
                {
                    assembly = Uri.UnescapeDataString(xmlAttribute.Value);
                }
            }

            if (context == SaveContext.File && !string.IsNullOrEmpty(assembly))
            {
                var document = nodeElement.OwnerDocument;
                var docPath = Nodes.Utilities.GetDocumentXmlPath(document);
                assembly = Nodes.Utilities.MakeAbsolutePath(docPath, assembly);

                if (libraryServices.IsLibraryLoaded(assembly))
                {
                    descriptor = libraryServices.GetFunctionDescriptor(assembly, function);
                }
                else
                {
                    // If the desired assembly is not loaded already. Check if it belongs to BuiltInFunctionGroup.
                    if (libraryServices.IsFunctionBuiltIn(assembly, name))
                    {
                        descriptor = libraryServices.GetFunctionDescriptor(function);
                    }
                    else
                    {
                        // If neither of these, Dynamo need to import the library.
                        try
                        {
                            libraryServices.ImportLibrary(assembly);
                            descriptor = libraryServices.GetFunctionDescriptor(assembly, function);
                        }
                        catch (LibraryLoadFailedException)
                        {
                            descriptor = libraryServices.GetFunctionDescriptor(function);
                        }
                    }
                }
            }
            else
            {
                descriptor = libraryServices.GetFunctionDescriptor(function);
            }

            if (null == descriptor)
            {
                var inputcount = DetermineFunctionInputCount(nodeElement);

                return new DummyNode(
                    inputcount,
                    1,
                    name,
                    nodeElement,
                    assembly,
                    DummyNode.Nature.Unresolved);
            }

            DSFunctionBase result;
            if (descriptor.IsVarArg)
            {
                result = new DSVarArgFunction(descriptor);

                var akas = typeof (DSVarArgFunction).GetCustomAttribute<AlsoKnownAsAttribute>().Values;
                if (nodeElement.Name != typeof (DSVarArgFunction).FullName &&
                    akas.All(aka => aka != nodeElement.Name))
                {
                    VariableInputNodeController.SerializeInputCount(
                        nodeElement,
                        descriptor.Parameters.Count());
                }
            }
            else
            {
                result = new DSFunction(descriptor);
            }

            result.Deserialize(nodeElement, context);

            // In case of input parameters mismatch, use default arguments for parameters that have one
            if (!descriptor.MangledName.EndsWith(function))
            {
                string[] oldSignature = function.Split('@');
                string[] inputTypes = oldSignature.Length > 1 ? oldSignature[1].Split(',') : new string[]{};
                int i = 0, j = 0;
                foreach (var param in descriptor.InputParameters)
                {
                    if (i >= inputTypes.Length || param.Item2 != inputTypes[i])
                        result.InPorts[j].UsingDefaultValue = result.InPorts[j].DefaultValue != null;
                    else
                        i++;
                    j++;
                }
                
            }

            return result;
        }
        
        private static int DetermineFunctionInputCount(XmlElement element)
        {
            int additionalPort = 0;

            // "DSVarArgFunction" is a "VariableInputNode", therefore it will 
            // have "inputcount" as one of the attributes. If such attribute 
            // does not exist, throw an ArgumentException.
            if (element.Name.Equals("Dynamo.Graph.Nodes.ZeroTouch.DSVarArgFunction") || 
                element.Name.Equals("Dynamo.Nodes.DSVarArgFunction"))
            {
                var inputCountAttrib = element.Attributes["inputcount"];

                if (inputCountAttrib == null)
                {
                    throw new ArgumentException(string.Format(
                        "Function inputs cannot be determined ({0}).",
                        element.GetAttribute("nickname")));
                }

                return Convert.ToInt32(inputCountAttrib.Value);
            }

            var signature = string.Empty;
            var signatureAttrib = element.Attributes["function"];
            if (signatureAttrib != null)
                signature = signatureAttrib.Value;
            else if (element.ChildNodes.Count > 0)
            {
                // We have an old file format with "FunctionItem" child element.
                var childElement = element.ChildNodes[0] as XmlElement;
                signature = string.Format("{0}@{1}",
                                          childElement.GetAttribute("DisplayName"),
                                          childElement.GetAttribute("Parameters").Replace(';', ','));

                // We need one more port for instance methods/properties.
                switch (childElement.GetAttribute("Type"))
                {
                    case "InstanceMethod":
                    case "InstanceProperty":
                        additionalPort = 1; // For taking the instance itself.
                        break;
                }
            }

            if (string.IsNullOrEmpty(signature))
            {
                const string message = "Function signature cannot be determined.";
                throw new ArgumentException(message);
            }

            int atSignIndex = signature.IndexOf('@');
            if (atSignIndex >= 0) // An '@' sign found, there's param information.
            {
                signature = signature.Substring(atSignIndex + 1); // Skip past '@'.
                var parts = signature.Split(new[] { ',' });
                return (parts.Length) + additionalPort;
            }

            return additionalPort + 1; // At least one.
        }

        private static string DetermineAssemblyName(XmlElement element)
        {
            var assemblyName = string.Empty;
            var assemblyAttrib = element.Attributes["assembly"];
            if (assemblyAttrib != null)
                assemblyName = assemblyAttrib.Value;
            else if (element.ChildNodes.Count > 0)
            {
                // We have an old file format with "FunctionItem" child element.
                var childElement = element.ChildNodes[0] as XmlElement;
                var funcItemAsmAttrib = childElement.Attributes["Assembly"];
                if (funcItemAsmAttrib != null)
                    assemblyName = funcItemAsmAttrib.Value;
            }

            if (string.IsNullOrEmpty(assemblyName))
                return string.Empty;

            try
            {
                return Path.GetFileName(assemblyName); 
            }
            catch (Exception) 
            { 
                return string.Empty; 
            }
        }
    }
}