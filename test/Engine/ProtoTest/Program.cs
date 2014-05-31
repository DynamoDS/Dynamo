using NUnit.Framework;
namespace ProtoTest
{
    public class Program
    {
        static void Main(string[] args)
        {
            //EntryClass.Exec4();
        }
        [TestFixture]
        public class TestsFromScripts
        {
            [Test, TestCaseSource("LoadFiles")]
            public void TestFile(string filename)
            {
                System.Console.WriteLine(filename);
            }
            public static string[] LoadFiles()
            {
                string path = "..\\..\\..\\Scripts";
                return System.IO.Directory.GetFiles(path, "*.ds");
            }
        }



    }
}
