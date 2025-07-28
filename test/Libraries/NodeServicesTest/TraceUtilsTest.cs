using System;
using System.Runtime.Serialization;
using System.Threading;

using DynamoServices;

using NUnit.Framework;

namespace DynamoServicesTests
{
    [TestFixture]
    class TraceUtilsTest
    {
        [Test]
        [Category("UnitTests")]
        public void TestGetSetData()
        {
            string id = "TestID-{82AC4E65-CC86-4BF0-95EA-AE4B2B5E4A35}";
            string testString = "This is a test";

            TraceUtils.SetTraceData(id, testString);

            var ret = TraceUtils.GetTraceData(id);

            Assert.AreEqual(ret, testString);

        }

        [Test]
        [Category("UnitTests")]
        public void TestGetSetDataOnDifferentThreads()
        {
            string id = "TestID-{82AC4E65-CC86-4BF0-95EA-AE4B2B5E4A35}";
            string testString = "This is a test";

            TraceUtils.SetTraceData(id, testString);

            bool test = false;

            Thread th = new Thread(
                () =>
                {
                    var ret = TraceUtils.GetTraceData(id);

                    test = ret == null;
                        
                }
                );
            th.Start();
            th.Join();

            Assert.IsTrue(test);


            
        }

    }
}
