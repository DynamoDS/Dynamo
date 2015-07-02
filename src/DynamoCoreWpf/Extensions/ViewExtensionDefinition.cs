using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Dynamo.Wpf.Extensions
{
    [DataContract]
    internal class ViewExtensionDefinition
    {
        [DataMember]
        public string AssemblyName { get; set; }

        [DataMember]
        public string TypeName { get; set; }

        /// <summary>
        /// Indicates path to xml file with extension definition.
        /// I.e. *_ViewExtensionDefinition.xml
        /// </summary>
        public string ExtensionPath { get; set; }

        /// <summary>
        /// Gets path to extension assembly. 
        /// This assembly should be in folder with the same name as assembly's name.
        /// </summary>
        public string AssemblyLocation
        {
            get
            {
                var path = Path.GetDirectoryName(ExtensionPath);
                return Path.Combine(path, AssemblyName, AssemblyName + ".dll");                 
            }
        }
    }
}
