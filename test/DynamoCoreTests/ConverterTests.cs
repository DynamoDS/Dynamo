using System;
using System.Collections.Generic;
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
        public void DisplayModeToBackgroundConverterTest()
        {
            var converter = new DisplayModeToBackgroundConverter();
            var isSecondaryHeaderRightVisible = false;
            var displayMode = ClassInformationViewModel.DisplayMode.None;
            var parameter = "";
            object[] array = { displayMode, isSecondaryHeaderRightVisible };
            object result;

            //1. Array is null.
            //2. Parameter is null.
            //3. Right secondary header is invisible. Display mode is "None". Parameter is empty.
            //4. Right secondary header is invisible. Display mode is "Query". Parameter is empty.
            //5. Right secondary header is invisible. Display mode is "Action". Parameter is empty.
            //6. Right secondary header is visible. Display mode is "Action". Parameter is "Action".
            //7. Right secondary header is visible. Display mode is "Action". Parameter is "None".

            // 1 case
            Assert.Throws<NullReferenceException>(delegate { converter.Convert(null, null, null, null); });

            // 2 case
            result = converter.Convert(array, null, null, null);
            Assert.AreEqual(converter.ActiveColor, result);

            // 3 case
            result = converter.Convert(array, null, parameter, null);
            Assert.AreEqual(converter.ActiveColor, result);

            // 4 case
            array[0] = ClassInformationViewModel.DisplayMode.Query;
            result = converter.Convert(array, null, parameter, null);
            Assert.AreEqual(converter.ActiveColor, result);

            // 5 case
            array[0] = ClassInformationViewModel.DisplayMode.Action;
            result = converter.Convert(array, null, parameter, null);
            Assert.AreEqual(converter.ActiveColor, result);

            // 6 case
            parameter = "Action";
            isSecondaryHeaderRightVisible = true;
            array[1] = isSecondaryHeaderRightVisible;
            result = converter.Convert(array, null, parameter, null);
            Assert.AreEqual(converter.ActiveColor, result);

            // 7 case
            parameter = "None";
            result = converter.Convert(array, null, parameter, null);
            Assert.AreEqual(converter.NormalColor, result);
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
            // TODO(Vladimir): take a look.
#if false
            ElementTypeToBoolConverter converter = new ElementTypeToBoolConverter();
            var NseVM = new NodeSearchElementViewModel(new NodeSearchElement("name", "description", new List<string>() { "tag" }, SearchElementGroup.Action));
            var BieVM = new BrowserInternalElementViewModel(new BrowserInternalElement());
            var BiefcVM = new BrowserInternalElementForClassesViewModel(new BrowserInternalElementForClasses("name", BieVM.Model));
            var BreVM = new BrowserRootElementViewModel(new BrowserRootElement("name"));
            object result;

            //1. Element is null.
            //2. Element is NodeSearchElement.
            //3. Element is BrowserInternalElement.
            //4. Element is BrowserInternalElementForClasses.
            //5. Element is BrowserRootElement.

            // 1 case
            result = converter.Convert(null, null, null, null);
            Assert.AreEqual(false, result);

            // 2 case
            result = converter.Convert(NseVM, null, null, null);
            Assert.AreEqual(false, result);

            // 3 case
            result = converter.Convert(BieVM, null, null, null);
            Assert.AreEqual(true, result);

            // 4 case
            result = converter.Convert(BiefcVM, null, null, null);
            Assert.AreEqual(true, result);

            // 5 case
            result = converter.Convert(BreVM, null, null, null);
            Assert.AreEqual(true, result);
#endif
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

            // TODO(Vladimir): take a look.
            // 2 case
            //var CneVM = new CustomNodeSearchElementViewModel(new CustomNodeSearchElement(new CustomNodeInfo(new Guid(), "name", "cat", "desc", "path"), SearchElementGroup.Action));
            //result = converter.Convert(CneVM, null, null, null);
            //Assert.AreEqual(trueBrush, result);
        }

        [Test]
        public void RootElementToBoolConverterTest()
        {
            var converter = new RootElementVMToBoolConverter();
            var BreVM = new BrowserRootElementViewModel(new BrowserRootElement("BRE"));
            object result;

            //1. Element is null.
            //2. Element is BrowserRootElement.

            // 1 case
            result = converter.Convert(null, null, null, null);
            Assert.AreEqual(false, result);

            // 2 case
            result = converter.Convert(BreVM, null, null, null);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void BrowserInternalElementToBoolConverterTest()
        {
            var converter = new NodeCategoryVMToBoolConverter();
            var elementVM = new BrowserInternalElementViewModel(new BrowserInternalElement());
            object result;

            //1. Element is null.            
            //2. Element is BrowserInternalElement.

            // 1 case
            result = converter.Convert(null, null, null, null);
            Assert.AreEqual(false, result);

            // 2 case
            result = converter.Convert(elementVM, null, null, null);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void HasParentRootElementTest()
        {
            var converter = new HasParentRootElement();
            var BreVM = new BrowserRootElementViewModel(new BrowserRootElement("BRE"));
            var BieVM = new BrowserInternalElementViewModel(new BrowserInternalElement());
            object result;

            //1. Element is null.
            //2. Element is BrowserRootElement.
            //3. Element is not child of BrowserRootElement.
            //4. Element is child of BrowserRootElement.

            // 1 case
            result = converter.Convert(null, null, null, null);
            Assert.AreEqual(false, result);

            // 2 case
            result = converter.Convert(BreVM, null, null, null);
            Assert.AreEqual(true, result);

            // 3 case
            result = converter.Convert(BieVM, null, null, null);
            Assert.AreEqual(false, result);

            // 4 case
            // TODO(Vladimir): take a look.
            //BreVM.CastedModel.AddChild(BieVM.CastedModel);
            result = converter.Convert(BieVM, null, null, null);
            Assert.AreEqual(true, result);
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
            result = converter.Convert(null, null, null, null);
            Assert.AreEqual(thickness, result);

            // 2 case
            result = converter.Convert(name, null, null, null);
            Assert.AreEqual(thickness, result);

            // 3 case
            name = "Category";
            thickness = new Thickness(5, 0, 20, 0);
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
            // TODO(Vladimir): take a look.
#if false
            var converter = new LibraryTreeItemsHostVisibilityConverter();

            var result = converter.Convert(null, null, null, null);
            Assert.AreEqual(Visibility.Visible, result);

            var BieVM = new BrowserInternalElementViewModel(new BrowserInternalElement());
            result = converter.Convert(BieVM, null, null, null);
            Assert.AreEqual(Visibility.Visible, result);

            var rootElement = new BrowserRootElement("Top Category");
            var BIEFC = new BrowserInternalElementForClasses("Classes", rootElement);
            rootElement.Items.Add(BIEFC);

            var BreVM = new BrowserRootElementViewModel(rootElement);

            result = converter.Convert(BreVM.Items[0], null, null, null);
            Assert.AreEqual(Visibility.Collapsed, result);
#endif
        }

        [Test]
        public void SearchHighlightMarginConverterTest()
        {
            SearchHighlightMarginConverter converter = new SearchHighlightMarginConverter();
            TextBlock textBlock = new TextBlock();
            textBlock.Width = 50;
            textBlock.Height = 10;

            # region dynamoViewModel and searchModel
            var model = DynamoModel.Start();
            var vizManager = new VisualizationManager(model);
            var watchHandler = new DefaultWatchHandler(vizManager, model.PreferenceSettings);
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
        }

        [Test]
        public void LeftSecondaryHeaderStyleConverterTest()
        {
            var converter = new LeftSecondaryHeaderStyleConverter();

            var styleA = new Style();
            styleA.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(1)));

            var styleB = new Style();
            styleB.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(255, 0, 0))));

            converter.PrimaryHeaderStyle = styleA;
            converter.SecondaryHeaderStyle = styleB;

            Assert.AreEqual(styleB, converter.Convert(true, null, null, null));
            Assert.AreEqual(styleA, converter.Convert(false, null, null, null));
        }
    }
}
