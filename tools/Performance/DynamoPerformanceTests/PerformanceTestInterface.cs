using System.Collections.Generic;

namespace DynamoPerformanceTests
{
    /// <summary>
    /// This interface defines the common properties for all performance tests to share
    /// </summary>
    interface IPerformanceTest
    {
        /// <summary>
        /// Each graph path
        /// </summary>
        string DynamoFilePath { get; set; }

        /// <summary>
        /// Populates the test cases based on DYN files in the performance tests folder.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> PerformanceTestSource();
    }
}
