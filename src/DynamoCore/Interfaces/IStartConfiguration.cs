using Dynamo.UpdateManager;

using Greg;

namespace Dynamo.Interfaces
{
    /// <summary>
    /// An interface which defines a start configuration.
    /// </summary>
    public interface IStartConfiguration
    {
        string Context { get; set; }
        string DynamoCorePath { get; set; }
        IPreferences Preferences { get; set; }
        bool StartInTestMode { get; set; }
        IUpdateManager UpdateManager { get; set; }
        ISchedulerThread SchedulerThread { get; set; }
        string GeometryFactoryPath { get; set; }
        IAuthProvider AuthProvider { get; set; }
        string PackageManagerAddress { get; set; }
    }
}
