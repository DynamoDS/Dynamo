using System;
using System.Collections.Generic;
using System.IO;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Utils;

namespace ProtoFFI
{
    public class ImportModuleHandler
    {
        readonly Dictionary<string, ImportNode> mModuleTable = new Dictionary<string, ImportNode>(StringComparer.CurrentCultureIgnoreCase);
        readonly ImportNode mRootImportNode = new ImportNode { CodeNode = new CodeBlockNode() };
        readonly ProtoCore.Core _coreObj;

        // line and column info used to log error message 
        int curLine = -1; int curCol = -1;

        // public static readonly string kInstanceVarName = "__instance";

        public ImportModuleHandler(ProtoCore.Core coreObj)
        {
            _coreObj = coreObj;
            if (_coreObj.Options.RootModulePathName != null)
                mModuleTable[_coreObj.Options.RootModulePathName] = null;
        }

        public ImportNode RootImportNode { get { return mRootImportNode; } }

        public ProtoCore.AST.AssociativeAST.ImportNode Import(string moduleName, string typeName, string alias, int line = -1, int col = -1)
        {
            curLine = line;
            curCol = col;

            moduleName = moduleName.Replace("\"", String.Empty);
            ProtoCore.AST.AssociativeAST.ImportNode node = new ProtoCore.AST.AssociativeAST.ImportNode();
            node.ModuleName = moduleName;

            string modulePathFileName = FileUtils.GetDSFullPathName(moduleName, _coreObj.Options);

            // Tracking directory paths for all imported DS files during preload assembly stage so that they can be accessed by Graph compiler before execution - pratapa
            if (_coreObj.IsParsingPreloadedAssembly)
            {
                string dirName = Path.GetDirectoryName(modulePathFileName);
                if (!string.IsNullOrEmpty(dirName) && !_coreObj.Options.IncludeDirectories.Contains(dirName))
                    _coreObj.Options.IncludeDirectories.Add(dirName);
            }

            if (string.IsNullOrEmpty(typeName))
            {
                //Check if moduleName is a type name with namespace.
                Type type = Type.GetType(moduleName);
                if (type != null)
                {
                    typeName = type.FullName;
                    modulePathFileName = type.Assembly.Location;
                }
            }

            if (modulePathFileName == null || !File.Exists(modulePathFileName))
            {
                if (!FFIExecutionManager.Instance.IsInternalGacAssembly(moduleName))
                {
                    System.Diagnostics.Debug.Write(@"Cannot import file: '" + modulePathFileName);
                    _coreObj.LogWarning(ProtoCore.BuildData.WarningID.kFileNotFound, string.Format(ProtoCore.StringConstants.kFileNotFound, modulePathFileName));
                    return null;
                }
            }

            node.ModulePathFileName = modulePathFileName;
            node.CodeNode = null;
            if (typeName.Length > 0)
                node.Identifiers.Add(typeName);

            ProtoCore.AST.AssociativeAST.ImportNode importedNode = null;
            if (TryGetImportNode(modulePathFileName, typeName, out importedNode))
            {
                node.HasBeenImported = true;
                return node;
            }

            ProtoCore.AST.AssociativeAST.CodeBlockNode codeNode = null;
            if (importedNode == null)
                codeNode = new ProtoCore.AST.AssociativeAST.CodeBlockNode();
            else
                codeNode = importedNode.CodeNode;

            try
            {
                codeNode = ImportCodeBlock(modulePathFileName, typeName, alias, codeNode);
            }
            catch (System.Exception ex)
            {
                if (ex.InnerException != null)
                    _coreObj.BuildStatus.LogSemanticError(ex.InnerException.Message);
                _coreObj.BuildStatus.LogSemanticError(ex.Message);
            }

            //Cache the codeblock of root import node.
            CodeBlockNode rootImportCodeBlock = mRootImportNode.CodeNode;
            mRootImportNode.CodeNode = new CodeBlockNode(); //reset with the new one.

            //Remove all empty nodes and add to root import node.
            codeNode.Body.RemoveAll(AddToRootImportNodeIfEmpty);

            if (mRootImportNode.CodeNode.Body.Count == 0) //empty
                mRootImportNode.CodeNode = rootImportCodeBlock; //reset the old one.

            if (importedNode != null)
            {
                //module has import node, but type is not imported yet.
                //MergeCodeBlockNode(ref importedNode, codeBlockNode);

                //update existing import node and return null.
                importedNode.CodeNode = codeNode;
                return null; 
            }

            node.CodeNode = codeNode;
            mModuleTable.Add(modulePathFileName, node);
            return node;
        }

        /// <summary>
        /// Checks if the given class node is empty. Also adds the empty class 
        /// node to code block of mRootImportNode.
        /// </summary>
        /// <param name="node">Given node</param>
        /// <returns>True if the given node is empty class node</returns>
        private bool AddToRootImportNodeIfEmpty(AssociativeNode node)
        {
            ClassDeclNode classNode = node as ClassDeclNode;
            if (null == classNode || !IsEmptyClassNode(classNode))
                return false;

            mRootImportNode.CodeNode.Body.Add(classNode);
            return true;
        }

