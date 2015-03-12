using Dynamo.Models;
using Dynamo.Search.SearchElements;
using Dynamo.UI;
using Dynamo.Wpf.Services;
using DynamoShapeManager;

using NUnit.Framework;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Media;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class IconsTests
    {
        private static string geometryFactoryPath = "";
        private IEnumerable searchEntries;
        private IconServices iconServices = new IconServices();

        private static string ResourcePath = String.Empty;

        [SetUp]
        public void InitializeDirectoryPaths()
        {
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var relativeDir = Path.Combine(assemblyDir, @"..\..\..\src\Resources");
            var absoluteDir = Path.GetFullPath(relativeDir);
            Assert.IsFalse(string.IsNullOrEmpty(absoluteDir));
            Assert.IsTrue(Directory.Exists(absoluteDir));
            ResourcePath = absoluteDir;
        }

        [SetUp]
        public void PreloadShapeManager()
        {
            var exePath = Assembly.GetExecutingAssembly().Location;
            var rootFolder = Path.GetDirectoryName(exePath);

            var versions = new[]
            {
                LibraryVersion.Version219,
                LibraryVersion.Version220,
                LibraryVersion.Version221
            };

            var preloader = new Preloader(rootFolder, versions);
            geometryFactoryPath = preloader.GeometryFactoryPath;
        }



        [Test]
        [Category("UnitTests")]
        [Category("Failure")]
        // Test checks png images. If at least one icon is not presented, test fails.
        public void SearchForPNGFiles()
        {
            var model = DynamoModel.Start(
                new DynamoModel.StartConfiguration()
                {
                    GeometryFactoryPath = geometryFactoryPath,
                    Preferences = PreferenceSettings.Load()
                });

            searchEntries = model.SearchModel.SearchEntries;

            List<String> missingIcons = new List<string>();
            foreach (var entry in searchEntries)
            {
                if (!(entry is NodeSearchElement))
                    continue;

                var searchEle = entry as NodeSearchElement;
                if (String.IsNullOrEmpty(searchEle.IconName))
                    continue;

                var smallIconName = searchEle.IconName + Configurations.SmallIconPostfix;
                var largeIconName = searchEle.IconName + Configurations.LargeIconPostfix;


                var warehouse = iconServices.GetForAssembly(searchEle.Assembly);
                ImageSource smallIcon = null;
                ImageSource largeIcon = null;
                if (warehouse != null)
                {
                    smallIcon = warehouse.LoadIconInternal(smallIconName);
                    largeIcon = warehouse.LoadIconInternal(largeIconName);
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
