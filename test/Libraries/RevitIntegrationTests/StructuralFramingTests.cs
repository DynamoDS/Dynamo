using System;
using System.Linq;

using Autodesk.Revit.DB;

using DSRevitNodesUI;

using Dynamo.Nodes;

using NUnit.Framework;

using RevitServices.Persistence;

using RTF.Framework;

using FamilySymbol = Revit.Elements.FamilySymbol;

namespace RevitSystemTests
{
    [TestFixture]
    public class StructuralFramingTests : SystemTest
    {
        [Test, TestModel(@".\StructuralFraming\StructuralFraming.rvt")]
        public void StructuralFraming_Beam()
        {
            OpenAndRun(@".\StructuralFraming\Beam.dyn");

            CompareStructuralTypeAgainstElements();
            CompareSliderCountAndMemberCount(BuiltInCategory.OST_StructuralFraming,10);
            CompareSliderCountAndMemberCount(BuiltInCategory.OST_StructuralFraming, 5);
        }

        [Test, TestModel(@".\StructuralFraming\StructuralFraming.rvt")]
        public void StructuralFraming_Brace()
        {
            OpenAndRun(@".\StructuralFraming\Brace.dyn");

            CompareStructuralTypeAgainstElements();
            CompareSliderCountAndMemberCount(BuiltInCategory.OST_StructuralFraming, 10);
            CompareSliderCountAndMemberCount(BuiltInCategory.OST_StructuralFraming, 5);
        }

        [Test, TestModel(@".\StructuralFraming\StructuralFraming.rvt")]
        public void StructuralFraming_Column()
        {
            OpenAndRun(@".\StructuralFraming\Column.dyn");

            CompareStructuralTypeAgainstElements();
            CompareSliderCountAndMemberCount(BuiltInCategory.OST_StructuralColumns, 10);
            CompareSliderCountAndMemberCount(BuiltInCategory.OST_StructuralColumns, 5);
        }

        private void CompareStructuralTypeAgainstElements()
        {
            AssertTypeAndCountWhenSelectingFromDropDown(0);
            AssertTypeAndCountWhenSelectingFromDropDown(1);
        }

        private void AssertTypeAndCountWhenSelectingFromDropDown(int selectedIndex)
        {
            var slider = ViewModel.Model.AllNodes.FirstOrDefault(x => x is IntegerSlider) as IntegerSlider;

            var typeSelector = ViewModel.Model.AllNodes.FirstOrDefault(x => x is AllElementsInBuiltInCategory) as RevitDropDownBase;
            typeSelector.SelectedIndex = selectedIndex;

            RunCurrentModel();
            
            var dynamoSymbol = typeSelector.GetValue(0).Data as FamilySymbol;
            var revitSymbol = dynamoSymbol.InternalElement;

            Console.WriteLine("Family type is now set to {0}", revitSymbol);

            var symbolFilter = new FamilyInstanceFilter(
                DocumentManager.Instance.CurrentDBDocument,
                revitSymbol.Id);
            var fec = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);
            fec.WherePasses(symbolFilter);

            Assert.AreEqual(fec.ToElements().Count, slider.Value);
        }

        private void CompareSliderCountAndMemberCount(BuiltInCategory cat, int sliderCount)
        {
            var slider = ViewModel.Model.AllNodes.FirstOrDefault(x => x is IntegerSlider) as IntegerSlider;
            slider.Value = sliderCount;

            RunCurrentModel();

            var fec = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);
            fec.OfClass(typeof(FamilyInstance)).OfCategory(cat);

            Assert.AreEqual(slider.Value, fec.ToElements().Count);
        }
    }
}
