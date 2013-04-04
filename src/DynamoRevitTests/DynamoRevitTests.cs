using System;
using System.Collections.Generic;
using System.IO;
using Dynamo.Commands;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class DynamoElementsTests
    {
        [SetUp]
        public void Init()
        {
            StartDynamo();
        }

        [TearDown]
        public void Cleanup()
        {
            EmptyTempFolder();
        }

        private static string TempFolder;

        private static void StartRevit()
        {

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