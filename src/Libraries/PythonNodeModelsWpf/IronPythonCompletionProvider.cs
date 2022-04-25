using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Logging;
using Dynamo.PythonServices;
using ICSharpCode.AvalonEdit.CodeCompletion;
using PythonNodeModels;

namespace Dynamo.Python
{
    //This class needs to stay here with a reference to IronPython for now, its use will be
    //completely removed from Dynamo though - so the dependencey on Ironpython will ONLY be at compile time.
    [Obsolete("Do Not Use! This class will be removed in a future version of Dynamo," +
        "Instead Reference the completion providers for a specific scripting engine, which implement " +
        "IExternalCodeCompletionProvider")]
    /// <summary>
    /// Provides code completion for the Python Editor
    /// </summary>
    public class IronPythonCompletionProvider : LogSourceBase
    {
        private readonly IExternalCodeCompletionProviderCore providerImplementation;
        private const string providerTypeName = "Dynamo.PythonServices.PythonCodeCompletionProviderCommon, DynamoServices";

        #region Properties and fields

        /// <summary>
        /// The engine used for autocompletion.  This essentially keeps
        /// track of the state of the editor, allowing access to variable types and
        /// imported symbols.
        /// </summary>
        public object Engine
        {
            get { return (providerImplementation as ILegacyPythonCompletionCore).Engine; }
            set { (providerImplementation as ILegacyPythonCompletionCore).Engine = value; }
        }

        /// <summary>
        /// The scope used by the engine.  This is where all the loaded symbols
        /// are stored.  It's essentially an environment dictionary.
        /// </summary>
        public object Scope
        {
            get { return (providerImplementation as ILegacyPythonCompletionCore).Scope; }
            set { (providerImplementation as ILegacyPythonCompletionCore).Scope = value; }
        }

        /// <summary>
        /// Already discovered variable types
        /// </summary>
        public Dictionary<string, Type> VariableTypes
        {
            get => providerImplementation.VariableTypes;
            set => providerImplementation.VariableTypes = value;
        }


        /// <summary>
        /// Types that have already been imported into the scope
        /// </summary>
        public Dictionary<string, Type> ImportedTypes
        {
            get => providerImplementation.ImportedTypes;
            set => providerImplementation.ImportedTypes = value;
        }

        /// <summary>
        /// Maps a regex to a particular python type.  Useful for matching things like dicts,
        /// floats, strings, etc.  Initialized by
        /// </summary>
        public Dictionary<string, Type> RegexToType = new Dictionary<string, Type>();

        #endregion

        #region Constructors
        /// <summary>
        /// Class constructor
        /// </summary>
        public IronPythonCompletionProvider() : this("")
        {
        }

