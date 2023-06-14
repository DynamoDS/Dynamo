using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dynamo.Logging;
using NUnit.Framework;

namespace Dynamo.PackageManager.Tests
{
    class PackageTests
    {
        private readonly AssemblyName assemName =
            new AssemblyName("Neatamo, Version=2013.3.6.0, Culture=neutral, PublicKeyToken=null");

        [Test]
        public void IsNodeLibrary_IsTrueForNullArg1()
        {
            IList<ILogMessage> ws = new List<ILogMessage>();
            Assert.True(Package.IsNodeLibrary(null, assemName, ref ws));
            Assert.IsEmpty(ws);
        }

        [Test]
        public void IsNodeLibrary_ThrowsForNullArg2()
        {
            IList<ILogMessage> ws = new List<ILogMessage>();
            Assert.Throws<ArgumentNullException>(() => Package.IsNodeLibrary(null, null, ref ws));
        }

        [Test]
        public void IsNodeLibrary_IsTrueForExactMatch()
        {
            IList<ILogMessage> ws = new List<ILogMessage>();
            Assert.True(Package.IsNodeLibrary(new List<string>
            {
                "Neatamo, Version=2013.3.6.0, Culture=neutral, PublicKeyToken=null"
            }, assemName, ref ws));
        }

        [Test]
        public void IsNodeLibrary_IsTrueForInexactMatch()
        {
            IList<ILogMessage> ws = new List<ILogMessage>();
            Assert.True(Package.IsNodeLibrary(new List<string>
            {
                "Neatamo, Version=0.0.0.1, Culture=neutral, PublicKeyToken=null"
            }, assemName, ref ws));
        }

        [Test]
        public void IsNodeLibrary_IsFalseForNoMatch()
        {
            IList<ILogMessage> ws = new List<ILogMessage>();
            Assert.False(Package.IsNodeLibrary(new List<string>
            {
                "CoolName, Version=0.0.0.1, Culture=neutral, PublicKeyToken=null"
            }, assemName, ref ws));
        }

        [Test]
        public void IsNodeLibrary_IsFalseAndHasMessagesForBadlyFormatted()
        {
            IList<ILogMessage> ws = new List<ILogMessage>();
            Assert.False(Package.IsNodeLibrary(new List<string>
            {
                "\\ x x x x x x"
            }, assemName, ref ws));
            Assert.AreEqual(2, ws.Count());
        }
    }
}
