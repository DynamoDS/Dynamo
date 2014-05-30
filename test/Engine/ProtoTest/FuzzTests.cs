using NUnit.Framework;
namespace ProtoTest
{
    [TestFixture]
    public class FuzzTests
    {
        ProtoCore.Core core;
        [SetUp]
        public void Setup()
        {

        }

        [Test, TestCaseSource("LoadFiles")]
        public void TestFile(string filename)
        {
            core = new ProtoCore.Core(new ProtoCore.Options());
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            fsr.LoadAndExecute(filename, core);
        }
        public static string[] LoadFiles()
        {
            string path = "..\\..\\..\\Scripts\\fuzz";
            return System.IO.Directory.GetFiles(path, "*.ds");
        }
    }
}
