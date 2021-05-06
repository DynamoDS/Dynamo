using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Graph;

namespace Dynamo.Extensions
{
    public interface IExtensionStorageAccess
    {
        /// <summary>
        /// A unique id for this extension instance. 
        /// 
        /// The id will be equivalent to the extension that implements this interface id.
        /// </summary>
        string UniqueId { get; }

        /// <summary>
        /// A name for the Extension.
        /// 
        /// The name will be equivalent to the extension that implements this interface name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Action to be invoked when the workspace is opened. The passed extensionData dictionary is a copy of the <see cref="ExtensionData"/> data dictionary.
        /// Modifying the extensionData from this action will not modify the stored <see cref="ExtensionData"/>.
        /// To modify the stored <see cref="ExtensionData"/> data dictionary use the <see cref="OnWorkspaceSaving(Dictionary{string, string}, SaveContext)"/>.
        /// </summary>
        /// <param name="extensionData">A copy of the ExtensionData dictionary</param>
        void OnWorkspaceOpen(Dictionary<string, string> extensionData);

        /// <summary>
        /// Action to be invoked when the workspace has begun its saving process.
        /// The passed extensionData dictionary is a direct reference to the stored <see cref="ExtensionData"/> data dictionary, modifications to this dictionary
        /// will be reflected in the stored <see cref="ExtensionData"/> data dictionary
        /// </summary>
        /// <param name="extensionData"></param>
        /// <param name="saveContext"></param>
        void OnWorkspaceSaving(Dictionary<string, string> extensionData, SaveContext saveContext);
    }
}
