using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using NUnit.Framework;

namespace Dynamo.PackageManager
{
    public static class PackageTests
    {
        [Test]
        public static void ParseNodeLibraryNames_CorrectlyParsesEmptyOrNullString()
        {
            Assert.AreEqual(null, Package.ParseNodeLibraryNames(null));
            Assert.AreEqual(null, Package.ParseNodeLibraryNames(""));
        }

        [Test]
        public static void ParseNodeLibraryNames_CorrectlyParsesStringWithoutDeclaration()
        {
            Assert.AreEqual(null, Package.ParseNodeLibraryNames("containsSomeStuff"));
        }

        [Test]
        public static void ParseNodeLibraryNames_CorrectlyParsesStringWithKeyButNoNames()
        {
            var names = Package.ParseNodeLibraryNames(Package.NodeLibraryKey + "{}");
            Assert.AreEqual(0, names.Length);
        }

        [Test]
        public static void ParseNodeLibraryNames_CorrectlyParsesStringWithKeyAndSingleName()
        {
            var names = Package.ParseNodeLibraryNames(Package.NodeLibraryKey + "{mylib, Version=1.2.1900.0, Culture=neutral, PublicKeyToken=a14f3033def15840}");
            Assert.AreEqual(1, names.Length);
        }

        [Test]
        public static void ParseNodeLibraryNames_CorrectlyParsesStringWithKeyAndMultipleNames()
        {
            var names = Package.ParseNodeLibraryNames(Package.NodeLibraryKey + "{mylib1, Version=1.2.1900.0, Culture=neutral, PublicKeyToken=a14f3033def15840;"+
                "mylib2, Version=1.2.1900.0, Culture=neutral, PublicKeyToken=a14f3033def15840}");
            Assert.AreEqual(2, names.Length);
        }

        [Test]
        public static void SerializeNodeLibraryNames_CorrectlySerializesEmptyEnumerable()
        {
            Assert.Throws<ArgumentNullException>(() => Package.SerializeNodeLibraryNames(null));
            Assert.AreEqual(Package.NodeLibraryKey + "{}", Package.SerializeNodeLibraryNames(new List<Assembly>()));
        }

        [Test]
        public static void SerializeNodeLibraryNames_CorrectlySerializesEnumerableWithOneElement()
        {
            var assem = Assembly.GetExecutingAssembly();
            var res = Package.SerializeNodeLibraryNames(new[] { assem });

            Assert.AreEqual(res, Package.NodeLibraryKey + "{" + assem.FullName + "}");
        }

        [Test]
        public static void SerializeNodeLibraryNames_CorrectlySerializesEnumerableWithMultipleElements()
        {
            var sAssem = typeof(String).Assembly;
            var tAssem = typeof(TestAttribute).Assembly;
            var res = Package.SerializeNodeLibraryNames(new[] { sAssem, tAssem });

            Assert.AreEqual(res, Package.NodeLibraryKey + "{" + sAssem.FullName + ";" + tAssem.FullName + "}");
        }

        [Test]
        public static void CanSerializeAndParseItself()
        {
            var sAssem = typeof(String).Assembly;
            var tAssem = typeof(TestAttribute).Assembly;

            var res = Package.SerializeNodeLibraryNames(new[] { sAssem, tAssem });
            var res1 = Package.ParseNodeLibraryNames(res);

            Assert.AreEqual(2, res1.Length);

            Assert.AreEqual(sAssem.FullName, res1[0]);
            Assert.AreEqual(tAssem.FullName, res1[1]);
        }
    }
}
