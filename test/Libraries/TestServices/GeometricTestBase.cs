using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Dynamo;
using DynamoShapeManager;
using DynamoUtilities;

using NUnit.Framework;

namespace TestServices
{
    public class GeometricTestBase
    {
        IExtensionApplication application = Application.Instance as IExtensionApplication;
        TestExecutionSession session = new TestExecutionSession();

        [SetUp]
        public virtual void Setup()
        {
            AssemblyResolver.Setup();
            application.OnBeginExecution(session);
            HostFactory.Instance.StartUp();
        }

        [TearDown]
        public void ShutDownHostFactory()
        {
            //application.OnEndExecution(session);
            //HostFactory.Instance.ShutDown();
        }
    }

    /// <summary>
    /// This is a temporary session class which is only used for nodes that are using Geometries.
    /// When ProtoGeometry is loaded, the static instance GeometryFactory will be constructed which
    /// requires a session to be present.
    /// </summary>
    class TestExecutionSession : IExecutionSession, IConfiguration, IDisposable
    {
        private Dictionary<string, object> configValues;
        private Preloader preloader;
        private RemoteTestSessionConfig remoteConfig;

        public TestExecutionSession()
        {
            configValues = new Dictionary<string, object>();
            remoteConfig = new RemoteTestSessionConfig();
        }

        public IConfiguration Configuration
        {
            get { return this; }
        }

        public string SearchFile(string fileName)
        {
            var path = Path.Combine(RootModulePath, fileName);
            if (File.Exists(path))
                return path;
            return fileName;
        }

        public string RootModulePath
        {
            get { return AssemblyResolver.GetDynamoRootDirectory(); }
        }

        public string[] IncludeDirectories
        {
            get { return new string[] { RootModulePath }; }
        }

        public bool IsDebugMode
        {
            get { return false; }
        }

        public object GetConfigValue(string config)
        {
            if (string.Compare(ConfigurationKeys.GeometryFactory, config) == 0)
            {
                if (preloader != null) return preloader.GeometryFactoryPath;

                preloader = Preloader.FromSpecifiedLibraryVersionString(
                    remoteConfig.DynamoCorePath,
                    remoteConfig.RequestedLibraryVersion);

                preloader.Preload();

                return preloader.GeometryFactoryPath;
            }

            if (configValues.ContainsKey(config))
                return configValues[config];

            return null;
        }

        public void SetConfigValue(string config, object value)
        {
            configValues[config] = value;
        }

        public void Dispose()
        {
            configValues.Clear();
        }
    }
}
