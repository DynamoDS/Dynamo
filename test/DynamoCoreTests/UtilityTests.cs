﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DynNodes = Dynamo.Nodes;
using System.Xml;
using System.IO;
using Dynamo.Configuration;
using ProtoCore.AST.AssociativeAST;
using DynamoUtilities;

namespace Dynamo.Tests
{
    internal class UtilityTests : DynamoModelTestBase
    {
        [Test]
        [Category("UnitTests")]
        public void PreprocessTypeName00()
        {
            // 'null' as fullyQualifiedName throws an exception.
            Assert.Throws<ArgumentNullException>(() =>
            {
                string qualifiedName = null;
                Graph.Nodes.Utilities.PreprocessTypeName(qualifiedName);
            });
        }

        [Test]
        [Category("UnitTests")]
        public void PreprocessTypeName01()
        {
            // Empty fullyQualifiedName throws an exception.
            Assert.Throws<ArgumentNullException>(() =>
            {
                string qualifiedName = string.Empty;
                Graph.Nodes.Utilities.PreprocessTypeName(qualifiedName);
            });
        }

        [Test]
        [Category("UnitTests")]
        public void PreprocessTypeName02()
        {
            // "Dynamo.Elements." prefix should be replaced.
            string fqn = "Dynamo.Elements.MyClass";
            string result = Graph.Nodes.Utilities.PreprocessTypeName(fqn);
            Assert.AreEqual("Dynamo.Nodes.MyClass", result);
        }

        [Test]
        [Category("UnitTests")]
        public void PreprocessTypeName03()
        {
            // "Dynamo.Nodes." prefix should never be replaced.
            string fqn = "Dynamo.Nodes.MyClass";
            string result = Graph.Nodes.Utilities.PreprocessTypeName(fqn);
            Assert.AreEqual("Dynamo.Nodes.MyClass", result);
        }

        [Test]
        [Category("UnitTests")]
        public void PreprocessTypeName04()
        {
            // System type names should never be modified.
            string fqn = "System.Environment";
            string result = Graph.Nodes.Utilities.PreprocessTypeName(fqn);
            Assert.AreEqual("System.Environment", result);
        }

        [Test]
        [Category("UnitTests")]
        public void PreprocessTypeName05()
        {
            // "Dynamo.Elements.dyn" prefix should be replaced.
            string fqn = "Dynamo.Elements.dynMyClass";
            string result = Graph.Nodes.Utilities.PreprocessTypeName(fqn);
            Assert.AreEqual("Dynamo.Nodes.MyClass", result);
        }

        [Test]
        [Category("UnitTests")]
        public void PreprocessTypeName06()
        {
            // "Dynamo.Nodes.dyn" prefix should be replaced.
            string fqn = "Dynamo.Nodes.dynMyClass";
            string result = Graph.Nodes.Utilities.PreprocessTypeName(fqn);
            Assert.AreEqual("Dynamo.Nodes.MyClass", result);
        }

        [Test]
        [Category("UnitTests")]
        public void PreprocessTypeName07()
        {
            // "Dynamo.Elements.dynXYZ" prefix should be replaced.
            string fqn = "Dynamo.Elements.dynMyXYZClass";
            string result = Graph.Nodes.Utilities.PreprocessTypeName(fqn);
            Assert.AreEqual("Dynamo.Nodes.MyXyzClass", result);
        }

        [Test]
        [Category("UnitTests")]
        public void PreprocessTypeName08()
        {
            // "Dynamo.Nodes.dynUV" prefix should be replaced.
            string fqn = "Dynamo.Nodes.dynMyUVClass";
            string result = Graph.Nodes.Utilities.PreprocessTypeName(fqn);
            Assert.AreEqual("Dynamo.Nodes.MyUvClass", result);
        }

        [Test]
        [Category("UnitTests")]
        public void ResolveType00()
        {
            // 'null' as fullyQualifiedName throws an exception.
            Assert.Throws<ArgumentNullException>(() =>
            {
                string fqn = null;
                Type type;
                CurrentDynamoModel.NodeFactory.ResolveType(fqn, out type);
            });
        }

