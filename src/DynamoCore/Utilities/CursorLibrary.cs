using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Resources;

namespace Dynamo.Utilities
{
    public class CursorsLibrary
    {
        private static Cursor _handPan;
        private static Cursor _handPanActive;

        public static Cursor HandPan
        {
            get
            {
                if (_handPan == null)
                    _handPan = GetCursorFromFile("hand_pan.cur");
                return _handPan;
            }
        }

        public static Cursor HandPanActive
        {
            get
            {
                if (_handPanActive == null)
                    _handPanActive = GetCursorFromFile("hand_pan_active.cur");
                return _handPanActive;
            }
        }

        private static Cursor GetCursorFromFile(string cursorFile)
        {
            Uri uri = new Uri("/DynamoCore;component/UI/Images/" + cursorFile, UriKind.Relative);
            StreamResourceInfo cursorStream = Application.GetResourceStream(uri);
            return new Cursor(cursorStream.Stream);
        }
    }
}
