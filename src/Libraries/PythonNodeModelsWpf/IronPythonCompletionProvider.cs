using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Logging;
using ICSharpCode.AvalonEdit.CodeCompletion;
using IronPython.Runtime;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Hosting;
using PythonNodeModels;

namespace Dynamo.Python
{

    //TODO move to own file
    internal class SharedCompletionProvider : LogSourceBase
    {
        private IExternalCodeCompletionProviderCore providerImplementation;

        #region Properties and Fields
        #endregion

        #region constructors
        //TODO consider using string to make this constructor more flexible.
        internal SharedCompletionProvider(PythonEngineVersion version ,string dynamoCoreDir)
        {
            var versionName = Enum.GetName(typeof(PythonEngineVersion), version);
            var matchingCore = FindMatchingCodeCompletionCore(versionName, this.AsLogger()) ;
            if(matchingCore != null)
            {
                this.providerImplementation = matchingCore;
                this.providerImplementation.ImportStdLibrary(dynamoCoreDir);
            }
        }

        internal static IExternalCodeCompletionProviderCore FindMatchingCodeCompletionCore 
            (string versionName, ILogger logger = null)
        {
            try
            {
                var completionType = typeof(IExternalCodeCompletionProviderCore);
                var loadedCodeCompletionTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(p => completionType.IsAssignableFrom(p) && !p.IsInterface);
                //instantiate them - so we can check which is a match using their match method
                foreach (var type in loadedCodeCompletionTypes)
                {
                    var inst = Activator.CreateInstance(type);

                    if ((inst as IExternalCodeCompletionProviderCore).MatchingEngine(versionName))
                    {
                        return inst as IExternalCodeCompletionProviderCore;
                    }
                }

                //if no matching completionprovider is found, just use the first one.
                if (loadedCodeCompletionTypes.Any())
                {
                    var inst = Activator.CreateInstance(loadedCodeCompletionTypes.First());
                    logger?.Log($"could not find a matching completion core for {versionName}, using instance of {inst.GetType().ToString()}");
                    return inst as IExternalCodeCompletionProviderCore;
                }
            }
            catch(Exception e)
            {
                logger?.Log(e);
                return null;
            }
            logger?.Log("could not find any IExternalCodeCompletionCore Types Loaded");
            return null;

        }
        #endregion

        #region Methods
        internal ICompletionData[] GetCompletionData(string code, bool expand = false)
        {
            return this.providerImplementation?.GetCompletionData(code, expand).
                Select(x => new IronPythonCompletionData(x)).ToArray();
        }
        #endregion
    }


    //TODO this class needs to stay here with a reference to IronPython for now, its use will be
    //completely removed from Dynamo though - so the dependencey on Ironpython will ONLY be at compile time.
    [Obsolete("Do Not Use! This class will be removed in a future version of Dynamo," +
        "Instead Reference the completion providers for a specific scripting engine, which implement " +
        "IExternalCodeCompletionProvider")]
    /// <summary>
    /// Provides code completion for the Python Editor
    /// </summary>
    public class IronPythonCompletionProvider : LogSourceBase
    {
        private IExternalCodeCompletionProviderCore providerImplementation;
        private const string providerTypeName = "DSIronPython.IronPythonCodeCompletionProviderCore, DSIronPython";

        #region Properties and fields

        /// <summary>
        /// The engine used for autocompletion.  This essentially keeps
        /// track of the state of the editor, allowing access to variable types and
        /// imported symbols.
        /// </summary>
        private ScriptEngine engine;
        public ScriptEngine Engine
        {
            get { return (ScriptEngine)(providerImplementation as ILegacyPythonCompletionCore).Engine; }
            set { (providerImplementation as ILegacyPythonCompletionCore).Engine = value; }
        }

