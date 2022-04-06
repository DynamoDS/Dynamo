﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Dynamo.Graph.Nodes;
using Dynamo.Models;

using NUnit.Framework;

using ProtoCore.Mirror;

namespace Dynamo.Tests
{
    public abstract class DSEvaluationUnitTestBase : UnitTestBase
    {
        protected abstract DynamoModel GetModel();

        protected virtual void GetLibrariesToPreload(List<string> libraries)
        {
            // Nothing here by design. If you find yourself having to add 
            // anything here, something must be wrong. DynamoViewModelUnitTest
            // is designed to contain no test cases, so it does not need any 
            // preloaded library, all of which should only be specified in the
            // derived class.
        }

        protected static string GetAppDataFolder()
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dynamoVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            var appDataFolder = Path.Combine(Path.Combine(folder, "Dynamo", "Dynamo Core"),
                $"{dynamoVersion.FileMajorPart}.{dynamoVersion.FileMinorPart}");

            return appDataFolder;
        }

        protected virtual string GetUserUserDataRootFolder()
        {
            // Override in derived classed to provide a custom
            // UserAppDataRootFolder. Returning an empty string
            // here will cause the PathManager to use its default.
            return string.Empty;
        }

        protected virtual string GetCommonDataRootFolder()
        {
            // Override in derived classed to provide a custom
            // CommonAppDataRootFolder. Returning an empty string
            // here will cause the PathManager to use its default.
            return string.Empty;
        }

        protected void AssertNoDummyNodes()
        {
            var nodes = GetModel().CurrentWorkspace.Nodes;

            var dummyNodes = nodes.OfType<DummyNode>();
            string logs = string.Empty;
            foreach (var node in dummyNodes)
            {
                logs += string.Format("{0} is a {1} node\n", node.Name, node.NodeNature);
            }

            double dummyNodesCount = dummyNodes.Count();
            if (dummyNodesCount >= 1)
            {
                Assert.Fail(logs + "Number of dummy nodes found in Sample: " + dummyNodesCount);
            }
        }

        protected IEnumerable<object> GetPreviewValues()
        {
            var objects = new List<object>();
            foreach (var node in GetModel().CurrentWorkspace.Nodes)
            {
                objects.Add(GetPreviewValue(node.GUID));
            }
            return objects;
        }

        protected object GetPreviewValue(Guid guid)
        {
            string varname = GetVarName(guid);
            var mirror = GetRuntimeMirror(varname);
            Assert.IsNotNull(mirror);

            return mirror.GetData().Data;
        }

        protected string GetVarName(Guid guid)
        {
            var node = GetModel().CurrentWorkspace.NodeFromWorkspace(guid);
            Assert.IsNotNull(node);

            int outportCount = node.OutPorts.Count;
            Assert.IsTrue(outportCount > 0);

            return outportCount > 1 ? node.AstIdentifierBase : node.GetAstIdentifierForOutputIndex(0).Value;
        }

        protected string GetVarName(string guid)
        {
            var node = GetModel().CurrentWorkspace.NodeFromWorkspace(guid);

            Assert.IsNotNull(node);

            int outportCount = node.OutPorts.Count;
            Assert.IsTrue(outportCount > 0);

            return outportCount > 1 ? node.AstIdentifierBase : node.GetAstIdentifierForOutputIndex(0).Value;
        }

        /// <summary>
        ///   Used to reflect on runtime data such as values of a variable
        /// </summary>
        protected RuntimeMirror GetRuntimeMirror(string varName)
        {
            RuntimeMirror mirror = null;
            Assert.DoesNotThrow(() => mirror = GetModel().EngineController.GetMirror(varName));
            return mirror;
        }

        protected void AssertNullValues()
        {
            foreach (var node in GetModel().CurrentWorkspace.Nodes)
            {
                string varname = GetVarName(node.GUID);
                var mirror = GetRuntimeMirror(varname);
                Assert.IsNull(mirror);
            }
        }

        protected void AssertNonNull(string guid)
        {
            var previewVariable = GetVarName(guid);
            var mirror = GetRuntimeMirror(previewVariable);
            Assert.IsNotNull(mirror);

            var mirrorData = mirror.GetData();
            if (mirrorData.IsCollection)
            {
                var elements = mirrorData.GetElements();
                Assert.IsNotNull(elements);
                return;
            }

            Assert.IsNotNull(mirrorData);
            Assert.IsNotNull(mirrorData.Data);
        }

        protected void AssertError(string guid)
        {
            var node = GetModel().CurrentWorkspace.Nodes.First(n => n.GUID.ToString() == guid);
            Assert.True(node.IsInErrorState);
        }

        protected void AssertPreviewValue(string guid, object value)
        {
            string previewVariable = GetVarName(guid);
            AssertValue(previewVariable, value);
        }

        protected void AssertValue(string varname, object value)
        {
            var mirror = GetRuntimeMirror(varname);
            //Couldn't find the variable, so expected value should be null.
            if (mirror == null)
            {
                if (value != null)
                    Assert.IsNotNull(mirror, string.Format("Variable : {0}, not found.", varname));
                return;
            }

            Console.WriteLine(varname + " = " + mirror.GetStringData());
            var svValue = mirror.GetData();
            AssertValue(svValue, value);
        }

