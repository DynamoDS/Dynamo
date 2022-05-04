using System;
using System.IO;
using System.Reflection;
using Python.Runtime;

namespace Dynamo.PythonMigration.MigrationAssistant
{
    internal static class ScriptMigrator
    {
        private const string INPUT_NAME = "code";
        private const string PATH_NAME = "path_name";
        private const string RETURN_NAME = "output";

        /// <summary>
        /// Migrates Python 2 code to Python 3 using Pythons 2to3 library. 
        /// </summary>
        /// <param name="code">Python 2 code that needs to be migrated</param>
        /// <returns></returns>
        internal static string MigrateCode(string code)
        {
            DSCPython.CPythonEvaluator.InstallPython();

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
                    string output;
                    var asm = Assembly.GetExecutingAssembly();

                    using (PyScope scope = Py.CreateScope())
                    {
                        scope.Set(INPUT_NAME, code.ToPython());

                        var path = Path.GetDirectoryName(asm.Location);

                        scope.Set(PATH_NAME, path.ToPython());
                        scope.Exec(Get2To3MigrationScript(asm));

                        output = scope.Contains(RETURN_NAME) ? scope.Get(RETURN_NAME).ToString() : string.Empty;
                    }

                    // If the code contains tabs, normalize the whitespaces. This is a Python 3 requirement
                    // that's not addressed by 2to3.
                    if (output.Contains("\t"))
                    {
                        using (PyScope scope = Py.CreateScope())
                        {
                            scope.Set(INPUT_NAME, output.ToPython());
                            scope.Exec(GetReindentationScript(asm));
                            output = scope.Contains(RETURN_NAME) ? scope.Get(RETURN_NAME).ToString() : string.Empty;
                        }
                    }

                    return output;
                }
            }

            finally
            {
                PythonEngine.ReleaseLock(gs);
            }
        }

        private static string Get2To3MigrationScript(Assembly asm)
        {
            return GetEmbeddedScript(asm, "Dynamo.PythonMigration.MigrationAssistant.migrate_2to3.py");
        }

        private static string GetReindentationScript(Assembly asm)
        {
            return GetEmbeddedScript(asm, "Dynamo.PythonMigration.MigrationAssistant.reindent.py");
        }

        private static string GetEmbeddedScript(Assembly asm, string resourceName)
        {
            string script;
            using (var reader =
                new StreamReader(asm.GetManifestResourceStream(resourceName)))
            {
                script = reader.ReadToEnd();
            }
            return script;
        }
    }
}
