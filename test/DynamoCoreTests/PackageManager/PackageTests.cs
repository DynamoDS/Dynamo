using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace Dynamo.PackageManager
{
    
    class PackageTests
    {
        private readonly AssemblyName assemName =
            new AssemblyName("Rhynamo, Version=2013.3.6.0, Culture=neutral, PublicKeyToken=null");

        [Test]
        public void IsNodeLibrary_IsTrueForNullArg1()
        {
            Assert.True(Package.IsNodeLibrary(null, assemName));
        }

        [Test]
        public void IsNodeLibrary_ThrowsForNullArg2()
        {
            Assert.Throws<ArgumentNullException>(() => Package.IsNodeLibrary(null, null));
        }

        [Test]
        public void IsNodeLibrary_IsTrueForExactMatch()
        {
            Assert.True(Package.IsNodeLibrary(new List<string>
            {
                "Rhynamo, Version=2013.3.6.0, Culture=neutral, PublicKeyToken=null"
            }, assemName));
        }

        [Test]
        public void IsNodeLibrary_IsTrueForInexactMatch()
        {
            Assert.True(Package.IsNodeLibrary(new List<string>
            {
                "Rhynamo, Version=0.0.0.1, Culture=neutral, PublicKeyToken=null"
            }, assemName));
        }

        [Test]
        public void IsNodeLibrary_IsFalseForNoMatch()
        {
            Assert.False(Package.IsNodeLibrary(new List<string>
            {
                "CoolName, Version=0.0.0.1, Culture=neutral, PublicKeyToken=null"
            }, assemName));
        }
    }
}