        protected void AssertValue(MirrorData data, object value)
        {
            if (data.IsCollection)
            {
                if (!(value is IEnumerable))
                {
                    Assert.Fail("Data is collection but expected value is not.");
                }
                AssertCollection(data, value as IEnumerable);
            }
            else if (value == null)
            {
                Assert.IsTrue(data.IsNull, String.Format("Data {0} is not null", data.StringData));
            }
            else if (value is int)
            {
                try
                {
                    int mirrorData = Convert.ToInt32(data.Data);
                    Assert.AreEqual((int)value, mirrorData, String.Format("Integer values are not equal value: {0} and data {1}", value, mirrorData));
                }
                catch (Exception e)
                {
                    Assert.Fail(String.Format("Failed on Int assertion data {0} with this message: {1}", data.StringData, e.Message));
                }
            }
            else if (value is double)
            {
                try
                {
                    double mirrorData = Convert.ToDouble(data.Data);
                    Assert.AreEqual((double)value, mirrorData, 0.00001, String.Format("Double values are not equal value: {0} and data {1}", value, mirrorData));
                }
                catch (Exception e)
                {
                    Assert.Fail(String.Format("Failed on Double assertion data {0} with this message: {1}", data.StringData, e.Message));
                }
            }
            else if (data.IsPointer && data.Class.ClassName == "Function")
            {
                Assert.AreEqual(data.Class.ClassName, value, String.Format("Failed to validate the finding of Function {0},  value {1}", data.Class.ClassName, value));
            }
            else if (data.IsDictionary)
            {
                var thisData = data.Data as DesignScript.Builtin.Dictionary;

                if (value is DesignScript.Builtin.Dictionary)
                {
                    var otherVal = (DesignScript.Builtin.Dictionary)value;

                    Assert.AreEqual(otherVal.Count, thisData.Count, String.Format("The number of elements on the collections (DesignScript.Builtin.Dictionary) didn't match, value size: {0} data size: {1} , so data and expected value are 2 different dictionaries", otherVal.Count, thisData.Count));

                    foreach (var key in otherVal.Keys)
                    {
                        var val = thisData.ValueAtKey(key);

                        Assert.IsNotNull(val, String.Format("Element with key {0} was not found on the  data DesignScript.Builtin.Dictionary, so data and expected value are 2 different dictionaries.", key));

                        if (val.GetType().IsValueType)
                        {
                            Assert.AreEqual(val, thisData.ValueAtKey(key), String.Format("Elements with key {0} are not equal on the dictionaries (DesignScript.Builtin.Dictionary), value element: {1} and data element: {2}", key, val, thisData.ValueAtKey(key)));
                        }
                    }
                }
                else if (value is IDictionary)
                {
                    var otherVal = (IDictionary)value;

                    Assert.AreEqual(otherVal.Count, thisData.Count, String.Format("The number of elements on the collections (IDictionary) didn't match, value size: {0} data size: {1} , so data and expected value are 2 different dictionaries", otherVal.Count, thisData.Count));

                    foreach (var key in otherVal.Keys)
                    {
                        Assert.IsTrue(key is string, String.Format("The key {0} on the value dictionary is not a string", key));

                        var strKey = (string)key;
                        var val = thisData.ValueAtKey(strKey);

                        Assert.IsNotNull(val, String.Format("Element with key {0} was not found on the  data IDictionary, so data and expected value are 2 different dictionaries.", strKey));

                        if (val.GetType().IsValueType)
                        {
                            Assert.AreEqual(val, thisData.ValueAtKey(strKey), String.Format("Elements with key {0} are not equal on the dictionaries (IDictionary), value element: {1} and data element: {2}", strKey, val, thisData.ValueAtKey(strKey)));
                        }
                    }
                }
            }
            else
                Assert.AreEqual(value, data.Data, String.Format("The value {0} and data {1} are not equal", value, data));
        }

        private void AssertCollection(MirrorData data, IEnumerable collection)
        {
            Assert.IsTrue(data.IsCollection);
            List<MirrorData> elements = data.GetElements().ToList();
            int i = 0;
            foreach (var item in collection)
            {
                AssertValue(elements[i++], item);
            }
        }

        protected void SelectivelyAssertPreviewValues(string guid, Dictionary<int, object> selectedValue)
        {
            string previewVariable = GetVarName(guid);
            SelectivelyAssertValues(previewVariable, selectedValue);
        }

        /// <summary>
        /// To selectively verify the result, which is a collection, at some
        /// positions.
        /// </summary>
        /// <param name="varname"></param>
        /// <param name="selectedValues">Values to verify</param>
        protected void SelectivelyAssertValues(string varname, Dictionary<int, object> selectedValues)
        {
            var mirror = GetRuntimeMirror(varname);
            //Couldn't find the variable, so expected value should be null.
            if (mirror == null)
            {
                if (selectedValues != null)
                    Assert.IsNotNull(mirror, string.Format("Variable : {0}, not found.", varname));
                return;
            }

            Console.WriteLine(varname + " = " + mirror.GetStringData());
            var svValue = mirror.GetData();
            SelectivelyAssertValues(svValue, selectedValues);
        }

