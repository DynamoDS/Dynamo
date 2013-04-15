using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Dynamo.Nodes;
using System.IO;

namespace Dynamo.Utilities
{
    /// <summary>
    ///     Manages instantiation of custom nodes.  All custom nodes known to Dynamo should be stored
    ///     with this type.  This object implements late initialization of custom nodes by providing a 
    ///     single interface to initialize custom nodes.  
    /// </summary>
    public class CustomNodeLoader {

        #region Fields and properties

        private Dictionary<Guid, FunctionDefinition> loadedNodes = new Dictionary<Guid, FunctionDefinition>();
        public Dictionary<Guid, string> nodePaths = new Dictionary<Guid, string>();

        /// <summary>
        /// SearchPath property </summary>
        /// <value>
        /// The name of the node </value>
        public string SearchPath { get; set; }

        #endregion

        /// <summary>
        ///     Class Constructor
        /// </summary>
        /// <param name="searchPath">The path to search for definitions</param>
        public CustomNodeLoader(string searchPath) {
            SearchPath = searchPath;
        }

        /// <summary>
        ///     Enumerates all of the files in the search path and get's their guids.
        ///     Does not instantiate the nodes.
        /// </summary>
        /// <returns>False if SearchPath is not a valid directory, otherwise true</returns>
        public bool UpdateSearchPath()
        {
            if (!Directory.Exists(SearchPath))
            {
                return false;
            }

            foreach (string file in Directory.EnumerateFiles(SearchPath, "*.dyf"))
            {
                Guid guid;
                if (GetGuidFromPath(file, out guid))
                {
                    this.SetNodePath(guid, file);
                }
            }
            
            return true;
        }

        /// <summary>
        ///     Update a FunctionDefinition amongst the loaded FunctionDefinitions
        /// </summary>
        /// <returns>False if SearchPath is not a valid directory, otherwise true</returns>
        public bool SetFunctionDefinition(Guid guid, FunctionDefinition def)
        {
            return false;
        }

        /// <summary>
        ///     Stores the path and function definition without initializing node
        /// </summary>
        /// <param name="guid">The unique id for the node.</param>
        /// <param name="path">The path for the node.</param>
        private void SetNodePath(Guid id, string path)
        {
            if ( this.Contains( id ) ) {
                this.nodePaths[id] = path;
            } else {
                this.nodePaths.Add(id, path);
            }
        }

        /// <summary>
        ///     Stores the path and function definition without initializing node
        /// </summary>
        /// <param name="guid">The unique id for the node.</param>
        /// <returns>The path to the node or null if it wasn't found.</returns>
        public string GetNodePath(Guid id)
        {
            if (this.Contains(id))
            {
                return nodePaths[id];
            }
            return null;
        }

        /// <summary>
        ///     Tells whether the custom node's unique identifier is inside of the manager (initialized or not)
        /// </summary>
        /// <param name="guid">Whether the definition is stored with the manager.</param>
        public bool Contains(Guid guid)
        {
            return IsInitialized(guid) || nodePaths.ContainsKey(guid);
        }

        /// <summary>
        ///     Tells whether the custom node's unique identifier is initialized in the manager
        /// </summary>
        /// <param name="guid">Whether the definition is stored with the manager.</param>
        public bool IsInitialized(Guid guid)
        {
            return loadedNodes.ContainsKey(guid);
        }

        /// <summary>
        ///     Get a the type from a guid, also stores type info for future instantiation.
        ///     As a side effect, any of its dependent nodes are also initialized.
        /// </summary>
        /// <param name="guid">Open a definition from a path, without instantiating the nodes or dependents</param>
        public bool GetInstance( Guid guid, out dynFunction result ) {
            
            if ( !this.Contains(guid) ) {
                result = null;
                return false;
            }

            FunctionDefinition def = null;
            if (!this.IsInitialized(guid))
            {
                if (!GetDefinitionFromPath(GetNodePath(guid), out def))
                {
                    result = null;
                    return false;
                }
            }           

            dynWorkspace ws = def.Workspace;

            //TODO: Update to base off of Definition

                IEnumerable<string> inputs =
                    ws.Nodes.Where(e => e is dynSymbol)
                        .Select(s => (s as dynSymbol).Symbol);

                IEnumerable<string> outputs =
                    ws.Nodes.Where(e => e is dynOutput)
                        .Select(o => (o as dynOutput).Symbol);

                if (!outputs.Any())
                {
                    var topMost = new List<Tuple<int, dynNode>>();

                    IEnumerable<dynNode> topMostNodes = ws.GetTopMostNodes();

                    foreach (dynNode topNode in topMostNodes)
                    {
                        foreach (int output in Enumerable.Range(0, topNode.OutPortData.Count))
                        {
                            if (!topNode.HasOutput(output))
                                topMost.Add(Tuple.Create(output, topNode));
                        }
                    }

                    outputs = topMost.Select(x => x.Item2.OutPortData[x.Item1].NickName);
                }

            result = new dynFunction(inputs, outputs, def);
            result.NodeUI.NickName = ws.Name;

            return true;
        }

