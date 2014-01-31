using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphToDSCompiler
{
    /// <summary>
    /// An Exception/Errors List class for Graph Compilation
    /// </summary>
    public class GraphCompilationStatus
    {
        private readonly List<string> errors=new List<string>();
        public List<string> Errors
        {
            get
            {
                return errors;
            }
        }
        public void LogError(string msg, int line = -1, int col = -1)
        {          
            errors.Add(msg);
        }
        /// <summary>
        /// Record an internal error that represents an invalid system state
        /// </summary>
        /// <param name="e"></param>
        private static void LogInternalError(Exception e)
        {
            LogInternalError(e.Message);
        }


        /// <summary>
        /// Record an internal error that represents an invalid system state
        /// </summary>
        /// <param name="message">The message to be logged</param>
        private static void LogInternalError(String message)
        {
            DateTime dt = DateTime.Now;
            System.Diagnostics.Debug.WriteLine(dt.ToString("yyyy-MM-dd HH:mm:ss - " + message));
        }

        /// <summary>
        /// Process an exception
        /// </summary>
        /// <param name="e"></param>
        public static void HandleError(Exception e)
        {
            LogInternalError(e);
            throw e;

        }
    }
}
