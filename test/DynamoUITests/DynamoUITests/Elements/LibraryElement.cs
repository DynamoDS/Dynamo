using DynamoTests.DTO;
using OpenQA.Selenium.Appium.Windows;

namespace DynamoTests.Elements
{
    public class LibraryElement : ElementBase
    {
        public LibraryElement(WindowsElement uiElement)
            : base (uiElement)
        {
            TextLocation = new LocationDTO { X = 100, Y = 70 };
            FirstElementLocation = new LocationDTO { X = 30, Y = 180 };
        }

        public WindowsElement Library { get { return UiElemnt; } }

        public readonly LocationDTO TextLocation;
        public readonly LocationDTO FirstElementLocation;

        /// <summary>
        /// Get the location (X,Y) of the close button from the Library (search textbox)
        /// </summary>
        public LocationDTO SearchTextBoxInitialLocation { get {
                return new LocationDTO
                {
                    //They are harcoded because the Library is using HTML/JS (WPF MSWebBrowser) then the WinAppDriverRecorder or WindowsInspect doesn't work over the component
                    X = 45,
                    Y = 70
                };
            }
        }

    }
}
