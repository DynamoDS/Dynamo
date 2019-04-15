using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using NUnit.Framework;
using ProtoCore.Lang;

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


            List<String> keys = TraceUtils.TEMP_GetTraceKeys();

            SerializableString testStr1 = new 
                SerializableString("{0955D962-2936-4FB2-AAB3-635C6FF6E0AD}");

            SerializableString testStr2 = new 
                SerializableString("{2D7FE0ED-56F3-47A4-9BAA-8DF570170D97}");


            Dictionary<String, ISerializable> data = new Dictionary<string, ISerializable>();
            data.Add(keys[0], testStr1);

            TraceUtils.SetObjectToTLS(data);

            //Set complete, readback test

            Dictionary<String, ISerializable> readback = TraceUtils.GetObjectFromTLS();
            Assert.IsTrue(((SerializableString)readback[keys[0]]).Payload == testStr1.Payload);
        }


        [Test]
        [Category("Trace")]
        public static void OverwriteTest()
        {

            List<String> keys = TraceUtils.TEMP_GetTraceKeys();

            SerializableString testStr1 = new
                SerializableString("{0955D962-2936-4FB2-AAB3-635C6FF6E0AD}");

            SerializableString testStr2 = new
                SerializableString("{2D7FE0ED-56F3-47A4-9BAA-8DF570170D97}");


            var data = new Dictionary<string, ISerializable>();
            data.Add(keys[0], testStr1);

            var data2 = new Dictionary<string, ISerializable>();
            data2.Add(keys[0], testStr2);


            TraceUtils.SetObjectToTLS(data);

            //Set complete, readback test

            var readback = TraceUtils.GetObjectFromTLS();
            Assert.IsTrue(((SerializableString)readback[keys[0]]).Payload == testStr1.Payload);



            TraceUtils.SetObjectToTLS(data2);

            //Set complete, readback test

            readback = TraceUtils.GetObjectFromTLS();
            Assert.IsTrue(((SerializableString)readback[keys[0]]).Payload == testStr2.Payload);
        }


        [Test]
        [Category("Trace")]
        public static void OverwriteNullTest()
        {

            List<String> keys = TraceUtils.TEMP_GetTraceKeys();

            SerializableString testStr1 = new
                SerializableString("{0955D962-2936-4FB2-AAB3-635C6FF6E0AD}");


            Dictionary<String, ISerializable> data = new Dictionary<string, ISerializable>();
            data.Add(keys[0], testStr1);


            TraceUtils.SetObjectToTLS(data);

            //Set complete, readback test

            var readback = TraceUtils.GetObjectFromTLS();
            Assert.IsTrue(((SerializableString)readback[keys[0]]).Payload == testStr1.Payload);


            data[keys[0]] = null;

            TraceUtils.SetObjectToTLS(data);

            //Set complete, readback test

            readback = TraceUtils.GetObjectFromTLS();
            Assert.IsTrue((ISerializable)readback[keys[0]] == null);
        }




    }
}
