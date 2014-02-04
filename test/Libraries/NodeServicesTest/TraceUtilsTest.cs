using System;
using System.Runtime.Serialization;
using System.Threading;
using NUnit.Framework;

namespace DSNodeServicesTest
{
    [TestFixture]
    class TraceUtilsTest
    {
        [Test]
        public void TestGetSetData()
        {
            string id = "TestID-{82AC4E65-CC86-4BF0-95EA-AE4B2B5E4A35}";
            string testString = "This is a test";

            SerializableSring ssString = new SerializableSring(testString);

            DSNodeServices.TraceUtils.SetTraceData(id, ssString);

            ssString = null;

            SerializableSring ret = (SerializableSring)DSNodeServices.TraceUtils.GetTraceData(id);

            Assert.IsTrue(ret.Payload.Equals(testString));

        }

        [Test]
        public void TestGetSetDataOnDifferentThreads()
        {
            string id = "TestID-{82AC4E65-CC86-4BF0-95EA-AE4B2B5E4A35}";
            string testString = "This is a test";

            SerializableSring ssString = new SerializableSring(testString);

            DSNodeServices.TraceUtils.SetTraceData(id, ssString);

            ssString = null;

            bool test = false;

            Thread th = new Thread(
                () =>
                {
                    SerializableSring ret = (SerializableSring)DSNodeServices.TraceUtils.GetTraceData(id);

                    test = ret == null;
                        
                }
                );
            th.Start();
            th.Join();

            Assert.IsTrue(test);


            
        }


        private class SerializableSring : ISerializable
        {
            public String Payload;

            public SerializableSring(String payload)
            {
                this.Payload = payload;
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                throw new NotImplementedException();
            }

            // override object.Equals
            public override bool Equals(object obj)
            {
                //       
                // See the full list of guidelines at
                //   http://go.microsoft.com/fwlink/?LinkID=85237  
                // and also the guidance for operator== at
                //   http://go.microsoft.com/fwlink/?LinkId=85238
                //

                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }

                return Payload.Equals(((SerializableSring)obj).Payload);
                
            }

            public override int GetHashCode()
            {
                return Payload.GetHashCode();
            }

        }

    }
}
