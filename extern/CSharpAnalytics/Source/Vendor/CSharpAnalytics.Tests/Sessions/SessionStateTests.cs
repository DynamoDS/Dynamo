using System;
using System.IO;
using System.Runtime.Serialization;
using CSharpAnalytics.Sessions;
#if WINDOWS_STORE || WINDOWS_PHONE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Sessions
{
    [TestClass]
    public class SessionStateTests
    {
        [TestMethod]
        public void SessionState_Properties_Can_Be_Set()
        {
            var visitorId = Guid.NewGuid();
            var lastActivityAt = new DateTimeOffset(2006, 3, 1, 8, 00, 00, TimeSpan.Zero);
            var referrer = new Uri("http://attackpattern.com");

            var state = new SessionState
            {
                VisitorId = visitorId,
                LastActivityAt = lastActivityAt,
                Referrer = referrer
            };

            Assert.AreEqual(visitorId, state.VisitorId);
            Assert.AreEqual(lastActivityAt, state.LastActivityAt);
            Assert.AreEqual(referrer, state.Referrer);
        }

        [TestMethod]
        public void SessionState_Serialized_And_Deserialized_Correctly()
        {
            var original = CreateSampleState();

            var deserialized = SerializeAndDeserialize(original);

            Assert.AreEqual(original.VisitorId, deserialized.VisitorId);
            Assert.AreEqual(original.LastActivityAt, deserialized.LastActivityAt);            
            Assert.AreEqual(original.Referrer, deserialized.Referrer);
        }

        private static SessionState CreateSampleState()
        {
            return new SessionState
            {
                VisitorId = Guid.NewGuid(),
                LastActivityAt = DateTime.Now.Subtract(new TimeSpan(0, 0, 0, 1)),
                Referrer = new Uri("http://damieng.com")
            };
        }

        private static T SerializeAndDeserialize<T>(T objectToSerialize)
        {
            using (var memoryStream = new MemoryStream())
            {
                var serializer = new DataContractSerializer(typeof (T));
                serializer.WriteObject(memoryStream, objectToSerialize);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return (T) serializer.ReadObject(memoryStream);
            }
        }
    }
}