        /// <summary>
        /// The scope used by the engine.  This is where all the loaded symbols
        /// are stored.  It's essentially an environment dictionary.
        /// </summary>
        private ScriptScope scope;
        public ScriptScope Scope
        {
            get { return (ScriptScope)(providerImplementation as ILegacyPythonCompletionCore).Scope; }
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
        [Obsolete("This property has been replaced with BasicVariableTypes and will be removed in Dynamo 3.0")]
        public Dictionary<string, Type> RegexToType = new Dictionary<string, Type>();


        //TODO moved to IronPythonCompletionProviderCore.
        /// <summary>
        /// Maps a basic variable regex to a basic python type.
        /// </summary>      
        public static string commaDelimitedVariableNamesRegex = @"(([0-9a-zA-Z_]+,?\s?)+)";
        public static string variableName = @"([0-9a-zA-Z_]+(\.[a-zA-Z_0-9]+)*)";
        public static string doubleQuoteStringRegex = "(\"[^\"]*\")"; // Replaced w/ quotesStringRegex - Remove in Dynamo 3.0
        public static string singleQuoteStringRegex = "(\'[^\']*\')"; // Replaced w/ quotesStringRegex - Remove in Dynamo 3.0
        public static string arrayRegex = "(\\[.*\\])";
        public static string spacesOrNone = @"(\s*)";
        public static string atLeastOneSpaceRegex = @"(\s+)";
        public static string equals = @"(=)"; // Not CLS compliant - replaced with equalsRegex - Remove in Dynamo 3.0
        public static string dictRegex = "({.*})";
        public static string doubleRegex = @"([-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?)";
        public static string intRegex = @"([-+]?\d+)[\s\n]*$";
        public static string basicImportRegex = @"(import)";
        public static string fromImportRegex = @"^(from)";

        internal const string quotesStringRegex = "[\"']([^\"']*)[\"']";
        internal const string equalsRegex = @"(=)";

        internal static readonly Regex MATCH_LAST_NAMESPACE = new Regex(@"[\w.]+$", RegexOptions.Compiled);
        internal static readonly Regex MATCH_LAST_WORD = new Regex(@"\w+$", RegexOptions.Compiled);
        internal static readonly Regex MATCH_FIRST_QUOTED_NAME = new Regex(quotesStringRegex, RegexOptions.Compiled);
        internal static readonly Regex MATCH_VALID_TYPE_NAME_CHARACTERS_ONLY = new Regex(@"^\w+", RegexOptions.Compiled);
        internal static readonly Regex TRIPPLE_QUOTE_STRINGS = new Regex(".*?\\\"{{3}}[\\s\\S]+?\\\"{{3}}", RegexOptions.Compiled);

        internal static readonly Regex MATCH_IMPORT_STATEMENTS = new Regex(@"^import\s+?(.+)", RegexOptions.Compiled | RegexOptions.Multiline);
        internal static readonly Regex MATCH_FROM_IMPORT_STATEMENTS = new Regex(@"from\s+?([\w.]+)\s+?import\s+?([\w, *]+)", RegexOptions.Compiled | RegexOptions.Multiline);
        internal static readonly Regex MATCH_VARIABLE_ASSIGNMENTS = new Regex(@"^[ \t]*?(\w+(\s*?,\s*?\w+)*)\s*?=\s*(.+)", RegexOptions.Compiled | RegexOptions.Multiline);

        internal static readonly Regex STRING_VARIABLE = new Regex("[\"']([^\"']*)[\"']", RegexOptions.Compiled);
        internal static readonly Regex DOUBLE_VARIABLE = new Regex("^-?\\d+\\.\\d+", RegexOptions.Compiled);
        internal static readonly Regex INT_VARIABLE = new Regex("^-?\\d+", RegexOptions.Compiled);
        internal static readonly Regex LIST_VARIABLE = new Regex("\\[.*\\]", RegexOptions.Compiled);
        internal static readonly Regex DICT_VARIABLE = new Regex("{.*}", RegexOptions.Compiled);

        internal static readonly string BAD_ASSIGNEMNT_ENDS = ",([{";

        #endregion

        #region Constructors
        /// <summary>
        /// Class constructor
        /// </summary>
        [Obsolete("Use additional constructor passing the Dynamo Core Directory to enable Python Std Lib autocomplete.")]
        public IronPythonCompletionProvider() : this("")
        {
        }

        /// <summary>
        /// Class constructor
        /// </summary>
        public IronPythonCompletionProvider(string dynamoCoreDir)
        {
            var versionName = Enum.GetName(typeof(PythonEngineVersion), PythonEngineVersion.IronPython2);
            var matchingCore = SharedCompletionProvider.FindMatchingCodeCompletionCore(versionName,this.AsLogger());
            if (matchingCore != null)
            {
                this.providerImplementation = matchingCore;
                this.providerImplementation.ImportStdLibrary(dynamoCoreDir);
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
        [Obsolete("Please use GetCompletionData with additional parameters, this method will be removed in Dynamo 3.0.")]
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
            //TODO do conversion
            return this.providerImplementation.GetCompletionData(code, expand).
                Select(x => new IronPythonCompletionData(x)).ToArray();
        }

        /// <summary>
        /// List all of the members in a PythonModule
        /// </summary>
        /// <param name="module">A reference to the module</param>
        /// <param name="name">The name of the module</param>
        /// <returns>A list of completion data for the module</returns>
        public List<IronPythonCompletionData> EnumerateMembers(PythonModule module, string name)
        {
            var items = new List<IronPythonCompletionData>();
            foreach(var completion in (providerImplementation as ILegacyPythonCompletionCore).EnumerateMembers(module, name))
            {
                //convert generalCompletiontype to legacy completionType
                
                //TODO try catch
               var convertedCompletionType = (IronPythonCompletionData.CompletionType)Enum.Parse(typeof(IronPythonCompletionData.CompletionType), 
                    Enum.GetName(typeof(ExternalCodeCompletionType), completion.Item4));

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
        public List<IronPythonCompletionData> EnumerateMembers(NamespaceTracker ns, string name)
        {
            var items = new List<IronPythonCompletionData>();
            foreach (var completion in (providerImplementation as ILegacyPythonCompletionCore).EnumerateMembersFromTracker(ns, name))
            {
                //convert generalCompletiontype to legacy completionType

                //TODO try catch
                var convertedCompletionType = (IronPythonCompletionData.CompletionType)Enum.Parse(typeof(IronPythonCompletionData.CompletionType),
                     Enum.GetName(typeof(ExternalCodeCompletionType), completion.Item4));

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
                //convert generalCompletiontype to legacy completionType

                //TODO try catch
                var convertedCompletionType = (IronPythonCompletionData.CompletionType)Enum.Parse(typeof(IronPythonCompletionData.CompletionType),
                     Enum.GetName(typeof(ExternalCodeCompletionType), completion.Item4));

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
        public object LookupMember(string name, NamespaceTracker n)
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
        [Obsolete("Please use additional GetDescription method as this one will be removed in Dynamo 3.0.")]
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

        //TODO This method is only used from this tests...let's get rid of it or make it internal.
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