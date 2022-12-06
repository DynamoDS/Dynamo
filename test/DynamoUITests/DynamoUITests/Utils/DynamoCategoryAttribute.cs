using System;
using System.ComponentModel;
using System.Reflection;
using System.Resources;

namespace DynamoTests.Utils
{
    public class DynamoCategoryAttribute : CategoryAttribute
    {
        private static ResourceManager resourceManager 
            = new ResourceManager("DynamoTests.StaticFiles.Resources.CategoryResource", Assembly.GetExecutingAssembly());

        public DynamoCategoryAttribute(string name)
            : base(
            ((Func<string>)delegate
            {
                return resourceManager.GetString(name);
            })())
        {
        }

    }
}