        private ProtoCore.AST.AssociativeAST.CodeBlockNode MergeCodeBlockNode(ref ProtoCore.AST.AssociativeAST.ImportNode importedNode, 
            ProtoCore.AST.AssociativeAST.CodeBlockNode codeBlockNode)
        {
            if(codeBlockNode == null || codeBlockNode.Body == null || importedNode == null)
                return codeBlockNode;

            ProtoCore.AST.AssociativeAST.CodeBlockNode importedCodeBlock = importedNode.CodeNode;
            if (importedCodeBlock == null)
            {
                importedNode.CodeNode = codeBlockNode;
                return codeBlockNode;
            }

            foreach (var item in codeBlockNode.Body)
            {
                ProtoCore.AST.AssociativeAST.ClassDeclNode classNode = item as ProtoCore.AST.AssociativeAST.ClassDeclNode;
                if (classNode != null)
                {
                    ProtoCore.AST.AssociativeAST.ClassDeclNode importedClass = null;
                    if (TryGetClassNode(importedCodeBlock, classNode.className, out importedClass))
                    {
                        bool dummyClassNode = IsEmptyClassNode(classNode);
                        bool dummyImportClass = IsEmptyClassNode(importedClass);

                        Validity.Assert(dummyImportClass || dummyClassNode, string.Format("{0} is imported more than once!!", classNode.className));
                        if (dummyImportClass && !dummyClassNode)
                        {
                            importedNode.CodeNode.Body.Remove(importedClass);
                            importedNode.CodeNode.Body.Add(classNode);
                        }
                    }
                    else
                        importedNode.CodeNode.Body.Add(classNode);
                }
                else
                    importedNode.CodeNode.Body.Add(item); //TODO other conflict resolution needs to be done here.
            }
            return importedNode.CodeNode;
        }

        private static bool IsEmptyClassNode(ProtoCore.AST.AssociativeAST.ClassDeclNode classNode)
        {
            if (null == classNode)
                return true;

            if (classNode.IsExternLib && null == classNode.ExternLibName)
                return true;

            if (classNode.funclist.Count > 0)
                return false;

            if (classNode.varlist.Count > 0)
                return false;

            return true;
        }

        private bool TryGetImportNode(string moduleName, string typeName, out ProtoCore.AST.AssociativeAST.ImportNode node)
        {
            if (mModuleTable.TryGetValue(moduleName, out node))
            {
                if (typeName.Length == 0) //All types
                    return true;

                ProtoCore.AST.AssociativeAST.ClassDeclNode classNode;
                if (TryGetClassNode(node.CodeNode, typeName, out classNode) && !IsEmptyClassNode(classNode))
                    return true;
            }
            return false;
        }

        public static bool TryGetClassNode(ProtoCore.AST.AssociativeAST.CodeBlockNode node, string typeName, out ProtoCore.AST.AssociativeAST.ClassDeclNode classNode)
        {
            classNode = null;
            //Traverse thru the code block and check if this type is already available
            if (node == null || node.Body == null)
                return false;

            foreach (var item in node.Body)
            {
                ProtoCore.AST.AssociativeAST.ClassDeclNode clsnode = item as ProtoCore.AST.AssociativeAST.ClassDeclNode;
                if (clsnode != null && clsnode.className == typeName)
                {
                    classNode = clsnode;
                    return true;
                }
            }
            return false;
        }

        private CodeBlockNode ImportCodeBlock(string importModuleName, string typeName, string alias, CodeBlockNode refNode)
        {
            DLLModule dllModule = null;
            string extension = System.IO.Path.GetExtension(importModuleName).ToLower();
            if (extension == ".dll" || extension == ".exe")
            {
                try
                {
                    dllModule = DLLFFIHandler.GetModule(importModuleName);
                }
                catch
                {
                    _coreObj.LogSemanticError(string.Format("Failed to import {0}", importModuleName), _coreObj.CurrentDSFileName, curLine, curCol);
                }
            }
            

            CodeBlockNode codeBlockNode = refNode;
            if (null != dllModule)
            {
                codeBlockNode = dllModule.ImportCodeBlock(typeName, alias, refNode);
                Type type = dllModule.GetExtensionAppType();
                if (type != null)
                    FFIExecutionManager.Instance.RegisterExtensionApplicationType(_coreObj, type);
            }
            else if (extension == ".ds")
            {
                string origDSFile = _coreObj.CurrentDSFileName;
                _coreObj.CurrentDSFileName = System.IO.Path.GetFullPath(importModuleName);
                codeBlockNode = ImportDesignScriptFile(_coreObj.CurrentDSFileName, typeName, alias);
                _coreObj.CurrentDSFileName = origDSFile;
            }
            

            return codeBlockNode;
        }

        private ProtoCore.AST.AssociativeAST.CodeBlockNode ImportDesignScriptFile(string designScriptFile, string typeName, string alias)
        {
            string curDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(Path.GetDirectoryName(designScriptFile));
            ProtoCore.AST.AssociativeAST.CodeBlockNode importCodeblockNode = null;

            try
            {
                ProtoCore.DesignScriptParser.Scanner scanner = new ProtoCore.DesignScriptParser.Scanner(designScriptFile);
                ProtoCore.DesignScriptParser.Parser parser = new ProtoCore.DesignScriptParser.Parser(scanner, _coreObj, true);
                parser.ImportModuleHandler = this;

                //System.IO.StringWriter parseErrors = new System.IO.StringWriter();
                //parser.errors.errorStream = parseErrors;

                parser.Parse();
                //if (parseErrors.ToString() != String.Empty)
                    //_coreObj.BuildStatus.LogSyntaxError(parseErrors.ToString());
                //_coreObj.BuildStatus.errorCount += parser.errors.count;
                importCodeblockNode = (ProtoCore.AST.AssociativeAST.CodeBlockNode)parser.root;
            }
            finally
            {
                Directory.SetCurrentDirectory(curDirectory);
            }

            return importCodeblockNode;
        }
    }
}
