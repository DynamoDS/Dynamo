using System;
using System.Collections.Generic;

namespace Autodesk.DesignScript.Interfaces
{
    /// <summary>
    /// Provides application configuration
    /// </summary>
    internal interface IConfiguration
    {
        /// <summary>
        /// Provides the path of main executing script
        /// </summary>
        string RootModulePath { get; }

        /// <summary>
        /// Provides list of include directories.
        /// </summary>
        string[] IncludeDirectories { get; }

        /// <summary>
        /// Returns application defined configuration value.
        /// </summary>
        /// <param name="config">Configuration name</param>
        /// <returns>Configuration value</returns>
        object GetConfigValue(string config);

        /// <summary>
        /// Set application defined configuration value.
        /// </summary>
        /// <param name="config">Configuration name</param>
        /// <param name="value">Configuration value</param>
        void SetConfigValue(string config, object value);
    }

    internal class ConfigurationKeys
    {
        /// <summary>
        /// This key is used to configure the library filename, which implements 
        /// IGeometryFactory interface.
        /// </summary>
        public static readonly string GeometryFactory = "GeometryFactoryFileName";

        /// <summary>
        /// This key is used to configure the library filename, which implements 
        /// IPersistenceManager interface.
        /// </summary>
        public static readonly string PersistentManager = "PersistentManagerFileName";

        /// <summary>
        /// This key is used to set/get IContextDataProvider implementation
        /// by host application.
        /// </summary>
        public static readonly string GeometryProvider = "GeometryProvider";

        /// <summary>
        /// This key is used to set the session database object for the host
        /// application. THIS IS NOW DEPRECATED (use "SessionKey" instead).
        /// </summary>
        public static readonly string SessionDatabase = "SessionDatabase";

        /// <summary>
        /// This key is used to set the session key (by the host application) 
        /// which is understood by the corresponding IPersistenceManager when 
        /// it comes to telling one session from another. An example of 
        /// session would be the documents in the host application, a host 
        /// document can be identified with a session key, and each host 
        /// document has a unique session key that IPersistenceManager can use
        /// to differentiate between two documents. The corresponding value 
        /// for SessionKey is a value of "string" type.
        /// </summary>
        public static readonly string SessionKey = "SessionKey";

        /// <summary>
        /// This key is used to request explicit lock on database by the host
        /// application before execution of the script. The corresponding value
        /// is bool.
        /// </summary>
        public static readonly string RequestExplicitLock = "RequestExplicitLock";

        /// <summary>
        /// This key is used to check if the application is recording user actions.
        /// The corresponding value is bool.
        /// </summary>
        public static readonly string RecordingUserActions = "RecordingUserActions";

        /// <summary>
        /// This key is used to determine if the persistent objects should be cleared on 
        /// screen before each run. In live execution scenarios like DesignScript Studio,
        /// there is no clear distinction between runs. For more information, please see 
        /// "AsmExtensionApplication::OnBeginExecution" in "AsmExtensionApplication.cpp".
        /// </summary>
        public static readonly string ClearPersistedObjects = "ClearPersistedObjects";

        /// <summary>
        /// This key is being referenced in "DesignScriptStudio.Graph.Ui.GraphControl"
        /// as a way to determine if the underlying "RenderService" should be enabled.
        /// The default behaviour (if this flag is not specified) is to enable geometric 
        /// preview. The "object" value being passed for this configuration key is 
        /// expected to be a "bool" object rather than a "string" object.
        /// </summary>
        public static readonly string GeometricPreviewEnabled = "GeometricPreviewEnabled";

        /// <summary>
        /// This key is being referenced in "DesignScriptStudio.Graph.Ui.GraphControl"
        /// as a way to filtered out classes that is currently not supported in ProtoGL.
        /// The "object" value being passed for this configuration key is 
        /// expected to be a "string" object which specified the assembly and class that
        /// should be hidden. The location of the config file is "\installtools\Bundle\
        /// DesignScript.bundle\Contents\Win64" The format of the string should be 
        /// "%assemblyName%;%className&amp;;..;%assemblyName%;%className&amp;;", 
        /// The CoreCoponent will look for "%assemblyName%;%className&amp;;" (with two ';' at
        /// end of assembly name and class name) and filter the fully matched item out
        /// </summary>
        public static readonly string FilteredClasses = "FilteredClasses";

        /// <summary>
        /// This key is referenced in the geometry test framework.
        /// The value type for this key is bool.
        /// If the value is true, that means the core will generate XML properties.
        /// </summary>
        public static readonly string GeometryXmlProperties = "GeometryXmlProperties";
    }

    /// <summary>
    /// Represents a session object for current execution.
    /// </summary>
    internal interface IExecutionSession
    {
        /// <summary>
        /// Returns the configuration object for this execution session.
        /// </summary>
        IConfiguration Configuration { get; }

        /// <summary>
        /// Searches the given file and returns full path, if found.
        /// </summary>
        /// <param name="filename">File name to search.</param>
        /// <returns>Full path of given filename or empty string if file not 
        /// found.</returns>
        string SearchFile(string filename);
    }

    /// <summary>
    /// An options structure used when calling IExtensionApplication.Startup
    /// </summary>
    public struct ExtensionStartupParams
    {
        /// <summary>
        /// Disables ADP data collection.
        /// </summary>
        public bool DisableADP
        {
            get;
            set;
        }
    }

