using DynamoTests.Elements;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System;
using static DynamoTests.Utils.Enums;

namespace DynamoTests.FactoryElements
{
    public class PythonScriptEditorElementFactory : FactoryBase<PythonScriptEditorElement>
    {
        private AppiumWebElement textArea;
        private AppiumWebElement saveButton;
        private AppiumWebElement revertButton;

        public PythonScriptEditorElementFactory(WindowsDriver<WindowsElement> dynamoSession) : base(dynamoSession)
        {

        }

        public override PythonScriptEditorElement Build()
        {
            FindElement(ref elementToFind, "Python Script", FindElementBy.Name);
            var result = new PythonScriptEditorElement(elementToFind);

            FindElementIn(ref textArea, elementToFind, "editText", FindElementBy.AccessibilityId);
            result.TextArea = textArea;

            FindElementIn(ref saveButton, elementToFind, "SaveScriptChangesButton", FindElementBy.AccessibilityId);
            result.SaveButton = saveButton;

            FindElementIn(ref revertButton, elementToFind, "RevertScriptChangesButton", FindElementBy.AccessibilityId);
            result.RevertButton = revertButton;

            return result;
        }

    }
}
