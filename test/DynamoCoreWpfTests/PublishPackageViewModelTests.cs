using NUnit.Framework;
using System.Linq;
using Dynamo.PackageManager;
using System.IO;
using Dynamo.Tests;

namespace DynamoCoreWpfTests
{
     [TestFixture]
    public class PublishPackageViewModelTests: DynamoViewModelUnitTest
    {

        [Test]
        public void AddingDyfRaisesCanExecuteChangeOnDelegateCommand()
        {
            
            var vm = new PublishPackageViewModel(ViewModel);
            ViewModel.OnRequestPackagePublishDialog(vm);

            //find a customnode to add to the package
            string packagedirectory = Path.Combine(TestDirectory, "pkgs");
            var packages = Directory.EnumerateDirectories(packagedirectory);
            var first = Path.GetFullPath(packages.First());
            string dyfpath = Path.Combine(first, "dyf");
            var customnodes = Directory.GetFiles(dyfpath);
            var firstnode = customnodes.First();

            var canExecuteChangedFired = 0;
            vm.SubmitCommand.CanExecuteChanged += ((o, e) => { canExecuteChangedFired++; });
            //now add a customnode to the package
            vm.AddFile(firstnode);

            //assert that canExecute changed was fired one time 
            Assert.AreEqual(canExecuteChangedFired, 1);

        }

    }
}
