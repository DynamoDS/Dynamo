using DynamoServices;
using NUnit.Framework;
using System;
using System.Runtime.Serialization;

namespace ProtoTest.TD
{
    internal class SerializableString : ISerializable
    {
        public String Payload { get; set; }

        public SerializableString(String str)
        {
            this.Payload = str;

        }

        public SerializableString(SerializationInfo info, StreamingContext context)
        {
            Payload = info.GetString("Payload");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Payload", Payload);
        }
    }


    public class TraceUtilsTests
    {
        [Test]
        [Category("Trace")]
        public static void SetGetTest()
        {


            var key = TraceUtils.TEMP_GetTraceKeys()[0];

            SerializableString testStr1 = new 
                SerializableString("{0955D962-2936-4FB2-AAB3-635C6FF6E0AD}");

            SerializableString testStr2 = new 
                SerializableString("{2D7FE0ED-56F3-47A4-9BAA-8DF570170D97}");

            TraceUtils.SetTraceData(key, testStr1);

            //Set complete, readback test

            ISerializable readback = TraceUtils.GetTraceData(key);
            Assert.IsTrue(readback == testStr1);
        }


        [Test]
        [Category("Trace")]
        public static void OverwriteTest()
        {

            var key = TraceUtils.TEMP_GetTraceKeys()[0];

            SerializableString testStr1 = new
                SerializableString("{0955D962-2936-4FB2-AAB3-635C6FF6E0AD}");

            SerializableString testStr2 = new
                SerializableString("{2D7FE0ED-56F3-47A4-9BAA-8DF570170D97}");

            TraceUtils.SetTraceData(key, testStr1);

            //Set complete, readback test

            ISerializable readback = TraceUtils.GetTraceData(key);
            Assert.IsTrue(readback == testStr1);



            TraceUtils.SetTraceData(key, testStr2);

            //Set complete, readback test

            readback = TraceUtils.GetTraceData(key);
            Assert.IsTrue(readback == testStr2);
        }


        [Test]
        [Category("Trace")]
        public static void OverwriteNullTest()
        {

            var key = TraceUtils.TEMP_GetTraceKeys()[0];

            SerializableString testStr1 = new
                SerializableString("{0955D962-2936-4FB2-AAB3-635C6FF6E0AD}");

            TraceUtils.SetTraceData(key, testStr1);

            //Set complete, readback test

            ISerializable readback = TraceUtils.GetTraceData(key);
            Assert.IsTrue(readback == testStr1);

            TraceUtils.SetTraceData(key, null);

            //Set complete, readback test

            readback = TraceUtils.GetTraceData(key);
            Assert.IsTrue(readback == null);
        }




    }
}
