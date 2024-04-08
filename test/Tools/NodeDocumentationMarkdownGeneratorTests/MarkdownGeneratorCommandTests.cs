using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NodeDocumentationMarkdownGenerator;
using NodeDocumentationMarkdownGenerator.Commands;
using NodeDocumentationMarkdownGenerator.Verbs;
using NUnit.Framework;

namespace NodeDocumentationMarkdownGeneratorTests
{
    [TestFixture]
    public class MarkdownGeneratorCommandTests
    {
        private const string CORENODEMODELS_DLL_NAME = "CoreNodeModels.dll";
        private const string LibraryViewExtension_DLL_NAME = "LibraryViewExtensionWebView2.dll";
        private static readonly string DynamoCoreDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        private static readonly string DynamoCoreNodesDir = Path.Combine(DynamoCoreDir, "Nodes");
        private static string DynamoRepoRoot = new DirectoryInfo(DynamoCoreDir).Parent.Parent.Parent.FullName;
        private static readonly string NodeGeneratorToolBuildPath = Path.Combine(DynamoRepoRoot, "src","tools", "NodeDocumentationMarkdownGenerator","bin",
    "AnyCPU"
            );
        private static readonly string toolsTestFilesDirectory = Path.GetFullPath(Path.Combine(DynamoRepoRoot, "test","Tools", "docGeneratorTestFiles"));
        private static readonly string testLayoutSpecPath = Path.Combine(toolsTestFilesDirectory, "testlayoutspec.json");
        private static readonly string mockedDictionaryRoot = Path.Combine(toolsTestFilesDirectory, "sampledictionarycontent");
        private static readonly string mockedDictionaryJson = Path.Combine(mockedDictionaryRoot, "Dynamo_Nodes_Documentation.json");

        private static readonly List<string> preloadedLibraryPaths = new List<string>
            {
                "VMDataBridge.dll",
                "ProtoGeometry.dll",
                "DesignScriptBuiltin.dll",
                "DSCoreNodes.dll",
                "DSOffice.dll",
                "DSCPython.dll",
                "FunctionObject.ds",
                "BuiltIn.ds",
                "DynamoConversions.dll",
                "DynamoUnits.dll",
                "Tessellation.dll",
                "Analysis.dll",
                "GeometryColor.dll"
            };

        private DirectoryInfo tempDirectory = null;

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                var libviewExtensionAssem = Assembly.LoadFrom(Path.Combine(DynamoCoreDir, LibraryViewExtension_DLL_NAME));
                SaveCoreLayoutSpecToPath(libviewExtensionAssem, testLayoutSpecPath);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        [OneTimeTearDown]
        public void FixtureTearDown()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            if (File.Exists(testLayoutSpecPath))
            {
                File.Delete(testLayoutSpecPath);
            }
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            //resovle assemblies from the tool's bin folder - nmgt is not copied to dynamo bin.
            var requestedAssembly = new AssemblyName(args.Name);
            var masks = new[] { "*.dll", "*.exe" };
            var files = masks.SelectMany(x=> new DirectoryInfo(NodeGeneratorToolBuildPath).EnumerateFiles(x, SearchOption.AllDirectories));
            var found = files.Where(f => Path.GetFileNameWithoutExtension(f.FullName) == requestedAssembly.Name).FirstOrDefault();
            if (found != null)
            {
                return Assembly.LoadFrom(found.FullName);
            }
            return null;
        }

        [SetUp]
        public void SetUp()
        {
            AppDomain.CurrentDomain.AssemblyResolve += Program.CurrentDomain_AssemblyResolve;
        }

        [TearDown]
        public void CleanUp()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= Program.CurrentDomain_AssemblyResolve;

            if (tempDirectory == null || !tempDirectory.Exists) return;

