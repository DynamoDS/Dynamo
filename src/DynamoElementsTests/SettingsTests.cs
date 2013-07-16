using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamoElementsTests
{
    using Dynamo.Nodes.Search;
    using Dynamo.Search;
    using NUnit.Framework;

    namespace DynamoElementsTests
    {

        [TestFixture]
        internal class SearchTests
        {
            [Test]
            public void CanAddCategory()
            {
                var model = new SearchViewModel();
                var root = model.AddRootCategory("Peter");
                var leafCat = new BrowserInternalElement("Boyer", root);
                root.Items.Add(leafCat);

                Assert.Contains(leafCat, root.Items);
                Assert.Contains(root, model.BrowserRootCategories);

            }

        }

    }

}
