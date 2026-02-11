using DynamoServices;
using NUnit.Framework;
using System;
using System.Runtime.Serialization;

namespace ProtoTest.TD
{
    public class TraceUtilsTests
    {
        [Test]
        [Category("Trace")]
        public static void SetGetTest()
        {


            var key = TraceUtils.TEMP_GetTraceKeys()[0];

            var testStr1 = "{0955D962-2936-4FB2-AAB3-635C6FF6E0AD}";

            TraceUtils.SetTraceData(key, testStr1);

            //Set complete, readback test

            string readback = TraceUtils.GetTraceData(key);
            Assert.IsTrue(readback == testStr1);
        }


        [Test]
        [Category("Trace")]
        public static void OverwriteTest()
        {

            var key = TraceUtils.TEMP_GetTraceKeys()[0];

            var testStr1 = "{0955D962-2936-4FB2-AAB3-635C6FF6E0AD}";

            var testStr2 = "{2D7FE0ED-56F3-47A4-9BAA-8DF570170D97}";

            TraceUtils.SetTraceData(key, testStr1);

            //Set complete, readback test

            string readback = TraceUtils.GetTraceData(key);
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

            var testStr1 = "{0955D962-2936-4FB2-AAB3-635C6FF6E0AD}";

            TraceUtils.SetTraceData(key, testStr1);

            //Set complete, readback test

            string readback = TraceUtils.GetTraceData(key);
            Assert.IsTrue(readback == testStr1);

            TraceUtils.SetTraceData(key, null);

            //Set complete, readback test

            readback = TraceUtils.GetTraceData(key);
            Assert.IsTrue(readback == null);
        }




    }
}
