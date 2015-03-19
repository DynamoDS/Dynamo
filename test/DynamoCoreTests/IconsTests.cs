using Dynamo.Models;
using Dynamo.Search.SearchElements;
using Dynamo.UI;
using Dynamo.Wpf.Services;

using NUnit.Framework;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class IconsTests
    {
        [Test]
        [Category("UnitTests")]
        [Category("Failure")]
        // Test checks png images. If at least one icon is not presented, test fails.
        public void SearchForPNGFiles()
        {
            var model = DynamoModel.Start();

            IEnumerable searchEntries = model.SearchModel.SearchEntries.OfType<NodeSearchElement>();
            IconServices iconServices = new IconServices(model.PathManager);
            IconWarehouse currentWarehouse = null;
            var currentWarehouseAssembly = string.Empty;

            List<String> missingIcons = new List<string>();
            foreach (var entry in searchEntries)
            {
                var searchEle = entry as NodeSearchElement;
                if (String.IsNullOrEmpty(searchEle.IconName))
                    continue;

                var smallIconName = searchEle.IconName + Configurations.SmallIconPostfix;
                var largeIconName = searchEle.IconName + Configurations.LargeIconPostfix;


                // Only retrieve the icon warehouse for different assembly.
                if (currentWarehouseAssembly != searchEle.Assembly)
                {
                    currentWarehouseAssembly = searchEle.Assembly;
                    currentWarehouse = iconServices.GetForAssembly(searchEle.Assembly);
                }

                ImageSource smallIcon = null;
                ImageSource largeIcon = null;
                if (currentWarehouse != null)
                {
                    smallIcon = currentWarehouse.LoadIconInternal(smallIconName);
                    largeIcon = currentWarehouse.LoadIconInternal(largeIconName);
                }

                if (smallIcon == null)
                    missingIcons.Add(smallIconName);
                if (largeIcon == null)
                    missingIcons.Add(largeIconName);
            }

            Assert.IsTrue(missingIcons.Count == 0, String.Join(Environment.NewLine, missingIcons));
        }
    }
}
