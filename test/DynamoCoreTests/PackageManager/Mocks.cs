using Moq;

namespace Dynamo.Tests
{
    public static class Mocks
    {
        public static T Empty<T>() where T : class
        {
            return (new Mock<T>()).Object;
        }
    }
}