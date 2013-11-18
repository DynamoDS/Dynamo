using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;

namespace RevitPersistenceManager
{
    public class DocumentManager
    {
        //TODO(Luke): Fix the Singleton ritual stuff

        /// <summary>
        /// Provide source of the currently active document
        /// Dynamo is reponsible for updating this
        /// </summary>
        public static Document CurrentDoc;

    }
}
