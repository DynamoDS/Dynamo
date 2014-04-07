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

        private static Dictionary<string, byte[]> GACGeometryKeyTokens = new Dictionary<string, byte[]>() {
            /* {"ProtoGeometry", new byte[] { 0xD5, 0x24, 0x30, 0x4D, 0xAF, 0x1F, 0x8B, 0x35}}, */
        };

        private AssemblyName mExecutingAssemblyName = Assembly.GetExecutingAssembly().GetName();

        Dictionary<Core, FFIExecutionSession> mSessions = new Dictionary<Core, FFIExecutionSession>();
        ExtensionAppLoader mApploader = new ExtensionAppLoader();

        static FFIExecutionManager mSelf = null;

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
            else
            {
                string assemblyName = System.IO.Path.GetFileNameWithoutExtension(name);
                byte[] publicKeyToekn;

                if (GACGeometryKeyTokens.TryGetValue(assemblyName, out publicKeyToekn))
                {
                    AssemblyName an = new AssemblyName();
                    an.Name = assemblyName;
                    an.SetPublicKeyToken(publicKeyToekn);
                    an.Version = mExecutingAssemblyName.Version;
                    an.CultureInfo = mExecutingAssemblyName.CultureInfo;
                    an.ProcessorArchitecture = mExecutingAssemblyName.ProcessorArchitecture;
                    System.Diagnostics.Debug.Write("Assembly: " + assemblyName + "," + an.Version.ToString() + " is in GAC.");
                    return Assembly.Load(an);
                }
            }
            throw new System.IO.FileNotFoundException();
        }

        public bool IsInternalGacAssembly(string moduleName)
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                return false;
            }

            string assemblyName = System.IO.Path.GetFileNameWithoutExtension(moduleName);
            return GACGeometryKeyTokens.ContainsKey(assemblyName);
        }

        public IExecutionSession GetSession(Core core)
        {
            return GetSession(core, false);
        }

        public bool RegisterExtensionApplicationType(Core core, SysType type)
        {
            if (!typeof(IExtensionApplication).IsAssignableFrom(type))
                return false;

            FFIExecutionSession session = GetSession(core, true);
            session.AddExtensionAppType(type);
            return true;
        }

        private FFIExecutionSession GetSession(Core core, bool createIfNone)
        {
            FFIExecutionSession session = null;
            if(!mSessions.TryGetValue(core, out session) && createIfNone)
            {
                lock (mSessions)
                {
                    if (!mSessions.TryGetValue(core, out session))
                    {
                        session = new FFIExecutionSession(core);
                        core.ExecutionEvent += OnExecutionEvent;
                        core.Dispose += OnDispose;
                        mSessions.Add(core, session);
                    }
                }
            }

            return session;
        }

        private void OnDispose(Core sender)
        {
            if (null == mSessions)
                return;

            FFIExecutionSession session = null;
            if (mSessions.TryGetValue(sender, out session))
            {
                lock (mSessions)
                {
                    mSessions.Remove(sender);
                }
                mSessions.Remove(sender);
                session.Dispose();
                sender.ExecutionEvent -= OnExecutionEvent;
            }
        }

        private void OnExecutionEvent(object sender, ExecutionStateEventArgs e)
        {
            Core core = sender as Core;
            FFIExecutionSession session = GetSession(core, false);
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
        private Core core;
        private Dictionary<string, object> configValues;
        private List<SysType> extensionapps;
        public FFIExecutionSession(Core core)
        {
            this.core = core;
            this.configValues = new Dictionary<string, object>();
            this.extensionapps = new List<SysType>();
        }

        public IConfiguration Configuration
        {
            get { return this; }
        }

        public string SearchFile(string fileName)
        {
            return FileUtils.GetDSFullPathName(fileName, this.core.Options);
        }

        public string RootModulePath
        {
            get { return core.Options.RootModulePathName; }
        }

        public string[] IncludeDirectories
        {
            get { return core.Options.IncludeDirectories.ToArray(); }
        }

        public bool IsDebugMode
        {
            get { return core.Options.IDEDebugMode; }
        }

        public object GetConfigValue(string config)
        {
            object value = null;
            if (!this.configValues.TryGetValue(config, out value))
            {
                core.Configurations.TryGetValue(config, out value);
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
