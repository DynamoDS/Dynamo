using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Dynamo.Commands;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class DynamoRevitTests
    {
        [SetUp]
        public void Init()
        {
            StartRevit();
        }

        [TearDown]
        public void Cleanup()
        {
            EmptyTempFolder();
        }

        private static string TempFolder;
        private static Process revitProcess;

        private static void StartRevit()
        {
            revitProcess = Process.Start(@"C:\Program Files\Autodesk\Revit Architecture 2013\Program\Revit.exe");
        }

        private static void StartDynamo()
        {
            
        }

        public static void EmptyTempFolder()
        {
            var directory = new DirectoryInfo(TempFolder);
            foreach (FileInfo file in directory.GetFiles()) file.Delete();
            foreach (DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }

        [TestFixtureTearDown]
        public void FinalTearDown()
        {

        }

        [Test]
        public void CanAddANodeByName()
        {

        }
    }
}