using DynamoTests.DTO;
using OpenQA.Selenium;
using System;
using System.Drawing;

namespace DynamoTests.Utils
{
    public static class AppiumExtension
    {
        public static LocationDTO Center(this IWebElement windowsElement)
        {
            return new LocationDTO
            {
                X = windowsElement.Size.Width > 0 ? windowsElement.Size.Width / 2 : 0,
                Y = windowsElement.Size.Height > 0 ? windowsElement.Size.Height / 2 : 0
            };
        }

        public static Point NewPoint(this IWebElement windowsElement)
        {
            return new Point
            {
                X = windowsElement.Location.X,
                Y = windowsElement.Location.Y
            };
        }

        public static string EnumToString(this Enums.NodeType nodeType)
        {
            return Enum.GetName(typeof(Enums.NodeType), (int)nodeType);
        }

    }
}
