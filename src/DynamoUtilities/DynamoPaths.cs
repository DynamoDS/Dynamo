using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace DynamoUtilities
{
    /// <summary>
    /// DynamoPaths stores paths to dynamo libraries and assets.
    /// </summary>
    public static class DynamoPaths
    {
        /// <summary>
        /// The main execution path of Dynamo. This is the directory
        /// which contains DynamoCore.dll
        /// </summary>
        public static string MainExecPath { get; set; }

        /// <summary>
        /// The definitions folder, which contains custom nodes
        /// created by the user.
        /// </summary>
        public static string Definitions { get; set; }

        /// <summary>
        /// The packages folder, which contains pacakages downloaded
        /// with the package manager.
        /// </summary>
        public static string Packages { get; set; }

        /// <summary>
        /// The UI folder, which contains the UI resources.
        /// </summary>
        public static string Ui { get; set; }

        /// <summary>
        /// The ASM folder which contains LibG and the 
        /// ASM binaries.
        /// </summary>
        public static string Asm { get; set; }

        // All 'nodes' folders.
        public static HashSet<string> Nodes { get; set; } 

        /// <summary>
        /// Provided a main execution path, find other Dynamo paths
        /// relatively. This operation should be called only once at
        /// the beginning of a Dynamo session.
        /// </summary>
        /// <param name="mainExecPath">The main execution directory of Dynamo.</param>
        public static void SetupDynamoPaths(string mainExecPath)
        {
            if (Directory.Exists(mainExecPath))
            {
                MainExecPath = mainExecPath;
            }
            else
            {
                throw new Exception(string.Format("The specified main execution path: {0}, does not exist.", mainExecPath));
            }

            Definitions = Path.Combine(MainExecPath, "definitions");
            Packages = Path.Combine(MainExecPath , "dynamo_packages");
            Asm = Path.Combine(MainExecPath, "dll");
            Ui = Path.Combine(MainExecPath , "UI");

            if (Nodes == null)
            {
                Nodes = new HashSet<string>();
            }

            // Only register the core nodes directory
            Nodes.Add(Path.Combine(MainExecPath, "nodes"));

#if DEBUG
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("MainExecPath: {0}", MainExecPath));
            sb.AppendLine(string.Format("Definitions: {0}", Definitions));
            sb.AppendLine(string.Format("Packages: {0}", Packages));
            sb.AppendLine(string.Format("Ui: {0}", Asm));
            sb.AppendLine(string.Format("Asm: {0}", Ui));
            Nodes.ToList().ForEach(n=>sb.AppendLine(string.Format("Nodes: {0}", n)));
            
            Debug.WriteLine(sb);
#endif

        }
    }
}
