using Moq;

namespace Dynamo.Tests
{
    public static class MockMaker
    {
        public static T Empty<T>() where T : class
        {
            return (new Mock<T>()).Object;
        }
    }
}