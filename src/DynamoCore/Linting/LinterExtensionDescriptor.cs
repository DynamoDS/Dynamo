using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Linting
{
    public class LinterExtensionDescriptor
    {
        /// <summary>
        /// Id of linter extension
        /// </summary>
        public string Id { get; }

        public string Name { get; }

        public LinterExtensionDescriptor(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