        /// <summary>
        /// Class constructor
        /// </summary>
        public IronPythonCompletionProvider(string dynamoCoreDir)
        {
            var versionName = PythonEngineManager.IronPython2EngineName;
            var matchingCore = SharedCompletionProvider.FindMatchingCodeCompletionCore(versionName, this.AsLogger());
            if (matchingCore != null)
            {
                this.providerImplementation = matchingCore;
                this.providerImplementation.Initialize(dynamoCoreDir);
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Generates completion data for the specified text, while import the given types into the 
        /// scope and discovering variable assignments.
        /// </summary>
        /// <param name="line">The code to parse</param>
        /// <returns>Return a list of IronPythonCompletionData </returns>
        public ICompletionData[] GetCompletionData(string line)
        {
            return GetCompletionData(line, false);
        }

        /// <summary>
        /// Generate completion data for the specified text, while import the given types into the
        /// scope and discovering variable assignments.
        /// </summary>
        /// <param name="code">The code to parse</param>
        /// <param name="expand">Determines if the entire namespace should be used</param>
        /// <returns>Return a list of IronPythonCompletionData </returns>
        public ICompletionData[] GetCompletionData(string code, bool expand = false)
        {
            return this.providerImplementation.GetCompletionData(code, expand).
                Select(x => new IronPythonCompletionData(x)).ToArray();
        }

        /// <summary>
        /// List all of the members in a PythonModule
        /// </summary>
        /// <param name="module">A reference to the module</param>
        /// <param name="name">The name of the module</param>
        /// <returns>A list of completion data for the module</returns>
        public List<IronPythonCompletionData> EnumerateMembers(object module, string name)
        {
            var items = new List<IronPythonCompletionData>();
            foreach (var completion in (providerImplementation as ILegacyPythonCompletionCore).EnumerateMembers(module, name))
            {
                var convertedCompletionType = IronPythonCompletionData.ConvertCompletionType(completion.Item4);

                items.Add(new IronPythonCompletionData(completion.Item1, completion.Item2, completion.Item3, convertedCompletionType, this));
            }
            return items;
        }

        /// <summary>
        /// List all of the members in a CLR Namespace
        /// </summary>
        /// <param name="ns">A reference to the module</param>
        /// <param name="name">The name of the module</param>
        /// <returns>A list of completion data for the namespace</returns>
        public List<IronPythonCompletionData> EnumerateMembersFromTracker(object ns, string name)
        {
            var items = new List<IronPythonCompletionData>();
            foreach (var completion in (providerImplementation as ILegacyPythonCompletionCore).EnumerateMembersFromTracker(ns, name))
            {
                var convertedCompletionType = IronPythonCompletionData.ConvertCompletionType(completion.Item4);

                items.Add(new IronPythonCompletionData(completion.Item1, completion.Item2, completion.Item3, convertedCompletionType, this));

            }
            return items;
        }

        /// <summary>
        /// List all of the members in a CLR type
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="name">The name for the type</param>
        /// <returns>A list of completion data for the type</returns>
        protected List<IronPythonCompletionData> EnumerateMembers(Type type, string name)
        {
            var items = new List<IronPythonCompletionData>();
            foreach (var completion in (providerImplementation as ILegacyPythonCompletionCore).EnumerateMembers(type, name))
            {
                var convertedCompletionType = IronPythonCompletionData.ConvertCompletionType(completion.Item4);

                items.Add(new IronPythonCompletionData(completion.Item1, completion.Item2, completion.Item3, convertedCompletionType, this));

            }
            return items;
        }

        /// <summary>
        /// Recursively lookup a member in a given namespace.
        /// </summary>
        /// <param name="name">A name for a type, possibly delimited by periods.</param>
        /// <param name="n">The namespace</param>
        /// <returns>The type as an object</returns>
        public object LookupMember(string name, object n)
        {
            return (this.providerImplementation as ILegacyPythonCompletionCore).LookupMember(name, n);
        }

        /// <summary>
        /// Recursively lookup a variable in the _scope
        /// </summary>
        /// <param name="name">A name for a type, possibly delimited by periods.</param>
        /// <returns>The type as an object</returns>
        public object LookupMember(string name)
        {
            return (this.providerImplementation as ILegacyPythonCompletionCore).LookupMember(name);
        }

        /// <summary>
        /// Generates completion data for the specified text. The text should be everything before
        /// the dot character that triggered the completion. The text can contain the command line prompt
        /// '>>>' as this will be ignored.
        /// </summary>
        public void GetDescription(string stub, string item, DescriptionUpdateDelegate updateDescription, bool isInstance)
        {
            string description = this.GetDescription(stub, item, isInstance);
            updateDescription(description);
        }

        /// <summary>
        /// Try to generate a description from a typename
        /// </summary>
        /// <param name="stub">Everything before the last namespace or type name e.g. System.Collections in System.Collections.ArrayList</param>
        /// <param name="item">Everything after the stub</param>
        /// <param name="isInstance">Whether it's an instance or not</param>
        public string GetDescription(string stub, string item, bool isInstance)
        {
            return this.providerImplementation.GetDescription(stub, item, isInstance);
        }

        /// <summary>
        /// A delegate used to update the description - useful for multi-threading
        /// </summary>
        /// <param name="description"></param>
        public delegate void DescriptionUpdateDelegate(string description);

        /// <summary>
        /// Traverse the given source code and define variable types based on
        /// the current scope
        /// </summary>
        /// <param name="code">The source code to look through</param>
        public void UpdateVariableTypes(string code)
        {
            (providerImplementation as ILegacyPythonCompletionCore).UpdateVariableTypes(code);
        }

        /// <summary>
        /// Returns a type from a name.  For example: System.Collections or System.Collections.ArrayList
        /// </summary>
        /// <param name="name">The name</param>
        /// <returns>The type or null if its not a valid type</returns>
        protected Type TryGetType(string name)
        {
            return (providerImplementation as ILegacyPythonCompletionCore).TryGetType(name);
        }

        /// <summary>
        /// Attempts to find all import statements in the code
        /// </summary>
        /// <param name="code">The code to search</param>
        /// <returns>A list of tuples that contain the namespace, the module, and the custom name</returns>
        private static List<Tuple<string, string, string>> FindAllImportStatements(string code)
        {
            return Type.GetType(providerTypeName).GetMethod(nameof(FindAllImportStatements),
             BindingFlags.NonPublic | BindingFlags.Static)
            .Invoke(null, new[] { code }) as List<Tuple<string, string, string>>;
        }

        /// <summary>
        /// Attempts to find all variable assignments in the code. Has basic variable unpacking support.
        /// We don't need to check the line indices because regex matches are ordered as per the code.
        /// </summary>
        /// <param name="code">The code to search</param>
        /// <returns>A dictionary of variable name and type pairs</returns>
        public Dictionary<string, Type> FindAllVariableAssignments(string code)
        {
            return (providerImplementation as ILegacyPythonCompletionCore).FindAllVariableAssignments(code);
        }

        /// <summary>
        /// Attempts to find import statements that look like
        ///     from lib import *
        /// </summary>
        /// <param name="code">The code to search</param>
        /// <returns>A dictionary matching the lib to the code where lib is the library being imported from</returns>
        public static Dictionary<string, string> FindAllTypeImportStatements(string code)
        {
            return Type.GetType(providerTypeName).GetMethod(nameof(FindAllTypeImportStatements),
          BindingFlags.NonPublic | BindingFlags.Static)
        .Invoke(null, new[] { code }) as Dictionary<string, string>;

        }

        /// <summary>
        /// Attempts to find import statements that look like
        ///     from lib import type1, type2
        /// Doesn't currently match types with namespace qualifiers like Collections.ArrayList
        /// </summary>
        /// <param name="code">The code to search</param>
        /// <returns>A dictionary matching the lib to the code where lib is the library being imported from</returns>
        public static Dictionary<string, string> FindTypeSpecificImportStatements(string code)
        {
            return Type.GetType(providerTypeName).GetMethod(nameof(FindTypeSpecificImportStatements),
              BindingFlags.NonPublic | BindingFlags.Static)
            .Invoke(null, new[] { code }) as Dictionary<string, string>;
        }

        /// <summary>
        /// Attempts to find import statements that look like
        ///     import lib
        /// </summary>
        /// <param name="code">The code to search</param>
        /// <returns>A dictionary matching the lib to the code where lib is the library being imported from</returns>
        public static Dictionary<string, string> FindBasicImportStatements(string code)
        {
            return Type.GetType(providerTypeName).GetMethod(nameof(FindBasicImportStatements),
                 BindingFlags.NonPublic | BindingFlags.Static)
               .Invoke(null, new[] { code }) as Dictionary<string, string>;
        }

        /// <summary>
        /// Find a variable assignment of the form "varName = bla" where bla is matched by
        /// the given regex
        /// </summary>
        /// <param name="code">The code to search</param>
        /// <param name="valueRegex">Your regex to match the type</param>
        /// <returns>A dictionary of name to assignment line pairs</returns>
        public static Dictionary<string, string> FindVariableStatementWithRegex(string code, string valueRegex)
        {
            return Type.GetType(providerTypeName).GetMethod(nameof(FindVariableStatementWithRegex),
                BindingFlags.NonPublic | BindingFlags.Static)
              .Invoke(null, new[] { code, valueRegex }) as Dictionary<string, string>;
        }

        /// <summary>
        /// Find all import statements and import into scope.  If the type is already in the scope, this will be skipped.
        /// </summary>
        /// <param name="code">The code to discover the import statements.</param>
        public void UpdateImportedTypes(string code)
        {
            (providerImplementation as ILegacyPythonCompletionCore).UpdateImportedTypes(code);

        }

        /// <summary>
        /// Find all variable assignments in the source code and attempt to discover their type
        /// </summary>
        /// <param name="code">The code from which to get the assignments</param>
        /// <returns>A dictionary matching the name of the variable to a tuple of typeName, character at which the assignment was found, and the CLR type</returns>
        public Dictionary<string, Tuple<string, int, Type>> FindAllVariables(string code)
        {
            return (providerImplementation as ILegacyPythonCompletionCore).FindAllVariables(code);
        }

        #endregion
    }
}