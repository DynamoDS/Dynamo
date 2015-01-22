using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ProtoCore.DSASM.Mirror;

namespace ProtoTestFx
{
    public class InterfaceFactory
    {
        public static IDebugService CreateDebugService(string geometryFactoryName, string persistenceManagerName)
        {
            DebugService debugService = new DebugService();
            debugService.GeometryFactoryName = geometryFactoryName;
            debugService.PersistenceManagerName = persistenceManagerName;
            return debugService;
        }
    }


    public interface IDebugService
    {
        bool RunScript(string dsPath);
        bool DebugScript(string dsPath);
        void RunScriptWithInjection(string dsPath, string outputPath);
        string GetCoreDump();
    }

    public class DebugService : IDebugService
    {
        System.Text.StringBuilder executionLog = null;

        public string GeometryFactoryName
        {
            get;
            set;
        }

        public string PersistenceManagerName
        {
            get;
            set;
        }

        public bool RunScript(string dsPath)
        {
            if (string.IsNullOrEmpty(GeometryFactoryName))
                throw new Exception("GeometryFactory not set!");

            if (string.IsNullOrEmpty(PersistenceManagerName))
                throw new Exception("PersistenceManager not set!");

            if (!File.Exists(dsPath))
                throw new FileNotFoundException(dsPath + " Does not exist");

            bool success = false;
            System.IO.StringWriter stringStream = new StringWriter();
            executionLog = new StringBuilder();
            ProtoCore.Core core = null;
            try
            {
                var options = new ProtoCore.Options();
                string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string incDir = Path.GetDirectoryName(assemblyLocation);
                options.IncludeDirectories.Add(incDir);

                core = new ProtoCore.Core(options);
                core.BuildStatus.SetStream(stringStream);
                core.Options.RootModulePathName = ProtoCore.Utils.FileUtils.GetFullPathName(dsPath);
                core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
                core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
                core.Configurations.Add(Autodesk.DesignScript.Interfaces.ConfigurationKeys.GeometryFactory, GeometryFactoryName);
                core.Configurations.Add(Autodesk.DesignScript.Interfaces.ConfigurationKeys.PersistentManager, PersistenceManagerName);
                ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
                ExecutionMirror mirror = fsr.LoadAndExecute(dsPath, core);
                executionLog.AppendLine("Script executed successfully.");

                executionLog.AppendLine();
                executionLog.AppendLine("=================================CoreDump=================================");
                string coreDump = null;
                try
                {
                    coreDump = mirror.GetCoreDump();
                    executionLog.AppendLine();
                    executionLog.AppendLine(coreDump);
                    success = true;
                }
                catch (System.Exception ex)
                {
                    executionLog.AppendLine(ex.Message);
                    executionLog.AppendLine(ex.StackTrace);

                    success = false;
                }
                finally
                {
                    executionLog.AppendLine("=================================CoreDump=================================");
                }
                
            }
            catch (System.Exception ex)
            {
                success = false;
                executionLog.AppendLine("Fail to execute script.");
                executionLog.AppendLine("Exceptions:");
                executionLog.AppendLine(ex.Message);
                executionLog.AppendLine("StackTrace:");
                executionLog.AppendLine(ex.StackTrace);
            }
            finally
            {
                if (core != null)
                {
                    core.BuildStatus.SetStream(null);
                    core.Cleanup();
                }
            }

            return success;
        }

        public bool DebugScript(string dsPath)
        {
            if (string.IsNullOrEmpty(GeometryFactoryName))
                throw new Exception("GeometryFactory not set!");

            if (string.IsNullOrEmpty(PersistenceManagerName))
                throw new Exception("PersistenceManager not set!");

            if (!File.Exists(dsPath))
                throw new FileNotFoundException(dsPath + " Does not exist");

            bool success = false;
            System.IO.StringWriter stringStream = new StringWriter();
            executionLog = new StringBuilder();
            ProtoCore.Core core = null;
            try
            {
                core = new ProtoCore.Core(new ProtoCore.Options());
                core.BuildStatus.SetStream(stringStream);
                core.Options.RootModulePathName = ProtoCore.Utils.FileUtils.GetFullPathName(dsPath);
                core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
                core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
                core.Configurations.Add(Autodesk.DesignScript.Interfaces.ConfigurationKeys.GeometryFactory, GeometryFactoryName);
                core.Configurations.Add(Autodesk.DesignScript.Interfaces.ConfigurationKeys.PersistentManager, PersistenceManagerName);
                ProtoScript.Runners.DebugRunner debugRunner = new ProtoScript.Runners.DebugRunner(core);
                ProtoScript.Config.RunConfiguration runnerConfig = new ProtoScript.Config.RunConfiguration();
                runnerConfig.IsParrallel = false; 
                ExecutionMirror mirror; 
                debugRunner.LoadAndPreStart(dsPath, runnerConfig);
                ProtoScript.Runners.DebugRunner.VMState currentVmState ;

               

                executionLog.AppendLine("Script executed successfully.");                

                executionLog.AppendLine();
                executionLog.AppendLine("=================================CoreDump=================================");
                string coreDump;
                bool step = true;

                while (step == true)
                {

                    try
                    {
                        currentVmState = debugRunner.StepOver(); // Perform one step.
                        mirror = currentVmState.mirror;
                        coreDump = null;
                        coreDump = mirror.GetCoreDump();
                        executionLog.AppendLine();
                        executionLog.AppendLine(coreDump);
                        success = true;
                        if (currentVmState.isEnded == true)
                            step = false;
                    }
                    catch (System.Exception ex)
                    {
                        executionLog.AppendLine(ex.Message);
                        executionLog.AppendLine(ex.StackTrace);

                        success = false;
                    }
                }

                executionLog.AppendLine("=================================CoreDump=================================");


            }
            catch (System.Exception ex)
            {
                success = false;
                executionLog.AppendLine("Fail to execute script.");
                executionLog.AppendLine("Exceptions:");
                executionLog.AppendLine(ex.Message);
                executionLog.AppendLine("StackTrace:");
                executionLog.AppendLine(ex.StackTrace);
            }
            finally
            {
                if (core != null)
                {
                    core.BuildStatus.SetStream(null);                    
                }
            }

            return success;
        }

        public void RunScriptWithInjection(string dsPath, string outputPath)
        {
            executionLog = new StringBuilder();
            executionLog.AppendLine(string.Format("[RunScriptWithInjection]Start test case: {0}", dsPath));
            ProtoTestFx.AdharnessTest.RunAndGenerateBase(Path.GetDirectoryName(dsPath), dsPath, outputPath);
            executionLog.AppendLine(string.Format("[RunScriptWithInjection]Ended test case: {0}", dsPath));
        }

        public string GetCoreDump()
        {
            return executionLog.ToString();
        }
    }
}
