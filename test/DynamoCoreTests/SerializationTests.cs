using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using CoreNodeModels;
using DesignScript.Builtin;
using Dynamo.Engine;
using Dynamo.Engine.NodeToCode;
using Dynamo.Events;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Utilities;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Dynamo.Tests
{
    #region utils
    /// <summary>
    /// Test helpers we can share between core and view json serialization tests.
    /// </summary>
    public static class serializationTestUtils
    {
        /// <summary>
        /// replace the ids of models in the workspaceModel with non guids inside the given json string
        /// </summary>
        /// <param name="json"> the given json string to do replacements in</param>
        /// <param name="model"> the workspace the json represents</param>
        /// <param name="modelsGuidToIdMap"> a map of the old guids to the new ids we are replacing them with </param>
        /// <returns> returns a new json string without guids where applicable </returns>
        public static string replaceModelIdsWithNonGuids(string json, WorkspaceModel model, Dictionary<Guid, string> modelsGuidToIdMap)
        {
            var idcount = 0;

            //alter the output json so that all node ids are not guids
            foreach (var nodeId in model.Nodes.Select(x => x.GUID))
            {
                modelsGuidToIdMap.Add(nodeId, idcount.ToString());
                json = json.Replace(nodeId.ToString("N"), idcount.ToString());
                idcount = idcount + 1;
            }

            //alter the output json so that all port ids are not guids
            foreach (var node in model.Nodes)
            {
                foreach (var port in node.InPorts)
                {
                    modelsGuidToIdMap.Add(port.GUID, idcount.ToString());
                    json = json.Replace(port.GUID.ToString("N"), idcount.ToString());
                    idcount = idcount + 1;
                }

                foreach (var port in node.OutPorts)
                {
                    modelsGuidToIdMap.Add(port.GUID, idcount.ToString());
                    json = json.Replace(port.GUID.ToString("N"), idcount.ToString());
                    idcount = idcount + 1;
                }
            }
            //alter the output json so that all connectorModel ids are not guids
            foreach (var connector in model.Connectors)
            {
                modelsGuidToIdMap.Add(connector.GUID, idcount.ToString());
                json = json.Replace(connector.GUID.ToString("N"), idcount.ToString());
                idcount = idcount + 1;
            }


            //alter the output json so that all Notemodel ids are not guids
            foreach (var note in model.Notes)
            {
                modelsGuidToIdMap.Add(note.GUID, idcount.ToString());
                json = json.Replace(note.GUID.ToString("N"), idcount.ToString());
                idcount = idcount + 1;
            }
            //alter the output json so that all annotationModel ids are not guids
            foreach (var annotation in model.Annotations)
            {
                modelsGuidToIdMap.Add(annotation.GUID, idcount.ToString());
                json = json.Replace(annotation.GUID.ToString("N"), idcount.ToString());
                idcount = idcount + 1;
            }

            return json;
        }

        public static void ConvertCurrentWorkspaceToDesignScriptAndSave(string filePathBase, DynamoModel currentDynamoModel)
        {
            try
            {
                var workspace = currentDynamoModel.CurrentWorkspace;

                var libCore = currentDynamoModel.EngineController.LibraryServices.LibraryManagementCore;
                var libraryServices = new LibraryCustomizationServices(currentDynamoModel.PathManager);
                var nameProvider = new NamingProvider(libCore, libraryServices);
                var controller = currentDynamoModel.EngineController;
                var resolver = currentDynamoModel.CurrentWorkspace.ElementResolver;
                var namingProvider = new NamingProvider(controller.LibraryServices.LibraryManagementCore, libraryServices);

                var result = NodeToCodeCompiler.NodeToCode(libCore, workspace.Nodes, workspace.Nodes, namingProvider);
                NodeToCodeCompiler.ReplaceWithShortestQualifiedName(
                        controller.LibraryServices.LibraryManagementCore.ClassTable, result.AstNodes, resolver);
                var codegen = new ProtoCore.CodeGenDS(result.AstNodes);
                var ds = codegen.GenerateCode();

                var dsPath = filePathBase + ".ds";
                if (File.Exists(dsPath))
                {
                    File.Delete(dsPath);
                }
                File.WriteAllText(dsPath, ds);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Assert.Inconclusive("The current workspace could not be converted to Design Script.");
            }
        }

        /// <summary>
        /// Caches workspaces data for comparison.
        /// </summary>
        public class WorkspaceComparisonData
        {
            public Guid Guid { get; set; }
            public string Description { get; set; }
            public int NodeCount { get; set; }
            public int ConnectorCount { get; set; }
            public Dictionary<Guid, Type> NodeTypeMap { get; set; }
            public Dictionary<Guid, List<object>> NodeDataMap { get; set; }
            public Dictionary<Guid, string> NodeReplicationMap { get; set; }
            public Dictionary<Guid, int> InportCountMap { get; set; }
            public Dictionary<Guid, int> OutportCountMap { get; set; }
            public Dictionary<Guid, PortComparisonData> PortDataMap { get; set; }
            public Dictionary<Guid, NodeInputData> InputsMap { get; set; }
            public Dictionary<Guid, NodeOutputData> OutputsMap { get; set; }
            public string DesignScript { get; set; }

            public WorkspaceComparisonData(WorkspaceModel workspace, EngineController controller)
            {
                Guid = workspace.Guid;
                Description = workspace.Description;
                NodeCount = workspace.Nodes.Count();
                ConnectorCount = workspace.Connectors.Count();
                NodeTypeMap = new Dictionary<Guid, Type>();
                NodeDataMap = new Dictionary<Guid, List<object>>();
                InportCountMap = new Dictionary<Guid, int>();
                OutportCountMap = new Dictionary<Guid, int>();
                PortDataMap = new Dictionary<Guid, PortComparisonData>();
                NodeReplicationMap = new Dictionary<Guid, string>();
                InputsMap = new Dictionary<Guid, NodeInputData>();
                OutputsMap = new Dictionary<Guid, NodeOutputData>();

                foreach (var n in workspace.Nodes)
                {
                    NodeTypeMap.Add(n.GUID, n.GetType());
                    NodeReplicationMap.Add(n.GUID, n.ArgumentLacing.ToString());
                    //save input nodes to inputs block
                    if (n.IsSetAsInput && n.InputData != null)
                    {
                        InputsMap.Add(n.GUID, n.InputData);
                    }
                    //save output nodes to outputs block
                    if (n.IsSetAsOutput && n.OutputData != null)
                    {
                        OutputsMap.Add(n.GUID, n.OutputData);
                    }

                    var portvalues = new List<object>();
                    if (!n.IsFrozen)
                    {
                        portvalues = n.OutPorts.Select(p =>
                            ProtoCore.Utils.CoreUtils.GetDataOfValue(n.GetValue(p.Index, controller))).ToList();
                    }

                    n.InPorts.ToList().ForEach(p =>
                    {
                        PortDataMap.Add(p.GUID,
                            new PortComparisonData
                            {
                                ID = p.GUID.ToString(),
                                UseLevels = p.UseLevels,
                                KeepListStructure = p.KeepListStructure,
                                Level = p.Level,
                                UsingDefaultValue = p.UsingDefaultValue,
                                Description = p.ToolTip
                            });
                    });

                    n.OutPorts.ToList().ForEach(p =>
                    {
                        PortDataMap.Add(p.GUID,
                            new PortComparisonData
                            {
                                ID = p.GUID.ToString(),
                                Description = p.ToolTip
                            });
                    });

                    NodeDataMap.Add(n.GUID, portvalues);
                    InportCountMap.Add(n.GUID, n.InPorts.Count);
                    OutportCountMap.Add(n.GUID, n.OutPorts.Count);
                }
            }
        }
        
        /// <summary>
        /// compare two workspace comparison objects that represent workspace models
        /// </summary>
        /// <param name="a"> first workspace data to compare</param>
        /// <param name="b">second workspace data to compare</param>
        public static void CompareWorkspaceModels(serializationTestUtils.WorkspaceComparisonData a, serializationTestUtils.WorkspaceComparisonData b, Dictionary<Guid, string> c = null)
        {
            var nodeDiff = a.NodeTypeMap.Except(b.NodeTypeMap);

            // Ignore IntegerSlider nodes as they are being read as IntegerSlider64Bit JSON nodes.
            // TODO: Remove this filter once we deprecate IntegerSlider nodes in a future Dynamo version.
            nodeDiff = nodeDiff.Where(nd => nd.Value.FullName != "CoreNodeModels.Input.IntegerSlider");

            if (nodeDiff.Any())
            {
                Assert.Fail("The workspaces don't have the same number of nodes. The json workspace is missing: " + string.Join(",", nodeDiff.Select(i => i.Value.ToString())));
            }
            Assert.AreEqual(a.Description, b.Description, "The workspaces don't have the same description.");
            Assert.AreEqual(a.NodeCount, b.NodeCount, "The workspaces don't have the same number of nodes.");
            Assert.AreEqual(a.ConnectorCount, b.ConnectorCount, "The workspaces don't have the same number of connectors.");

            foreach (var kvp in a.InportCountMap)
            {
                var countA = kvp.Value;
                var countB = b.InportCountMap[kvp.Key];
                Assert.AreEqual(countA, countB, string.Format("One {0} node has {1} inports, while the other has {2}", a.NodeTypeMap[kvp.Key], countA, countB));
            }
            foreach (var kvp in a.OutportCountMap)
            {
                var countA = kvp.Value;
                var countB = b.OutportCountMap[kvp.Key];
                Assert.AreEqual(countA, countB, string.Format("One {0} node has {1} outports, while the other has {2}", a.NodeTypeMap[kvp.Key], countA, countB));
            }

            foreach (var portkvp in a.PortDataMap)
            {
                Assert.IsTrue(b.PortDataMap.ContainsKey(portkvp.Key));

                var aData = a.PortDataMap[portkvp.Key];
                var bData = b.PortDataMap[portkvp.Key];
                
                // With the change to JSON based IntegerSlider nodes returning 64 bit integers,
                // the description between the old XML and the new JSON based workspaces will be
                // "Int32" and "Int64" respectively.
                if (aData.Description == "Int32")
                {
                    Assert.IsTrue(bData.Description == "Int32" || bData.Description == "Int64");
                }
                else
                {
                    Assert.AreEqual(aData, bData);
                }
            }

            foreach (var kvp in a.NodeReplicationMap)
            {
                var valueA = kvp.Value;
                var valueB = b.NodeReplicationMap[kvp.Key];
                Assert.AreEqual(valueA, valueB);
            }

            foreach (var kvp in a.NodeDataMap)
            {
                var valueA = kvp.Value;
                var valueB = b.NodeDataMap[kvp.Key];

                // Ignore IntegerSlider nodes as they are being read as IntegerSlider64Bit JSON nodes.
                // TODO: Remove this filter once we deprecate IntegerSlider nodes in a future Dynamo version.
                if (a.NodeTypeMap[kvp.Key].FullName == "CoreNodeModels.Input.IntegerSlider")
                {
                    Assert.AreEqual("CoreNodeModels.Input.IntegerSlider64Bit", b.NodeTypeMap[kvp.Key].FullName);
                    continue;
                }

                Assert.AreEqual(a.NodeTypeMap[kvp.Key], b.NodeTypeMap[kvp.Key]);

                try
                {
                    // When values are geometry, sometimes the creation
                    // of the string representation for forming this message
                    // fails.
#if NET6_0_OR_GREATER
                    Assert.That(valueA, Is.EqualTo(valueB).Using<Dictionary>(DynamoDictionaryEquality),

#elif NETFRAMEWORK
                    Assert.That(valueA, Is.EqualTo(valueB),
#endif
                    string.Format("Node Type:{0} value, {1} is not equal to {2}",
                    a.NodeTypeMap[kvp.Key], valueA, valueB));
                }
                catch
                {
                    continue;
                }
            }

            foreach (var kvp in a.InputsMap)
            {
                var vala = kvp.Value;
                var valb = b.InputsMap[kvp.Key];
                Assert.AreEqual(vala, valb, "input datas are not the same.");
            }
        }

        internal static bool DynamoDictionaryEquality(Dictionary a, Dictionary b)
        {
            return (bool)a?.Keys.SequenceEqual(b?.Keys) && (bool)a?.Values.SequenceEqual(b?.Values);
        }


        public static void CompareWorkspacesDifferentGuids(serializationTestUtils.WorkspaceComparisonData a,
            serializationTestUtils.WorkspaceComparisonData b,
            Dictionary<Guid, string> modelGuidsToIDmap)
        {
            var nodeDiff = a.NodeTypeMap.Select(x => x.Value).Except(b.NodeTypeMap.Select(x => x.Value));

            // Ignore IntegerSlider nodes as they are being read as IntegerSlider64Bit JSON nodes.
            // TODO: Remove this filter once we deprecate IntegerSlider nodes in a future Dynamo version.
            nodeDiff = nodeDiff.Where(nd => nd.FullName != "CoreNodeModels.Input.IntegerSlider");

            if (nodeDiff.Any())
            {
                Assert.Fail("The workspaces don't have the same number of nodes. The json workspace is missing: " + string.Join(",", nodeDiff.Select(i => i.ToString())));
            }
            Assert.AreEqual(a.NodeCount, b.NodeCount, "The workspaces don't have the same number of nodes.");
            Assert.AreEqual(a.ConnectorCount, b.ConnectorCount, "The workspaces don't have the same number of connectors.");

            foreach (var kvp in a.InportCountMap)
            {
                var countA = kvp.Value;
                //convert the old guid to the new guid
                var newGuid = GuidUtility.Create(GuidUtility.UrlNamespace, modelGuidsToIDmap[kvp.Key]);
                var countB = b.InportCountMap[newGuid];
                Assert.AreEqual(countA, countB, string.Format("One {0} node has {1} inports, while the other has {2}", a.NodeTypeMap[kvp.Key], countA, countB));
            }
            foreach (var kvp in a.OutportCountMap)
            {
                var countA = kvp.Value;
                //convert the old guid to the new guid
                var newGuid = GuidUtility.Create(GuidUtility.UrlNamespace, modelGuidsToIDmap[kvp.Key]);
                var countB = b.OutportCountMap[newGuid];
                Assert.AreEqual(countA, countB, string.Format("One {0} node has {1} outports, while the other has {2}", a.NodeTypeMap[kvp.Key], countA, countB));
            }

            foreach (var portkvp in a.PortDataMap)
            {
                //convert the old guid to the new guid
                var newGuid = GuidUtility.Create(GuidUtility.UrlNamespace, modelGuidsToIDmap[portkvp.Key]);
                Assert.IsTrue(b.PortDataMap.ContainsKey(newGuid));
                var aPort = a.PortDataMap[portkvp.Key];
                var bPort = b.PortDataMap[newGuid];
                Assert.AreEqual(aPort.UseLevels, bPort.UseLevels);
                Assert.AreEqual(aPort.KeepListStructure, bPort.KeepListStructure);
                Assert.AreEqual(aPort.Level, bPort.Level);

                // With the change to JSON based IntegerSlider nodes returning 64 bit integers,
                // the description between the old XML and the new JSON based workspaces will be
                // "Int32" and "Int64" respectively.
                if (aPort.Description == "Int32")
                {
                    Assert.IsTrue(bPort.Description == "Int32" || bPort.Description == "Int64");
                }
                else
                {
                    Assert.AreEqual(aPort.Description, bPort.Description);
                }
            }

            foreach (var kvp in a.NodeReplicationMap)
            {
                var newGuid = GuidUtility.Create(GuidUtility.UrlNamespace, modelGuidsToIDmap[kvp.Key]);
                var valueA = kvp.Value;
                var valueB = b.NodeReplicationMap[newGuid];
                Assert.AreEqual(valueA, valueB);
            }

            foreach (var kvp in a.NodeDataMap)
            {
                var valueA = kvp.Value;
                //convert the old guid to the new guid
                var newGuid = GuidUtility.Create(GuidUtility.UrlNamespace, modelGuidsToIDmap[kvp.Key]);
                var valueB = b.NodeDataMap[newGuid];

                // Ignore IntegerSlider nodes as they are being read as IntegerSlider64Bit JSON nodes.
                // TODO: Remove this filter once we deprecate IntegerSlider nodes in a future Dynamo version.
                if (a.NodeTypeMap[kvp.Key].FullName == "CoreNodeModels.Input.IntegerSlider")
                {
                    Assert.AreEqual("CoreNodeModels.Input.IntegerSlider64Bit", b.NodeTypeMap[newGuid].FullName);
                    continue;
                }

                Assert.AreEqual(a.NodeTypeMap[kvp.Key], b.NodeTypeMap[newGuid]);

                try
                {
                    // When values are geometry, sometimes the creation
                    // of the string representation for forming this message
                    // fails.
#if NET6_0_OR_GREATER
                    Assert.That(valueA, Is.EqualTo(valueB).Using<Dictionary>(DynamoDictionaryEquality),

#elif NETFRAMEWORK
                    Assert.That(valueA, Is.EqualTo(valueB),
#endif
                    string.Format("Node Type:{0} value, {1} is not equal to {2}",
                    a.NodeTypeMap[kvp.Key], valueA, valueB));
                }
                catch
                {
                    continue;
                }
            }
        }
        /// <summary>
        /// saves the workspace comparison object to disk in a .data file
        /// </summary>
        /// <param name="wcd1"></param>
        /// <param name="filePathBase"></param>
        /// <param name="executionDuration"></param>
        /// <param name="modelGuidToIDMap"></param>
        public static void SaveWorkspaceComparisonData(serializationTestUtils.WorkspaceComparisonData wcd1,
            string filePathBase,
            TimeSpan executionDuration,
            Dictionary<Guid, string> modelGuidToIDMap = null)
        {
            var nodeData = new Dictionary<string, Dictionary<string, object>>();
            foreach (var d in wcd1.NodeDataMap)
            {
                var t = wcd1.NodeTypeMap[d.Key];
                var nodeDataDict = new Dictionary<string, object>();
                nodeDataDict.Add("nodeType", t.ToString());
                nodeDataDict.Add("portValues", d.Value);
                nodeData.Add(d.Key.ToString(), nodeDataDict);
            }

            var workspaceDataDict = new Dictionary<string, object>();
            workspaceDataDict.Add("nodeData", nodeData);
            workspaceDataDict.Add("executionDuration", executionDuration.TotalSeconds);

            var dataMapStr = JsonConvert.SerializeObject(workspaceDataDict,
                            new JsonSerializerSettings()
                            {
                                Formatting = Formatting.Indented,
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                                Culture = CultureInfo.InvariantCulture
                            });

            var dataPath = filePathBase + ".data";
            if (File.Exists(dataPath))
            {
                File.Delete(dataPath);
            }
            File.WriteAllText(dataPath, dataMapStr);
        }

        /// <summary>
        ///  saves workspace comparison object and remaps ids for saved data to the new ids
        /// </summary>
        /// <param name="wcd1"></param>
        /// <param name="filePathBase"></param>
        /// <param name="executionDuration"></param>
        /// <param name="modelsGuidToIdMap"></param>
        public static void SaveWorkspaceComparisonDataWithNonGuidIds(serializationTestUtils.WorkspaceComparisonData wcd1,
            string filePathBase,
            TimeSpan executionDuration,
            Dictionary<Guid, string> modelsGuidToIdMap)
        {
            var nodeData = new Dictionary<string, Dictionary<string, object>>();
            foreach (var d in wcd1.NodeDataMap)
            {
                var t = wcd1.NodeTypeMap[d.Key];
                var nodeDataDict = new Dictionary<string, object>();
                nodeDataDict.Add("nodeType", t.ToString());
                nodeDataDict.Add("portValues", d.Value);
                nodeData.Add(d.Key.ToString(), nodeDataDict);
            }

            var workspaceDataDict = new Dictionary<string, object>();
            workspaceDataDict.Add("nodeData", nodeData);
            workspaceDataDict.Add("executionDuration", executionDuration.TotalSeconds);

            var dataMapStr = JsonConvert.SerializeObject(workspaceDataDict,
                            new JsonSerializerSettings()
                            {
                                Formatting = Formatting.Indented,
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                                Culture = CultureInfo.InvariantCulture
                            });
            //replace all the guids in the data file with all of our remapped ids.
            foreach (var guidKey in modelsGuidToIdMap.Keys)
            {
                dataMapStr = dataMapStr.Replace(guidKey.ToString(), modelsGuidToIdMap[guidKey]);
            }

            // If "DynamoCoreWPFTests" test copy .data file to additional structured folder location
            if (filePathBase.Contains("DynamoCoreWPFTests"))
            {
                string structuredTestPath;
                string fileName = Path.GetFileNameWithoutExtension(filePathBase);
                string flattenedTestPath = Path.GetTempPath() + "jsonWithView_nonGuidIds\\" + fileName;
                string extension = Path.GetExtension(filePathBase);
                string pathWithoutExt = filePathBase.Substring(0, filePathBase.Length - extension.Length);

                // Determine if .dyn or .dyf
                // If .dyn and .dyf share common file name .ds and .data files is collide
                // To avoid this append _dyf to .data and .ds files for all .dyf files
                if (extension == ".dyf")
                {
                    structuredTestPath = pathWithoutExt + "_dyf.data";
                }

                else
                {
                    structuredTestPath = pathWithoutExt + ".data";
                }

                // Write to structured path
                if (File.Exists(structuredTestPath))
                {
                    File.Delete(structuredTestPath);
                }

                File.WriteAllText(structuredTestPath, dataMapStr);

                // Write to flattened path
                if (File.Exists(flattenedTestPath + ".data"))
                {
                    File.Delete(flattenedTestPath + ".data");
                }

                File.WriteAllText(flattenedTestPath + ".data", dataMapStr);
            }

            else
            {
                var dataPath = filePathBase + ".data";
                if (File.Exists(dataPath))
                {
                    File.Delete(dataPath);
                }
                File.WriteAllText(dataPath, dataMapStr);
            }
        }

        public class PortComparisonData
        {
            public string ID { get; set; }
            public bool UseLevels { get; set; }
            public bool KeepListStructure { get; set; }
            public int Level { get; set; }
            public bool UsingDefaultValue { get; set; }
            public string Description { get; set; }

            public override bool Equals(object obj)
            {
                var other = (obj as PortComparisonData);
                return ID == other.ID &&
                    other.KeepListStructure == this.KeepListStructure &&
                    other.Level == this.Level &&
                    other.UseLevels == this.UseLevels &&
                    other.UsingDefaultValue == this.UsingDefaultValue &&
                    other.Description == this.Description;
            }
        }
    }
#endregion

    /* The Serialization tests compare the results of a workspace opened and executed from its
     * original .dyn format, to one converted to json, deserialized and executed. In the process,
     * the tests save the following files:
     *  - xxx.json file representing the serialized version of the workspace to json, where xxx is the
     *  original .dyn file name.
     *  - xxx_data.json file containing the cached values of each of the workspaces
     *  - xxx.ds file containing the Design Script code for the workspace.
     */
    [TestFixture, Category("Serialization")]
    public class SerializationTests : DynamoModelTestBase
    {
        public static string jsonNonGuidFolderName = "json_nonGuidIds";
        public static string jsonFolderName = "json";
        public static string jsonFolderNameDifferentCulture = "json_differentCulture";
        private const int MAXNUM_SERIALIZATIONTESTS_TOEXECUTE = 300;

        private TimeSpan lastExecutionDuration = new TimeSpan();
        private Dictionary<Guid, string> modelsGuidToIdMap = new Dictionary<Guid, string>();


        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

#if NETFRAMEWORK
        [TestFixtureSetUp]
#elif NET6_0_OR_GREATER
        [OneTimeSetUp]
#endif
        public void FixtureSetup()
        {
            ExecutionEvents.GraphPostExecution += ExecutionEvents_GraphPostExecution;

            //Clear Temp directory folders before start of the new serialization test run
            var tempPath = Path.GetTempPath();
            var jsonFolder = Path.Combine(tempPath, jsonFolderName);
            var jsonNonGuidFolder = Path.Combine(tempPath, jsonNonGuidFolderName);

            //Try and delete all the files from the previous run. 
            //If there's an error in deleting files, the tests should countinue
            if (Directory.Exists(jsonFolder))
            {
                try
                {
                    Console.WriteLine("Deleting JSON directory from temp");
                    Directory.Delete(jsonFolder, true);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            if (Directory.Exists(jsonNonGuidFolder))
            {
                try
                {
                    Console.WriteLine("Deleting jsonNonGuid directory from temp");
                    Directory.Delete(jsonNonGuidFolder, true);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

#if NETFRAMEWORK
        [TestFixtureTearDown]
#elif NET6_0_OR_GREATER
        [OneTimeTearDown]
#endif
        public void TearDown()
        {
            ExecutionEvents.GraphPostExecution -= ExecutionEvents_GraphPostExecution;
        }

        private void ExecutionEvents_GraphPostExecution(Session.IExecutionSession session)
        {
            lastExecutionDuration = (TimeSpan)session.GetParameterValue(Session.ParameterKeys.LastExecutionDuration);
        }

        [Test]
        public void ConverterDoesNotThrowWithNullEngine()
        {
            CurrentDynamoModel.AddHomeWorkspace();
            Assert.DoesNotThrow(() => { CurrentDynamoModel.CurrentWorkspace.ToJson(null); });
        }
        [Test]
        public void NullWorkspaceRefsDeserializedAsEmpty()
        {

            var testFile = Path.Combine(TestDirectory, @"core\serialization\nullWorkspaceRefs.dyn");
            var json = File.ReadAllText(testFile);

            Assert.DoesNotThrow(() =>
            {
               var ws =  WorkspaceModel.FromJson(
                json, this.CurrentDynamoModel.LibraryServices,
                null,
                null,
                this.CurrentDynamoModel.NodeFactory,
                true,
                true,
                this.CurrentDynamoModel.CustomNodeManager);

                Assert.NotNull(ws);
            });
        }

        [Test]
        public void ReadConverterDoesNotThrowWithNullEngineAndScheduler()
        {
            CurrentDynamoModel.AddHomeWorkspace();
            var json = CurrentDynamoModel.CurrentWorkspace.ToJson(null);

            Assert.DoesNotThrow(() =>
            {
                WorkspaceModel.FromJson(
                json, this.CurrentDynamoModel.LibraryServices,
                null,
                null,
                this.CurrentDynamoModel.NodeFactory,
                true,
                true,
                this.CurrentDynamoModel.CustomNodeManager);
            });
        }

        [Test, Category("JsonTestExclude")]
        public void CustomNodeSerializationTest()
        {
            var customNodeTestPath = Path.Combine(TestDirectory, @"core\CustomNodes\TestAdd.dyn");
            DoWorkspaceOpenAndCompare(customNodeTestPath, "json", ConvertCurrentWorkspaceToJsonAndSave,
                serializationTestUtils.CompareWorkspaceModels,
                serializationTestUtils.SaveWorkspaceComparisonData);
        }

        [Test]
        public void NodeDescriptionDeserilizationTest()
        {
            // This test is in reference to this task: https://jira.autodesk.com/browse/DYN-2002
            // The description of the node in the below graph has been updated to a different value. 
            // We continue to serialize the description property to the json file but we do not want to
            // read this value back while deserializing. This test will make sure that the description is
            // not read from the json file and it gets the value from the node's config.
            var testFile = Path.Combine(TestDirectory, @"core\serialization\NodeDescriptionDeserilizationTest.dyn");
            OpenModel(testFile);
            var node = this.CurrentDynamoModel.CurrentWorkspace.Nodes.First();
            Assert.AreEqual(node.Description, CoreNodeModels.Properties.Resources.ListCreateDescription);
        }

        [Test]
        public void OutPortDescriptionDeserilizationTest()
        {
            //similar to above test, uses a modified graph to assert port tooltips not deserialized when possible.
            var testFile = Path.Combine(TestDirectory, @"core\serialization\PortTooltipDeserilizationTest.dyn");
            OpenModel(testFile);

            var ztNode = this.CurrentDynamoModel.CurrentWorkspace.Nodes.Where(x => x.GUID == new Guid("bda3e3e4c18c461dae5598df465035b2")).First();
            Assert.AreEqual(ztNode.OutPorts.First().ToolTip, "Arc created from three points");

            var nodeModelNode = this.CurrentDynamoModel.CurrentWorkspace.Nodes.Where(x => x.GUID == new Guid("c848cc3cb24a477f8248e53fc9304cc1")).First();
            Assert.AreEqual(nodeModelNode.OutPorts.First().ToolTip, "Selected colors");
        }

        [Test]
        [Category("Failure")]
        [Category("TechDebt")]
        public void OutPortDescriptionDeserilizationTest_VariableInputNodes()
        {
            //similar to above test, uses a modified graph to assert port tooltips not deserialized when possible.
            var testFile = Path.Combine(TestDirectory, @"core\serialization\PortTooltipDeserilizationTest.dyn");
            OpenModel(testFile);
            var variableInputNode = this.CurrentDynamoModel.CurrentWorkspace.Nodes.Where(x => x.GUID == new Guid("db8fd7b97be1413e91897316ae75b51a")).First();
            Assert.AreEqual(variableInputNode.OutPorts.First().ToolTip, "Combined lists");
        }

        [Test]
        public void InPortDescriptionDeserilizationTest()
        {
            //similar to above test, uses a modified graph to assert port tooltips not deserialized when possible.
            var testFile = Path.Combine(TestDirectory, @"core\serialization\PortTooltipDeserilizationTest.dyn");
            OpenModel(testFile);

            var ztNode = this.CurrentDynamoModel.CurrentWorkspace.Nodes.Where(x => x.GUID == new Guid("bda3e3e4c18c461dae5598df465035b2")).First();
            Assert.AreEqual(ztNode.InPorts.First().ToolTip, "1st point of arc\n\nPoint");

            var nodeModelNode = this.CurrentDynamoModel.CurrentWorkspace.Nodes.Where(x => x.GUID == new Guid("c848cc3cb24a477f8248e53fc9304cc1")).First();
            Assert.AreEqual(nodeModelNode.InPorts.First().ToolTip, "List of colors to include in the range");

        }
        [Test]
        [Category("Failure")]
        [Category("TechDebt")]
        public void InPortDescriptionDeserilizationTest_VariableInputNodes()
        {
            //similar to above test, uses a modified graph to assert port tooltips not deserialized when possible.
            var testFile = Path.Combine(TestDirectory, @"core\serialization\PortTooltipDeserilizationTest.dyn");
            OpenModel(testFile);
            var variableInputNode = this.CurrentDynamoModel.CurrentWorkspace.Nodes.Where(x => x.GUID == new Guid("db8fd7b97be1413e91897316ae75b51a")).First();
            Assert.AreEqual(variableInputNode.InPorts.First().ToolTip, "Function to use as combinator");

            var variableInputNodeWithPortAttributes = this.CurrentDynamoModel.CurrentWorkspace.Nodes.Where(x => x.GUID == new Guid("ddbbd4be7e0b471598151cd09f17f5a1")).First();
            Assert.AreEqual(variableInputNodeWithPortAttributes.InPorts.First().ToolTip, "Item Index #0");
            Assert.AreEqual(variableInputNodeWithPortAttributes.InPorts.ElementAt(2).ToolTip, "Item Index #2");
        }

        [Test]
        public void NodeFreezeStateDeserilizationTest()
        {
            // The freeze state of the node is saved in node view block. However the property on node model
            // will impact headless Dynamo clients graph run, e.g. DynamoPlayer, Refinery.
            // This test will make sure that the isFrozenExplicitly is read from the node view block in Json file.
            var testFile = Path.Combine(TestDirectory, @"core\serialization\NodeDescriptionDeserilizationTest.dyn");
            OpenModel(testFile);
            var node = this.CurrentDynamoModel.CurrentWorkspace.Nodes.First();
            Assert.AreEqual(node.isFrozenExplicitly, true);
        }

        [Test]
        public void NodeIsSetAsInputStateDeserilizationTest()
        {
            // The IsSetAsInput state of the node is saved in node view block. However the property on node model
            // will impact headless Dynamo clients graph run, e.g. DynamoPlayer, Refinery.
            // This test will make sure that the IsSetAsInput is read from the node view block in Json file.
            var testFile = Path.Combine(TestDirectory, @"core\serialization\NodeDescriptionDeserilizationTest.dyn");
            OpenModel(testFile);
            var node = this.CurrentDynamoModel.CurrentWorkspace.Nodes.ToList()[1];
            Assert.AreEqual(node.IsSetAsInput, true);
        }

        [Test]
        public void NodeWithInputTypeFromTheFutureShouldNotBreakFileDeserialization()
        {
            // When an input node has a type from the future, we don't deserialize that inputData, but 
            // the node and the rest of graph should still deserialize correctly.

            var testFile = Path.Combine(TestDirectory, @"core\serialization\input_type_from_future.dyn");
            OpenModel(testFile);
            Assert.AreEqual(9, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            var selectionNode = this.CurrentDynamoModel.CurrentWorkspace.Nodes.ToList().Where(x => x.GUID == Guid.Parse("da7f5c18d72d4f649602197e0aa0d0fa")).FirstOrDefault();
            //this is still set true as its also serialized on the nodeView and deserialized in the workspaceReadConverter.
            Assert.AreEqual(selectionNode.IsSetAsInput, true);
        }
        [Test]
        public void NodeWithInputTypeAndType2FromTheFutureShouldNotBreakFileDeserialization()
        {
            // When an input node has a type from the future, we don't deserialize that inputData, but 
            // the node and the rest of graph should still deserialize correctly.

            var testFile = Path.Combine(TestDirectory, @"core\serialization\type1and2_future.dyn");
            OpenModel(testFile);
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            var selectionNode = this.CurrentDynamoModel.CurrentWorkspace.Nodes.ToList().Where(x => x.GUID == Guid.Parse("60316051425445ab8728f099bdf6f0d1")).FirstOrDefault();
            //this is still set true as its also serialized on the nodeView and deserialized in the workspaceReadConverter.
            Assert.AreEqual(selectionNode.IsSetAsInput, true);
        }


        [Test]
        public void NodeIsSetAsOutputStateDeserilizationTest()
        {
            // The IsSetAsOutput state of the node is saved in node view block. However the property on node model
            // will impact headless Dynamo clients graph run, e.g. DynamoPlayer, Refinery.
            // This test will make sure that the IsSetAsOutput is read from the node view block in Json file.
            var testFile = Path.Combine(TestDirectory, @"core\serialization\NodeDescriptionDeserilizationTest.dyn");
            OpenModel(testFile);
            var node = this.CurrentDynamoModel.CurrentWorkspace.Nodes.First();
            Assert.AreEqual(node.IsSetAsOutput, true);
        }

        [Test]
        public void NodeNameDeserilizationTest()
        {
            // The name of the node is saved in node view block. However the property on node model
            // will impact headless Dynamo clients graph run, e.g. DynamoPlayer, Refinery.
            // This test will make sure that the name is read from the node view block in Json file.
            var testFile = Path.Combine(TestDirectory, @"core\serialization\NodeDescriptionDeserilizationTest.dyn");
            OpenModel(testFile);
            var node = this.CurrentDynamoModel.CurrentWorkspace.Nodes.First();
            Assert.AreEqual(node.Name, "List Create");
        }

        [Test]
        public void DropDownNodesNodeInputDataSerializationTest()
        {
            // Arrange
            var pkgDir = Path.Combine(TestDirectory, "pkgs\\Dynamo Samples");
            this.LoadPackage(pkgDir);

            var filePath = @"core\NodeInputOutputData\dropDownInputData.dyn";

            // Act
            // Assert
            DoWorkspaceOpenAndCompare(
                filePath, 
                jsonFolderName, 
                ConvertCurrentWorkspaceToJsonAndSave, 
                serializationTestUtils.CompareWorkspaceModels,
                serializationTestUtils.SaveWorkspaceComparisonData);
        }

        [Test]
        public void ColorPaletteNodeInputDataSerializationTest()
        {
            // Arrange
            var filePath = @"core\NodeInputOutputData\colorPaletteInputData.dyn";

            // Act
            // Assert
            DoWorkspaceOpenAndCompare(
                filePath,
                jsonFolderName,
                ConvertCurrentWorkspaceToJsonAndSave,
                serializationTestUtils.CompareWorkspaceModels,
                serializationTestUtils.SaveWorkspaceComparisonData);

        }

        [Test, Category("JsonTestExclude")]
        public void FunctionNodeLoadsWhenSignatureChanges()
        {
            var testFile = Path.Combine(TestDirectory, @"core\serialization\functionSignatureDifferentNumParamsThanGraph.dyn");
            OpenModel(testFile);
            //assert that the nodes are loaded even though their signature changed and we don't have the
            //same number of serialized ports as the functionSignature implies.
            var polyCurveConstructors = this.CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<DSFunction>().Where(x => x.FunctionSignature.Contains("PolyCurve.By"));
            Assert.AreEqual(polyCurveConstructors.Count(), 2);
            Assert.AreEqual(polyCurveConstructors.ElementAt(0).InPorts.Count, 2);
            Assert.AreEqual(polyCurveConstructors.ElementAt(0).OutPorts.Count, 1);
            Assert.AreEqual(polyCurveConstructors.ElementAt(0).InPorts.ElementAt(1).UsingDefaultValue, true);

            //assert all the first ports of the polycurve nodes are connected.
            Assert.True(polyCurveConstructors.All(x => x.InPorts[0].IsConnected));

        }

        [Test, Category("JsonTestExclude")]
        public void AllTypesSerialize()
        {
            var customNodeTestPath = Path.Combine(TestDirectory, @"core\serialization\serialization.dyn");
            DoWorkspaceOpenAndCompare(customNodeTestPath, "json", ConvertCurrentWorkspaceToJsonAndSave,
                serializationTestUtils.CompareWorkspaceModels,
                serializationTestUtils.SaveWorkspaceComparisonData);
        }

        public static object[] FindWorkspaces()
        {
            var di = new DirectoryInfo(TestDirectory);
            var fis = di.GetFiles("*.dyn", SearchOption.AllDirectories);
            return fis.Select(fi => fi.FullName).Take(MAXNUM_SERIALIZATIONTESTS_TOEXECUTE).ToArray();
        }

        /// <summary>
        /// This parameterized test finds all .dyn files in directories within
        /// the test directory, opens them and executes, then converts them to
        /// json and executes again, comparing the values from the two runs.
        /// </summary>
        /// <param name="filePath">The path to a .dyn file. This parameter is supplied
        /// by the test framework.</param>
        [Test, TestCaseSource("FindWorkspaces"), Category("JsonTestExclude")]
        public void SerializationTest(string filePath)
        {
            modelsGuidToIdMap.Clear();
            DoWorkspaceOpenAndCompare(filePath, jsonFolderName, ConvertCurrentWorkspaceToJsonAndSave,
                serializationTestUtils.CompareWorkspaceModels,
                serializationTestUtils.SaveWorkspaceComparisonData);
        }

        /// <summary>
        /// This parameterized test finds all .dyn files in directories within
        /// the test directory, opens them and executes, then converts them to
        /// json and executes again, comparing the values from the two runs
        /// while being in a different culture.
        /// </summary>
        /// <param name="filePath">The path to a .dyn file. This parameter is supplied
        /// by the test framework.</param>
        [Test, TestCaseSource("FindWorkspaces"), Category("JsonTestExclude")]
        public void SerializationInDifferentCultureTest(string filePath)
        {
            var frCulture = CultureInfo.CreateSpecificCulture("fr-FR");

            // Save current culture - usually "en-US"
            var currentCulture = Thread.CurrentThread.CurrentCulture;
            var currentUICulture = Thread.CurrentThread.CurrentUICulture;

            // Set "fr-FR"
            Thread.CurrentThread.CurrentCulture = frCulture;
            Thread.CurrentThread.CurrentUICulture = frCulture;

            DoWorkspaceOpenAndCompare(filePath, jsonFolderNameDifferentCulture, ConvertCurrentWorkspaceToJsonAndSave,
                serializationTestUtils.CompareWorkspaceModels,
                serializationTestUtils.SaveWorkspaceComparisonData);

            // Restore "en-US"
            Thread.CurrentThread.CurrentCulture = currentCulture;
            Thread.CurrentThread.CurrentUICulture = currentUICulture;
        }

        /// <summary>
        /// This parameterized test finds all .dyn files in directories within
        /// the test directory, opens them and executes, then converts them to
        /// json and executes again, comparing the values from the two runs.
        /// This set of tests has slightly modified json where the id properties
        /// are altered when serialized to test deserialization of non-guid ids.
        /// </summary>
        /// <param name="filePath">The path to a .dyn file. This parameter is supplied
        /// by the test framework.</param>
        [Test, TestCaseSource("FindWorkspaces"), Category("JsonTestExclude")]
        public void SerializationNonGuidIdsTest(string filePath)
        {
            modelsGuidToIdMap.Clear();
            DoWorkspaceOpenAndCompare(filePath, jsonNonGuidFolderName,
                ConvertCurrentWorkspaceToNonGuidJsonAndSave,
                serializationTestUtils.CompareWorkspacesDifferentGuids,
                serializationTestUtils.SaveWorkspaceComparisonDataWithNonGuidIds);
        }

        public static List<string> bannedTests = new List<string>()
            {
                "NestedIF",
                "recorded",
                "excel",
                "CASE",
                "WatchPreviewBubble",
                "visualization",
                "migration",
                "missing_custom_node",
                "PackageLoadExceptionTest",
                "Dummy.dyn",
                // Tests which require late initialization
                // of custom nodes...
                "noro.dyn",
                "Number1.dyn",
                "MultipleIF",
                "packageTest",
                "reduce-example",
                "TestFrozen",
                "TestImperativeInCBN",
                "CustomNodeContainedInMultiplePackages"
            };

        private void DoWorkspaceOpenAndCompare(string filePath, string dirName,
            Func<DynamoModel, string, string> saveFunction,
            Action<serializationTestUtils.WorkspaceComparisonData, serializationTestUtils.WorkspaceComparisonData, Dictionary<Guid, String>> workspaceCompareFunction,
            Action<serializationTestUtils.WorkspaceComparisonData, string, TimeSpan, Dictionary<Guid, string>> workspaceDataSaveFunction)
        {
            var openPath = filePath;

            if (bannedTests.Any(t => filePath.Contains(t)))
            {
                Assert.Inconclusive("Skipping test known to kill the test framework...");
            }

            OpenModel(openPath);

            var model = CurrentDynamoModel;
            var ws1 = model.CurrentWorkspace;
            ws1.Description = "TestDescription";

            var dummyNodes = ws1.Nodes.Where(n => n is DummyNode);
            if (dummyNodes.Any())
            {
                Assert.Inconclusive("The Workspace contains dummy nodes for: " + string.Join(",", dummyNodes.Select(n => n.Name).ToArray()));
            }

            var cbnErrorNodes = ws1.Nodes.Where(n => n is CodeBlockNodeModel && n.State == ElementState.Error);
            if (cbnErrorNodes.Any())
            {
                Assert.Inconclusive("The Workspace contains code block nodes in error state due to which rest " +
                                    "of the graph will not execute; skipping test ...");
            }

            if (((HomeWorkspaceModel)ws1).RunSettings.RunType == Dynamo.Models.RunType.Manual)
            {
                RunCurrentModel();
            }

            var wcd1 = new serializationTestUtils.WorkspaceComparisonData(ws1, CurrentDynamoModel.EngineController);

            var dirPath = Path.Combine(Path.GetTempPath(), dirName);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            var fi = new FileInfo(filePath);
            var filePathBase = dirPath + @"\" + Path.GetFileNameWithoutExtension(fi.Name);

            serializationTestUtils.ConvertCurrentWorkspaceToDesignScriptAndSave(filePathBase, CurrentDynamoModel);

            string json = saveFunction(model, filePathBase);

            workspaceDataSaveFunction(wcd1, filePathBase, lastExecutionDuration, modelsGuidToIdMap);

            lastExecutionDuration = new TimeSpan();

            var ws2 = WorkspaceModel.FromJson(json, model.LibraryServices,
                model.EngineController, model.Scheduler, model.NodeFactory, DynamoModel.IsTestMode, false,
                model.CustomNodeManager);

            if (ws2 is CustomNodeWorkspaceModel)
            {
                model.AddCustomNodeWorkspace((CustomNodeWorkspaceModel)ws2);
            }

            foreach (var c in ws2.Connectors)
            {
                Assert.NotNull(c.Start.Owner, "The node is not set for the start of connector " + c.GUID + ". The end node is " + c.End.Owner + ".");
                Assert.NotNull(c.End.Owner, "The node is not set for the end of connector " + c.GUID + ". The start node is " + c.Start.Owner + ".");
            }

            // The following logic is taken from the DynamoModel.Open method.
            // It assumes a single home workspace model. So, we remove all
            // others, before adding a new one.
            if (ws2 is HomeWorkspaceModel)
            {
                var currentHomeSpaces = model.Workspaces.OfType<HomeWorkspaceModel>().ToList();
                if (currentHomeSpaces.Any())
                {
                    var end = ws2 is HomeWorkspaceModel ? 0 : 1;

                    for (var i = currentHomeSpaces.Count - 1; i >= end; i--)
                    {
                        model.RemoveWorkspace(currentHomeSpaces[i]);
                    }
                }

                model.AddWorkspace(ws2);

                var hws = ws2 as HomeWorkspaceModel;
                if (hws != null)
                {
                    model.ResetEngine();

                    if (hws.RunSettings.RunType == RunType.Periodic)
                    {
                        hws.StartPeriodicEvaluation();
                    }
                }

                model.CurrentWorkspace = ws2;
            }

            Assert.NotNull(ws2);

            dummyNodes = ws2.Nodes.Where(n => n is DummyNode);
            if (dummyNodes.Any())
            {
                Assert.Inconclusive("The Workspace contains dummy nodes for: " + string.Join(",", dummyNodes.Select(n => n.Name).ToArray()));
            }

            var wcd2 = new serializationTestUtils.WorkspaceComparisonData(ws2, CurrentDynamoModel.EngineController);

            workspaceCompareFunction(wcd1, wcd2, modelsGuidToIdMap);

            var functionNodes = ws2.Nodes.Where(n => n is Function).Cast<Function>();
            if (functionNodes.Any())
            {
                Assert.True(functionNodes.All(n => CurrentDynamoModel.CustomNodeManager.LoadedDefinitions.Contains(n.Definition)));
            }

            foreach (var c in ws2.Connectors)
            {
                Assert.NotNull(c.Start.Owner);
                Assert.NotNull(c.End.Owner);
                Assert.True(ws2.Nodes.Contains(c.Start.Owner));
                Assert.True(ws2.Nodes.Contains(c.End.Owner));
            }

            //assert that the inputs in the saved json file are the same as those we can gather from the 
            //graph at runtime - because we don't deserialize these directly we check the json itself.
            //Use load vs parse to preserve date time strings.
            var jsonReader = new JsonTextReader(new StringReader(json));
            jsonReader.DateParseHandling = DateParseHandling.None;
            var jObject = JObject.Load(jsonReader);

            var jToken = jObject["Inputs"];
            var inputs = jToken.ToArray().Select(x => x.ToObject<NodeInputData>()).ToList();
            var inputs2 = ws1.Nodes.Where(x => x.IsSetAsInput == true && x.InputData != null).Select(input => input.InputData).ToList();

            //inputs2 might come from a WS with non guids, so we need to replace the ids with guids if they exist in the map
            foreach (var input in inputs2)
            {
                if (modelsGuidToIdMap.ContainsKey(input.Id))
                {
                    input.Id = GuidUtility.Create(GuidUtility.UrlNamespace, modelsGuidToIdMap[input.Id]);
                }
            }
            Assert.IsTrue(inputs.SequenceEqual(inputs2));

            //assert that the outputs in the saved json file are the same as those we can gather from the 
            //graph at runtime - because we don't deserialize these directly we check the json itself.
            var jTokenOutput = jObject["Outputs"];
            var outputs = jTokenOutput.ToArray().Select(x => x.ToObject<NodeOutputData>()).ToList();
            var outputs2 = ws1.Nodes.Where(x => x.IsSetAsOutput == true && x.OutputData != null).Select(output => output.OutputData).ToList();

            //Outputs2 might come from a WS with non guids, so we need to replace the ids with guids if they exist in the map
            foreach (var output in outputs2)
            {
                if (modelsGuidToIdMap.ContainsKey(output.Id))
                {
                    output.Id = GuidUtility.Create(GuidUtility.UrlNamespace, modelsGuidToIdMap[output.Id]);
                }
            }
            Assert.IsTrue(outputs.SequenceEqual(outputs2));
        }



        private static string ConvertCurrentWorkspaceToJsonAndSave(DynamoModel model, string filePathBase)
        {
            var json = model.CurrentWorkspace.ToJson(model.EngineController);
            Assert.That(json, Is.Not.Null.Or.Empty);

            var tempPath = Path.GetTempPath();
            var jsonFolder = Path.Combine(tempPath, jsonFolderName);

            if (!Directory.Exists(jsonFolder))
            {
                Directory.CreateDirectory(jsonFolder);
            }

            var jsonPath = filePathBase + ".dyn";
            if (File.Exists(jsonPath))
            {
                File.Delete(jsonPath);
            }
            File.WriteAllText(jsonPath, json);

            return json;
        }

        private string ConvertCurrentWorkspaceToNonGuidJsonAndSave(DynamoModel model, string filePathBase)
        {
            var json = model.CurrentWorkspace.ToJson(model.EngineController);

            json = serializationTestUtils.replaceModelIdsWithNonGuids(json, model.CurrentWorkspace, modelsGuidToIdMap);

            Assert.That(json, Is.Not.Null.Or.Empty);

            var tempPath = Path.GetTempPath();
            var jsonFolder = Path.Combine(tempPath, jsonNonGuidFolderName);

            if (!Directory.Exists(jsonFolder))
            {
                Directory.CreateDirectory(jsonFolder);
            }

            var jsonPath = filePathBase + ".dyn";
            if (File.Exists(jsonPath))
            {
                File.Delete(jsonPath);
            }
            File.WriteAllText(jsonPath, json);

            return json;
        }
    }
}
