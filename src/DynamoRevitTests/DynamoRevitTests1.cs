using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Dynamo.Commands;
using Dynamo.Nodes;
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
            Console.WriteLine("Init tests");
            // StartDynamo();
        }

        [TearDown]
        public void Cleanup()
        {
            Console.WriteLine("Cleanup tests");
            //try
            //{
            //    dynSettings.Writer.Close();
            //    EmptyTempFolder();
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.StackTrace);
            //}
        }

        private static string TempFolder;

        private static void StartDynamo()
        {
            try
            {

                string tempPath = Path.GetTempPath();
                var random = new Random();
                string logPath = Path.Combine(tempPath, "dynamoLog" + random.Next() + ".txt");

                TempFolder = Path.Combine(tempPath, "dynamoTmp");

                if (!Directory.Exists(TempFolder))
                {
                    Directory.CreateDirectory(TempFolder);
                }
                else
                {
                    EmptyTempFolder();
                }

                TextWriter tw = new StreamWriter(logPath);
                tw.WriteLine("Dynamo log started " + DateTime.Now.ToString());
                dynSettings.Writer = tw;

                //create a new instance of the ViewModel
                var controller = new DynamoController(new FSchemeInterop.ExecutionEnvironment());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        public static void EmptyTempFolder()
        {
            try
            {
                var directory = new DirectoryInfo(TempFolder);
                foreach (FileInfo file in directory.GetFiles()) file.Delete();
                foreach (DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        // OpenCommand

        [Test]
        public void CanOpenGoodFile()
        {
            Assert.AreEqual(5, 5);
        }

        [Test]
        public void CanOpenGoodFile2()
        {
            Assert.AreEqual(5, 5);
            Assert.AreEqual(5, 1);
        }

    }
}