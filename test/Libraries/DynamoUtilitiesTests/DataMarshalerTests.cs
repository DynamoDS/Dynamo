using Dynamo.Utilities;
using NUnit.Framework;

namespace DynamoUtilitiesTests
{
    public class DataMarshalerTests
    {
        [Test]
        [Category("UnitTests")]
        public static void MarshalTest()
        {
            var marshaler = new DataMarshaler();
            marshaler.RegisterMarshaler((double d) => "Double: " + d);
            marshaler.RegisterMarshaler((int i) => "Int: " + i);
            marshaler.RegisterMarshaler((object o) => "Other: " + o);

            Assert.AreEqual("Double: 1", marshaler.Marshal(1.0));
            Assert.AreEqual("Int: 1", marshaler.Marshal(1));
            Assert.AreEqual("Other: hello", marshaler.Marshal("hello"));
        }

        private class Super { }
        private class Sub : Super { }
        private class Sub2 : Sub { }
        private class Sub3 : Sub2 { }

        [Test]
        [Category("UnitTests")]
        public static void MarshalSubclass()
        {
            var marshaler = new DataMarshaler();
            marshaler.RegisterMarshaler((Super s) => "Super");
            marshaler.RegisterMarshaler((Sub2 s2) => "Sub2");
            marshaler.RegisterMarshaler((object o) => "Other");

            Assert.AreEqual("Super", marshaler.Marshal(new Super()));
            Assert.AreEqual("Super", marshaler.Marshal(new Sub()));
            Assert.AreEqual("Sub2", marshaler.Marshal(new Sub2()));
            Assert.AreEqual("Sub2", marshaler.Marshal(new Sub3()));
            Assert.AreEqual("Other", marshaler.Marshal(1));
        }

        [Test]
        [Category("UnitTests")]
        public static void MarshalEnumerables()
        {
            var marshaler = new DataMarshaler();
            marshaler.RegisterMarshaler((string s) => s.Length);

            Assert.AreEqual(new[] { 0, 1, 2 }, marshaler.Marshal(new[] { "", " ", "  " }));
            Assert.AreEqual(new[] { 0, 1, 2, 3, 4 }, marshaler.Marshal(new object[] { "", 1, 2, "   ", 4 }));
        }
    }
}