    /// <summary>
    /// An FFI library can implement this interface to get some notifications
    /// from DesignScript application.
    /// </summary>
    internal interface IExtensionApplication
    {
        /// <summary>
        /// Called when first time this application is loaded.
        /// </summary>
        void StartUp(ExtensionStartupParams startParams);

        /// <summary>
        /// Called when script execution starts.
        /// </summary>
        /// <param name="session">Execution session object with which script
        /// execution starts.</param>
        void OnBeginExecution(IExecutionSession session);

        /// <summary>
        /// Called when script execution is suspended for debugging/inspection
        /// </summary>
        /// <param name="session">Execution session object with which script
        /// execution starts.</param>
        void OnSuspendExecution(IExecutionSession session);

        /// <summary>
        /// Called when script execution is resumed after debugging/inspection
        /// </summary>
        /// <param name="session">Execution session object with which script
        /// execution starts.</param>
        void OnResumeExecution(IExecutionSession session);

        /// <summary>
        /// Called when script execution has ended.
        /// </summary>
        /// <param name="session">Execution session object with which script
        /// execution had started.</param>
        void OnEndExecution(IExecutionSession session);

        /// <summary>
        /// Called when designscript application is shutting down.
        /// </summary>
        void ShutDown();

        /// <summary>
        /// Preloads unmanaged ASM libraries 
        /// </summary>
        /// <param name="baseDirectory"></param>
        void PreloadAsmLibraries(string baseDirectory);
    }

    internal interface IExternalCodeCompletionProviderCore
    {

        /// <summary>
        /// Already discovered variable types
        /// </summary>
        Dictionary<string, Type> VariableTypes { get; set; }

        /// <summary>
        /// Types that have already been imported into the scope
        /// </summary>
        Dictionary<string, Type> ImportedTypes { get; set; }

        /// <summary>
        /// Generate completion data for the specified text, while import the given types into the
        /// scope and discovering variable assignments.
        /// </summary>
        /// <param name="code">The code to parse</param>
        /// <param name="expand">Determines if the entire namespace should be used</param>
        /// <returns>Return a list of IronPythonCompletionData </returns>
        IExternalCodeCompletionData[] GetCompletionData(string code, bool expand = false);

        /// <summary>
        /// Try to generate a description from a typename.
        /// </summary>
        /// <param name="stub">Everything before the last namespace or type name e.g. System.Collections in System.Collections.ArrayList</param>
        /// <param name="item">Everything after the stub</param>
        /// <param name="isInstance">Whether it's an instance or not</param>
        string GetDescription(string stub, string item, bool isInstance);

        /// <summary>
        /// Used to determine if this IExternalCodeCompletionProviderCore can provide completions for the given engine.
        /// </summary>
        /// <param name="engineName"></param>
        /// <returns></returns>
        bool IsSupportedEngine(string engineName);

        /// <summary>
        /// Used to load initialize libraries and types that should be available by default.
        /// </summary>
        /// <param name="dynamoCorePath"></param>
        void Initialize(string dynamoCorePath);

    }


    /// <summary>
    /// This interface is essentially the same as ICompletionData from AvalonEdit 
    /// except it does not include any references to WPF.
    /// </summary>
    internal interface IExternalCodeCompletionData
    {
        /// <summary>
        /// The CompletionType this completionData represents.
        /// </summary>
        ExternalCodeCompletionType CompletionType { get; }
        /// <summary>
        ///  Gets the text. This property is used to filter the list of visible elements.
        /// </summary>
        string Text { get; }
        /// <summary>
        ///    The displayed content. This can be the same as 'Text', or a WPF UIElement if
        ///     you want to display rich content.
        /// </summary>
        object Content { get; }
        /// <summary>
        ///   Gets the description.
        /// </summary>
        string Description { get; }
        /// <summary>
        ///  Gets the priority. This property is used in the selection logic. You can use
        ///   it to prefer selecting those items which the user is accessing most frequently.
        /// </summary>
        double Priority { get; }
    }

    /// <summary>
    /// This Enum represents the current completion type that is found.
    /// Currently this is used to decorate the returned completion with an image.
    /// </summary>
    internal enum ExternalCodeCompletionType
    {
        Other,
        Namespace,
        Method,
        Field,
        Class,
        Property,
        Enum,
    };

    /// <summary>
    /// This internal interface exists to support the legacy IronPythonCompletionProvider 
    /// without having a compile time reference from PythonNodeModel/WPF on DSIronPython.
    /// This interface should be removed when the legacy class is removed in 3.0.
    /// </summary>
    internal interface ILegacyPythonCompletionCore
    {
        void UpdateImportedTypes(string code);
        void UpdateVariableTypes(string code);
        Dictionary<string, Type> FindAllVariableAssignments(string code);
        Dictionary<string, Tuple<string, int, Type>> FindAllVariables(string code);
        Type TryGetType(string name);
        IEnumerable<Tuple<string, string, bool, ExternalCodeCompletionType>> EnumerateMembers(object module, string name);
        IEnumerable<Tuple<string, string, bool, ExternalCodeCompletionType>> EnumerateMembers(Type type, string name);
        IEnumerable<Tuple<string, string, bool, ExternalCodeCompletionType>> EnumerateMembersFromTracker(object nameSpaceTracker, string name);

        object LookupMember(string name, object nameSpaceTracker);
        object LookupMember(string name);
        object Engine { get; set; }
        object Scope { get; set; }
    }

}
