using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using DSCoreNodesUI;
using Dynamo.DSEngine;
using Dynamo.Nodes;

namespace Dynamo.Models.NodeLoaders
{
    /// <summary>
    ///     Xml Loader for ZeroTouch nodes.
    /// </summary>
    public class ZeroTouchNodeLoader : INodeLoader<NodeModel>
    {
        private readonly LibraryServices libraryServices;

        public ZeroTouchNodeLoader(LibraryServices libraryServices)
        {
            this.libraryServices = libraryServices;
        }

        public NodeModel CreateNodeFromXml(XmlElement nodeElement, SaveContext context)
        {
            string assembly = "";
            string function;
            var nickname = nodeElement.Attributes[/*NXLT*/"nickname"].Value;

            FunctionDescriptor descriptor;

            Trace.Assert(nodeElement.Attributes != null, /*NXLT*/"nodeElement.Attributes != null");

            if (nodeElement.Attributes[/*NXLT*/"assembly"] == null)
            {
                assembly = DetermineAssemblyName(nodeElement);
                function = nickname.Replace(/*NXLT*/".get", /*NXLT*/".");
            }
            else
            {
                var xmlAttribute = nodeElement.Attributes[/*NXLT*/"assembly"];
                if (xmlAttribute != null)
                    assembly = xmlAttribute.Value;

                string xmlSignature = nodeElement.Attributes[/*NXLT*/"function"].Value;

                string hintedSigniture =
                    libraryServices.FunctionSignatureFromFunctionSignatureHint(xmlSignature);

                function = hintedSigniture ?? xmlSignature;
            }

            if (context == SaveContext.File && !string.IsNullOrEmpty(assembly))
            {
                var document = nodeElement.OwnerDocument;
                var docPath = Nodes.Utilities.GetDocumentXmlPath(document);
                assembly = Nodes.Utilities.MakeAbsolutePath(docPath, assembly);

                descriptor = libraryServices.IsLibraryLoaded(assembly) || libraryServices.ImportLibrary(assembly)
                    ? libraryServices.GetFunctionDescriptor(assembly, function)
                    : libraryServices.GetFunctionDescriptor(function);
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
                    nickname,
                    nodeElement,
                    assembly,
                    DummyNode.Nature.Unresolved);
            }

            DSFunctionBase result;
            if (descriptor.IsVarArg)
            {
                result = new DSVarArgFunction(descriptor);
                if (nodeElement.Name != typeof(DSVarArgFunction).FullName)
                {
                    VariableInputNodeController.SerializeInputCount(
                        nodeElement,
                        descriptor.Parameters.Count());
                }
            }
            else
                result = new DSFunction(descriptor);

            result.Deserialize(nodeElement, context);
            return result;
        }
        
        private static int DetermineFunctionInputCount(XmlElement element)
        {
            int additionalPort = 0;

            // "DSVarArgFunction" is a "VariableInputNode", therefore it will 
            // have "inputcount" as one of the attributes. If such attribute 
            // does not exist, throw an ArgumentException.
            if (element.Name.Equals(/*NXLT*/"Dynamo.Nodes.DSVarArgFunction"))
            {
                var inputCountAttrib = element.Attributes[/*NXLT*/"inputcount"];

                if (inputCountAttrib == null)
                {
                    throw new ArgumentException(string.Format(
                        /*NXLT*/"Function inputs cannot be determined ({0}).",
                        element.GetAttribute(/*NXLT*/"nickname")));
                }

                return Convert.ToInt32(inputCountAttrib.Value);
            }

            var signature = string.Empty;
            var signatureAttrib = element.Attributes[/*NXLT*/"function"];
            if (signatureAttrib != null)
                signature = signatureAttrib.Value;
            else if (element.ChildNodes.Count > 0)
            {
                // We have an old file format with "FunctionItem" child element.
                var childElement = element.ChildNodes[0] as XmlElement;
                signature = string.Format(/*NXLT*/"{0}@{1}",
                                          childElement.GetAttribute(/*NXLT*/"DisplayName"),
                                          childElement.GetAttribute(/*NXLT*/"Parameters").Replace(';', ','));

                // We need one more port for instance methods/properties.
                switch (childElement.GetAttribute(/*NXLT*/"Type"))
                {
                    case /*NXLT*/"InstanceMethod":
                    case /*NXLT*/"InstanceProperty":
                        additionalPort = 1; // For taking the instance itself.
                        break;
                }
            }

            if (string.IsNullOrEmpty(signature))
            {
                const string message = /*NXLT*/"Function signature cannot be determined.";
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
            var assemblyAttrib = element.Attributes[/*NXLT*/"assembly"];
            if (assemblyAttrib != null)
                assemblyName = assemblyAttrib.Value;
            else if (element.ChildNodes.Count > 0)
            {
                // We have an old file format with "FunctionItem" child element.
                var childElement = element.ChildNodes[0] as XmlElement;
                var funcItemAsmAttrib = childElement.Attributes[/*NXLT*/"Assembly"];
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