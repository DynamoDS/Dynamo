using System;
using CSharpAnalytics.Protocols.Measurement;
using CSharpAnalytics.Sessions;
using CSharpAnalytics.Test.Environment;

namespace CSharpAnalytics.Test.Protocols.Measurement
{
    internal static class MeasurementTestHelpers
    {
        public static readonly MeasurementConfiguration Configuration = new MeasurementConfiguration("UA-319000-10", "AppName", "1.2.3.4");

        public static SessionManager CreateSessionManager()
        {
            return new SessionManager(new SessionState());
        }

        public static TestableEnvironment CreateEnvironment()
        {
            return new TestableEnvironment("en-us");
        }

        public static void ConfigureForTest(MeasurementAnalyticsClient client, Action<Uri> sender)
        {
            client.Configure(Configuration, CreateSessionManager(), CreateEnvironment(), sender);
        }
    }
}