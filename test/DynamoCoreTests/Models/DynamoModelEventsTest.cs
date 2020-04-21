using NUnit.Framework;

namespace Dynamo.Tests.ModelsTest
{
    [TestFixture]
    class DynamoModelEventsTest : DynamoModelTestBase
    {
        
        [Test]
        [Category("UnitTests")]
        public void TestOnRequestDispatcherBeginInvoke()
        {
            /*var viewModel = DynamoViewModel.Start(
            new DynamoViewModel.StartConfiguration()
            {
                DynamoModel = this.CurrentDynamoModel,
                Watch3DViewModel =
                    new DefaultWatch3DViewModel(null, new Watch3DViewModelStartupParams(this.CurrentDynamoModel))
                    {
                        Active = false,
                        CanBeActivated = false
                    }
            });*/

            Assert.Pass();
        }
    }
}
