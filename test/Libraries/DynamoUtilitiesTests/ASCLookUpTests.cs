using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamoInstallDetective;
using Microsoft.Win32;
using Moq;

using NUnit.Framework;

namespace DynamoUtilitiesTests
{
    public class ASCLookUpTests
    {
        [Test, Category("ASCLookUp"), Category("UnitTests")]
        public void GetASCInstallDirectoryWorking()
        {
            var working = new AscSdkWrapper("WorkingMajorVersion");
            working.BaseKey = Registry.CurrentUser;

            string installDirectory = string.Empty;

            var ret = working.GetInstalledPath(ref installDirectory);

            Assert.AreEqual(ret, AscSdkWrapper.ASC_STATUS.SUCCESS);
            Assert.IsNotEmpty(installDirectory);
        }

        [Test, Category("ASCLookUp"), Category("UnitTests")]
        public void GetASCInstallDirectoryNonWorking()
        {
            var working = new AscSdkWrapper("nonWorkingMajorVersion");
            working.BaseKey = Registry.CurrentUser;

            string installDirectory = string.Empty;

            var ret = working.GetInstalledPath(ref installDirectory);

            Assert.AreEqual(ret, AscSdkWrapper.ASC_STATUS.INCORRECT_REG_PATH);
            Assert.IsNotEmpty(installDirectory);
        }

        [Test, Category("ASCLookUp"), Category("UnitTests")]
        public void GetASCInstallDirectoryBadVersion()
        {
            var working = new AscSdkWrapper("badMajorVersion");
            working.BaseKey = Registry.CurrentUser;

            string installDirectory = string.Empty;

            var ret = working.GetInstalledPath(ref installDirectory);

            Assert.AreEqual(ret, AscSdkWrapper.ASC_STATUS.REG_FAILED);
            Assert.IsNotEmpty(installDirectory);
        }

        [Test, Category("ASCLookUp"), Category("UnitTests")]
        public void GetASCInstallDirectoryEmptyVersion()
        {
            var working = new AscSdkWrapper("");
            working.BaseKey = Registry.CurrentUser;

            string installDirectory = string.Empty;

            var ret = working.GetInstalledPath(ref installDirectory);

            Assert.AreEqual(ret, AscSdkWrapper.ASC_STATUS.INITIALIZE_FAILED);
            Assert.IsEmpty(installDirectory);
        }
    }
}
