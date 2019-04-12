using System;
using System.IO;
using System.Linq;
using System.Xml;
using Dynamo.Configuration;
using Dynamo.Core;
using Dynamo.Engine;
using NUnit.Framework;
using ProtoCore;
using TestServices;

namespace Dynamo.Tests
{
    [TestFixture]
    class LibraryTests : UnitTestBase
    {
        private LibraryServices libraryServices;
        private ProtoCore.Core libraryCore;

        protected static bool LibraryLoaded { get; set; }

        public override void Setup()
        {
            base.Setup();

            libraryCore = new ProtoCore.Core(new Options());
            libraryCore.Compilers.Add(Language.Associative, new ProtoAssociative.Compiler(libraryCore));
            libraryCore.Compilers.Add(Language.Imperative, new ProtoImperative.Compiler(libraryCore));
            libraryCore.ParsingMode = ParseMode.AllowNonAssignment;

            var pathResolver = new TestPathResolver();
            pathResolver.AddPreloadLibraryPath("DesignScriptBuiltin.dll");
            pathResolver.AddPreloadLibraryPath("DSCoreNodes.dll");

            var pathManager = new PathManager(new PathManagerParams
            {
                PathResolver = pathResolver
            });

            var settings = new PreferenceSettings();

            libraryServices = new LibraryServices(libraryCore, pathManager, settings);

            RegisterEvents();
        }

        public override void Cleanup()
        {
            UnRegisterEvents();
            libraryServices.Dispose();
            base.Cleanup();
        }

        private void RegisterEvents()
        {
            libraryServices.LibraryLoaded += OnLibraryLoaded;
            libraryServices.LibraryLoadFailed += OnLibraryLoadFailed;
        }

        private void UnRegisterEvents()
        {
            libraryServices.LibraryLoaded -= OnLibraryLoaded;
            libraryServices.LibraryLoadFailed -= OnLibraryLoadFailed;
        }

        public static void OnLibraryLoaded(object sender, EventArgs e)
        {
            LibraryLoaded = true;
        }

        public static void OnLibraryLoadFailed(object sender, EventArgs e)
        {
            LibraryServices.LibraryLoadFailedEventArgs a = e as LibraryServices.LibraryLoadFailedEventArgs;
            if (null != a)
                Assert.Fail("Failed to load library: " + a.LibraryPath);
            else
                Assert.Fail("Failed to load library");
        }

        #region Test cases
        [Test]
        [Category("UnitTests")]
        public void TestLoadNoNamespaceClass()
        {
            LibraryLoaded = false;

            string libraryPath = "FFITarget.dll";

            // All we need to do here is to ensure that the target has been loaded
            // at some point, so if it's already thre, don't try and reload it
            if (!libraryServices.IsLibraryLoaded(libraryPath))
            {
                libraryServices.ImportLibrary(libraryPath);
                Assert.IsTrue(LibraryLoaded);
            }

            // Get function groups for global classes with no namespace
            var functions = libraryServices.GetFunctionGroups(libraryPath)
                                            .Where(x => x.Functions
                                            .Where(y => string.IsNullOrEmpty(y.Namespace)).Any());

            if (functions.Any())
            {
                var ctorfg = functions.Where(x => x.Functions.Where(y => y.Type == FunctionType.Constructor).Any());
                Assert.IsTrue(ctorfg.Any());
                foreach (var fg in ctorfg)
                {
                    foreach (var ctor in fg.Functions)
                        Assert.IsTrue(ctor.ClassName == ctor.UnqualifedClassName);
                }

            }
        }

        [Test]
        [Category("UnitTests")]
        public void TestZeroTouchMigrationNoFileFound()
        {
            LibraryLoaded = false;

            const string libraryPath = "FFITarget.dll";
            string tempLibraryPath = Path.Combine(TempFolder, libraryPath);

            File.Copy(libraryPath, tempLibraryPath);

            libraryServices.ImportLibrary(tempLibraryPath);

            Assert.IsTrue(LibraryLoaded);
        }

