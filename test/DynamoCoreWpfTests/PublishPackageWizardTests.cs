using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.PackageManager.UI;
using Dynamo.Tests;
using Dynamo.UI.Views;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class PublishPackageWizardTests : DynamoViewModelUnitTest
    {
        [Test]
        public void ArePackageContentsEqual_IdenticalLists_ReturnsTrue()
        {
            // Arrange

            var files = new[] { new FileInfo(@"C:\pkg\file1.dyn"), new FileInfo(@"C:\pkg\file2.DYN") };
            var oldContents = new List<PackageItemRootViewModel>
            {

                new PackageItemRootViewModel (files[0]),
                new PackageItemRootViewModel ( files[1] )
            };

            var newContents = new List<PackageItemRootViewModel>
            {
                new PackageItemRootViewModel (files[0]),
                new PackageItemRootViewModel ( files[1] )
            };

            // Act
            var result = PackageManagerWizard.ArePackageContentsEqual(oldContents, newContents);

            // Assert
            Assert.True(result);
        }

        [Test]
        public void ArePackageContentsEqual_DifferentLists_ReturnsFalse()
        {
            // Arrange
            var files = new[] { new FileInfo(@"C:\pkg\file1.dyn"), new FileInfo(@"C:\pkg\file2.DYN") };
            var oldContents = new List<PackageItemRootViewModel>
            {
                new PackageItemRootViewModel (files[0])
            };

            var newContents = new List<PackageItemRootViewModel>
            {
                new PackageItemRootViewModel (files[1])
            };

            // Act
            var result = PackageManagerWizard.ArePackageContentsEqual(oldContents, newContents);

            // Assert
            Assert.False(result);
        }

        [Test]
        public void ArePackageContentsEqual_DifferentCounts_ReturnsFalse()
        {
            // Arrange
            var files = new[] { new FileInfo(@"C:\pkg\file1.dyn"), new FileInfo(@"C:\pkg\file2.DYN") };
            var oldContents = new List<PackageItemRootViewModel>
            {
                new PackageItemRootViewModel (files[0]),
                new PackageItemRootViewModel ( files[1] )
            };

            var newContents = new List<PackageItemRootViewModel>
            {
                new PackageItemRootViewModel (files[0]),
            };

            // Act
            var result = PackageManagerWizard.ArePackageContentsEqual(oldContents, newContents);

            // Assert
            Assert.False(result);
        }

        [Test]
        public void ArePackageContentsEqual_SameItemsDifferentOrder_ReturnsTrue()
        {
            // Arrange
            var files = new[] { new FileInfo(@"C:\pkg\file1.dyn"), new FileInfo(@"C:\pkg\file2.DYN") };

            var oldContents = new List<PackageItemRootViewModel>
            {
                new PackageItemRootViewModel (files[0]),
                new PackageItemRootViewModel ( files[1] )
            };

            var newContents = new List<PackageItemRootViewModel>
            {
                new PackageItemRootViewModel (files[1]),
                new PackageItemRootViewModel ( files[0] )
            };

            // Act
            var result = PackageManagerWizard.ArePackageContentsEqual(oldContents, newContents);

            // Assert
            Assert.True(result);
        }


        [Test]
        public void ArePackageContentsEqual_IdenticalNestedStructure_ReturnsTrue()
        {
            // Arrange
            var files = new[] { new FileInfo(@"C:\pkg\file1.dyn"), new FileInfo(@"C:\pkg\subfolder\file2.DYN") };

            var oldContents = new List<PackageItemRootViewModel>
    {
        new PackageItemRootViewModel(files[0])
        {
            ChildItems = new ObservableCollection<PackageItemRootViewModel>
            {
                new PackageItemRootViewModel(files[1])
            }
        }
    };

            var newContents = new List<PackageItemRootViewModel>
    {
        new PackageItemRootViewModel(files[0])
        {
            ChildItems = new ObservableCollection<PackageItemRootViewModel>
            {
                new PackageItemRootViewModel(files[1])
            }
        }
    };

            // Act
            var result = PackageManagerWizard.ArePackageContentsEqual(oldContents, newContents);

            // Assert
            Assert.True(result);
        }

        [Test]
        public void ArePackageContentsEqual_DifferentNestedStructure_ReturnsFalse()
        {
            // Arrange
            var files = new[]
            {
        new FileInfo(@"C:\pkg\file1.dyn"),
        new FileInfo(@"C:\pkg\subfolder\file2.DYN"),
        new FileInfo(@"C:\pkg\subfolder\file3.dyn") // Extra file in newContents
    };

            var oldContents = new List<PackageItemRootViewModel>
    {
        new PackageItemRootViewModel(files[0])
        {
            ChildItems = new ObservableCollection<PackageItemRootViewModel>
            {
                new PackageItemRootViewModel(files[1])
            }
        }
    };

            var newContents = new List<PackageItemRootViewModel>
    {
        new PackageItemRootViewModel(files[0])
        {
            ChildItems = new ObservableCollection<PackageItemRootViewModel>
            {
                new PackageItemRootViewModel(files[1]),
                new PackageItemRootViewModel(files[2]) // Extra child item
            }
        }
    };

            // Act
            var result = PackageManagerWizard.ArePackageContentsEqual(oldContents, newContents);

            // Assert
            Assert.False(result);
        }

        [Test]
        public void ArePackageContentsEqual_DifferentFilePathInNestedStructure_ReturnsFalse()
        {
            // Arrange
            var files = new[]
            {
        new FileInfo(@"C:\pkg\file1.dyn"),
        new FileInfo(@"C:\pkg\subfolder\file2.DYN"),
        new FileInfo(@"C:\pkg\otherfolder\file2.DYN") // Different file path for second item
    };

            var oldContents = new List<PackageItemRootViewModel>
    {
        new PackageItemRootViewModel(files[0])
        {
            ChildItems = new ObservableCollection<PackageItemRootViewModel>
            {
                new PackageItemRootViewModel(files[1])
            }
        }
    };

            var newContents = new List<PackageItemRootViewModel>
    {
        new PackageItemRootViewModel(files[0])
        {
            ChildItems = new ObservableCollection<PackageItemRootViewModel>
            {
                new PackageItemRootViewModel(files[2]) // Different path
            }
        }
    };

            // Act
            var result = PackageManagerWizard.ArePackageContentsEqual(oldContents, newContents);

            // Assert
            Assert.False(result);
        }

        [Test]
        public void ArePackageContentsEqual_SameNestedItemsDifferentOrder_ReturnsTrue()
        {
            // Arrange
            var files = new[]
            {
        new FileInfo(@"C:\pkg\file1.dyn"),
        new FileInfo(@"C:\pkg\subfolder\file2.DYN"),
        new FileInfo(@"C:\pkg\subfolder\file3.dyn")
    };

            var oldContents = new List<PackageItemRootViewModel>
    {
        new PackageItemRootViewModel(files[0])
        {
            ChildItems = new ObservableCollection<PackageItemRootViewModel>
            {
                new PackageItemRootViewModel(files[1]),
                new PackageItemRootViewModel(files[2])
            }
        }
    };

            var newContents = new List<PackageItemRootViewModel>
    {
        new PackageItemRootViewModel(files[0])
        {
            ChildItems = new ObservableCollection<PackageItemRootViewModel>
            {
                new PackageItemRootViewModel(files[2]), // Swapped order
                new PackageItemRootViewModel(files[1])
            }
        }
    };

            // Act
            var result = PackageManagerWizard.ArePackageContentsEqual(oldContents, newContents);

            // Assert
            Assert.True(result);
        }
    }


}
