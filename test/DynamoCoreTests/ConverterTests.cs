using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dynamo.Controls;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels;
using NUnit.Framework;

namespace Dynamo.Tests
{
    class ConverterTests
    {
        [Test]
        public void SearchResultsToVisibilityConverterTest()
        {
            SearchResultsToVisibilityConverter converter = new SearchResultsToVisibilityConverter();
            int numberOfFoundSearchCategories = 0;
            string searchText = "";
            object result;

            object[] array = { numberOfFoundSearchCategories, searchText };

            //1. There are no found search categories. Search text is empty.
            //2. There are no found search categories. Search text is not empty.
            //3. There are some search categories. Search text is not empty.
            //4. There are some search categories. Search text is empty.

            // 1 case
            result = converter.Convert(array, null, null, null);
            Assert.AreEqual(Visibility.Collapsed, result);

            // 2 case
            searchText = "search text";
            array[1] = searchText;
            result = converter.Convert(array, null, null, null);
            Assert.AreEqual(Visibility.Visible, result);

            // 3 case
            array[0] = 1;
            array[1] = "search text";
            result = converter.Convert(array, null, null, null);
            Assert.AreEqual(Visibility.Collapsed, result);

            // 4 case
            array[0] = 1;
            array[1] = "";
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
            Assert.AreEqual("Class\nLong Name", result);

            // 6 case
            name = "ClassWithReallyLongName";
            parameter = "ClassButton";
            result = converter.Convert(name, null, parameter, null);
            Assert.AreEqual("Class\n..ng Name", result);

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
            Assert.AreEqual(" : someInput", result);

            // 6 case
            input = "someInput";
            parameter = "someParam";
            result = converter.Convert(input, null, parameter, null);
            Assert.AreEqual("someInput", result);
        }

        [Test]
        public void ViewModeToVisibilityConverterTest()
        {
            ViewModeToVisibilityConverter converter = new ViewModeToVisibilityConverter();
            string parameter = "";
            SearchViewModel.ViewMode viewMode = SearchViewModel.ViewMode.LibraryView;
            object result;

            //1. Parameter is null.
            //2. View mode is null.
            //3. View mode is LibraryView. Parameter is empty.
            //4. View mode is LibraryView. Parameter is "LibraryView".
            //5. View mode is LibraryView. Parameter is "LibrarySearchView".

            // 1 case
            result = converter.Convert(viewMode, null, null, null);
            Assert.AreEqual(Visibility.Collapsed, result);

            // 2 case
            Assert.Throws<NullReferenceException>(delegate { converter.Convert(null, null, parameter, null); });

            // 3 case
            result = converter.Convert(viewMode, null, parameter, null);
            Assert.AreEqual(Visibility.Collapsed, result);

            // 4 case
            parameter = "LibraryView";
            result = converter.Convert(viewMode, null, parameter, null);
            Assert.AreEqual(Visibility.Visible, result);

            // 5 case
            parameter = "LibrarySearchView";
            result = converter.Convert(viewMode, null, parameter, null);
            Assert.AreEqual(Visibility.Collapsed, result);
        }

        [Test]
        public void ElementTypeToBoolConverterTest()
        {
            ElementTypeToBoolConverter converter = new ElementTypeToBoolConverter();
            var NseVM = new NodeSearchElementViewModel(
                new NodeModelSearchElement(new TypeLoadData(typeof(Dynamo.Nodes.Symbol))), null);
            var NcVM = new NodeCategoryViewModel("");
            var RncVM = new RootNodeCategoryViewModel("");
            var CncVM = new ClassesNodeCategoryViewModel(RncVM);

            object result;

            //1. Element is null.
            //2. Element is NodeSearchElement.
            //3. Element is NodeCategoryViewModel.
            //4. Element is RootNodeCategoryViewModel.
            //5. Element is RootNodeCategoryViewModel with ClassesNodeCategoryViewModel.

            // 1 case
            result = converter.Convert(null, null, null, null);
            Assert.AreEqual(false, result);

            // 2 case
            result = converter.Convert(NseVM, null, null, null);
            Assert.AreEqual(false, result);

            // 3 case
            result = converter.Convert(NcVM, null, null, null);
            Assert.AreEqual(true, result);

            // 4 case
            result = converter.Convert(RncVM, null, null, null);
            Assert.AreEqual(true, result);

            // 5 case
            RncVM.SubCategories.Add(CncVM);
            result = converter.Convert(RncVM, null, null, null);
            Assert.AreEqual(false, result);
        }

