using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DynamoUtilitiesTests
{
    [TestFixture]
    public class CLIWrapperTests
    {
        /// <summary>
        /// A test class that always responds with startToken... causing a loop.
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
                return GetData(2000, () => { return startofDataToken; });
            }

            protected override void StartProcess(string relativeEXEPath, string argString)
            {
                //don't start anything, we're going to mock data responses.
                return;
            }

            protected override bool CheckIfProcessHasExited()
            {
                return false;
            }


        }
        /// <summary>
        /// this test class waits before reading from the console, so GetData is slow.
        /// </summary>
        private class SlowCLIWrapper : HangingCLIWrapper
        {
            internal new string GetData()
            {
                return GetData(2000, () => { Thread.Sleep(4000);return ""; });
            }
        }

        /// <summary>
        /// this test class should get mock data and should not time out.
        /// </summary>
        private class MockCLIWraper : HangingCLIWrapper
        {
            int count = 0;
            internal new string GetData()
            {
                return GetData(2000, () => {
                    count++;

                    switch (count)
                    {
                        case 1:
                            return startofDataToken;
                        case 2:
                            return "some data";
                        case 3:
                            return endOfDataToken;
                        default:
                            return string.Empty;
                    }
                    
                });
            }
        }

        [Test]
        public void CLIWrapperDoesNotHangIfProcessDoesNotWriteFullSequenceToStdOut()
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            var wrapper = new HangingCLIWrapper();
            Assert.AreEqual(string.Empty,wrapper.GetData());
            sw.Stop();
            Assert.GreaterOrEqual(sw.ElapsedMilliseconds,2000);
            wrapper.Dispose();

        }
        [Test]
        public void CLIWrapperTimesOutIfGetDataIsSlow()
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            var wrapper = new SlowCLIWrapper();
            Assert.AreEqual(string.Empty, wrapper.GetData());
            sw.Stop();
            Assert.GreaterOrEqual(sw.ElapsedMilliseconds, 2000);
            wrapper.Dispose();

        }
        [Test]
        public void CLIGetsDataIfDoesNotTimeout()
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            var wrapper = new MockCLIWraper();
            Assert.AreEqual("some data", wrapper.GetData().TrimEnd());
            sw.Stop();
            Assert.LessOrEqual(sw.ElapsedMilliseconds, 2000);
            wrapper.Dispose();

        }
    }
}
