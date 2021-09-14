using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Moq;
using NUnit.Framework;

namespace Dynamo.Tests.Search
{
    /// <summary>
    /// This test class was created in order to have public access to the next protected/private properties:
    /// protected readonly List<double> keywordWeights
    /// private readonly ICustomNodeSource customNodeManager
    /// </summary>
    public class CustomNodeSearchElementTest : NodeSearchElement
    {
        private readonly ICustomNodeSource customNodeManager;
        private string path;

        public Guid ID { get; private set; }

        public override string CreationName { get { return this.ID.ToString(); } }

        public string Path
        {
            get { return path; }
            private set
            {
                if (value == path) return;
                path = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomNodeSearchElement"/> class.
        /// </summary>
        /// <param name="customNodeManager">Custom node manager</param>
        /// <param name="info">Custom node info</param>
        public CustomNodeSearchElementTest(ICustomNodeSource customNodeManager, CustomNodeInfo info)
        {
            this.customNodeManager = customNodeManager;
            inputParameters = new List<Tuple<string, string>>();
            outputParameters = new List<string>();
            SyncWithCustomNodeInfo(info);
        }

        /// <summary>
        ///     Updates the properties of this search element.
        /// </summary>
        /// <param name="info">Actual data of custom node</param>        
        public void SyncWithCustomNodeInfo(CustomNodeInfo info)
        {
            ID = info.FunctionId;
            Name = info.Name;
            FullCategoryName = info.Category;
            Description = info.Description;
            Path = info.Path;
            iconName = ID.ToString();

            ElementType = ElementTypes.CustomNode;
            if (info.IsPackageMember)
                ElementType |= ElementTypes.Packaged; // Add one more flag.
        }

        protected override NodeModel ConstructNewNodeModel()
        {
            return customNodeManager.CreateCustomNodeInstance(ID);
        }

        public ICollection<double> WeightsCollection
        {
            get { return keywordWeights; }
        }

        public ICustomNodeSource getCustomNodeManager()
        {
            return customNodeManager;
        }
    }

    [TestFixture]
    class SearchLibraryTest
    {
        private static NodeSearchModel search;

        [SetUp]
        public void Init()
        {
            search = new NodeSearchModel();
        }

        /// <summary>
        /// This test method will execute the SearchLibrary.Update(TEntry entry, bool isCategoryChanged = false) method
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestSearchLibraryUpdate()
        {
            //Arrange
            const string nodeName = "TheNoodle";
            const string catName = "TheCat";
            const string descr = "TheCat";
            const string path = @"C:\temp\graphics.dyn";
            const string newNodeName = "TheTurtle";

            var guid1 = Guid.NewGuid();
            var dummyInfo1 = new CustomNodeInfo(guid1, nodeName, catName, descr, path);
            var dummySearch1 = new CustomNodeSearchElementTest(null, dummyInfo1);
            var newInfo = new CustomNodeInfo(guid1, newNodeName, catName, descr, path);

            dummySearch1.SearchKeywords.Add("tag1");
            dummySearch1.WeightsCollection.Add(10);

            //Act
            search.Add(dummySearch1);
            dummySearch1.SyncWithCustomNodeInfo(newInfo);
            search.Update(dummySearch1);//internal void Update(TEntry entry, bool isCategoryChanged = false)
            search.Update(dummySearch1,true);

            //Assert
            //Just validate that the tags and weights were added correctly
            Assert.AreEqual(dummySearch1.SearchKeywords.Count, 1);
            Assert.AreEqual(dummySearch1.WeightsCollection.Count, 1);
        }

        /// <summary>
        /// This test method will execute the event OnItemProduced from the SearchLibrary class
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestSearchLibraryOnItemProduced()
        {
            //Arrange
            const string nodeName = "TheNoodle";
            const string catName = "TheCat";
            const string descr = "TheCat";
            const string path = @"C:\temp\graphics.dyn";

            var guid1 = Guid.NewGuid();
            var dummyInfo1 = new CustomNodeInfo(guid1, nodeName, catName, descr, path);
            var moq = new Mock<ICustomNodeSource>();
            var dummySearch1 = new CustomNodeSearchElementTest(moq.Object, dummyInfo1);

            //Act
            search.Add(dummySearch1);
            dummySearch1.ProduceNode();//Internally this will execute the event OnItemProduced

            //Assert
            //Just validate that the OnItemProduced(ConstructNewNodeModel()); method was executed
            Assert.IsNotNull(dummySearch1.getCustomNodeManager());

            //This will execute the event OnItemProduced when the parameter is null
            Assert.Throws<NullReferenceException>(() => search.Add(null));
        }

        /// <summary>
        /// This test method will execute the SearchCategoryUtil.GetAllCategoryNames method
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestSearchGetAllCategoryNames()
        {
            //Arrange
            IEnumerable<string> allCategoryNames = null;

            //Arrange
            const string nodeName = "TheNoodle";
            const string catName = "TheCat.CatSon";//This will create a Category and Subcategory
            const string descr = "TheCat";
            const string path = @"C:\temp\graphics.dyn";

            var guid1 = Guid.NewGuid();
            var dummyInfo1 = new CustomNodeInfo(guid1, nodeName, catName, descr, path);
            var dummySearch1 = new CustomNodeSearchElementTest(null, dummyInfo1);

            var searchLibrary = new SearchLibrary<NodeSearchElement, NodeModel>();
            searchLibrary.Add(dummySearch1);

            //Act
            var root = SearchCategoryUtil.CategorizeSearchEntries(
                searchLibrary.SearchEntries,
                entry => entry.Categories);

            foreach (var category in root.SubCategories)
            {
                allCategoryNames = SearchCategoryUtil.GetAllCategoryNames(category);
            }

            //Assert
            //Just validate that search entries is not null
            Assert.IsNotNull(root);
            //Validate that the GetAllCategoryNames returned at least one element
            Assert.Greater(allCategoryNames.Count(),0);
        }

    }
} 