        [Test]
        [Category("UnitTests")]
        public void ResolveType01()
        {
            string fqn = string.Empty;
            Type type;
            Assert.IsFalse(CurrentDynamoModel.NodeFactory.ResolveType(fqn, out type));
        }

        [Test]
        [Category("UnitTests")]
        public void ResolveType02()
        {
            // Unknown type returns a 'null'.
            string fqn = "Dynamo.Connectors.ConnectorModel";
            Type type;
            Assert.IsFalse(CurrentDynamoModel.NodeFactory.ResolveType(fqn, out type));
            Assert.AreEqual(null, type);
        }

        //[Test]
        //[Category("UnitTests")]
        //public void ResolveType03()
        //{
        //    // Known internal type.
        //    string fqn = "Dynamo.Nodes.Addition";
        //    Type type;
        //    Assert.IsTrue(ViewModel.Model.NodeFactory.ResolveType(fqn, out type));
        //    Assert.IsNotNull(type);
        //    Assert.AreEqual("Dynamo.Nodes.Addition", type.FullName);
        //}

        [Test]
        [Category("UnitTests")]
        public void ResolveType04()
        {
            // System type names should be discoverable.
            string fqn = "System.Environment";
            Type type;
            Assert.IsTrue(CurrentDynamoModel.NodeFactory.ResolveType(fqn, out type));
            Assert.IsNotNull(type);
            Assert.AreEqual("System.Environment", type.FullName);
        }

        //[Test]
        //[Category("UnitTests")]
        //public void ResolveType05()
        //{
        //    // 'NumberRange' class makes use of this attribute.
        //    string fqn = "Dynamo.Nodes.dynBuildSeq";
        //    Type type;
        //    Assert.IsTrue(ViewModel.Model.NodeFactory.ResolveType(fqn, out type));
        //    Assert.IsNotNull(type);
        //    Assert.AreEqual("Dynamo.Nodes.NumberRange", type.FullName);
        //}