        [Test]
        public void NodeTypeToColorConverterTest()
        {
            NodeTypeToColorConverter converter = new NodeTypeToColorConverter();
            SolidColorBrush trueBrush = new SolidColorBrush(Colors.Green);
            SolidColorBrush falseBrush = new SolidColorBrush(Colors.Red);
            converter.FalseBrush = falseBrush;
            converter.TrueBrush = trueBrush;

            object result;

            //1. Element is null.
            //2. Element is CustomNodeSearchElement.

            // 1 case
            result = converter.Convert(null, null, null, null);
            Assert.AreEqual(falseBrush, result);

            // 2 case
            var CneVM = new CustomNodeSearchElementViewModel(
                new CustomNodeSearchElement(null, new CustomNodeInfo(Guid.NewGuid(), "", "", "", "")), null);

            result = converter.Convert(CneVM, null, null, null);
            Assert.AreEqual(trueBrush, result);
        }

        [Test]
        public void RootElementToBoolConverterTest()
        {
            var converter = new RootElementVMToBoolConverter();
            var RncVM = new RootNodeCategoryViewModel("");
            object result;

            //1. Element is null.
            //2. Element is RootNodeCategoryViewModel.

            // 1 case
            result = converter.Convert(null, null, null, null);
            Assert.AreEqual(false, result);

            // 2 case
            result = converter.Convert(RncVM, null, null, null);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void BrowserInternalElementToBoolConverterTest()
        {
            var converter = new NodeCategoryVMToBoolConverter();
            var NcVM = new NodeCategoryViewModel("");
            var RncVM = new RootNodeCategoryViewModel("");
            var CncVM = new ClassesNodeCategoryViewModel(RncVM);
            object result;

            //1. Element is null.            
            //2. Element is NodeCategoryViewModel.
            //2. Element is RootNodeCategoryViewModel.
            //2. Element is ClassesNodeCategoryViewModel.

            // 1 case
            result = converter.Convert(null, null, null, null);
            Assert.AreEqual(false, result);

            // 2 case
            result = converter.Convert(NcVM, null, null, null);
            Assert.AreEqual(true, result);

            // 3 case
            result = converter.Convert(RncVM, null, null, null);
            Assert.AreEqual(false, result);

            // 4 case
            result = converter.Convert(CncVM, null, null, null);
            Assert.AreEqual(false, result);
        }

        [Test]
        public void NullValueToCollapsedConverterTest()
        {
            NullValueToCollapsedConverter converter = new NullValueToCollapsedConverter();
            object result;

            //1. Value is null.
            //2. Value is not null.

            // 1 case
            result = converter.Convert(null, null, null, null);
            Assert.AreEqual(Visibility.Collapsed, result);

            // 2 case
            result = converter.Convert("not null", null, null, null);
            Assert.AreEqual(Visibility.Visible, result);
        }

        [Test]
        public void FullCategoryNameToMarginConverterTest()
        {
            FullCategoryNameToMarginConverter converter = new FullCategoryNameToMarginConverter();
            string name = "";
            Thickness thickness = new Thickness(5, 0, 0, 0);
            object result;

            //1. Name is null.
            //2. Name is empty.
            //3. Name is "Category".
            //4. Name is "Category.NestedClass1".
            //5. Name is "Category.NestedClass1.NestedClass2".

            // 1 case            
            Assert.Throws<ArgumentException>(() => { converter.Convert(null, null, null, null); });

            // 2 case            
            Assert.Throws<ArgumentException>(() => { converter.Convert(name, null, null, null); });

            // 3 case
            name = "Category";
            thickness = new Thickness(5, 0, 0, 0);
            result = converter.Convert(name, null, null, null);
            Assert.AreEqual(thickness, result);

            // 4 case
            name = "Category.NestedClass1";
            thickness = new Thickness(25, 0, 20, 0);
            result = converter.Convert(name, null, null, null);
            Assert.AreEqual(thickness, result);

            // 5 case
            name = "Category.NestedClass1.NestedClass2";
            thickness = new Thickness(45, 0, 20, 0);
            result = converter.Convert(name, null, null, null);
            Assert.AreEqual(thickness, result);
        }

        [Test]
        public void IntToVisibilityConverterTest()
        {
            IntToVisibilityConverter converter = new IntToVisibilityConverter();
            object result;

            //1. Number is null.
            //2. Number < 0.
            //3. Number == 0.
            //4. Number >0.

            // 1 case
            Assert.Throws<NullReferenceException>(delegate { converter.Convert(null, null, null, null); });

            // 2 case
            result = converter.Convert(-1, null, null, null);
            Assert.AreEqual(Visibility.Collapsed, result);

            // 3 case
            result = converter.Convert(0, null, null, null);
            Assert.AreEqual(Visibility.Collapsed, result);

            // 4 case
            result = converter.Convert(1, null, null, null);
            Assert.AreEqual(Visibility.Visible, result);
        }

        [Test]
        public void LibraryTreeItemsHostVisibilityConverterTest()
        {
            var converter = new LibraryTreeItemsHostVisibilityConverter();

            var result = converter.Convert(null, null, null, null);
            Assert.AreEqual(Visibility.Visible, result);

            var NcVM = new NodeCategoryViewModel("");
            result = converter.Convert(NcVM, null, null, null);
            Assert.AreEqual(Visibility.Visible, result);

            var RncVM = new ClassesNodeCategoryViewModel(NcVM);

            result = converter.Convert(RncVM, null, null, null);
            Assert.AreEqual(Visibility.Collapsed, result);
        }

        [Test]
        public void SearchHighlightMarginConverterTest()
        {
            SearchHighlightMarginConverter converter = new SearchHighlightMarginConverter();
            TextBlock textBlock = new TextBlock();
            textBlock.Width = 50;
            textBlock.Height = 10;

            # region dynamoViewModel and searchModel
            DynamoViewModel dynamoViewModel = DynamoViewModel.Start();
            NodeSearchModel searchModel = new NodeSearchModel();
            # endregion

            SearchViewModel searhViewModel = new SearchViewModel(dynamoViewModel, searchModel);
            object[] array = { textBlock, searhViewModel };
            Thickness thickness = new Thickness(0, 0, textBlock.ActualWidth, textBlock.ActualHeight);
            object result;

            //1. Array is null.
            //2. TextBlock.Text is empty.
            //3. TextBlock contains highlighted phrase.

            // 1 case
            Assert.Throws<NullReferenceException>(delegate { converter.Convert(null, null, null, null); });

            // 2 case
            textBlock.Text = "";
            array[0] = textBlock;
            result = converter.Convert(array, null, null, null);
            Assert.AreEqual(thickness, result);

            // 3 case
            // This case we can't check properly, because TextBlock.ActualWidth and TextBlock.ActualHeight equals 0. 
            textBlock.Text = "abcd";
            array[0] = textBlock;
            searhViewModel.SearchText = "a";
            thickness = new Thickness(0, 0, -6.6733333333333338, 0);
            result = converter.Convert(array, null, null, null);
            Assert.AreEqual(thickness, result);

            var shutdownParams = new DynamoViewModel.ShutdownParams(shutdownHost: false, allowCancellation: false);
            dynamoViewModel.PerformShutdownSequence(shutdownParams);
        }

        [Test]
        public void SelectedItemToActiveConverterTest()
        {
            var converter = new SelectedItemToActiveConverter();
            converter.NormalColor = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            converter.ActiveColor = new SolidColorBrush(Color.FromRgb(0, 0, 0));

            Assert.Throws<NullReferenceException>(() => { converter.Convert(null, null, null, null); });

            var array = new object[] { 5 };
            Assert.Throws<ArgumentException>(() => { converter.Convert(array, null, null, null); });

            array = new object[] { -1, 1 };
            var result = converter.Convert(array, null, null, null);
            Assert.AreEqual(converter.NormalColor, result);

            array = new object[] { "string", "string" };
            result = converter.Convert(array, null, null, null);
            Assert.AreEqual(converter.ActiveColor, result);
        }
    }
}
