using System;

namespace Dynamo.Tests
{
    [AttributeUsage(AttributeTargets.Method)]
    public class TestModelAttribute : Attribute
    {
        public string Path { get; private set; }

        public TestModelAttribute(string path)
        {
            Path = path;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class RunDynamoAttribute : Attribute
    {
        public bool RunDynamo { get; private set; }
        public RunDynamoAttribute(bool run)
        {
            RunDynamo = run;
        }
    }
}