            tempDirectory.Delete(true);
            tempDirectory = null;
        }

        [Test]
        public void ProducesCorrectOutputFromDirectory()
        {
            // Test output is generated with the following args:
            // 
            // NodeDocumentationMarkdownGenerator.exe
            // fromdirectory
            // -i "..\Dynamo\bin\nodes"
            // -o "..\Dynamo\test\Tools\docGeneratorTestFiles\TestMdOutput_CoreNodeModels"
            // -f "CoreNodeModels.dll"

            // Arrange
            var testOutputDirName = "TestMdOutput_CoreNodeModels";
            var expectedOutputDirectory = new DirectoryInfo(Path.Combine(toolsTestFilesDirectory, testOutputDirName));
            Assert.That(expectedOutputDirectory.Exists);

            var coreNodeModelsDll = Path.Combine(DynamoCoreNodesDir, CORENODEMODELS_DLL_NAME);
            Assert.That(File.Exists(coreNodeModelsDll));

            // Act
            tempDirectory = CreateTempOutputDirectory();
            Assert.That(tempDirectory.Exists);

            var opts = new FromDirectoryOptions
            {
                InputFolderPath = DynamoCoreNodesDir,
                OutputFolderPath = tempDirectory.FullName,
                Filter = new List<string> { CORENODEMODELS_DLL_NAME },
                ReferencePaths = new List<string>()
            };

            FromDirectoryCommand.HandleDocumentationFromDirectory(opts);

            var generatedFileNames = tempDirectory.GetFiles().Select(x => x.Name);

            // Assert file names are correct.
            CollectionAssert.AreEquivalent(expectedOutputDirectory.GetFiles().Select(x => x.Name), generatedFileNames);
        }

        [Test]
        public void ProducesCorrectOutputFromCoreDirectory_preloadedbinaries()
        {
            // Arrange
            var testOutputDirName = "TestMdOutput_CoreNodeModels";
         
            // Act
            tempDirectory = CreateTempOutputDirectory();
            Assert.That(tempDirectory.Exists);

            var opts = new FromDirectoryOptions
            {
                InputFolderPath = DynamoCoreDir,
                RecursiveScan = true,
                OutputFolderPath = tempDirectory.FullName,
                Filter = preloadedLibraryPaths.Concat(new string[] 
                {CORENODEMODELS_DLL_NAME,"GeometryUI.dll","PythonNodeModels.dll","Watch3dNodeModels.dll","UnitsNodeModels.dll","" }),
                ReferencePaths = new List<string>()
            };

            FromDirectoryCommand.HandleDocumentationFromDirectory(opts);

            var generatedFileNames = tempDirectory.GetFiles().Select(x => x.Name);
            Assert.AreEqual(707, generatedFileNames.Count());
        }

        [Test]
        public void ProducesCorrectOutputFromCoreDirectory_dsFiles()
        {
            // Arrange
            var testOutputDirName = "TestMdOutput_CoreNodeModels";

            var expectedFileNames = new List<string>
            {
                "LoopWhile.md", 
                "List.Equals.md", 
                "List.GroupByFunction.md",
                "List.MaximumItemByKey.md",
                "List.MinimumItemByKey.md", 
                "List.Rank.md", 
                "List.RemoveIfNot.md", 
                "List.SortByFunction.md", 
                "List.TrueForAll.md", 
                "List.TrueForAny.md"
            };

            // Act
            tempDirectory = CreateTempOutputDirectory();
            Assert.That(tempDirectory.Exists);

            var opts = new FromDirectoryOptions
            {
                InputFolderPath = DynamoCoreDir,
                OutputFolderPath = tempDirectory.FullName,
                Filter = new List<string> { "FunctionObject.ds",
                "BuiltIn.ds", },
                ReferencePaths = new List<string>()
            };

            FromDirectoryCommand.HandleDocumentationFromDirectory(opts);

            var generatedFileNames = tempDirectory.GetFiles().Select(x => x.Name);
            //assert count is correct.
            Assert.AreEqual(10, generatedFileNames.Count());
            CollectionAssert.AreEquivalent(expectedFileNames, generatedFileNames);

        }


        [Test]
        public void DictionaryContentIsFoundCorrectlyForCoreNodes()
        {
            // Test output is generated with the following args:
            // 
            // NodeDocumentationMarkdownGenerator.exe
            // fromdirectory
            // -i "..\Dynamo\bin\nodes"
            // -o "..\Dynamo\test\Tools\docGeneratorTestFiles\TestMdOutput_CoreNodeModels"
            // -d "..\Dynamo\test\Tools\docGeneratorTestFiles\sampledictionarycontent\Dynamo_Nodes_Documentation.json"
            // -x  "..\Dynamo\test\Tools\docGeneratorTestFiles\testlayoutspec.json"
            // -f "CoreNodeModels.dll"

            // Arrange
            var testOutputDirName = "TestMdOutput_CoreNodeModels";
 
            var coreNodeModelsDll = Path.Combine(DynamoCoreNodesDir, CORENODEMODELS_DLL_NAME);
            Assert.That(File.Exists(coreNodeModelsDll));

            // Act
            tempDirectory = CreateTempOutputDirectory();
            Assert.That(tempDirectory.Exists);

            var opts = new FromDirectoryOptions
            {
                InputFolderPath = DynamoCoreNodesDir,
                OutputFolderPath = tempDirectory.FullName,
                DictionaryDirectory = mockedDictionaryJson,
                LayoutSpecPath = testLayoutSpecPath,
                Filter = new List<string> { CORENODEMODELS_DLL_NAME },
                ReferencePaths = new List<string>(),
                Overwrite = true
            };

            FromDirectoryCommand.HandleDocumentationFromDirectory(opts);

            var generatedFileNames = tempDirectory.GetFiles().Select(x => x.FullName);

            //assert that the generated markdown files all contain an "indepth section" from the dictionary entry, which means
            //they were all found.

            Assert.True(generatedFileNames.Where(x=>Path.GetExtension(x).Contains("md")).All(x => File.ReadAllText(x).ToLower().Contains("in depth")));
          
        }

        [Test]
        public void DictionaryImagesAreCompressed()
        {
           
            // Arrange
            var testOutputDirName = "TestMdOutput_CoreNodeModels";
            var sizesBeforeCompression = new DirectoryInfo(mockedDictionaryRoot).GetFiles("*.*", SearchOption.AllDirectories).Where(
                x => x.Extension.ToLower().Contains("gif") || x.Extension.ToLower().Contains("jpg")).OrderBy(x=>x.FullName).Select(f => File.ReadAllBytes(f.FullName).Length).ToArray();

            var coreNodeModelsDll = Path.Combine(DynamoCoreNodesDir, CORENODEMODELS_DLL_NAME);
            Assert.That(File.Exists(coreNodeModelsDll));

            // Act
            tempDirectory = CreateTempOutputDirectory();
            Assert.That(tempDirectory.Exists);

            var opts = new FromDirectoryOptions
            {
                InputFolderPath = DynamoCoreNodesDir,
                OutputFolderPath = tempDirectory.FullName,
                DictionaryDirectory = mockedDictionaryJson,
                LayoutSpecPath = testLayoutSpecPath,
                Filter = new List<string> { CORENODEMODELS_DLL_NAME },
                ReferencePaths = new List<string>(),
                Overwrite = true,
                CompressGifs = true,
                CompressImages = true,
                Verbose = true
            };

            FromDirectoryCommand.HandleDocumentationFromDirectory(opts);

            var generatedFileImages = tempDirectory.GetFiles().Where(
                x => x.Extension.ToLower().Contains("gif") || x.Extension.ToLower().Contains("jpg")).OrderBy(x => x.FullName);
            var generatedFileSizes = generatedFileImages.Select(x=> File.ReadAllBytes(x.FullName).Length);
            //check all files were larger before in bytes.
            Assert.IsTrue(generatedFileSizes.Select((x, i) => sizesBeforeCompression[i] >= x).All(x=>x));

        }


        [Test]
        public void ProducesCorrectOutputFromPackage()
        {
            // Arrange
            var packageName = "Dynamo Samples";
            var packageDirectory = Path.GetFullPath(Path.Combine(toolsTestFilesDirectory, @"..\..\pkgs", packageName));
            var testOutputDirName = "doc";
            tempDirectory = new DirectoryInfo(Path.Combine(packageDirectory, testOutputDirName));
            Assert.IsFalse(tempDirectory.Exists);

            var expectedFileNames = new List<string>
            {
                "Examples.BasicExample.Awesome.md",
                "Examples.BasicExample.Create(point).md",
                "Examples.BasicExample.Create(x, y, z).md",
                "Examples.BasicExample.MultiReturnExample.md",
                "Examples.BasicExample.MultiReturnExample2.md",
                "Examples.BasicExample.Point.md",
                "Examples.CustomRenderExample.Create.md",
                "Examples.PeriodicIncrement.Increment.md",
                "Examples.PeriodicUpdateExample.PointField.md",
                "Examples.TransformableExample.ByGeometry.md",
                "Examples.TransformableExample.Geometry.md",
                "Examples.TransformableExample.TransformObject.md",
                "SampleLibraryUI.Examples.ButtonCustomNodeModel.md",
                "SampleLibraryUI.Examples.DropDownExample.md",
                "SampleLibraryUI.Examples.LocalizedCustomNodeModel.md",
                "SampleLibraryUI.Examples.SliderCustomNodeModel.md"
            };
            
            // Act
            var opts = new FromPackageOptions
            {
                InputFolderPath = packageDirectory,
                ReferencePaths = new List<string> { toolsTestFilesDirectory }
            };

            FromPackageFolderCommand.HandlePackageDocumentation(opts);

            tempDirectory.Refresh();

            // Assert
            Assert.IsTrue(tempDirectory.Exists);
            CollectionAssert.AreEquivalent(expectedFileNames, tempDirectory.GetFiles().Select(x => x.Name));
        }

        [Test]
        public void ProducesCorrectOutputFromPackageIncludingDYF()
        {
            // Arrange
            var packageName = "EvenOdd";
            var packageDirectory = Path.GetFullPath(Path.Combine(toolsTestFilesDirectory, @"..\..\pkgs", packageName));
            var testOutputDirName = "doc";
            tempDirectory = new DirectoryInfo(Path.Combine(packageDirectory, testOutputDirName));
            Assert.IsFalse(tempDirectory.Exists);

            var expectedFileNames = new List<string>
            {

                "Test.EvenOdd.md"
            };

            // Act
            var opts = new FromPackageOptions
            {
                InputFolderPath = packageDirectory,
                Overwrite = true
            };

            FromPackageFolderCommand.HandlePackageDocumentation(opts);

            tempDirectory.Refresh();

            // Assert
            Assert.IsTrue(tempDirectory.Exists);
            CollectionAssert.AreEquivalent(expectedFileNames, tempDirectory.GetFiles().Select(x => x.Name));
        }


        [Test]
        public void CanOverWriteExistingFiles()
        {
            // Arrange
            var originalOutDirName = "TestMdOutput_CoreNodeModels";
            var originalOutDir = new DirectoryInfo(Path.Combine(toolsTestFilesDirectory, originalOutDirName));

            // Act
            tempDirectory = CreateTempOutputDirectory();
            Assert.That(tempDirectory.Exists);

            CopyFilesRecursively(originalOutDir, tempDirectory);

            var lastWriteTimeBefore = tempDirectory
                .GetFiles()
                .Select(x => x.LastWriteTime)
                .ToList();

            var opts = new FromDirectoryOptions
            {
                InputFolderPath = DynamoCoreNodesDir,
                OutputFolderPath = tempDirectory.FullName,
                Filter = new List<string> { CORENODEMODELS_DLL_NAME },
                ReferencePaths = new List<string>(),
                Overwrite = false
            };

            FromDirectoryCommand.HandleDocumentationFromDirectory(opts);

            tempDirectory.Refresh();
            var lastWriteTimeAfterCommandWithoutOverwrite = tempDirectory
                .GetFiles()
                .Select(x => x.LastWriteTime)
                .ToList();

            opts.Overwrite = true;
            FromDirectoryCommand.HandleDocumentationFromDirectory(opts);

            tempDirectory.Refresh();
            var lastWriteTimeAfterCommandWithOverwrite = tempDirectory
                .GetFiles()
                .Select(x => x.LastWriteTime)
                .ToList();

            // Assert
            Assert.IsTrue(lastWriteTimeAfterCommandWithOverwrite.Count() == lastWriteTimeBefore.Count());

            // Compare last write times on original files
            // and new files without overwrite (-w)
            for (int i = 0; i < lastWriteTimeBefore.Count(); i++)
            {
                // as overwrite has not been specified here
                // it is expected that last write time will
                // be the same for all the files
                Assert.That(DateTime.Compare(lastWriteTimeBefore[i], lastWriteTimeAfterCommandWithoutOverwrite[i]) == 0);
            }

            // Compare last write times on original files
            // and new files with overwrite (-w)
            for (int i = 0; i < lastWriteTimeBefore.Count(); i++)
            {
                // if Compare returns less than 0 first element
                // is earlier then 2nd
                Assert.That(DateTime.Compare(lastWriteTimeBefore[i], lastWriteTimeAfterCommandWithOverwrite[i]) < 0);
            }

            CollectionAssert.AreEquivalent(
                originalOutDir.GetFiles().Select(x => x.Name), 
                tempDirectory.GetFiles().Select(x => x.Name));
        }

        [Test]
        public void CanScanAssemblyFromPath()
        {
            // Arrange
            var assemblyPath = Path.Combine(DynamoCoreNodesDir, CORENODEMODELS_DLL_NAME);
            var coreNodeModelMdFilesDir = new DirectoryInfo(Path.Combine(toolsTestFilesDirectory, "TestMdOutput_CoreNodeModels"));
            var coreNodeModelMdFiles = coreNodeModelMdFilesDir.GetFiles();

            // Act
            var mdFileInfos = AssemblyHandler.ScanAssemblies(new List<string> { assemblyPath });


            // Assert
            Assert.IsTrue(coreNodeModelMdFiles.Count() == mdFileInfos.Count);
            AssertMdFileInfos(mdFileInfos, coreNodeModelMdFiles);
        }
        [Test]
        public void ReferencesFlagAddsReferencePaths()
        {   
            // Arrange
            //using the dynamosamples package as a reference because it's not in the default bin paths.
            var packageName = "Dynamo Samples";
            var packageDirectory = Path.GetFullPath(Path.Combine(toolsTestFilesDirectory, @"..\..\pkgs", packageName)); ;
            var opts = new FromDirectoryOptions
            {
                InputFolderPath = DynamoCoreNodesDir,
                Filter = new string[] { "doesnotexist.dll" },
                ReferencePaths = new List<string> { packageDirectory }
            };


            // Act
            FromDirectoryCommand.HandleDocumentationFromDirectory(opts);


            // Assert
            Assert.IsTrue(Program.ReferenceAssemblyPaths.Select(x => new FileInfo(x).Name).Contains("SampleLibraryUI.dll"));
        }

        [Test]
        public void CanRenameFile()
        {
            // Arrange
            var originalOutDirName = "fallback_docs";
            var originalOutDir = new DirectoryInfo(Path.Combine(toolsTestFilesDirectory, originalOutDirName));

            var targetMdFile = "CoreNodeModels.HigherOrder.Map.md";
            var renamedTargetMdFile = "SVLKFMPW6YIPCHS5TA2H3KJQQTSPUZOGUBWJG3VEPVFVB7DMGFDQ.md";

            tempDirectory = CreateTempOutputDirectory();
            Assert.That(tempDirectory.Exists);

            CopyFilesRecursively(originalOutDir, tempDirectory);
            var mdFile = Path.Combine(tempDirectory.FullName, targetMdFile);
            var renamedMdFile = Path.Combine(tempDirectory.FullName, renamedTargetMdFile);

            // Act
            var opts = new RenameOptions
            {
                InputMdFile = mdFile
            };

            RenameCommand.HandleRename(opts);

            // Assert
            var mdFiles = tempDirectory.GetFiles("*.md", SearchOption.TopDirectoryOnly)
                .Select(x => x.Name);

            var content = File.ReadAllText(renamedMdFile);

            Assert.IsTrue(mdFiles.Contains(renamedTargetMdFile));
            Assert.IsTrue(content.Contains("CoreNodeModels.HigherOrder.Map"));
        }

        [Test]
        public void CanRenameFileLongName()
        {
            // Arrange
            var originalOutDirName = "fallback_docs";
            var filesDirectory = "LongNameFiles";
            var emptySpaceChar = "%20";
            var originalOutDir = new DirectoryInfo(Path.Combine(toolsTestFilesDirectory, originalOutDirName, filesDirectory));

            tempDirectory = CreateTempOutputDirectory();
            Assert.That(tempDirectory.Exists);

            CopyFilesRecursively(originalOutDir, tempDirectory);

            var originalMdFile = tempDirectory.GetFiles("*.md", SearchOption.TopDirectoryOnly)
                .Select(x => x.Name).FirstOrDefault();
            Assert.IsNotNull(originalMdFile);

            //Check that the original MD file contains space characters URL encoded
            var originalMdFileContent = Path.Combine(tempDirectory.FullName, originalMdFile);
            Assert.IsTrue(File.ReadAllText(originalMdFileContent).Contains(emptySpaceChar));

            // Act
            var opts = new RenameOptions
            {
                InputMdDirectory = tempDirectory.FullName,
                MaxLength = 90
            };

            //Rename all the files in the temp directory
            RenameCommand.HandleRename(opts);

            // Assert
            var finalMdFile = tempDirectory.GetFiles("*.md", SearchOption.TopDirectoryOnly)
                .Select(x => x.Name).FirstOrDefault();
            Assert.IsNotNull(finalMdFile);

            var hashedName = Path.GetFileNameWithoutExtension(finalMdFile); 

            //Validates that all the renamed files start with the hashed name
            var allFiles = tempDirectory.GetFiles("*.*", SearchOption.TopDirectoryOnly).Select(x => x.Name);
            foreach(var file in allFiles)
            {
                Assert.IsTrue(file.StartsWith(hashedName));
            }

            //Get the image file name renamed
            var imageFile = tempDirectory.GetFiles("*.jpg", SearchOption.TopDirectoryOnly)
                .Select(x => x.Name).FirstOrDefault();
            Assert.IsNotNull(imageFile);

            //Validates that the image file name is present inside the md file content.
            var finalMdFileContent = Path.Combine(tempDirectory.FullName, finalMdFile);
            Assert.IsTrue(File.ReadAllText(finalMdFileContent).Contains(imageFile));
        }

        [Test]
        public void CanRenameFilesInADirectory()
        {
            // Arrange
            var originalOutDirName = "fallback_docs";
            var originalOutDir = new DirectoryInfo(Path.Combine(toolsTestFilesDirectory, originalOutDirName));

            var expectedFileNames = new List<string>
            {
                "FGRJU5ZIMM4EKNHFEXZGHJTKI73262KTH4CSUBI2IEXVH46TACRA.md",
                "HEG35EENB6LZZUAB4OKNCYCDHDTBEF7IR2YWCH7I4EOIQPFOJGFQ.md",
                "SVLKFMPW6YIPCHS5TA2H3KJQQTSPUZOGUBWJG3VEPVFVB7DMGFDQ.md",
                "list.rank.md",
                "loopwhile.md"
            };

            tempDirectory = CreateTempOutputDirectory();
            Assert.That(tempDirectory.Exists);

            CopyFilesRecursively(originalOutDir, tempDirectory);

            // Act
            var opts = new RenameOptions
            {
                InputMdDirectory = tempDirectory.FullName,
                MaxLength = 15
            };

            RenameCommand.HandleRename(opts);

            // Assert
            CollectionAssert.AreEquivalent(expectedFileNames, tempDirectory.GetFiles().Select(x => x.Name));
        }

        #region Helpers
        internal void AssertMdFileInfos(List<MdFileInfo> mdFileInfos, FileInfo[] coreNodeModelMdFiles)
        {
            var expectedFileNames = coreNodeModelMdFiles.Select(x => Path.GetFileNameWithoutExtension(x.FullName));
            var expectedMdFileInfoNamespace = "CoreNodeModels";
            foreach (var info in mdFileInfos)
            {
                Assert.That(expectedFileNames.Contains(info.FileName));
                Assert.IsTrue(info.NodeNamespace.StartsWith(expectedMdFileInfoNamespace));
            }
        }

        protected DirectoryInfo CreateTempOutputDirectory()
        {
            string tempDirectoryPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_markdownGeneratorTestOutput");
            var tempDir = Directory.CreateDirectory(tempDirectoryPath);
            return tempDir;
        }

        protected static void CopyFilesRecursively(DirectoryInfo originalDir, DirectoryInfo targetDir)
        {
            foreach (var file in originalDir.GetFiles())
            {
                file.CopyTo(Path.Combine(targetDir.FullName, file.Name));
            }
        }

        protected static void SaveCoreLayoutSpecToPath(Assembly assembly, string savePath)
        {
            var resource = "Dynamo.LibraryViewExtensionWebView2.web.library.layoutSpecs.json";
            assembly = assembly == null ? Assembly.GetExecutingAssembly() : assembly;
            var stream = assembly.GetManifestResourceStream(resource);
            var fs = File.Create(savePath);
            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(fs);
            fs.Close();
            stream.Close();
        }
        #endregion
    }
}
