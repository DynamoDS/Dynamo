using Dynamo.Models;
using Dynamo.Search.SearchElements;
using Dynamo.UI;
using Dynamo.Wpf.Services;
using DynamoShapeManager;
using DynamoUtilities;

using NUnit.Framework;

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class IconsTests
    {
        private static string geometryFactoryPath = "";
        private IEnumerable searchEntries;

        static readonly string AssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        static readonly string AnyCPUPath = Directory.GetParent(AssemblyPath).FullName;
        static readonly string binPath = Directory.GetParent(AnyCPUPath).FullName;
        static readonly string DynamoPath = Directory.GetParent(binPath).FullName;

        static readonly string ResourcePath = Path.Combine(DynamoPath, @"src\Resources");

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
            bool testResult = true;

            foreach (var entry in searchEntries)
            {
                if (entry is NodeSearchElement)
                {
                    var searchEle = entry as NodeSearchElement;

                    string smallIconPath = Path.Combine(ResourcePath, Path.GetFileNameWithoutExtension(searchEle.Assembly),
                        "SmallIcons", searchEle.IconName + Configurations.SmallIconPostfix + ".png");

                    string largeIconPath = Path.Combine(ResourcePath, Path.GetFileNameWithoutExtension(searchEle.Assembly),
                        "LargeIcons", searchEle.IconName + Configurations.LargeIconPostfix + ".png");

                    bool smallIconExists = File.Exists(smallIconPath);
                    bool largeIconExists = File.Exists(largeIconPath);

                    // Alert which icon is missed.
                    if (!smallIconExists)
                        Debug.WriteLine(smallIconPath);
                    if (!largeIconExists)
                        Debug.WriteLine(largeIconPath);

                    testResult = testResult && smallIconExists && largeIconExists;
                }
            }

            Assert.IsTrue(testResult);
        }
    }
}
