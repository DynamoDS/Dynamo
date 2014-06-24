using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Runtime;

namespace DSCore
{
    /// <summary>
    ///     Provides callback registration by GUID, allows for hooking Actions into the VM.
    /// </summary>
    public static class VMDataBridge
    {
        private static readonly Dictionary<Guid, Action<object>> Callbacks = new Dictionary<Guid, Action<object>>();

        /// <summary>
        ///     Registers a callback for a given GUID.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="callback"></param>
        [SupressImportIntoVM]
        public static void RegisterCallback(Guid id, Action<object> callback)
        {
            Callbacks[id] = callback;
        }

        /// <summary>
        ///     Unregisters a callback for a given GUID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [SupressImportIntoVM]
        public static bool UnregisterCallback(Guid id)
        {
            return Callbacks.Remove(id);
        }

        /// <summary>
        ///     Calls the registered callback for the given guid string
        ///     with the given data. This is safe to include in standalone
        ///     DS scripts, since if there are no callbacks registered
        ///     then the method will do nothing.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="data"></param>
        public static void BridgeData(string guid, [ArbitraryDimensionArrayImport] object data)
        {
            Guid id;
            if (!Guid.TryParse(guid, out id))
                return;

            Action<object> callback;
            if (Callbacks.TryGetValue(id, out callback))
                callback(data);
        }
    }
}
