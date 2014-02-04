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

        public static T NodeFromWorkspace<T>(this WorkspaceModel ws, Guid guid) where T : NodeModel
        {
            var nodeToT = NodeFromWorkspace(ws, guid);
            Assert.NotNull(nodeToT);
            Assert.IsAssignableFrom(typeof(T), nodeToT);
            return nodeToT as T;
        }

        public static T NodeFromWorkspace<T>(this WorkspaceModel ws, string guidString)
            where T : NodeModel
        {
            Guid guid;
            Guid.TryParse(guidString, out guid);
            return ws.NodeFromWorkspace<T>(guid);
        }

        public static T FirstNodeFromWorkspace<T>(this WorkspaceModel model) where T : NodeModel
        {
            return model.Nodes.OfType<T>().FirstOrDefault();
        }

        public static double GetDoubleFromFSchemeValue(this FScheme.Value value)
        {
            var doubleWatchVal = 0.0;
            Assert.IsTrue(Utils.Convert(value, ref doubleWatchVal));
            return doubleWatchVal;
        }

        public static string GetStringFromFSchemeValue(this FScheme.Value value)
        {
            string stringValue = string.Empty;
            Assert.IsTrue(Utils.Convert(value, ref stringValue));
            return stringValue;
        }

        public static FSharpList<FScheme.Value> GetListFromFSchemeValue(this FScheme.Value value)
        {
            FSharpList<FScheme.Value> listWatchVal = null;
            Assert.IsTrue(Utils.Convert(value, ref listWatchVal));
            return listWatchVal;
        }

        public static T GetObjectFromFSchemeValue<T>(this FScheme.Value value)
        {
            Assert.IsInstanceOf<FScheme.Value.Container>(value);
            var o = (value as FScheme.Value.Container).Item;
            Assert.IsInstanceOf<T>(o);
            return (T)o;
        }

        public static dynamic UnwrapFSchemeValue(this FScheme.Value value)
        {
            return value.IsList
                ? (value as FScheme.Value.List).Item.Select(UnwrapFSchemeValue).ToFSharpList()
                : (value as dynamic).Item;
        }
    }
}