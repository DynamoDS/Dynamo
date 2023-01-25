using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace CodeGenILTests
{
    public class MicroTests
    {
        private ProtoScript.Runners.LiveRunner liveRunner = null;
        private ProtoCore.MSILRuntimeCore runtimeCore = null;

        protected EmitMSIL.CodeGenIL codeGen;
        protected Dictionary<string, IList> inputs = new Dictionary<string, IList>();
        protected string opCodeFilePath;

        protected virtual void GetLibrariesToPreload(ref List<string> libraries)
        {
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("FFITarget.dll");
        }

        [SetUp]
        public void Setup()
        {
            //TODO_MSIL: remove the dependency on the old VM by implementing
            //necesary Emit functions(ex mitFunctionDefinition and EmitImportStatements and all the preloading logic)
            liveRunner = new ProtoScript.Runners.LiveRunner();

            List<string> libraries = new List<string>();
            GetLibrariesToPreload(ref libraries);

            liveRunner.ResetVMAndResyncGraph(libraries);
            runtimeCore = new ProtoCore.MSILRuntimeCore(liveRunner.RuntimeCore);

            var assemblyPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            var outputpath = Path.Combine(assemblyPath, "MSILTestOutput");
            System.IO.Directory.CreateDirectory(outputpath);
            opCodeFilePath = Path.Combine(outputpath, $"OpCodesTEST{NUnit.Framework.TestContext.CurrentContext.Test.Name}.txt");
            codeGen = new EmitMSIL.CodeGenIL(inputs, opCodeFilePath, runtimeCore);
        }

        [TearDown]
        public void TearDown()
        {
            liveRunner.Dispose();
            codeGen.Dispose();
        }
    }
}
