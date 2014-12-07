using CSharpAnalytics.Sessions;
using System;
using System.Linq;
#if WINDOWS_STORE || WINDOWS_PHONE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Sessions
{
    [TestClass]
    public class VisitorTests
    {
        [TestMethod]
        public void New_Default_Visitor_Has_Random_Id()
        {
            var ids = Enumerable.Range(1, 50).Select(_ => new Visitor().ClientId).ToArray();

            CollectionAssert.AllItemsAreUnique(ids);
        }

        [TestMethod]
        public void New_Visitor_With_Parameters_Sets_Properties()
        {
            var id = Guid.NewGuid();
            
            var visitor = new Visitor(id);

            Assert.AreEqual(id, visitor.ClientId);
        }
    }
}