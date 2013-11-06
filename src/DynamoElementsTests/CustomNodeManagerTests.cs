using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.PackageManager;
using Dynamo.Tests;
using Dynamo.Utilities;
using NUnit.Framework;

//namespace Dynamo
//{
//    class CustomNodeManagerTests : DynamoUnitTest
//    {
//        //[Test]
//        //public void CanGetDependenciesOnLoad()
//        //{
            
//        //}
//    }
//}
    // need tests for

        //public Guid GuidFromPath(string path)
        
        //public IEnumerable<string> GetNodeNames()
        
        //public IEnumerable<Tuple<string, string, Guid>> GetNodeNameCategoryAndGuidList()
        
        //public IEnumerable<FunctionDefinition> GetLoadedDefinitions()
        
        //public void AddFunctionDefinition(Guid id, FunctionDefinition def)
        
        //public CustomNodeInfo AddFileToPath(string file)
        
        //public bool TypesFromFolderAreInUse(string path, ref HashSet<Tuple<string, string>> whereTypesAreLoaded)

        //public List<CustomNodeInfo> GetInfosFromFolder(string dir)
        
        //public bool RemoveTypesLoadedFromFolder(string path)
        
        //public bool RemoveFolder(string path)
        
        //public void Remove(Guid guid)
        
        //public List<CustomNodeInfo> UpdateSearchPath()
        
        //public IEnumerable<CustomNodeInfo> ScanNodeHeadersInDirectory(string dir)
        
        //public void SetFunctionDefinition(Guid guid, FunctionDefinition def)
        
        //public void SetNodePath(Guid id, string path)
        
        //public void SetNodeInfo(CustomNodeInfo info)
        
        //public void SetNodeName(Guid id, string name)
        
        //public void SetNodeCategory(Guid id, string category)
        
        //public void SetNodeDescription(Guid id, string description)
        
        //public string GetDefaultSearchPath()
        
        //public FunctionDefinition GetFunctionDefinition(Guid id)
        
        //public string GetNodePath(Guid id)
        
        //public bool Contains(Guid guid)
        
        //public bool Contains(string name)
        
        //public bool IsInitialized(string name)
        
        //public bool IsInitialized(Guid guid)
        
        //public Guid GetGuidFromName(string name)
        
        //public bool GetNodeInstance(DynamoController controller, string name, out Function result)
        
        //public bool GetNodeInstance(Guid guid, out Function result)
        
        //public static CustomNodeInfo GetHeaderFromPath(string path)

        //public static bool GetHeaderFromPath(string path, out Guid guid, out string name, out string category, out string description)
       
        //public FunctionDefinition GetDefinitionFromWorkspace(WorkspaceModel workspace)

        //private bool GetDefinitionFromPath(Guid funcDefGuid, out FunctionDefinition def)
      
        //public void OnGetDefinitionFromPath(FunctionDefinition def)

        //public static FScheme.Expression CompileFunction(FunctionDefinition definition)

        //public static FScheme.Expression CompileFunction(FunctionDefinition definition, out IEnumerable<string> inputNames, out IEnumerable<string> outputNames)
       
        //private static string FormatFileName(string filename)
       
        //internal static string RemoveChars(string s, IEnumerable<string> chars)

        //internal bool AddDirectoryToSearchPath(string p)

        //internal CustomNodeInfo GetNodeInfo(Guid x)

        //internal bool Refactor(Guid guid, string newName, string newCategory, string newDescription)
 
