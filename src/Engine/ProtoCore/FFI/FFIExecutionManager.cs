using System;
using System.Collections.Generic;
using System.Reflection;
using Autodesk.DesignScript.Interfaces;
using ProtoCore;
using ProtoCore.Utils;
using SysType = System.Type;

namespace ProtoFFI
{
    internal class FFIExecutionManager
    {
        private FFIExecutionManager()
        {
        }

        Dictionary<RuntimeCore, FFIExecutionSession> mSessions = new Dictionary<RuntimeCore, FFIExecutionSession>();
        ExtensionAppLoader mApploader = ExtensionAppLoader.Instance;

        static FFIExecutionManager mSelf;

        public static FFIExecutionManager Instance
        {
            get
            {
                if (null == mSelf)
                    mSelf = new FFIExecutionManager();
                return mSelf;
            }
        }

        public Assembly LoadAssembly(string name)
        {
            System.Diagnostics.Debug.Write("Trying to load assembly: " + name);
            if (System.IO.File.Exists(name))
            {
                return Assembly.LoadFrom(name);
            }
            throw new System.IO.FileNotFoundException();
        }

        public IExecutionSession GetSession(RuntimeCore runtimeCore)
        {
            return GetSession(runtimeCore, false);
        }

        public bool RegisterExtensionApplicationType(RuntimeCore runtimeCore, SysType type)
        {
            if (!typeof(IExtensionApplication).IsAssignableFrom(type))
                return false;

            FFIExecutionSession session = GetSession(runtimeCore, true);
            session.AddExtensionAppType(type);
            return true;
        }

        private FFIExecutionSession GetSession(RuntimeCore runtimeCore, bool createIfNone)
        {
            FFIExecutionSession session = null;
            lock (mSessions)
            {
                if (!mSessions.TryGetValue(runtimeCore, out session) && createIfNone)
                {
                    session = new FFIExecutionSession(runtimeCore);
                    runtimeCore.ExecutionEvent += OnExecutionEvent;
                    runtimeCore.Dispose += OnDispose;
                    mSessions.Add(runtimeCore, session);
                }
            }

            return session;
        }

        private void OnDispose(RuntimeCore sender)
        {
            if (mSessions == null)
                return;

            FFIExecutionSession session = null;
            lock (mSessions)
            {
                if (mSessions.TryGetValue(sender, out session))
                {
                    mSessions.Remove(sender);
                    session.Dispose();
                    sender.Dispose -= OnDispose;
                    sender.ExecutionEvent -= OnExecutionEvent;
                }
                mSelf = null;
            }

        }

        private void OnExecutionEvent(object sender, ExecutionStateEventArgs e)
        {
            RuntimeCore runtimeCore = sender as RuntimeCore;
            FFIExecutionSession session = GetSession(runtimeCore, false);
            //If there wasn't any session created, there was no extension app 
            //registered for the session
            if (null == session)
                return;

            session.State = e.ExecutionState;
            mApploader.Notify(session);
        }

        /// <summary>
        /// For nunit-setup
        /// </summary>
        internal static void ForceStartUpAllApps()
        {
            Instance.mApploader.ForceStartUpAllApps();
        }

        /// <summary>
        /// For nunit-teardown
        /// </summary>
        internal static void ForceShutDownAllApps()
        {
            Instance.mApploader.ForceShutDownAllApps();
        }
    }

    class FFIExecutionSession : IExecutionSession, IConfiguration, IDisposable
    {
        private RuntimeCore runtimeCore;
        private Dictionary<string, object> configValues;
        private List<SysType> extensionapps;
        public FFIExecutionSession(RuntimeCore runtimeCore)
        {
            this.runtimeCore = runtimeCore;
            this.configValues = new Dictionary<string, object>();
            this.extensionapps = new List<SysType>();
        }

        public IConfiguration Configuration
        {
            get { return this; }
        }

        public string SearchFile(string fileName)
        {
            return FileUtils.GetDSFullPathName(fileName, this.runtimeCore.Options);
        }

        public string RootModulePath
        {
            get { return runtimeCore.Options.RootModulePathName; }
        }

        public string[] IncludeDirectories
        {
            get { return runtimeCore.Options.IncludeDirectories.ToArray(); }
        }

        public object GetConfigValue(string config)
        {
            object value = null;
            if (!this.configValues.TryGetValue(config, out value))
            {
                runtimeCore.DSExecutable.Configurations.TryGetValue(config, out value);
            }
            return value;
        }

        public void SetConfigValue(string config, object value)
        {
            this.configValues[config] = value;
        }

        public void AddExtensionAppType(SysType type)
        {
            if (!extensionapps.Contains(type))
                extensionapps.Add(type);
        }

        public SysType[] ExtensionAppTypes { get { return this.extensionapps.ToArray(); } }

        internal ExecutionStateEventArgs.State State { get; set; }

        public void Dispose()
        {
            this.configValues.Clear();
            this.extensionapps.Clear();
        }
    }
}
