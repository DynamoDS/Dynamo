using System;
using System.Linq;
using Autodesk.DesignScript.Runtime;

namespace DSCore
{
    [IsVisibleInDynamoLibrary(false)]
    public class Types
    {
        public static Type FindTypeByNameInAssembly(string typeName, string assemblyName)
        {
            var found = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(x => x.GetName().Name == assemblyName);

            if(found == null)
                throw new Exception(string.Format("Could not find {0} in the loaded assemblies.", assemblyName));

            var type = found.GetTypes().First(x => (x.Name == typeName && x.IsPublic == true));

            if(type == null)
                throw new Exception(string.Format("Could not find {0} in the loaded types.", typeName));

            return type;
        }
    }
}
