using DynamoTests.Elements;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System.Collections.Generic;
using System.Linq;
using static DynamoTests.Utils.Enums;

namespace DynamoTests.FactoryElements
{
    public class WorkspaceViewElementFactory : FactoryBase<WorkspaceViewElement>
    {
        private List<AppiumWebElement> elementList;

        public WorkspaceViewElementFactory(WindowsDriver<WindowsElement> dynamoSession)
            : base(dynamoSession)
        { }

        public override WorkspaceViewElement Build()
        {
            FindElement(ref elementToFind, "WorkspaceView", FindElementBy.ClassName);
            var result = new WorkspaceViewElement(elementToFind);


            FindElementsIn(ref elementList, result.WorkspaceView, "RepeatButton", FindElementBy.ClassName);
            elementList = elementList.Where(button => button.Location.X > 0 && button.Location.Y > 0).ToList();

            if (elementList.Count >= 2)
            {
                result.ZoomInButton = elementList[0];
                result.ZoomOutButton = elementList[1];
            }

            FindElementsIn(ref elementList, result.WorkspaceView, "CheckBox", FindElementBy.ClassName);
            elementList = elementList.Where(check => check.Size.Width >= 55 && check.Size.Width <= 56).ToList();

            if (elementList.Count >= 2)
            {
                result.BackgroudViewCheck = elementList[0];
                result.GraphViewCheck = elementList[1];
            }

            return result;
        }
    }
}
