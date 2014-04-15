using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;

namespace RevitTestFrameworkRunner
{
    public enum TestStatus{None,Cancelled, Error, Failure, Ignored, Inconclusive, NotRunnable, Skipped, Success,TimedOut}

    public interface IAssemblyData
    {
        string Path { get; set; }
        string Name { get; set; }
        IList<IFixtureData> Fixtures { get; set; } 
    }

    public interface IFixtureData
    {
        IAssemblyData Assembly { get; set; }
        string Name { get; set; }
        IList<ITestData> Tests { get; set; } 
    }

    public interface ITestData
    {
        IFixtureData Fixture { get; set; }
        string Name { get; set; }
        bool RunDynamo { get; set; }
        string ModelPath { get; set; }
        TestStatus TestStatus { get; set; }
        ObservableCollection<IResultData> ResultData { get; set; }
    }

    public interface IResultData
    {
        string Message { get; set; }
        string StackTrace { get; set; }
    }
}
