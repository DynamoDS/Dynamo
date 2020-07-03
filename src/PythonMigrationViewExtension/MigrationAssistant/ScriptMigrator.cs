using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Python.Included;

namespace Dynamo.PythonMigration.MigrationAssistant
{
    internal static class ScriptMigrator
    {
        private const string INPUT_NAME = "code";
        private const string RETURN_NAME = "output";

        /// <summary>
        /// Migrates Python 2 code to Python 3 using Pythons 2to3 library. 
        /// </summary>
        /// <param name="code">Python 2 code that needs to be migrated</param>
        /// <returns></returns>
        internal static string MigrateCode(string code)
        {
            Installer.SetupPython().Wait();

            if (!PythonEngine.IsInitialized)
            {
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
            }

            IntPtr gs = PythonEngine.AcquireLock();

            try
            {
                using (Py.GIL())
                {

                    using (PyScope scope = Py.CreateScope())
                    {
                        scope.Set(INPUT_NAME, code.ToPython());
                        scope.Exec(GetPythonMigrationScript());

                        var result = scope.Contains(RETURN_NAME) ? scope.Get(RETURN_NAME) : null;
                        return result.ToString();
                    }
                }
            }

            catch (PythonException pe)
            {
                throw;
            }
            finally
            {
                PythonEngine.ReleaseLock(gs);
            }
        }

        private static string GetPythonMigrationScript()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string script;
            using (var reader = 
                new StreamReader(asm.GetManifestResourceStream("Dynamo.PythonMigration.MigrationAssistant.migrate_2to3.py")))
            {
                script = reader.ReadToEnd();
            }
            return script;
        }
    }
}
