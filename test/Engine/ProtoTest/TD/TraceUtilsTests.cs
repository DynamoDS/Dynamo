using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoCore.Lang;

namespace ProtoTest.TD
{
    public class TraceUtilsTests
    {
        [Test]
        [Category("Trace")]
        public static void SetGetTest()
        {

            List<String> keys = TraceUtils.TEMP_GetTraceKeys();

            String testStr1 = "{0955D962-2936-4FB2-AAB3-635C6FF6E0AD}";
            String testStr2 = "{2D7FE0ED-56F3-47A4-9BAA-8DF570170D97}";


             Dictionary<String,Object> data = new Dictionary<string, object>();
            data.Add(keys[0], testStr1);

            TraceUtils.SetObjectToTLS(data);

            //Set complete, readback test

            Dictionary<String, Object> readback = TraceUtils.GetObjectFromTLS();
            Assert.IsTrue((String)readback[keys[0]] == testStr1);
        }


        [Test]
        [Category("Trace")]
        public static void OverwriteTest()
        {

            List<String> keys = TraceUtils.TEMP_GetTraceKeys();

            String testStr1 = "{0955D962-2936-4FB2-AAB3-635C6FF6E0AD}";
            String testStr2 = "{2D7FE0ED-56F3-47A4-9BAA-8DF570170D97}";


            Dictionary<String, Object> data = new Dictionary<string, object>();
            data.Add(keys[0], testStr1);

            Dictionary<String, Object> data2 = new Dictionary<string, object>();
            data2.Add(keys[0], testStr2);


            TraceUtils.SetObjectToTLS(data);

            //Set complete, readback test

            Dictionary<String, Object> readback = TraceUtils.GetObjectFromTLS();
            Assert.IsTrue((String)readback[keys[0]] == testStr1);



            TraceUtils.SetObjectToTLS(data2);

            //Set complete, readback test

            readback = TraceUtils.GetObjectFromTLS();
            Assert.IsTrue((String)readback[keys[0]] == testStr2);
        }


        [Test]
        [Category("Trace")]
        public static void OverwriteNullTest()
        {

            List<String> keys = TraceUtils.TEMP_GetTraceKeys();

            String testStr1 = "{0955D962-2936-4FB2-AAB3-635C6FF6E0AD}";


            Dictionary<String, Object> data = new Dictionary<string, object>();
            data.Add(keys[0], testStr1);


            TraceUtils.SetObjectToTLS(data);

            //Set complete, readback test

            Dictionary<String, Object> readback = TraceUtils.GetObjectFromTLS();
            Assert.IsTrue((String)readback[keys[0]] == testStr1);


            data[keys[0]] = null;

            TraceUtils.SetObjectToTLS(data);

            //Set complete, readback test

            readback = TraceUtils.GetObjectFromTLS();
            Assert.IsTrue((String)readback[keys[0]] == null);
        }




    }
}
