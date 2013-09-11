using System;
using System.Linq;
using Dynamo.FSchemeInterop;
using Dynamo.Models;
using Microsoft.FSharp.Collections;
using NUnit.Framework;

namespace Dynamo.Tests
{
    public static class TestExtensions
    {
        public static NodeModel NodeFromWorkspace(this WorkspaceModel vm, string guidString)
        {
            Guid guid;
            Guid.TryParse(guidString, out guid);
            return vm.NodeFromWorkspace(guid);
        }

        public static NodeModel NodeFromWorkspace(this WorkspaceModel ws, Guid guid)
        {
            return ws.Nodes.FirstOrDefault(node => node.GUID == guid);
        }

        public static T NodeFromWorkspace<T>(this WorkspaceModel ws, Guid guid) 
            where T : NodeModel
        {
            var nodeToT = NodeFromWorkspace(ws, guid);
            Assert.NotNull(nodeToT);
            Assert.IsAssignableFrom(typeof(T), nodeToT);
            return (T)nodeToT;
        }

        public static T NodeFromWorkspace<T>(this WorkspaceModel ws, string guidString)
            where T : NodeModel
        {
            Guid guid;
            Guid.TryParse(guidString, out guid);
            return ws.NodeFromWorkspace<T>(guid);
        }

        public static T FirstNodeFromWorkspace<T>(this WorkspaceModel model)
            where T : NodeModel
        {
            return model.Nodes.OfType<T>().FirstOrDefault();
        }

        public static double GetDoubleFromFSchemeValue(this FScheme.Value value)
        {
            var doubleWatchVal = 0.0;
            Assert.AreEqual(true, Utils.Convert(value, ref doubleWatchVal));
            return doubleWatchVal;
        }

        public static string getStringFromFSchemeValue(this FScheme.Value value)
        {
            string stringValue = string.Empty;
            Assert.AreEqual(true, FSchemeInterop.Utils.Convert(value, ref stringValue));
            return stringValue;
        }


        public static FSharpList<FScheme.Value> GetListFromFSchemeValue(this FScheme.Value value)
        {
            FSharpList<FScheme.Value> listWatchVal = null;
            Assert.AreEqual(true, Utils.Convert(value, ref listWatchVal));
            return listWatchVal;
        }
    }
}