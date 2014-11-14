using Dynamo.Controls;
using NUnit.Framework;
using System.Windows;

namespace Dynamo
{
    class ConvertersTest
    {
        [Test]
        public void SearchResultsToVisibilityConverterTest()
        {
            SearchResultsToVisibilityConverter converter = new SearchResultsToVisibilityConverter();
            int numberOfFoundSearchCategories = 0;
            bool addonsVisibility = false;
            string searchText = "";

            object[] array = { numberOfFoundSearchCategories, addonsVisibility, searchText };

            //1. There are no found search categories. Addons are invisible. Search text is empty.
            //2. There are no found search categories. Addons are invisible. Search text is not empty.
            //3. There are no found search categories. Addons are visible. Search text is empty.
            //4. There are no found search categories. Addons are visible. Search text is not empty.
            //5. There are some search categories. Addons are invisible. Search text is not empty.
            //6. There are some search categories. Addons are invisible. Search text is empty.
            //7. There are some search categories. Addons are visible. Search text is not empty.
            //8. There are some search categories. Addons are visible. Search text is empty.

            // 1 case
            object result = converter.Convert(array, null, null, null);
            Assert.AreEqual(Visibility.Collapsed, result);

            // 2 case
            searchText = "search text";
            array[2] = searchText;
            result = converter.Convert(array, null, null, null);
            Assert.AreEqual(Visibility.Visible, result);

            // 3 case
            searchText = "";
            array[2] = searchText;
            addonsVisibility = true;
            array[1] = addonsVisibility;
            result = converter.Convert(array, null, null, null);
            Assert.AreEqual(Visibility.Collapsed, result);

            // 4 case
            searchText = "search text";
            array[2] = searchText;
            result = converter.Convert(array, null, null, null);
            Assert.AreEqual(Visibility.Collapsed, result);

            // 5 case
            numberOfFoundSearchCategories = 5;
            array[0] = numberOfFoundSearchCategories;
            addonsVisibility = false;
            array[1] = addonsVisibility;
            result = converter.Convert(array, null, null, null);
            Assert.AreEqual(Visibility.Collapsed, result);

            // 6 case
            searchText = "";
            array[2] = searchText;
            result = converter.Convert(array, null, null, null);
            Assert.AreEqual(Visibility.Collapsed, result);

            // 7 case
            addonsVisibility = true;
            array[1] = addonsVisibility;
            searchText = "search text";
            array[2] = searchText;
            result = converter.Convert(array, null, null, null);
            Assert.AreEqual(Visibility.Collapsed, result);

            // 8 case
            searchText = "";
            array[2] = searchText;
            result = converter.Convert(array, null, null, null);
            Assert.AreEqual(Visibility.Collapsed, result);
        }

        [Test]
        public void FullyQualifiedNameToDisplayConverterTest()
        {
        }

        [Test]
        public void InOutParamTypeConverterTest()
        {
        }

        [Test]
        public void BrowserRootElementToSubclassesConverterTest()
        {
        }

        [Test]
        public void DisplayModeToTextDecorationsConverterTest()
        {
        }

        [Test]
        public void ViewModeToVisibilityConverterTest()
        {
        }

        [Test]
        public void ElementTypeToBoolConverterTest()
        {
        }

        [Test]
        public void NodeTypeToColorConverterTest()
        {
        }

        [Test]
        public void RootElementToBoolConverterTest()
        {
        }

        [Test]
        public void HasParentRootElementTest()
        {
        }

        [Test]
        public void MultiBoolToVisibilityConverterTest()
        {
        }

        [Test]
        public void NullValueToCollapsedConverterTest()
        {
        }

        [Test]
        public void FullCategoryNameToMarginConverterTest()
        {
        }

        [Test]
        public void IntToVisibilityConverterTest()
        {
        }

        [Test]
        public void SearchHighlightMarginConverterTest()
        {
        }
    }
}
