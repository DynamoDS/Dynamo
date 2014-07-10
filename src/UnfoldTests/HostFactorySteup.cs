using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using System.Threading;

namespace UnfoldTests
{
    [SetUpFixture]
    public class HostFactorySetup
    {
        [SetUp]
        public static void SetUpNamespace()
        {
            Console.WriteLine(" startup");
           
            HostFactory.Instance.StartUp();
        }

        [TearDown]
        public static void TearDownNamespace()
        {
            Console.WriteLine("shutting down");
            
           HostFactory.Instance.ShutDown();
           GC.Collect();
           GC.WaitForPendingFinalizers();
           
        }

    }

}

