using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DynamoUtilitiesTests
{
    [TestFixture]
    public class CLIWrapperTests
    {
        /// <summary>
        /// A test class that starts up the DynamoFF CLI and then kills it to cause a deadlock.
        /// </summary>
        private class HangingCLIWrapper: Dynamo.Utilities.CLIWrapper
        {
            private string relativePath = Path.Combine("DynamoFeatureFlags", "DynamoFeatureFlags.exe");
            protected override string GetCantStartErrorMessage()
            {
                throw new NotImplementedException();
            }

            protected override string GetCantCommunicateErrorMessage()
            {
                throw new NotImplementedException();
            }
            internal HangingCLIWrapper()
            {
                StartProcess(relativePath, null);
            }

            internal string GetData()
            {
                //wait a bit, and then kill the process
                //this will cause GetData to hang and timeout.
                Task.Run(() =>
                {   System.Threading.Thread.Sleep(100);
                    process.Kill();
                });
                return GetData(2000);
            }
        }

        [Test]
        public void CLIWrapperDoesNotHangIfProcessDoesNotWriteToStdOut()
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            var wrapper = new HangingCLIWrapper();
            Assert.AreEqual(string.Empty,wrapper.GetData());
            sw.Stop();
            Assert.GreaterOrEqual(sw.ElapsedMilliseconds,2000);

        }
    }
}