        [Test]
        [Category("UnitTests")]
        public void TestZeroTouchMigrationFileBadlyFormatted()
        {
            LibraryLoaded = false;

            string libraryPath = "FFITarget.dll";
            string badXMLPath = "FFITarget.Migrations.xml";

            string badXmlText = "<?xml version=\"1.0\"?>" + System.Environment.NewLine +
                "<migrations>" + System.Environment.NewLine +
                "<priorNameHint>" + System.Environment.NewLine +
                "Because I'm bad, I'm bad, Really, Really Bad." + System.Environment.NewLine +
                "</priorNameHint>" + System.Environment.NewLine +
                "</migrations>" + System.Environment.NewLine;

            string tempLibraryPath = Path.Combine(TempFolder, libraryPath);
            string tempBadXMLPath = Path.Combine(TempFolder, badXMLPath);

            File.Copy(libraryPath, tempLibraryPath);

            System.IO.File.WriteAllText(tempBadXMLPath, badXmlText);

            // The proper behavior is for ImportLibrary to ignore the migrations file if it's badly formatted

            libraryServices.ImportLibrary(tempLibraryPath);

            Assert.IsTrue(LibraryLoaded);
        }

        [Test]
        [Category("UnitTests")]
        public void TestZeroTouchMigrationCannotFindOldMethodName()
        {
            LibraryLoaded = false;

            string libraryPath = "FFITarget.dll";
            string badXMLPath = "FFITarget.Migrations.xml";

            string badXmlText = "<?xml version=\"1.0\"?>" + System.Environment.NewLine +
                "<migrations>" + System.Environment.NewLine +
                "<priorNameHint>" + System.Environment.NewLine +
                "<oldName>I.Am.A.Method.That.Does.Not.Exist</oldName>" + System.Environment.NewLine +
                "<newName>FFITarget.Dummy.TwiceNewName</newName>" + System.Environment.NewLine +
                "</priorNameHint>" + System.Environment.NewLine +
                "</migrations>" + System.Environment.NewLine;

            string tempLibraryPath = Path.Combine(TempFolder, libraryPath);
            string tempBadXMLPath = Path.Combine(TempFolder, badXMLPath);

            File.Copy(libraryPath, tempLibraryPath);

            System.IO.File.WriteAllText(tempBadXMLPath, badXmlText);

            // The proper behavior is for ImportLibrary to ignore the migrations file if it has errors

            libraryServices.ImportLibrary(tempLibraryPath);

            Assert.IsTrue(LibraryLoaded);
        }

        [Test]
        [Category("UnitTests")]
        //This test builds a migration for a zero touch node from DynamoCore
        public void CanReadFileWithZeroTouchMigrationOfFunctionSignatureWithoutParameters()
        {
            
            string libraryPath = "DSCoreNodes.dll";
            string XmlPath = "DSCoreNodes.Migrations.xml";
            string migrations = "<?xml version=\"1.0\"?>" + System.Environment.NewLine +
                "<migrations>" + System.Environment.NewLine +
                "<priorNameHint>" + System.Environment.NewLine +
                "<oldName>DSCore.DateTime.Now</oldName>" + System.Environment.NewLine +
                "<newName>DSCore.DateTime.Never</newName>" + System.Environment.NewLine +
                "</priorNameHint>" + System.Environment.NewLine +
                "</migrations>" + System.Environment.NewLine;


            string xmlstring =@"<Dynamo.Nodes.DSFunction guid=""f05953f3-6ead-44f7-b872-1e0203c784cc""
            type=""Dynamo.Nodes.DSFunction"" nickname=""DateTime.Now"" x=""259.5"" y=""260.5"" 
            isVisible=""true""  lacing=""Shortest""
            isSelectedInput=""False"" assembly=""DSCoreNodes.dll"" function=""DSCore.DateTime.Now"" />";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlstring);
            var xmlElement = doc.DocumentElement;

            string tempXMLPath = Path.Combine(TempFolder, XmlPath);
            string tempLibraryPath = Path.Combine(TempFolder, libraryPath);

            System.IO.File.WriteAllText(tempXMLPath, migrations);
            
            libraryServices.LoadLibraryMigrations(tempLibraryPath);
            Assert.DoesNotThrow(() => { libraryServices.AddAdditionalAttributesToNode("DSCore.DateTime.Now", xmlElement); });
            Assert.DoesNotThrow(() => { libraryServices.AddAdditionalElementsToNode("DSCore.DateTime.Now", xmlElement); });
          
           
        }


