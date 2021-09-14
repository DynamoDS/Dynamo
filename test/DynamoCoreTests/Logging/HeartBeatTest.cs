using Dynamo.Graph.Nodes;
using Dynamo.Logging;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Dynamo.Tests.Loggings
{
    [TestFixture]
    class HeartBeatTest : DynamoModelTestBase
    {
        /// <summary>
        /// This test method will execute using Reflection the private Heartbeat.GetVersionString method
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void test_heart_beat_version_string()
        {
            //Arrange
            var heartbeat = Heartbeat.GetInstance(CurrentDynamoModel);

            //Act
            //Using reflection we execute the GetVersionString method and store the result
            MethodInfo dynMethod = typeof(Heartbeat).GetMethod("GetVersionString", BindingFlags.NonPublic | BindingFlags.Instance);
            var fileVersionString = (string)dynMethod.Invoke(heartbeat, null);

            //Assert
            //Check that the string response has something and has the right length
            Assert.IsFalse(string.IsNullOrEmpty(fileVersionString));
            var versionParts = fileVersionString.Split('.');
            Assert.Greater(versionParts.Length, 0);
            foreach(var strPart in versionParts)
            {
                Assert.IsTrue(Regex.IsMatch(strPart, @"^\d+$"));
            }
            Heartbeat.DestroyInstance();
        }

        /// <summary>
        /// This test method set the model = null to execute the ExecThread() method and send an exception
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void test_heart_beat_execthread_exception()
        {
            //Arrange/Act
            //The ExecThread() function is executed in a different Thread 
            //Then If we pass the parameter DynamoModel dynModel as null it will reach the Exception section in ExecThread()
            var heartbeat = Heartbeat.GetInstance(null);
            

            //Using reflection we execute the ValidateLength method with null in the second parameter
            MethodInfo dynMethod = typeof(Heartbeat).GetMethod("GetVersionString", BindingFlags.NonPublic | BindingFlags.Instance);
            var fileVersionString = (string)dynMethod.Invoke(heartbeat, null);

            //Assert
            Assert.IsFalse(string.IsNullOrEmpty(fileVersionString));
            var versionParts = fileVersionString.Split('.');
            Assert.Greater(versionParts.Length, 0);

            Heartbeat.DestroyInstance();
        }

        /// <summary>
        /// This test method will assure the the final section of the Heartbeat.ComputeErrorFrequencies() method
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void test_heart_beat_compute_error_freq()
        {
            //Arrange
            //This file has a code block with a error
            string openPath = Path.Combine(TestDirectory,
                @"core\dsevaluation\Test_PortErrorBehavior_CodeBlockErrorsInFile.dyn");
            OpenModel(openPath);

            //We get the code block that has a error state
           var cbn = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<CodeBlockNodeModel>(
                Guid.Parse("dad587d1-acee-445c-890d-98500b408ec6"));

            //Act
            //Inside the Heartbeat constructor is called the method but inside a Thread
            var heartbeat = Heartbeat.GetInstance(CurrentDynamoModel);

            //Assert
            //We just make some validations about the codeblock state and the heartbeat instance
            Assert.AreEqual(ElementState.Error, cbn.State);
            Assert.IsNotNull(heartbeat);

            Heartbeat.DestroyInstance();
        }

    }
}
