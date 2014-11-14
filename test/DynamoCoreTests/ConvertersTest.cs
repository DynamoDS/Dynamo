using Dynamo.Controls;
using NUnit.Framework;
using System.Windows;
using System;

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
            object result;

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
            result = converter.Convert(array, null, null, null);
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
            string name = "";
            string parameter = "";
            FullyQualifiedNameToDisplayConverter converter = new FullyQualifiedNameToDisplayConverter();
            object result;

            //1. Class name is "ClassA.ForTooltip". Parameter is "ToolTip".
            //2. Class name is "ClassWithReallyLoooooongName.ForTooltip". Parameter is "ToolTip".
            //3. Class name is "ClassA". Parameter is "ClassButton".
            //4. Class name is "ClAaB". Parameter is "ClassButton".
            //5. Class name is "ClassLongName". Parameter is "ClassButton".
            //6. Class name is "ClassWithReallyLongName". Parameter is "ClassButton".
            //7. Class name is empty. Parameter is "ToolTip".
            //8. Class name is empty. Parameter is "ClassButton".
            //9. Class name is empty. Parameter is empty.
            
            // 1 case
            name = "ClassA.ForTooltip";
            parameter = "ToolTip";
            result = converter.Convert(name, null, parameter, null);
            Assert.AreEqual("ClassA.ForTooltip", result);

            // 2 case
            name = "ClassWithReallyLoooooongName.ForTooltip";
            parameter = "ToolTip";
            result = converter.Convert(name, null, parameter, null);
            Assert.AreEqual("ClassWithReallyLoooooongName.\nForTooltip", result);

            // 3 case
            name = "ClassA";
            parameter = "ClassButton";
            result = converter.Convert(name, null, parameter, null);
            Assert.AreEqual("Class A", result);

            // 4 case
            name = "ClAaB";
            parameter = "ClassButton";
            result = converter.Convert(name, null, parameter, null);
            Assert.AreEqual("Cl Aa B", result);

            // 5 case
            name = "ClassLongName";
            parameter = "ClassButton";
            result = converter.Convert(name, null, parameter, null);
            Assert.AreEqual("Class \nLong Name", result);

            // 6 case
            name = "ClassWithReallyLongName";
            parameter = "ClassButton";
            result = converter.Convert(name, null, parameter, null);
            Assert.AreEqual("Class \nWith Really ..", result);

            // 7 case
            name = "";
            parameter = "ToolTip";
            result = converter.Convert(name, null, parameter, null);
            Assert.AreEqual("", result);

            // 8 case
            name = "";
            parameter = "ClassButton";
            result = converter.Convert(name, null, parameter, null);
            Assert.AreEqual("", result);

            // 9 case
            name = "";
            parameter = "";
            Assert.Throws<NotImplementedException>(delegate { converter.Convert(name, null, parameter, null); });
        }

        [Test]
        public void InOutParamTypeConverterTest()
        {
            string input = "";
            string parameter = "";
            InOutParamTypeConverter converter = new InOutParamTypeConverter();
            object result;

            //1. Input is empty. Parameter is empty.
            //2. Input is "input". Parameter is empty.
            //3. Input is "none". Parameter is empty.
            //4. Input is "none". Parameter is "inputParam".
            //5. Input is "someInput". Parameter is "inputParam".
            //6. Input is "someInput". Parameter is "someParam".

            // 1 case
            result = converter.Convert(input, null, parameter, null);
            Assert.AreEqual("", result);

            // 2 case
            input = "input";
            result = converter.Convert(input, null, parameter, null);
            Assert.AreEqual("input", result);

            // 3 case
            input = "none";
            result = converter.Convert(input, null, parameter, null);
            Assert.AreEqual("none", result);

            // 4 case
            input = "none";
            parameter = "inputParam";
            result = converter.Convert(input, null, parameter, null);
            Assert.AreEqual("none", result);

            // 5 case
            input = "someInput";
            parameter = "inputParam";
            result = converter.Convert(input, null, parameter, null);
            Assert.AreEqual(": someInput", result);

            // 6 case
            input = "someInput";
            parameter = "someParam";
            result = converter.Convert(input, null, parameter, null);
            Assert.AreEqual("someInput", result);
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
