using System;

namespace Dynamo.Events
{
    /// <summary>
    /// This class is intended to support handling of shutdown and startup of the legacy ExtensionApp classes
    /// which implements the <see cref="Autodesk.DesignScript.Interfaces.IExtensionApplication"/> interface.
    /// </summary>
    public class ExtensionAppEvents
    {

        public static Action<CancelableExtensionsShutdownArgs> TerminatingExtensionApplication;

        internal static void OnTerminateExtensionApplication(CancelableExtensionsShutdownArgs args)
        {
            TerminatingExtensionApplication(args);
        }

    }

    public class CancelableExtensionsShutdownArgs
    {
        public bool CancelShutdown { get; set; } = false;
        public string ExtensionAppName { get; }

        internal CancelableExtensionsShutdownArgs(string extensionAppName)
        {
            ExtensionAppName = extensionAppName;
        }
    }
}