        [Test]
        [Category("UnitTests")]
        public void MethodWithRefOutParams_NoLoad()
        {
            LibraryLoaded = false;

            const string libraryPath = "FFITarget.dll";

            // All we need to do here is to ensure that the target has been loaded
            // at some point, so if it's already thre, don't try and reload it
            if (!libraryServices.IsLibraryLoaded(libraryPath))
            {
                libraryServices.ImportLibrary(libraryPath);
                Assert.IsTrue(LibraryLoaded);
            }

            // Get function groups for ClassFunctionality Class
            var functions = libraryServices.GetFunctionGroups(libraryPath)
                                            .SelectMany(x => x.Functions)
                                            .Where(y => y.ClassName.Contains("FFITarget.ClassWithRefParams"));

            Assert.IsTrue(functions.Select(x => x.FunctionName).Contains("ClassWithRefParams"));

            foreach (var function in functions)
            {
                string functionName = function.FunctionName;
                Assert.IsTrue(functionName != "MethodWithRefParameter" && functionName != "MethodWithOutParameter" && functionName != "MethodWithRefOutParameters");
            }
        }

        [Test]
        [Category("UnitTests")]
        public void TestBuiltInFunctionRecognition()
        {
            string name, categoryName;

            name = "/"; categoryName = LibraryServices.Categories.Operators;
            Assert.IsTrue(libraryServices.IsFunctionBuiltIn(categoryName, name));

            name = "*"; categoryName = LibraryServices.Categories.Operators;
            Assert.IsTrue(libraryServices.IsFunctionBuiltIn(categoryName, name));

            name = "=="; categoryName = "";
            Assert.IsFalse(libraryServices.IsFunctionBuiltIn(categoryName, name));

            name = "AllFalse"; categoryName = LibraryServices.Categories.BuiltIn;
            Assert.IsTrue(libraryServices.IsFunctionBuiltIn(categoryName, name));

            name = "SetUnion"; categoryName = LibraryServices.Categories.BuiltIn;
            Assert.IsTrue(libraryServices.IsFunctionBuiltIn(categoryName, name));

            name = "Reorder"; categoryName = "";
            Assert.IsFalse(libraryServices.IsFunctionBuiltIn(categoryName, name));

            name = ""; categoryName = "";
            Assert.IsFalse(libraryServices.IsFunctionBuiltIn(categoryName, name));

            name = ""; categoryName = LibraryServices.Categories.BuiltIn;
            Assert.IsFalse(libraryServices.IsFunctionBuiltIn(categoryName, name));

        }
        
        [Test]
        [Category("UnitTests")]
        public void GetAllFunctionDescriptorsTest()
        {
            const string libraryPath = "FFITarget.dll";
            if (!libraryServices.IsLibraryLoaded(libraryPath))
            {
                libraryServices.ImportLibrary(libraryPath);
                Assert.IsTrue(LibraryLoaded);
            }

            // Get the function descriptors that are named FFITarget.TestOverloadConstructor.TestOverloadConstructor
            var descriptors = libraryServices.GetAllFunctionDescriptors("FFITarget.TestOverloadConstructor.TestOverloadConstructor");
            Assert.AreEqual(4, descriptors.Count());
        }
        #endregion
    }
}