        [Test]
        [Category("UnitTests")]
        public void SetDocumentXmlPath00()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // Test method call without a valid XmlDocument.
                Graph.Nodes.Utilities.SetDocumentXmlPath(null, null);
            });
        }

        [Test]
        [Category("UnitTests")]
        public void SetDocumentXmlPath01()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                // Test XmlDocument without any root element.
                XmlDocument document = new XmlDocument();
                Graph.Nodes.Utilities.SetDocumentXmlPath(document, null);
            });
        }

        [Test]
        [Category("UnitTests")]
        public void SetDocumentXmlPath02()
        {
            XmlDocument document = new XmlDocument();
            document.AppendChild(document.CreateElement("RootElement"));

            var path = Path.Combine(Path.GetTempPath(), "SomeFile.dyn");
            Graph.Nodes.Utilities.SetDocumentXmlPath(document, path);

            var storedPath = Graph.Nodes.Utilities.GetDocumentXmlPath(document);
            Assert.AreEqual(path, storedPath); // Ensure attribute has been added.
        }

        [Test]
        [Category("UnitTests")]
        public void SetDocumentXmlPath03()
        {
            XmlDocument document = new XmlDocument();
            document.AppendChild(document.CreateElement("RootElement"));

            var path = Path.Combine(Path.GetTempPath(), "SomeFile.dyn");
            Graph.Nodes.Utilities.SetDocumentXmlPath(document, path);

            var storedPath = Graph.Nodes.Utilities.GetDocumentXmlPath(document);
            Assert.AreEqual(path, storedPath); // Ensure attribute has been added.

            // Test target file path removal through an empty string.
            Graph.Nodes.Utilities.SetDocumentXmlPath(document, string.Empty);

            Assert.Throws<InvalidOperationException>(() =>
            {
                Graph.Nodes.Utilities.GetDocumentXmlPath(document);
            });
        }

        [Test]
        [Category("UnitTests")]
        public void SetDocumentXmlPath04()
        {
            XmlDocument document = new XmlDocument();
            document.AppendChild(document.CreateElement("RootElement"));

            var path = Path.Combine(Path.GetTempPath(), "SomeFile.dyn");
            Graph.Nodes.Utilities.SetDocumentXmlPath(document, path);

            var storedPath = Graph.Nodes.Utilities.GetDocumentXmlPath(document);
            Assert.AreEqual(path, storedPath); // Ensure attribute has been added.

            // Test target file path removal through a null value.
            Graph.Nodes.Utilities.SetDocumentXmlPath(document, null);

            Assert.Throws<InvalidOperationException>(() =>
            {
                Graph.Nodes.Utilities.GetDocumentXmlPath(document);
            });
        }

        [Test]
        [Category("UnitTests")]
        public void GetDocumentXmlPath00()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // Test method call without a valid XmlDocument.
                Graph.Nodes.Utilities.GetDocumentXmlPath(null);
            });
        }

        [Test]
        [Category("UnitTests")]
        public void GetDocumentXmlPath01()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                // Test XmlDocument without any root element.
                XmlDocument document = new XmlDocument();
                Graph.Nodes.Utilities.GetDocumentXmlPath(document);
            });
        }

        [Test]
        [Category("UnitTests")]
        public void GetDocumentXmlPath02()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                // Test XmlDocument root element without path.
                XmlDocument document = new XmlDocument();
                document.AppendChild(document.CreateElement("RootElement"));
                Graph.Nodes.Utilities.GetDocumentXmlPath(document);
            });
        }

        [Test]
        [Category("UnitTests")]
        public void SaveTraceDataToXmlDocument00()
        {
            XmlDocument document = new XmlDocument();
            var data = new Dictionary<Guid, List<string>>();

            Assert.Throws<ArgumentNullException>(() =>
            {
                // Test XmlDocument being null.
                Graph.Nodes.Utilities.SaveTraceDataToXmlDocument(null, data);
            });

            Assert.Throws<ArgumentException>(() =>
            {
                // Test valid XmlDocument without document element.
                Graph.Nodes.Utilities.SaveTraceDataToXmlDocument(document, data);
            });

            document.AppendChild(document.CreateElement("RootElement"));

            Assert.Throws<ArgumentNullException>(() =>
            {
                // Test Dictionary being null.
                Graph.Nodes.Utilities.SaveTraceDataToXmlDocument(document, null);
            });

            Assert.Throws<ArgumentException>(() =>
            {
                // Test valid Dictionary without any entry.
                Graph.Nodes.Utilities.SaveTraceDataToXmlDocument(document, data);
            });
        }

        [Test]
        [Category("UnitTests")]
        public void SaveTraceDataToXmlDocument01()
        {
            // Create a valid XmlDocument object.
            XmlDocument document = new XmlDocument();
            document.AppendChild(document.CreateElement("RootElement"));

            var nodeGuid0 = Guid.NewGuid();
            var nodeGuid1 = Guid.NewGuid();

            var nodeData0 = new List<string>()
            {
                "TraceData00", "TraceData01", "TraceData02"
            };

            var nodeData1 = new List<string>()
            {
                "TraceData10", "TraceData11", "TraceData12"
            };

            // Create sample data.
            var data = new Dictionary<Guid, List<string>>();
            data.Add(nodeGuid0, nodeData0);
            data.Add(nodeGuid1, nodeData1);

            IEnumerable<KeyValuePair<Guid, List<string>>> outputs = null;

            Assert.DoesNotThrow(() =>
            {
                Graph.Nodes.Utilities.SaveTraceDataToXmlDocument(document, data);
                outputs = Graph.Nodes.Utilities.LoadTraceDataFromXmlDocument(document);
            });

            Assert.NotNull(outputs);
            Assert.AreEqual(2, outputs.Count());
            Assert.AreEqual(nodeGuid0, outputs.ElementAt(0).Key);
            Assert.AreEqual(nodeGuid1, outputs.ElementAt(1).Key);

            var outputData0 = outputs.ElementAt(0).Value;
            var outputData1 = outputs.ElementAt(1).Value;

            Assert.IsNotNull(outputData0);
            Assert.IsNotNull(outputData1);
            Assert.AreEqual(3, outputData0.Count);
            Assert.AreEqual(3, outputData1.Count);

            Assert.AreEqual("TraceData00", outputData0[0]);
            Assert.AreEqual("TraceData01", outputData0[1]);
            Assert.AreEqual("TraceData02", outputData0[2]);

            Assert.AreEqual("TraceData10", outputData1[0]);
            Assert.AreEqual("TraceData11", outputData1[1]);
            Assert.AreEqual("TraceData12", outputData1[2]);
        }

        [Test]
        [Category("UnitTests")]
        public void LoadTraceDataFromXmlDocument00()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // Test method call without a valid XmlDocument.
                Graph.Nodes.Utilities.LoadTraceDataFromXmlDocument(null);
            });

            Assert.Throws<ArgumentException>(() =>
            {
                // Test XmlDocument without a document element.
                XmlDocument document = new XmlDocument();
                Graph.Nodes.Utilities.LoadTraceDataFromXmlDocument(document);
            });
        }

        [Test]
        [Category("UnitTests")]
        public void LoadTraceDataFromXmlDocument01()
        {
            IEnumerable<KeyValuePair<Guid, List<string>>> outputs = null;

            Assert.DoesNotThrow(() =>
            {
                XmlDocument document = new XmlDocument();
                document.AppendChild(document.CreateElement("RootElement"));
                outputs = Graph.Nodes.Utilities.LoadTraceDataFromXmlDocument(document);
            });

            Assert.IsNotNull(outputs);
            Assert.AreEqual(0, outputs.Count());
        }

        [Test]
        [Category("UnitTests")]
        public void MakeRelativePath00()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // Test method call without a valid base path.
                Graph.Nodes.Utilities.MakeRelativePath(null, Path.GetTempPath());
            });
        }

        [Test]
        [Category("UnitTests")]
        public void MakeRelativePath01()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // Test method call without an empty base path.
                Graph.Nodes.Utilities.MakeRelativePath("", Path.GetTempPath());
            });
        }

        [Test]
        [Category("UnitTests")]
        public void MakeRelativePath02()
        {
            var basePath = Path.Combine(Path.GetTempPath(), "home.dyn");
            var result = Graph.Nodes.Utilities.MakeRelativePath(basePath, null);
            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        [Category("UnitTests")]
        public void MakeRelativePath03()
        {
            var basePath = Path.Combine(Path.GetTempPath(), "home.dyn");
            var result = Graph.Nodes.Utilities.MakeRelativePath(basePath, "");
            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        [Category("UnitTests")]
        public void MakeRelativePath04()
        {
            var justFileName = "JustSingleFileName.dll";
            var basePath = Path.Combine(Path.GetTempPath(), "home.dyn");
            var result = Graph.Nodes.Utilities.MakeRelativePath(basePath, justFileName);
            Assert.AreEqual(justFileName, result);
        }

        [Test]
        [Category("UnitTests")]
        public void MakeRelativePath05()
        {
            var tempPath = Path.GetTempPath();
            var basePath = Path.Combine(tempPath, "home.dyn");

            var filePath = Path.Combine(new string[]
            {
                tempPath, "This", "Is", "Sub", "Directory", "MyLibrary.dll"
            });

            var result = Graph.Nodes.Utilities.MakeRelativePath(basePath, filePath);
            Assert.AreEqual(@"This\Is\Sub\Directory\MyLibrary.dll", result);
        }

        [Test]
        [Category("UnitTests")]
        public void MakeRelativePath06()
        {
            var tempPath = Path.GetTempPath();
            var basePath = Path.Combine(tempPath, "home.dyn");

            var absolutePath1 = Path.Combine(tempPath, "dummy.dll");
            var relativePath = Graph.Nodes.Utilities.MakeRelativePath(basePath, absolutePath1);
            var absolutePath2 = Graph.Nodes.Utilities.MakeAbsolutePath(basePath, relativePath);
            Assert.AreEqual(absolutePath1, absolutePath2);
        }

        [Test]
        [Category("UnitTests")]
        public void MakeAbsolutePath00()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                Graph.Nodes.Utilities.MakeAbsolutePath(null, "Dummy");
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                Graph.Nodes.Utilities.MakeAbsolutePath(string.Empty, "Dummy");
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                Graph.Nodes.Utilities.MakeAbsolutePath("Dummy", null);
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                Graph.Nodes.Utilities.MakeAbsolutePath("Dummy", string.Empty);
            });
        }

        [Test]
        [Category("UnitTests")]
        public void MakeAbsolutePath01()
        {
            var validPath = Path.Combine(Path.GetTempPath(), "TempFile.dyn");

            Assert.Throws<UriFormatException>(() =>
            {
                Graph.Nodes.Utilities.MakeAbsolutePath("Test", validPath);
            });

            Assert.DoesNotThrow(() =>
            {
                // "Test" is a completely valid relative path string.
                Graph.Nodes.Utilities.MakeAbsolutePath(validPath, "Test");
            });
        }

        [Test]
        [Category("UnitTests")]
        public void MakeAbsolutePath02()
        {
            var basePath = Path.GetTempPath();
            var relativePath = @"This\Is\Sub\Directory\MyLibrary.dll";
            var result = Graph.Nodes.Utilities.MakeAbsolutePath(basePath, relativePath);

            var expected = Path.Combine(new string[]
            {
                basePath, "This", "Is", "Sub", "Directory", "MyLibrary.dll"
            });

            Assert.AreEqual(expected, result);
        }

        [Test]
        [Category("UnitTests")]
        public void MakeAbsolutePath03()
        {
            var basePath = Path.GetTempPath();
            var relativePath = @"MyLibrary.dll";

            // "result" should be the same as "relativePath" because it is 
            // just a file name without directory information, therefore it 
            // will not be modified to prefix with a directory.
            // 
            var result = Graph.Nodes.Utilities.MakeAbsolutePath(basePath, relativePath);
            Assert.AreEqual(relativePath, result);
        }

        [Test]
        [Category("UnitTests")]
        public void MakeAbsolutePath04()
        {
            var basePath = @"C:\This\Is\Sub\Directory\Home.dyn";
            var relativePath = @"..\..\Another\Sub\Directory\MyLibrary.dll";
            var result = Graph.Nodes.Utilities.MakeAbsolutePath(basePath, relativePath);

            var expected = Path.Combine(new string[]
            {
                "C:\\", "This", "Is", "Another", "Sub", "Directory", "MyLibrary.dll"
            });

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void WrapTextTest()
        {
            string testingSTR = string.Empty;
            IEnumerable<string> result;
            //1. When original is empty string
            //2. When original is null
            //3. When original is whitespace
            //4. When original is AaaBbbbCDE
            //5. When original is SurfaceAnalysisData
            //6. When original is BoundingBox
            //7. When original is ImportFromCSV
            //8. When original is ImportFromCSV
            //9. When original is ImportFromCSV

            // case 1
            result = Graph.Nodes.Utilities.WrapText("", Configurations.MaxLengthRowClassButtonTitle);
            Assert.AreEqual(new List<string>() { }, result);

            // case 2
            result = Graph.Nodes.Utilities.WrapText(null, Configurations.MaxLengthRowClassButtonTitle);
            Assert.AreEqual(new List<string>() { }, result);

            // case 3
            result = Graph.Nodes.Utilities.WrapText("    ", Configurations.MaxLengthRowClassButtonTitle);
            Assert.AreEqual(new List<string>() { }, result);

            // case 4
            result = Graph.Nodes.Utilities.WrapText("AaaBbbbCDE", 3);
            Assert.AreEqual(new List<string>() { "Aaa", "Bbbb", "CDE" }, result);

            // case 5
            result = Graph.Nodes.Utilities.WrapText("SurfaceAnalysisData", 9);
            Assert.AreEqual(new List<string>() { "Surface", "Analysis", "Data" }, result);

            // case 6
            result = Graph.Nodes.Utilities.WrapText("BoundingBox", 9);
            Assert.AreEqual(new List<string>() { "Bounding", "Box" }, result);

            // case 7
            result = Graph.Nodes.Utilities.WrapText("ImportFromCSV", 4);
            Assert.AreEqual(new List<string>() { "Import", "From", "CSV" }, result);

            // case 8
            result = Graph.Nodes.Utilities.WrapText("ImportFromCSV", 6);
            Assert.AreEqual(new List<string>() { "Import", "From", "CSV" }, result);

            // case 9
            result = Graph.Nodes.Utilities.WrapText("ImportFromCSV", 11);
            Assert.AreEqual(new List<string>() { "Import From", "CSV" }, result);
        }

        [Test]
        public void ReduceRowCountTest()
        {
            IEnumerable<string> result;
            List<string> original;
            //1. When original is null, 0, 0
            //2. When original is ("Aaa", "Bbbb", "CDE"), maxRows = 1
            //3. When original is ("Aaa", "Bbbb", "CDE"), maxRows = 2
            //4. When original is ("Aaa", "Bbbb", "CDE"), maxRows = 3
            //5. When original is ("Day", "Of", "Week"), maxRows = 2
            //6. When original is ("Import", "From", "CSV"), maxRows = 2

            // case 1
            Assert.Throws<ArgumentException>(() =>
            {
                Graph.Nodes.Utilities.ReduceRowCount(null, 0);
            });

            // case 2
            original = new List<string>() { "Aaa", "Bbbb", "CDE" };
            result = Graph.Nodes.Utilities.ReduceRowCount(original, 1);
            Assert.AreEqual(new List<string>() { "Aaa Bbbb CDE" }, result);

            // case 3
            original = new List<string>() { "Aaa", "Bbbb", "CDE" };
            result = Graph.Nodes.Utilities.ReduceRowCount(original, 2);
            Assert.AreEqual(new List<string>() { "Aaa", "Bbbb CDE" }, result);

            // case 4
            original = new List<string>() { "Aaa", "Bbbb", "CDE" };
            result = Graph.Nodes.Utilities.ReduceRowCount(original, 3);
            Assert.AreEqual(new List<string>() { "Aaa", "Bbbb", "CDE" }, result);

            // case 5
            original = new List<string>() { "Day", "Of", "Week" };
            result = Graph.Nodes.Utilities.ReduceRowCount(original, 2);
            Assert.AreEqual(new List<string>() { "Day", "Of Week" }, result);

            // case 6
            original = new List<string>() { "Import", "From", "CSV" };
            result = Graph.Nodes.Utilities.ReduceRowCount(original, 2);
            Assert.AreEqual(new List<string>() { "Import", "From CSV" }, result);
        }

        [Test]
        public void TruncateRowsTest()
        {
            IEnumerable<string> result;
            IEnumerable<string> original;
            //1. When original is null, 0
            //2. When original is ("Aaa", "Bbbb"), maxCharacters = 3
            //3. When original is ("Day", "Of", "Week"), maxCharacters = 9
            //4. When original is ("Surface", "Analysis Data"), maxCharacters = 9
            //5. When original is ("Coordinate", "System"), maxCharacters = 9
            //6. When original is ("Rectangle"), maxCharacters = 9
            //7. When original is ("By", "Geometry", "Coordinate", "System"), maxCharacters = 9
            //8. When original is ("By Geometry", "Coordinate System"), maxCharacters = 9
            //9. When original is ("Application"), maxCharacters = 9

            // case 1
            Assert.Throws<ArgumentException>(() =>
            {
                Graph.Nodes.Utilities.TruncateRows(null, 0);
            });

            // case 2
            original = new List<string>() { "Aaa", "Bbbb" };
            result = Graph.Nodes.Utilities.TruncateRows(original, 3);
            Assert.AreEqual(new List<string>() { "Aaa", "..b" }, result);

            // case 3
            original = new List<string>() { "Day", "Of", "Week" };
            result = Graph.Nodes.Utilities.TruncateRows(original, 9);
            Assert.AreEqual(new List<string>() { "Day", "Of", "Week" }, result);

            // case 4
            original = new List<string>() { "Surface", "Analysis Data" };
            result = Graph.Nodes.Utilities.TruncateRows(original, 8);
            Assert.AreEqual(new List<string>() { "Surface", "..s Data" }, result);

            // case 5
            original = new List<string>() { "Coordinate", "System" };
            result = Graph.Nodes.Utilities.TruncateRows(original, 9);
            Assert.AreEqual(new List<string>() { "Coordin..", "System" }, result);

            // case 6
            original = new List<string>() { "Rectangle" };
            result = Graph.Nodes.Utilities.TruncateRows(original, 9);
            Assert.AreEqual(new List<string>() { "Rectangle" }, result);

            // case 7
            original = new List<string>() { "By", "Geometry", "Coordinate", "System" };
            result = Graph.Nodes.Utilities.TruncateRows(original, 9);
            Assert.AreEqual(new List<string>() { "By", "Geometry", "Coordin..", "System" }, result);

            // case 8
            original = new List<string>() { "By Geometry", "Coordinate System" };
            result = Graph.Nodes.Utilities.TruncateRows(original, 9);
            Assert.AreEqual(new List<string>() { "By Geom..", ".. System" }, result);

            // case 9
            original = new List<string>() { "Application" };
            result = Graph.Nodes.Utilities.TruncateRows(original, 9);
            Assert.AreEqual(new List<string>() { "Applica.." }, result);
        }

        [Test]
        public void NormalizeAsResourceNameTest()
        {
            string testingSTR = string.Empty;
            //1. When resource is empty string
            //2. When resource is null
            //3. When resource is whitespaces (\n, \t or space)
            //4. When resource is %Aaa2Bb**CDE
            //5. When resource is Ab/b.double-int

            // case 1
            testingSTR = Graph.Nodes.Utilities.NormalizeAsResourceName("");
            Assert.AreEqual("", testingSTR);

            // case 2
            testingSTR = Graph.Nodes.Utilities.NormalizeAsResourceName(null);
            Assert.AreEqual("", testingSTR);

            // case 3
            testingSTR = Graph.Nodes.Utilities.NormalizeAsResourceName("   ");
            Assert.AreEqual("", testingSTR);

            //case 4
            testingSTR = Graph.Nodes.Utilities.NormalizeAsResourceName("%Aaa2Bb**CDE");
            Assert.AreEqual("Aaa2BbCDE", testingSTR);

            // case 5
            testingSTR = Graph.Nodes.Utilities.NormalizeAsResourceName("Ab/b.double-int");
            Assert.AreEqual("Abb.double-int", testingSTR);
        }
		
        [Category("UnitTests")]
        public void TestTypeSwitch()
        {
            object v = null;
            object node = null;

            DoubleNode doubleNode = new DoubleNode(1.2);
            node = doubleNode;
            TypeSwitch.Do(
                node,
                TypeSwitch.Case<IntNode>(n => v = n.Value),
                TypeSwitch.Case<DoubleNode>(n => v = n.Value),
                TypeSwitch.Case<BooleanNode>(n => v = n.Value),
                TypeSwitch.Case<StringNode>(n => v = n.value),
                TypeSwitch.Default(() => v = null));
            Assert.AreEqual(v, 1.2);

            IntNode intNode = new IntNode(42);
            node = intNode;
            TypeSwitch.Do(
                node,
                TypeSwitch.Case<IntNode>(n => v = n.Value),
                TypeSwitch.Case<DoubleNode>(n => v = n.Value),
                TypeSwitch.Case<BooleanNode>(n => v = n.Value),
                TypeSwitch.Case<StringNode>(n => v = n.value),
                TypeSwitch.Default(() => v = null));
            Assert.AreEqual(v, 42);

            StringNode sNode = new StringNode(); 
            node = sNode;
            TypeSwitch.Do(
                node,
                TypeSwitch.Case<IntNode>(n => v = n.Value),
                TypeSwitch.Case<DoubleNode>(n => v = n.Value),
                TypeSwitch.Case<BooleanNode>(n => v = n.Value),
                TypeSwitch.Default(() => v = 24));
            Assert.AreEqual(v, 24);
        }
    }
}
