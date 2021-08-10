using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using NUnit.Framework;
using ProtoFFI.Reflection;

namespace Dynamo
{
    [NodeDescription("This is a test node.")]
    [NodeName("Test Node")]
    public class TestNode : NodeModel
    {
        public TestNode()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("input A", "This is input A.")));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("output A", "This is output A.")));
            RegisterAllPorts();
        }
    }

    [NodeDescription("This is another test node.")]
    [NodeName("Dummy test Node")]
    [InPortNames("input1", "input2")]
    [InPortTypes("int", "double")]
    [InPortDescriptions("This is input1", "This is input2")]

    [OutPortNames("output1", "output2")]
    [OutPortTypes("foo", "bla")]
    [OutPortDescriptions(typeof(Dynamo.Properties.Resources), "DescriptionResource1")]
    public class DummyNodeModel : NodeModel
    {
        public DummyNodeModel()
        {
            RegisterAllPorts();
        }
    }

    /// <summary>
    /// DerivedTestNode is used to test a node class which derives from
    /// another node class and needs to add nodes.
    /// </summary>
    public class DerivedTestNode : TestNode
    {
        public DerivedTestNode()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("input B", "This is input B.")));
        }
    }

    [TestFixture]
    class NodeConstructionTests
    {
        [Test]
        public void TestNodeHasToolTipsOnInputPorts()
        {
            var node = new TestNode();
            Assert.AreEqual(node.InPorts[0].ToolTip, "This is input A.");
        }

        [Test]
        public void TestNodeHasToolTipsOnOutputPorts()
        {
            var node = new TestNode();
            Assert.AreEqual(node.OutPorts[0].ToolTip, "This is output A.");
        }

        [Test]
        public void DerivedTestNodeHasToolTipsOnInputPorts()
        {
            var node = new DerivedTestNode();
            Assert.AreEqual(node.InPorts[1].ToolTip, "This is input B.");
        }

        [Test]
        public void TestNodeCanLoadInputPortsFromAttributes()
        {
            var node = new DummyNodeModel();
            Assert.AreEqual(2, node.InPorts.Count);

            Assert.AreEqual("input1", node.InPorts[0].Name);
            Assert.AreEqual("input2", node.InPorts[1].Name);

            Assert.AreEqual("This is input1", node.InPorts[0].ToolTip);
            Assert.AreEqual("This is input2", node.InPorts[1].ToolTip);

            var typeLoadData = new TypeLoadData(node.GetType());
            Assert.AreEqual(2, typeLoadData.InputParameters.Count());

            Assert.AreEqual(Tuple.Create("input1", "int"), typeLoadData.InputParameters.ElementAt(0));
            Assert.AreEqual(Tuple.Create("input2", "double"), typeLoadData.InputParameters.ElementAt(1));
        }

        [Test]
        public void TestNodeCanLoadOutputPortsFromAttributes()
        {
            var node = new DummyNodeModel();
            Assert.AreEqual(2, node.InPorts.Count);

            Assert.AreEqual("output1", node.OutPorts[0].Name);
            Assert.AreEqual("output2", node.OutPorts[1].Name);

            Assert.AreEqual("some description", node.OutPorts[0].ToolTip);
            Assert.AreEqual("", node.OutPorts[1].ToolTip);

            var typeLoadData = new TypeLoadData(node.GetType());
            Assert.AreEqual(2, typeLoadData.OutputParameters.Count());

            Assert.AreEqual("foo", typeLoadData.OutputParameters.ElementAt(0));
            Assert.AreEqual("bla", typeLoadData.OutputParameters.ElementAt(1));
        }


        [Test]
        public void CanCreateTypeLoadDataFromReflectionOnlyLoadedType()
        {
            // Arrange
            var coreDirectory = new DirectoryInfo(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
            var coreNodeModelsDll = coreDirectory.GetFiles("CoreNodeModels.dll", SearchOption.AllDirectories).FirstOrDefault();

            // Act
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomain_ReflectionOnlyAssemblyResolve;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve; ;
            var loadedAssembly = Assembly.LoadFrom(coreNodeModelsDll.FullName);
            var reflectionAssembly = Assembly.ReflectionOnlyLoadFrom(coreNodeModelsDll.FullName);

            System.Type[] reflectionTypes;
            try
            {
                reflectionTypes = reflectionAssembly.GetTypes();
            }
            // see https://docs.microsoft.com/en-us/dotnet/api/system.reflection.assembly.gettypes?view=netframework-4.8#remarks
            catch (ReflectionTypeLoadException ex)
            {
                reflectionTypes = ex.Types;
            }

            var nodeModelAssemblyName = typeof(NodeModel).Assembly.GetName();
            var dynamoCoreAsm = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies()
                .Where(x => x.GetName().Name == nodeModelAssemblyName.Name)
                .FirstOrDefault();
            var nodeModelType = dynamoCoreAsm.GetType("Dynamo.Graph.Nodes.NodeModel");

            var typeLoadDatasFromReflection = new List<TypeLoadData>();
            foreach (var type in reflectionTypes)
            {
                if (!NodeModelAssemblyLoader.IsNodeSubTypeReflectionLoaded(type, nodeModelType))
                {
                    continue;
                }
                typeLoadDatasFromReflection.Add(new TypeLoadData(type, type.GetAttributesFromReflectionContext().ToArray()));
            }

            var loadedTypes = loadedAssembly.GetTypes();
            var typeLoadDatas = new List<TypeLoadData>();
            foreach (var type in loadedTypes)
            {
                if (!NodeModelAssemblyLoader.IsNodeSubType(type)) continue;
                typeLoadDatas.Add(new TypeLoadData(type));
            }

            // Assert
            Assert.AreEqual(typeLoadDatas, typeLoadDatasFromReflection);

        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var requestedAssembly = new AssemblyName(args.Name);

            Assembly assembly = null;
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            try
            {
                assembly = Assembly.Load(requestedAssembly.FullName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            return assembly;
        }

        internal static Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var requestedAssembly = new AssemblyName(args.Name);

            Assembly assembly = null;

            try
            {
                assembly = Assembly.ReflectionOnlyLoad(requestedAssembly.FullName);
            }
            catch (Exception e)
            {

                var t = e;
            }
            
            return assembly;
        }
    }
}
