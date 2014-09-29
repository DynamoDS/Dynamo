using CSharpAnalytics.Activities;
#if WINDOWS_STORE || WINDOWS_PHONE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Activities
{
    [TestClass]
    public class TransactionActivityTests
    {
        [TestMethod]
        public void ItemActivity_Constructor_With_Minimal_Parameters_Sets_Correct_Properties()
        {
            var activity = new TransactionItemActivity("code", "name", 1.23m, 1);

            Assert.AreEqual("code", activity.Code);
            Assert.AreEqual("name", activity.Name);
            Assert.AreEqual(1.23m, activity.Price);
            Assert.AreEqual(1, activity.Quantity);
            Assert.IsNull(activity.Variation);
        }

        [TestMethod]
        public void ItemActivity_Constructor_With_All_Parameters_Sets_Correct_Properties()
        {
            var activity = new TransactionItemActivity("code", "name", 1.23m, 4, "variation");

            Assert.AreEqual("code", activity.Code);
            Assert.AreEqual("name", activity.Name);
            Assert.AreEqual(1.23m, activity.Price);
            Assert.AreEqual(4, activity.Quantity);
            Assert.AreEqual("variation", activity.Variation);
        }
    }
}