using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;

using DynamoShapeManager;


namespace TestServices
{
#if NETFRAMEWORK
using NUnit.Framework;

    public class GeometricTestBase
    {
        private AssemblyResolver assemblyResolver;

        [SetUp]
        public virtual void Setup()
        {
            var config = GetTestSessionConfiguration();
            var session = new TestExecutionSession(config);
            var application = Application.Instance as IExtensionApplication;

            if (assemblyResolver == null)
            {
                assemblyResolver = new AssemblyResolver();
                assemblyResolver.Setup(config.DynamoCorePath);
            }

            application.OnBeginExecution(session);
            HostFactory.Instance.StartUp();
        }

        [TearDown]
        public void ShutDownHostFactory()
        {
            //application.OnEndExecution(session);
            //HostFactory.Instance.ShutDown();

            if (assemblyResolver != null)
            {
                assemblyResolver.TearDown();
                assemblyResolver = null;
            }
        }

        /// <summary>
        /// Override this method in derived class to establish a 
        /// custom configuration.
        /// </summary>
        /// <returns></returns>
        protected virtual TestSessionConfiguration GetTestSessionConfiguration()
        {
            return new TestSessionConfiguration();
        }
    }
#endif
    /// <summary>
    /// This is a temporary session class which is only used for nodes that are using Geometries.
    /// When ProtoGeometry is loaded, the static instance GeometryFactory will be constructed which
    /// requires a session to be present.
    /// </summary>
    class TestExecutionSession : IExecutionSession, IConfiguration, IDisposable
    {
        private Dictionary<string, object> configValues;
        private Preloader preloader;
        private TestSessionConfiguration testConfig;

        public TestExecutionSession(TestSessionConfiguration testConfig)
        {
            configValues = new Dictionary<string, object>();
            this.testConfig = testConfig;
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
            get
            {
                var assemPath = Assembly.GetExecutingAssembly().Location;
                var assemDir = new DirectoryInfo(Path.GetDirectoryName(assemPath));
                return assemDir.Parent.FullName;
            }
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

                preloader = new Preloader(testConfig.DynamoCorePath, new[] { testConfig.RequestedLibraryVersion2 });
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
