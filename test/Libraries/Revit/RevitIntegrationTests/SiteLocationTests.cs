using System.Linq;

using Autodesk.Revit.DB;

using Dynamo.Models;

using NUnit.Framework;

using Revit.GeometryConversion;

using RevitServices.Persistence;

using RTF.Framework;

using Location = DynamoUnits.Location;
using SiteLocation = DSRevitNodesUI.SiteLocation;

namespace RevitSystemTests
{
    [TestFixture]
    class SiteLocationTests : SystemTest
    {
        [Test, TestModel(@".\SiteLocation\SiteLocation.rvt")]
        public void SiteLocation_ValidArgs()
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;

            OpenAndRunDynamoDefinition(@".\SiteLocation\SiteLocation.dyn");

            var locNode = ViewModel.Model.Nodes.FirstOrDefault(x => x is SiteLocation);

            // Set to Dubai 
            SetLocation(doc, 25.287, 55.321);
            
            RunCurrentModel();
            AssertLocationValues(locNode, doc);

            // Set to New York  
            SetLocation(doc, 40.783, -73.966);

            RunCurrentModel();
            AssertLocationValues(locNode, doc);

            // Set to San Francisco
            SetLocation(doc, 37.777, -122.420);

            RunCurrentModel();
            AssertLocationValues(locNode, doc);
        }

        private void AssertLocationValues(NodeModel locNode, Document doc)
        {
            var locValue = GetPreviewValue(locNode.GUID.ToString()) as Location;
            Assert.AreEqual(locValue.Name, doc.SiteLocation.PlaceName);
            Assert.AreEqual(locValue.Latitude, doc.SiteLocation.Latitude.ToDegrees(), 0.001);
            Assert.AreEqual(locValue.Longitude, doc.SiteLocation.Longitude.ToDegrees(), 0.001);
        }

        private static void SetLocation(Document doc, double latitude, double longitude)
        {
            using (var t = new Transaction(doc))
            {
                t.Start("Set location.");
                doc.SiteLocation.Latitude = latitude.ToRadians();
                doc.SiteLocation.Longitude = longitude.ToRadians();
                t.Commit();
            }
        }
    }
}
