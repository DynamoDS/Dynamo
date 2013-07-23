using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.LibG;

namespace Dynamo
{
    public class DynamoAsm
    {
        DynamoAsm()
        {
            Start();
        }

        ~DynamoAsm()
        {
            End();
        }

        public static void EnsureStarted()
        {
            if (_instance == null)
                _instance = new DynamoAsm();
        }

        private static bool _hasStartde = false;
        private static DynamoAsm _instance = null;

        public static bool HasShutdown = false;

        public static void Start()
        {
            LibG.start_asm_library();
            LibG.start_viewer();
            LibG.start_window_link();
        }

        public static void End()
        {
            HasShutdown = true;

            LibG.end_window_link();
            LibG.end_viewer();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            if (assemblies.Any(x => x.FullName.Contains("RevitAPI")) && assemblies.Any(x => x.FullName.Contains("RevitAPIUI")))
                return;

            LibG.end_asm_library();
        }

    }
}
