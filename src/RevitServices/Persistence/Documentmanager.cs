using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;

namespace RevitServices.Persistence
{

    /// <summary>
    /// Singleton class to manage Revit document resources
    /// </summary>
    public class DocumentManager
    {

        private static DocumentManager instance = null;
        private static Object mutex = new Object();

        public static DocumentManager GetInstance()
        {
            lock (mutex)
            {
                if (instance == null)
                    instance = new DocumentManager();

                return instance;
            }

        }

        private DocumentManager()
        {
                
        }

        /// <summary>
        /// Provide source of the currently active document
        /// Dynamo is reponsible for updating this before use
        /// </summary>
        public Document CurrentDBDocument { get; set; }

    }
}
