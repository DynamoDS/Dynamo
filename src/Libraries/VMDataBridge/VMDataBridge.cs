using System;
using System.Collections.Generic;

using Autodesk.DesignScript.Runtime;

using ProtoCore.AST.AssociativeAST;

namespace VMDataBridge
{
    /// <summary>
    ///     Provides callback registration by GUID, allows for hooking Actions into the VM.
    /// </summary>
    public static class DataBridge
    {
        private static readonly Dictionary<Guid, Action<object>> callbacks =
            new Dictionary<Guid, Action<object>>();

        /// <summary>
        ///     Registers a callback for a given GUID.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="callback"></param>
        [SupressImportIntoVM]
        public static void RegisterCallback(Guid id, Action<object> callback)
        {
            callbacks[id] = callback;
        }

        /// <summary>
        ///     Unregisters a callback for a given GUID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [SupressImportIntoVM]
        public static bool UnregisterCallback(Guid id)
        {
            return callbacks.Remove(id);
        }

        /// <summary>
        ///     Calls the registered callback for the given guid string
        ///     with the given data. This is safe to include in standalone
        ///     DS scripts, since if there are no callbacks registered
        ///     then the method will do nothing.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="data"></param>
        [IsVisibleInDynamoLibrary(false)]
        public static void BridgeData(string guid, [ArbitraryDimensionArrayImport] object data)
        {
            Guid id;
            if (!Guid.TryParse(guid, out id))
                return;

            Action<object> callback;
            if (callbacks.TryGetValue(id, out callback))
                callback(data);
        }

        /// <summary>
        ///     Produces AST that, when executed by the VM, will perform Data Bridging
        ///     by calling BridgeData.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="input"></param>
        [SupressImportIntoVM]
        public static AssociativeNode GenerateBridgeDataAst(Guid id, AssociativeNode input)
        {
            Action<string, object> bridgeData = BridgeData;

            return AstFactory.BuildFunctionCall(
                bridgeData,
                new List<AssociativeNode> { AstFactory.BuildStringNode(id.ToString()), input });
        }
    }
}
