using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Lang;
using ProtoCore.Mirror;
using ProtoCore.Utils;
using ProtoFFI;
using ProtoScript.Runners;
using System.IO;
using System.Diagnostics;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using ProtoCore.DSASM;
using System.Collections;
using System.Text.RegularExpressions;

namespace ProtoTestFx.TD
{
    public class TestFrameWork
    {
        private static ProtoCore.Core testCore;
        private static ProtoCore.RuntimeCore testRuntimeCore;

        private ExecutionMirror testMirror;
        private readonly ProtoScriptTestRunner runner;
        private static string mErrorMessage = "";
        bool testImport;
        bool testDebug;
        bool dumpDS=false;
        bool cfgImport = Convert.ToBoolean(Environment.GetEnvironmentVariable("Import"));
        bool cfgDebug = Convert.ToBoolean(Environment.GetEnvironmentVariable("Debug"));
 
        public TestFrameWork()
        {
            runner = new ProtoScriptTestRunner();
        }

        public ProtoCore.Core GetTestCore()
        {
            return testCore;
        }

        public ProtoCore.RuntimeCore GetTestRuntimeCore()
        {
            return testRuntimeCore;
        }

        public ProtoCore.Core SetupTestCore()
        {
            testCore = new ProtoCore.Core(new ProtoCore.Options());

            testCore.Configurations.Add(ConfigurationKeys.GeometryFactory, "DSGeometry.dll");
            testCore.Configurations.Add(ConfigurationKeys.PersistentManager, "DSGeometry.dll");
            testCore.Compilers.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Compiler(testCore));
            testCore.Compilers.Add(ProtoCore.Language.kImperative, new ProtoImperative.Compiler(testCore));

            // this setting is to fix the random failure of replication test case
            testCore.Options.ExecutionMode = ProtoCore.ExecutionMode.Serial;
            testCore.Options.Verbose = false;

            //FFI registration and cleanup
            DLLFFIHandler.Register(FFILanguage.CPlusPlus, new ProtoFFI.PInvokeModuleHelper());
            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
            CLRModuleType.ClearTypes();
            mErrorMessage = string.Empty;
            if (cfgImport)
            {
                
                 testImport = cfgImport;
            }
            else
            {
                 testImport = false;
            }

            if (cfgDebug)
            {
                testImport = cfgDebug;
            }
            else
            {
                testDebug = false;
                
            }
        
            return testCore;
        }

        /// <summary>
        /// Build a Core with default options and contains no function or class entries
        /// </summary>
        /// <returns></returns>
        
