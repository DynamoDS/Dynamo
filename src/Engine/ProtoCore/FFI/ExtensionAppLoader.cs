using System;
using System.Collections;
using System.IO;
using System.Reflection;
using Autodesk.DesignScript.Interfaces;

namespace ProtoFFI
{
    class ExtensionAppLoader
    {
        System.Collections.Hashtable mAssemblies = new System.Collections.Hashtable();
        System.Collections.Hashtable mExtensionApps = new System.Collections.Hashtable();
        string mProtoInterface = string.Empty;

        internal ExtensionAppLoader()
        {
            mProtoInterface = typeof(IExtensionApplication).Assembly.GetName().Name;
            Initialize();
        }

        public void Notify(FFIExecutionSession session)
        {
            Type[] appTypes = session.ExtensionAppTypes;
            foreach (var item in appTypes)
            {
                //If the specified key is not found then the HashTable returns null.
                IExtensionApplication app = mExtensionApps[item] as IExtensionApplication;
                if (null == app)
                    continue;

                switch (session.State)
                {
                    case ProtoCore.ExecutionStateEventArgs.State.kExecutionBegin:
                        app.OnBeginExecution(session);
                        break;
                    case ProtoCore.ExecutionStateEventArgs.State.kExecutionEnd:
                        app.OnEndExecution(session);
                        break;
                    case ProtoCore.ExecutionStateEventArgs.State.kExecutionBreak:
                        app.OnSuspendExecution(session);
                        break;
                    case ProtoCore.ExecutionStateEventArgs.State.kExecutionResume:
                        app.OnResumeExecution(session);
                        break;
                    default:
                        break;
                }
            }
        }

        private void OnDomainUnload(object sender, EventArgs e)
        {
            Terminate();
        }

        private void Initialize()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyLoad += new AssemblyLoadEventHandler(OnAssemblyLoad);
            currentDomain.AssemblyResolve += new ResolveEventHandler(OnAssemblyResolve);
            currentDomain.ProcessExit += new EventHandler(OnDomainUnload);
        }

        private void Terminate()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyLoad -= new AssemblyLoadEventHandler(OnAssemblyLoad);
            currentDomain.AssemblyResolve -= new ResolveEventHandler(OnAssemblyResolve);
            currentDomain.ProcessExit -= new EventHandler(OnDomainUnload);

            IDictionaryEnumerator i = mExtensionApps.GetEnumerator();
            while (i.MoveNext())
            {
                IExtensionApplication app = i.Value as IExtensionApplication;
                if (null != app)
                    app.ShutDown();
            }
            mExtensionApps.Clear();
            mAssemblies.Clear();
        }

        private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args == null || string.IsNullOrEmpty(args.Name))
                return null;
            try
            {
                //First check if it's one of the alreadt loaded assemblies.
                string shortname = (args.Name.Split(','))[0];
                if (mAssemblies.Contains(shortname))
                    return mAssemblies[shortname] as Assembly;

                //load from search path, if file exists
                string filepath = ProtoCore.Utils.FileUtils.GetDSFullPathName(shortname + ".dll");
                if (File.Exists(filepath))
                    return Assembly.LoadFrom(filepath);
            }
            catch (Exception)
            {
            }

            return null;
        }

        private void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            if (null != args)
                ProcessAssembly(args.LoadedAssembly);
        }

        private void ProcessAssembly(Assembly assembly)
        {
            if (assembly.IsDynamic || mAssemblies.ContainsKey(assembly.GetName().Name))
                return;

            mAssemblies.Add(assembly.GetName().Name, assembly);

            AssemblyName[] refAssemblies = assembly.GetReferencedAssemblies();
            bool found = false;
            foreach (var item in refAssemblies)
            {
                if (string.Compare(item.Name, mProtoInterface, true, System.Globalization.CultureInfo.InvariantCulture) == 0)
                {
                    found = true;
                    break;
                }
            }

            //Doesn't reference ProtoInterface.
            if (!found)
                return;

            InitializeExtensionApp(assembly);
        }

        private void InitializeExtensionApp(Assembly assembly)
        {
            Type extensionAppType = typeof(IExtensionApplication);
            Type assemblyAttribute = typeof(Autodesk.DesignScript.Runtime.ExtensionApplicationAttribute);
            System.Type appType = CLRDLLModule.GetImplemetationType(assembly, extensionAppType, assemblyAttribute, true);

            if (null == appType)
                return;

            IExtensionApplication extesionApp = null;
            lock (mAssemblies)
            {
                if (!mAssemblies.ContainsKey(assembly))
                {
                    extesionApp = (IExtensionApplication)Activator.CreateInstance(appType, true);
                    mExtensionApps.Add(appType, extesionApp);
                }
            }

            if (null != extesionApp)
                extesionApp.StartUp();
        }

        /// <summary>
        /// For nunit-setup
        /// </summary>
        internal void ForceStartUpAllApps()
        {
            IDictionaryEnumerator i = mExtensionApps.GetEnumerator();
            while (i.MoveNext())
            {
                IExtensionApplication app = i.Value as IExtensionApplication;
                if (null != app)
                    app.StartUp();
            }
        }

        /// <summary>
        /// For nunit-teardown
        /// </summary>
        internal void ForceShutDownAllApps()
        {
            IDictionaryEnumerator i = mExtensionApps.GetEnumerator();
            while (i.MoveNext())
            {
                IExtensionApplication app = i.Value as IExtensionApplication;
                if (null != app)
                    app.ShutDown();
            }
        }
    }
}
