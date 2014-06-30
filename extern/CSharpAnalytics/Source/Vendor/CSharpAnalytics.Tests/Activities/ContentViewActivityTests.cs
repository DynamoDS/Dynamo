using System;
using CSharpAnalytics.Activities;
#if WINDOWS_STORE || WINDOWS_PHONE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Activities
{
    [TestClass]
    public class ContentViewActivityTests
    {
        [TestMethod]
        public void ContentViewActivity_Constructor_With_Minimum_Parameters_Sets_Correct_Properties()
        {
            var documentLocation = new Uri("http://unitest.csharpanalytics.com/some/sample/page?id=1");
            var activity = new ContentViewActivity(documentLocation, "Sample Page");

            Assert.AreEqual(documentLocation.AbsolutePath, activity.DocumentLocation.AbsolutePath);
            Assert.AreEqual("Sample Page", activity.DocumentTitle);

            Assert.IsNull(activity.DocumentHostName);
            Assert.IsNull(activity.DocumentPath);
            Assert.IsNull(activity.ContentDescription);
        }

        [TestMethod]
        public void ContentViewActivity_Constructor_With_All_Parameters_Sets_Correct_Properties()
        {
            var documentLocation = new Uri("http://unitest.csharpanalytics.com/some/sample/page?id=1");

            var activity = new ContentViewActivity(documentLocation, "Document Title", "A content description", "/document/path", "newhostname.csharpanalytics.com");

            Assert.AreEqual(documentLocation.AbsoluteUri, activity.DocumentLocation.AbsoluteUri);
            Assert.AreEqual("Document Title", activity.DocumentTitle);
            Assert.AreEqual("A content description", activity.ContentDescription);
            Assert.AreEqual("/document/path", activity.DocumentPath);
            Assert.AreEqual("newhostname.csharpanalytics.com", activity.DocumentHostName);
        }
    }
}