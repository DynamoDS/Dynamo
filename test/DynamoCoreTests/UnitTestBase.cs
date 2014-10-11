﻿using System;
using System.IO;
using System.Reflection;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo
{
    public class UnitTestBase
    {

        protected string ExecutingDirectory { get; set; }
        protected string TempFolder { get; private set; }

        [SetUp]
        public virtual void Init()
        {
            SetupDirectories();
        }

        [TearDown]
        public virtual void Cleanup()
        {
            try
            {
                var directory = new DirectoryInfo(TempFolder);
                directory.Delete(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        public string GetNewFileNameOnTempPath(string fileExtension = "dyn")
        {
            return Path.Combine(TempFolder, Guid.NewGuid().ToString() + "." + fileExtension);
        }

        public string GetTestDirectory()
        {
            var directory = new DirectoryInfo(ExecutingDirectory);
            return Path.Combine(directory.Parent.Parent.Parent.FullName, "test");
        }

        public string GetSampleDirectory()
        {
            var directory = new FileInfo(ExecutingDirectory);
            string assemblyDir = directory.DirectoryName;
            string sampleLocation = Path.Combine(assemblyDir, @"..\..\doc\distrib\Samples\");
            string samplePath = Path.GetFullPath(sampleLocation);

            return samplePath;

        }

        protected void SetupDirectories()
        {
            ExecutingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string tempPath = Path.GetTempPath();

            TempFolder = Path.Combine(tempPath, "dynamoTmp\\" + Guid.NewGuid().ToString("N"));

            if (!Directory.Exists(TempFolder))
                Directory.CreateDirectory(TempFolder);
        }
    }
}
