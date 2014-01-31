using System;
using Autodesk.DesignScript.Interfaces;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using Autodesk.DesignScript.Runtime;
namespace ProtoTest
{
    [TestFixture]
    public class FFITests
    {
        public ProtoCore.Core core;
        [SetUp]
        public void Setup()
        {
            Console.WriteLine("Setup");
            core = new ProtoCore.Core(new ProtoCore.Options());

            core.Configurations.Add(ConfigurationKeys.GeometryFactory, "DSGeometry.dll");
            core.Configurations.Add(ConfigurationKeys.PersistentManager, "DSGeometry.dll");
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
            //DLLFFIHandler.Env = ProtoFFI.CPPModuleHelper.GetEnv(); 
            //DLLFFIHandler.Register(FFILanguage.CPlusPlus, new ProtoFFI.PInvokeModuleHelper()); 
        }
        [Test, Ignore]
        public static void TestMinFac()
        {
            Assert.Ignore("Testing old C++ FFI. Ignored");
            String code =
            @"[Associative]              {                external (""factorial"") def factorial : int (num4 : int);                a = factorial(4);              }            ";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ProtoCore.Core core = new ProtoCore.Core(new ProtoCore.Options());
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
            ExecutionMirror mirror = fsr.Execute(code, core);
            Obj o = mirror.GetValue("a");
            Assert.IsTrue((Int64)o.Payload == 24);
        }
        public void TestScriptFile(string filename)
        {
            ProtoCore.Core core = new ProtoCore.Core(new ProtoCore.Options());
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            fsr.LoadAndExecute(filename, core);
        }
        public void TestScript(string scriptCode)
        {
            ProtoCore.Core core = new ProtoCore.Core(new ProtoCore.Options());
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            fsr.Execute(scriptCode, core);
        }
    }
}
