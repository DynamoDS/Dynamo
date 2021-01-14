using System;
using System.Collections.Generic;

namespace Dynamo.Extensions
{
    public class ExtensionData
    {
        /// <summary>
        /// Extensions unique id.
        /// </summary>
        public string ExtensionGuid { get; private set; }

        /// <summary>
        /// Name of extension.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Extension version.
        /// </summary>
        public string Version { get; private set; }

        /// <summary>
        /// Extension specific data.
        /// </summary>
        public Dictionary<string,string> Data { get; set; }

        public ExtensionData(string extensionGuid, string name, string version, Dictionary<string, string> data)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or empty", nameof(name));
            }

            if (string.IsNullOrEmpty(version))
            {
                throw new ArgumentException($"'{nameof(version)}' cannot be null or empty", nameof(version));
            }

            ExtensionGuid = extensionGuid ?? throw new ArgumentNullException(nameof(extensionGuid));
            Data = data ?? throw new ArgumentNullException(nameof(data));

            Name = name;
            Version = version;
        }
    }
}
