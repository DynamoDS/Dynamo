using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RevitInstallDetective
{
    [TestFixture]
    class TestDetective
    {
        [Test]
        public void CheckForInstallation_CanFindVasariBeta3()
        {
            // only works on a machine with vasari beta 3!
            Assert.True(Detective.InstallationExists("VasariBeta3"));
        }

        [Test]
        public void CheckForInstallation_CantFindVasariBeta2()
        {
            // non-existent version
            Assert.False(Detective.InstallationExists("VasariBeta2"));
        }

        [Test]
        public void CheckForInstallation_CantFindRevit2012()
        {
            // cannot have revit 2012 installed
            Assert.False(Detective.InstallationExists("Revit2012"));
        }

        [Test]
        public void CheckForInstallation_CanFindRevit2015()
        {
            // must install revit 2015
            Assert.True(Detective.InstallationExists("Revit2015"));
        }

        [Test]
        public void CheckForInstallation_CanFindRevit2014()
        {
            // must install revit 2014
            Assert.True(Detective.InstallationExists("Revit2014"));
        }

        [Test]
        public void CheckForInstallation_CanFindRevit2013()
        {
            // must install revit 2013
            Assert.True(Detective.InstallationExists("Revit2013"));
        }
    }
}