        /// <summary>
        ///     Get a FunctionDefinition from a specific path
        /// </summary>
        /// <param name="path">The path from which to get the definition</param>
        /// <param name="def">A reference to the function definition (OUT).  null if function returns false. </param>
        /// <returns>Whether we successfully obtained the funcDef or not</returns>
        public static bool GetDefinitionFromPath(string path, out FunctionDefinition def) {
                
            try {

                var funName = "";
                var category = "";
                var cx = 0.0;
                var cy = 0.0;
                var id = "";

                #region Get xml document and parse 

                var xmlDoc = new XmlDocument();
                xmlDoc.Load(path);

                // load the header
                foreach (XmlNode node in xmlDoc.GetElementsByTagName("dynWorkspace"))
                {
                    foreach (XmlAttribute att in node.Attributes)
                    {
                        if (att.Name.Equals("X"))
                            cx = Convert.ToDouble(att.Value);
                        else if (att.Name.Equals("Y"))
                            cy = Convert.ToDouble(att.Value);
                        else if (att.Name.Equals("Name"))
                            funName = att.Value;
                        else if (att.Name.Equals("Category"))
                            category = att.Value;
                        else if (att.Name.Equals("ID"))
                        {
                            id = att.Value;
                        }   
                    }
                }

                // we have a dyf and it lacks an ID field, we need to assign it
                // a deterministic guid based on its name.  By doing it deterministically,
                // files remain compatible
                if (string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(funName))
                {
                    id = GuidUtility.Create(GuidUtility.UrlNamespace, funName).ToString();
                }

                //If there is no function name, then we are opening a dyn
                if ( string.IsNullOrEmpty(funName) )
                {
                    def = null;
                    return false;
                }

                category = category.Length > 0 ? category : BuiltinNodeCategories.MISC;

                #endregion

                var workSpace = new FuncWorkspace(funName, category, cx, cy);
                def = new FunctionDefinition( Guid.Parse( id ) ) { 
                    Workspace = workSpace 
                };
                return true;

            } catch {

                def = null;
                return false;

            }

        }

        /// <summary>
        ///     Get a guid from a specific path, internally this first calls GetDefinitionFromPath
        /// </summary>
        /// <param name="path">The path from which to get the guid</param>
        /// <param name="guid">A reference to the guid (OUT) Guid.Empty if function returns false. </param>
        /// <returns>Whether we successfully obtained the guid or not.  </returns>
        public static bool GetGuidFromPath(string path, out Guid guid) {


            try
            {
                var funName = "";
                var id = "";

                #region Get xml document and parse

                var xmlDoc = new XmlDocument();
                xmlDoc.Load(path);

                // load the header
                foreach (XmlNode node in xmlDoc.GetElementsByTagName("dynWorkspace"))
                {
                    foreach (XmlAttribute att in node.Attributes)
                    {
                        if (att.Name.Equals("Name"))
                            funName = att.Value;
                        else if (att.Name.Equals("ID"))
                        {
                            id = att.Value;
                        }
                    }
                }
                #endregion

                // we have a dyf and it lacks an ID field, we need to assign it
                // a deterministic guid based on its name.  By doing it deterministically,
                // files remain compatible
                if (string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(funName))
                {
                    guid = GuidUtility.Create(GuidUtility.UrlNamespace, funName);
                }
                else
                {
                    guid = Guid.Parse(id);
                }

                return true;

            }
            catch
            {

                guid = Guid.Empty;
                return false;

            }

        }

    }
}