        public ProtoCore.Core CreateTestCore()
        {
            ProtoCore.Core core = new ProtoCore.Core(new ProtoCore.Options());
            core.Compilers.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Compiler(core));
            core.Compilers.Add(ProtoCore.Language.kImperative, new ProtoImperative.Compiler(core));
            core.Options.ExecutionMode = ProtoCore.ExecutionMode.Serial;
            core.ParsingMode = ProtoCore.ParseMode.AllowNonAssignment;
            core.IsParsingCodeBlockNode = true;
            core.IsParsingPreloadedAssembly = false;
            return core;
        }

       
        public ExecutionMirror RunScriptFile(string directory, string filename)
        {
            string currentFile = filename;
            string errorString = "";
            if (testImport)
            {
                    currentFile = Path.GetFileName(filename);
                    string fileToRun = Path.GetFileNameWithoutExtension(currentFile) + "_Import.ds";
                    string importCode = string.Format("import(\"{0}\");", currentFile);
                    directory = Path.Combine(directory, filename);
                    directory = Path.GetDirectoryName(directory) + @"\" ;
                    createDSFile(fileToRun, directory, importCode);
                    errorString = "tested as import file";
                    
            }
            else if (testDebug)
            {
                    Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
                    string fname = Path.Combine(directory, filename);
                    TextReader file = new StreamReader(fname);
                    WatchTestFx.GeneratePrintStatements(file, ref map);
                    file = new StreamReader(fname);
                    String src = file.ReadToEnd();
                    file.Close();

                    WatchTestFx fx = new WatchTestFx();
                    testCore = fx.TestCore;
                  
                    fx.CompareRunAndWatchResults(Path.GetFullPath(filename), src, map);
                    testMirror = fx.Mirror;

                    return testMirror;
                       
            }

            string dsFullPathName = directory + currentFile;
            return RunScript(dsFullPathName, errorString);
        }

        public ExecutionMirror VerifyRunScriptFile(string directory, string filename, string errorstring)
        {
            string dsFullPathName = directory + filename;
            Assert.DoesNotThrow(() => testMirror = RunScript(dsFullPathName, errorstring), errorstring);
            return testMirror;
        }

        public ExecutionMirror RunScript(string pathname, string errorstring = "", string includePath = "")
        {
            SetupTestCore();
            Console.WriteLine( errorstring);
            if (!String.IsNullOrEmpty(includePath))
            {
                if (System.IO.Directory.Exists(includePath))
                {
                    testCore.Options.IncludeDirectories.Add(includePath);
                }
                else
                {
                    Console.WriteLine(String.Format("Path: {0} does not exist.", includePath));
                }
            }
            testMirror = runner.LoadAndExecute(pathname, testCore, out testRuntimeCore);
            SetErrorMessage(errorstring);
            return testMirror;
        }

        public ExecutionMirror VerifyRunScript(string pathname, string errorstring)
        {
            Assert.DoesNotThrow(() => testMirror = RunScript(pathname, errorstring), errorstring);
            return testMirror;
        }

        public void SetErrorMessage(string msg)
        {
            if (null == msg)
                mErrorMessage = "";
            else
                mErrorMessage = msg;
        }

        /// <summary>
        /// Method to run an inline Ds script.
        /// </summary>
        /// <param name="sourceCode">The String contains the ds codes</param>
        /// <returns></returns>
        public virtual ExecutionMirror RunScriptSource(string sourceCode, string errorstring = "", string includePath = "")
        {
            
            if (testImport)
            {
                Guid g;
                g = Guid.NewGuid();
                
                StackTrace trace = new StackTrace();
                int caller = 2;
                
                string tempPath = System.IO.Path.GetTempPath();
                string import = @"testImport\";
                string importDir = Path.Combine(tempPath, import);
                
                if (!Directory.Exists(importDir))
                {
                    System.IO.Directory.CreateDirectory(importDir);
                }

                
                string importFileName = (g.ToString()+".ds");
                createDSFile(importFileName,importDir,sourceCode);

                return testMirror = RunScriptFile(importDir, importFileName);

            }
            else if (testDebug)
            {
                Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
                if (!String.IsNullOrEmpty(includePath))
                {
                    if (System.IO.Directory.Exists(includePath))
                    {
                        testCore.Options.IncludeDirectories.Add(includePath);
                    }
                    else
                    {
                        Console.WriteLine(String.Format("Path: {0} does not exist.", includePath));
                    }
                }

                StringReader file = new StringReader(sourceCode);
                WatchTestFx.GeneratePrintStatements(file, ref map);

                WatchTestFx fx = new WatchTestFx();
                testCore = fx.TestCore;
                fx.CompareRunAndWatchResults("", sourceCode, map);
                testMirror = fx.Mirror;
                
                return testMirror;
            }
            else
            {
                SetupTestCore();
                if (!String.IsNullOrEmpty(includePath))
                {
                    if (System.IO.Directory.Exists(includePath))
                    {
                        testCore.Options.IncludeDirectories.Add(includePath);
                    }
                    else
                    {
                        Console.WriteLine(String.Format("Path: {0} does not exist.", includePath));
                    }
                }
                testMirror = runner.Execute(sourceCode, testCore, out testRuntimeCore);
                
                if (dumpDS )
                {

                    String fileName = TestContext.CurrentContext.Test.Name + ".ds";
                    String folderName = TestContext.CurrentContext.Test.FullName;

                    string[] substrings = folderName.Split('.');

                    string path = "..\\..\\..\\test\\core\\dsevaluation\\DSFiles\\";
                    if (!System.IO.Directory.Exists(path))
                        System.IO.Directory.CreateDirectory(path);

                    createDSFile(fileName, path, sourceCode);
                }

                SetErrorMessage(errorstring);
                return testMirror;
            }
        }

        /// <summary>
        /// Method to run an AST directly
        /// </summary>
        /// <param name="sourceCode">The AST</param>
        /// <returns></returns>
        public ExecutionMirror RunASTSource(List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList, string errorstring = "", string includePath = "")
        {
            SetupTestCore();
            if (!String.IsNullOrEmpty(includePath))
            {
                if (System.IO.Directory.Exists(includePath))
                {
                    testCore.Options.IncludeDirectories.Add(includePath);
                }
                else
                {
                    Console.WriteLine(String.Format("Path: {0} does not exist.", includePath));
                }
            }
            testMirror = runner.Execute(astList, testCore);
            SetErrorMessage(errorstring);
            return testMirror;
        }

        public ExecutionMirror VerifyRunScriptSource(string sourceCode, string errorstring = "", string importPath = null)
        {
            Assert.DoesNotThrow(() => testMirror = RunScriptSource(sourceCode, errorstring, importPath), errorstring);
                return testMirror;
        }
        public void createDSFile(string fileName,string path, string code )
        {
                  string fullPath = Path.Combine(path,fileName);
                  if (File.Exists(fullPath))
                  {
                      File.Delete(fullPath);
                  }
                  FileStream files = new System.IO.FileStream(fullPath, System.IO.FileMode.Append, System.IO.FileAccess.Write);
            
                  System.IO.StreamWriter sw = new System.IO.StreamWriter(files);
                  
                  sw.WriteLine(code);
                  sw.Close();
                  files.Close();
        }

        public void DebugModeVerification(ExecutionMirror mirror, string variable, object expectedValue)
        {
            Obj dsObj = mirror.GetDebugValue(variable);
            Assert.IsTrue(mirror.EqualDotNetObject(dsObj, expectedValue), mErrorMessage);
        }

        private static string BuildIndicesString(List<int> indices)
        {
            StringBuilder builder = new StringBuilder("");
            foreach (var index in indices)
            {
                builder.Append("[" + index.ToString() + "]"); 
            }
            return builder.ToString();
        }

        private static void VerifyPodType<T>(T expectedValue, Obj dsObject, string dsVariable, List<int>indices)
        {
            try
            {
                T realValue = (T)Convert.ChangeType(dsObject.Payload, typeof(T));
                if (!expectedValue.Equals(realValue))
                {
                    Assert.Fail(String.Format("\t{0}{1} is expected to be {2}, but its actual value is {3}. \n{4}", dsVariable, 
                        BuildIndicesString(indices), expectedValue, realValue, mErrorMessage));
                }
            }
            catch (System.InvalidCastException)
            {
                Assert.Fail(String.Format("\t{0}{1} is expected to be {2}, but its actual value can't be converted to {3}. \n{4}", dsVariable, 
                    BuildIndicesString(indices), expectedValue, typeof(T), mErrorMessage));
            }
        }

        // Verify for single object
        private static void VerifyInternal(object expectedObject, Obj dsObject, string dsVariable, List<int> indices)
        {
            if (expectedObject == null)
            {
                if (!dsObject.DsasmValue.IsNull)
                {
                    Assert.Fail(String.Format("\t{0}{1} is expected to be null, but it isn't.\n{2}", dsVariable, 
                        TestFrameWork.BuildIndicesString(indices), TestFrameWork.mErrorMessage));
                }
                return;
            }

            Type expectedType = expectedObject.GetType();
            if (dsObject.DsasmValue.IsNull && expectedObject != null)
            {
                Assert.Fail(String.Format("\tThe value of {0} was null, but wasn't expected to be.\n{1}", dsVariable, mErrorMessage));
                
            }
            else if (expectedObject is Int32 || expectedObject is Int64)
            {
                Int64 expectedValue = Convert.ToInt64(expectedObject);
                if (dsObject.Type.UID != (int)ProtoCore.PrimitiveType.kTypeInt)
                {
                    Assert.Fail(String.Format("\t{0}{1} is expected to be {2}, but its actual value is not an integer. \n{2}", dsVariable, 
                        BuildIndicesString(indices), expectedValue, mErrorMessage));
                }
                else
                {
                    TestFrameWork.VerifyPodType(expectedValue, dsObject, dsVariable, indices);
                }
            }
            else if (expectedObject is Double)
            {
                Double expectedValue = Convert.ToDouble(expectedObject);
                if (dsObject.Type.UID != (int)ProtoCore.PrimitiveType.kTypeDouble)
                {
                    Assert.Fail(String.Format("\t{0}{1} is expected to be {2}, but its actual value is not a double. \n{3}", dsVariable, 
                        BuildIndicesString(indices), expectedValue, mErrorMessage));
                }
                else
                {
                    try
                    {
                        Double dsValue = Convert.ToDouble(dsObject.Payload);

                        if (!MathUtils.Equals(expectedValue, dsValue))
                        {
                            Assert.Fail(String.Format("\t{0}{1} is expected to be {2}, but its actual value is {3}. \n{4}", dsVariable, 
                                BuildIndicesString(indices), expectedValue, dsValue, mErrorMessage));
                        }
                    }
                    catch (System.InvalidCastException)
                    {
                        Assert.Fail(String.Format("\t{0}{1} is expected to be {2}, but its actual value can't be converted to Double. \n{3}", dsVariable, 
                            BuildIndicesString(indices), expectedValue, mErrorMessage));
                    }
                }
            }
            else if (expectedObject is Boolean)
            {
                Boolean expectedValue = Convert.ToBoolean(expectedObject);
                if (dsObject.Type.UID != (int)ProtoCore.PrimitiveType.kTypeBool)
                {
                    Assert.Fail(String.Format("\t{0}{1} is expected to be {2}, but its actual type is not bool. \n{3}", dsVariable, 
                        BuildIndicesString(indices), expectedValue, mErrorMessage));
                }
                else
                {
                    TestFrameWork.VerifyPodType(expectedValue, dsObject, dsVariable, indices);
                }
            }
            else if (expectedObject is Char)
            {
                Char expectedValue = Convert.ToChar(expectedObject);

                if (dsObject.Type.UID != (int)ProtoCore.PrimitiveType.kTypeChar)
                {
                    Assert.Fail(String.Format("\t{0}{1} is expected to be {2}, but its actual type is not char. \n{3}", dsVariable, BuildIndicesString(indices), expectedValue, mErrorMessage));
                }
                else
                {
                    try
                    {
                        Int64 utf8Encoding = Convert.ToInt64(dsObject.Payload);
                        Char dsValue = EncodingUtils.ConvertInt64ToCharacter(utf8Encoding);

                        if (!expectedObject.Equals(dsValue))
                        {
                            Assert.Fail(String.Format("\t{0}{1} is expected to be {2}, but its actual value is {3}. \n{4}", dsVariable, 
                                BuildIndicesString(indices), expectedValue, dsValue, mErrorMessage));
                        }
                    }
                    catch (System.InvalidCastException)
                    {
                        Assert.Fail(String.Format("\t{0}{1} is expected to be {2}, but its actual value can't be converted to Char. \n{3}", dsVariable, 
                            BuildIndicesString(indices), expectedValue, mErrorMessage));
                    }
                }
            }
            else if (expectedObject is String)
            {
                string stringValue = dsObject.Payload as string;
                if (stringValue == null)
                {
                    Assert.Fail(String.Format("\t{0}{1} is expected to be a string, but its actual value is not a string\n{2}", dsVariable, 
                        BuildIndicesString(indices), mErrorMessage));
                }
                else if (!expectedObject.Equals(stringValue))
                {
                    Assert.Fail(String.Format("\t{0}{1} is expected to be a string \"{2}\", but its actual value is \"{3}\".\n{4}", dsVariable, 
                        BuildIndicesString(indices), expectedObject, stringValue, mErrorMessage));
                }
            }
            else if (typeof(IEnumerable).IsAssignableFrom(expectedType))
            {
                IEnumerable collection = expectedObject as IEnumerable;
                int index = 0;
                ProtoCore.DSASM.Mirror.DsasmArray dsArray = dsObject.Payload as ProtoCore.DSASM.Mirror.DsasmArray;
                if (dsArray == null)
                {
                    Assert.Fail(String.Format("{0}{1} is expected to be an array, but its actual value isn't an array.\n{2}", dsVariable, 
                        BuildIndicesString(indices), mErrorMessage));
                }
                foreach (var item in collection)
                {
                    indices.Add(index);
                    VerifyInternal(item, dsArray.members[index], dsVariable, indices);
                    indices.RemoveAt(indices.Count - 1);
                    ++index;
                }
            }
            else
            {
                Assert.Fail(string.Format("\tUnexpected object type.\n{0}", mErrorMessage));
            }
        }

        public void VerifyReferenceCount(string dsVariable, int referencCount)
        {
            try
            {
                StackValue sv = testMirror.GetRawFirstValue(dsVariable);

                if (!sv.IsArray && !sv.IsPointer)
                {
                    if (referencCount != 0)
                    {
                        Assert.Fail(String.Format("\t{0} is not a heap element, it doesn't sense to verify its reference count. Should always be 0", dsVariable));
                    }
                }
                else
                {
                    ProtoCore.DSASM.HeapElement he = testMirror.MirrorTarget.rmem.Heap.GetHeapElement(sv);
                }
            }
            catch (NotImplementedException)
            {
                Assert.Fail("\tFailed to get the value of variable " + dsVariable);
            }
        }

        public static void Verify(ExecutionMirror mirror, string dsVariable, object expectedValue, int startBlock = 0)
        {
            try
            {
                Obj dsObj = mirror.GetFirstValue(dsVariable, startBlock);
                var indices = new List<int>();
                VerifyInternal(expectedValue, dsObj, dsVariable, indices);
            }
            catch (NotImplementedException)
            {
                Assert.Fail(string.Format("\tFailed to get the value of variable {0}\n{1}", dsVariable, mErrorMessage));
            }
        }

        public void VerifyMethodExists(string className, string methodName, bool doAssert = true)
        {
            int classIndex = testCore.ClassTable.IndexOf(className);
            if (classIndex == ProtoCore.DSASM.Constants.kInvalidIndex)
            {
                if(doAssert)
                    Assert.Fail(string.Format("\tFailed to find type \"{0}\" \n{1}", className, mErrorMessage));
                return;
            }

            ProtoCore.DSASM.ClassNode thisClass = testCore.ClassTable.ClassNodes[classIndex];
            if (!thisClass.vtable.procList.Exists(memberFunc => String.Compare(memberFunc.name, methodName) == 0))
            {
                if(doAssert)
                    Assert.Fail(string.Format("\tMethod \"{0}.{1}\" doesn't exist \n{2}", className, methodName, mErrorMessage)); 
            }
            else if(!doAssert)
                Assert.Fail(string.Format("\tMethod \"{0}.{1}\" does exist \n{2}", className, methodName, mErrorMessage));
        }

        public string[] GetAllMatchingClasses(string name)
        {
            return testCore.ClassTable.GetAllMatchingClasses(name);
        }

        public int GetClassIndex(string className)
        {
            return testCore.ClassTable.IndexOf(className);
        }

        public void Verify(string dsVariable, object expectedValue, int startBlock = 0)
        {
            RuntimeMirror mirror = new RuntimeMirror(dsVariable, startBlock, GetTestRuntimeCore());
            AssertValue(mirror.GetData(), expectedValue);
            //Verify(testMirror, dsVariable, expectedValue, startBlock);
        }

        public static void VerifyBuildWarning(ProtoCore.BuildData.WarningID id)
        {
            Assert.IsTrue(testCore.BuildStatus.Warnings.Any(w => w.ID == id), mErrorMessage);
        }

        /// <summary>
        /// Verify the total warning count
        /// </summary>
        /// <param name="count"></param>
        public void VerifyBuildWarningCount(int count)
        {
            Assert.IsTrue(testCore.BuildStatus.WarningCount == count, mErrorMessage);
        }

        /// <summary>
        /// Verify the number of times that the warning 'id' has occured
        /// </summary>
        /// <param name="id"></param>
        /// <param name="count"></param>
        public void VerifyBuildWarningCount(ProtoCore.BuildData.WarningID id, int count)
        {
            int warningCount = testCore.BuildStatus.Warnings.Count(w => w.ID == id);
            Assert.IsTrue(warningCount == count, mErrorMessage);
        }

        public static void VerifyRuntimeWarning(ProtoCore.Runtime.WarningID id)
        {
            VerifyRuntimeWarning(testRuntimeCore, id);
        }

        public static void VerifyRuntimeWarning(ProtoCore.RuntimeCore runtimeCore, ProtoCore.Runtime.WarningID id)
        {
            Assert.IsTrue(runtimeCore.RuntimeStatus.Warnings.Any(w => w.ID == id), mErrorMessage);
        }

        public void VerifyRuntimeWarningCount(int count)
        {
            Assert.IsTrue(testRuntimeCore.RuntimeStatus.WarningCount == count, mErrorMessage);
        }

        public void VerifyProperty(string dsVariable, string propertyName, object expectedValue, int startBlock = 0)
        {
            VerifyProperty(testMirror, dsVariable, propertyName, expectedValue, startBlock);
        }

        public void VerifyProperty(ExecutionMirror mirror, string dsVariable, string propertyName, object expectedValue, int startBlock = 0)
        {
            Assert.IsTrue(null != mirror);
            Obj dsObj = mirror.GetFirstValue(dsVariable, startBlock);
            Dictionary<string, Obj> propBag = mirror.GetProperties(dsObj);

            string expression = string.Format("{0}.{1}", dsVariable, propertyName);
            Assert.IsTrue(null != propBag && propBag.TryGetValue(propertyName, out dsObj), string.Format("Property \"{0}\" not found.\n{1}", expression, mErrorMessage));
            
            var indices = new List<int>();

            VerifyInternal(expectedValue, dsObj, expression, indices);
        }
        string GetFFIObjectStringValue(string dsVariable, int startBlock = 1, int arrayIndex = -1)
        {
            var helper = DLLFFIHandler.GetModuleHelper(FFILanguage.CSharp);
            var marshaller = helper.GetMarshaller(TestFrameWork.testRuntimeCore);
            Obj val = testMirror.GetFirstValue(dsVariable, startBlock);
            StackValue sv;

            if (val.Payload == null)
            {
                return null;
            }
            else if (val.Payload is DsasmArray)
            {
                DsasmArray arr = val.Payload as DsasmArray;
                Assert.IsTrue((arrayIndex >= 0 && arrayIndex < arr.members.Length), "Invalid array index for FFIObjectOutOfScope verification.");
                sv = arr.members[arrayIndex].DsasmValue;
            }
            else
                sv = val.DsasmValue;

            return marshaller.GetStringValue(sv);
        }

        public void VerifyFFIObjectOutOfScope(string dsVariable, int startBlock = 1, int arrayIndex = -1)
        {
            Assert.IsTrue(string.IsNullOrEmpty(GetFFIObjectStringValue(dsVariable, startBlock, arrayIndex)), string.Format("\"{0}\" is not null, arrayIndex = \"{1}\"", dsVariable, arrayIndex));
        }


        public void VerifyFFIObjectStillInScope(string dsVariable, int startBlock = 1, int arrayIndex = -1)
        {
            string str = GetFFIObjectStringValue(dsVariable, startBlock, arrayIndex);
            Assert.IsFalse(string.IsNullOrEmpty(str), string.Format("\"{0}\" is null, arrayIndex = {1}", dsVariable, arrayIndex));

        }


        public static void AssertValue(string varname, object value, ILiveRunner liveRunner)
        {
            var mirror = liveRunner.InspectNodeValue(varname);
            MirrorData data = mirror.GetData();
            AssertValue(data, value);
        }

        public static void AssertInfinity(string dsVariable, int startBlock = 0)
        {
            RuntimeMirror mirror = new RuntimeMirror(dsVariable, startBlock, testRuntimeCore);
            MirrorData data = mirror.GetData();
            Assert.IsTrue( Double.IsInfinity(Convert.ToDouble(data.Data)));
        }

        public static void AssertNan(string dsVariable, int startBlock = 0)
        {
            RuntimeMirror mirror = new RuntimeMirror(dsVariable, startBlock, testRuntimeCore);
            MirrorData data = mirror.GetData();
            Assert.IsTrue(Double.IsNaN(Convert.ToDouble(data.Data)));
        }

        public static void AssertValue(MirrorData data, object value)
        {
            if (value == null)
            {
                Assert.IsTrue(data.IsNull, "data is null");
            }
            else if (value is int)
            {
                if (data.IsNull)
                    throw new AssertionException("Incorrect verification of null value with int");

                Assert.AreEqual((int)value, Convert.ToInt32(data.Data));
            }
            else if (value is double)
            {
                if (data.IsNull)
                    throw new AssertionException("Incorrect verification of null value with double");

                Assert.AreEqual((double)value, Convert.ToDouble(data.Data), 0.00001);
            }
            else if (data.IsCollection)
            {
                var values = value as IEnumerable;
                if (object.ReferenceEquals(values, null))
                {
                    string errorMessage = string.Format(
                        "The value is {1}, but the expected value is {2}.",
                        data.Data ?? "null",
                        value);
                    throw new AssertionException(errorMessage);
                }
                AssertCollection(data, values);
            }
            else
            {
                Assert.AreEqual(value, data.Data);
            }
        }

        public static Subtree CreateSubTreeFromCode(Guid guid, string code)
        {
            var cbn = ProtoCore.Utils.ParserUtils.Parse(code) as CodeBlockNode;
            var subtree = null == cbn ? new Subtree(null, guid) : new Subtree(cbn.Body, guid);
            return subtree;
        }

        public IList<MethodMirror> GetMethods(string className, string methodName)
        {
            ClassMirror classMirror = new ClassMirror(className, testCore);
            return classMirror.GetOverloads(methodName).ToList();
        }

        private static void AssertCollection(MirrorData data, IEnumerable collection)
        {
            Assert.IsTrue(data.IsCollection);
            List<MirrorData> elements = data.GetElements();
            int i = 0;
            foreach (var item in collection)
            {
                AssertValue(elements[i++], item);
            }
        }

        public void AssertPointer(string dsVariable, int startBlock = 0)
        {
            RuntimeMirror mirror = new RuntimeMirror(dsVariable, startBlock, testRuntimeCore);
            Assert.IsTrue(mirror.GetData().IsPointer);
        }

        public void CleanUp()
        {
            if (testCore != null)
            {
                testRuntimeCore.Cleanup();
                testCore = null;
            }
        }
    }
}
