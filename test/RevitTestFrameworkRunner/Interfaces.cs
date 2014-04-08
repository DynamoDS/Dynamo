using System.Collections.Generic;

namespace RevitTestFrameworkRunner
{
    public interface IAssemblyData
    {
        string Path { get; set; }
        string Name { get; set; }
        IList<IFixtureData> Fixtures { get; set; } 
    }

    public interface IFixtureData
    {
        string Name { get; set; }
        IList<ITestData> Tests { get; set; } 
    }

    public interface ITestData
    {
        string Name { get; set; }
        bool RunDynamo { get; set; }
        string ModelPath { get; set; }
    }
}