        private void SelectivelyAssertValues(MirrorData data, Dictionary<int, object> selectedValues)
        {
            Assert.IsTrue(data.IsCollection);

            if (data.IsCollection)
            {
                List<MirrorData> elements = data.GetElements().ToList();
                foreach (var pair in selectedValues)
                {
                    AssertValue(elements[pair.Key], pair.Value);
                }
            }
        }

        /// <summary>
        /// Used to reflect on static data such as classes and class members
        /// </summary>
        /// <param name="varName"></param>
        /// <returns></returns>
        protected ClassMirror GetClassMirror(string className)
        {
            ProtoCore.Core core = GetModel().EngineController.LiveRunnerCore;
            var classMirror = new ClassMirror(className, core);
            return classMirror;
        }

        protected void VerifyModelExistence(Dictionary<string, bool> modelExistenceMap)
        {
            var nodes = GetModel().CurrentWorkspace.Nodes;
            foreach (var pair in modelExistenceMap)
            {
                Guid guid = Guid.Parse(pair.Key);
                var node = nodes.FirstOrDefault((x) => (x.GUID == guid));
                bool nodeExists = (null != node);
                Assert.AreEqual(nodeExists, pair.Value);
            }
        }

        protected void AssertPreviewCount(string guid, int count)
        {
            string varname = GetVarName(guid);
            var mirror = GetRuntimeMirror(varname);
            Assert.IsNotNull(mirror);

            var data = mirror.GetData();
            Assert.IsTrue(data.IsCollection, "preview data is not a list");
            Assert.AreEqual(count, data.GetElements().ToList().Count);
        }

        protected object GetPreviewValueAtIndex(string guid, int index)
        {
            string varname = GetVarName(guid);
            var mirror = GetRuntimeMirror(varname);
            Assert.IsNotNull(mirror);

            return mirror.GetData().GetElements().ToList()[index].Data;
        }

        protected void AssertInfinity(string dsVariable, int startBlock = 0)
        {
            RuntimeMirror mirror = GetRuntimeMirror(dsVariable);
            MirrorData data = mirror.GetData();
            Assert.IsTrue(System.Double.IsInfinity(Convert.ToDouble(data.Data)));
        }

        /// <summary>
        /// Compares preview value of two nodes and asserts they are same.
        /// </summary>
        /// <param name="guid1">guid for first node</param>
        /// <param name="guid2">guid for second node</param>
        protected void AssertSamePreviewValues(string guid1, string guid2)
        {
            string var1 = GetVarName(guid1);
            var data1 = GetRuntimeMirror(var1).GetData();
            string var2 = GetVarName(guid2);
            var data2 = GetRuntimeMirror(var2).GetData();
            AssertMirrorData(data1, data2);
        }

        protected void AssertClassName(string guid, string className)
        {
            var classMirror = GetClassMirror(className);
            Assert.IsNotNull(classMirror);

            Assert.AreEqual(classMirror.Name, className);
        }

        protected object GetPreviewValue(string guid)
        {
            string varname = GetVarName(guid);
            var mirror = GetRuntimeMirror(varname);
            Assert.IsNotNull(mirror);

            return mirror.GetData().Data;
        }

        protected string GetPreviewValueInString(string guid)
        {
            string varname = GetVarName(guid);
            var mirror = GetRuntimeMirror(varname);
            Assert.IsNotNull(mirror);
            return mirror.GetStringData();
        }

        private void AssertMirrorData(MirrorData data1, MirrorData data2)
        {
            if (data1.IsNull)
                Assert.True(data2.IsNull);
            else if (data1.IsCollection)
            {
                Assert.True(data2.IsCollection);
                List<MirrorData> elems1 = data1.GetElements().ToList();
                List<MirrorData> elems2 = data2.GetElements().ToList();
                Assert.AreEqual(elems1.Count, elems2.Count);
                int i = 0;
                foreach (var item in elems1)
                {
                    AssertMirrorData(item, elems2[i++]);
                }
            }
            else
                Assert.AreEqual(data1.Data, data2.Data);
        }

        protected List<object> GetFlattenedPreviewValues(string guid)
        {
            string varname = GetVarName(guid);
            var mirror = GetRuntimeMirror(varname);
            Assert.IsNotNull(mirror);
            var data = mirror.GetData();
            if (data == null) return null;
            if (!data.IsCollection)
            {
                return data.Data == null ? new List<object>() : new List<object>() { data.Data };
            }
            var elements = data.GetElements();

            var objects = GetSublistItems(elements);

            return objects;
        }

        private static List<object> GetSublistItems(IEnumerable<MirrorData> datas)
        {
            var objects = new List<object>();
            foreach (var data in datas)
            {
                if (!data.IsCollection)
                {
                    objects.Add(data.Data);
                }
                else
                {
                    objects.AddRange(GetSublistItems(data.GetElements()));
                }
            }
            return objects;
        }
    }
}
