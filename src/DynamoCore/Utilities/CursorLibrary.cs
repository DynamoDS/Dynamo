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
        private static Cursor _arcAdding;
        private static Cursor _arcRemoving;
        private static Cursor _usualPointer;
        private static Cursor _libraryClick;
        private static Cursor _condense;
        private static Cursor _expand;
        private static Cursor _rectangularSelection;

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

        public static Cursor ArcAdding
        {
            get
            {
                if (_arcAdding == null)
                    _arcAdding = GetCursorFromFile("arc_add.cur");
                return _arcAdding;
            }
        }

        public static Cursor ArcRemoving
        {
            get
            {
                if (_arcRemoving == null)
                    _arcRemoving = GetCursorFromFile("arc_remove.cur");
                return _arcRemoving;
            }
        }

        public static Cursor UsualPointer
        {
            get
            {
                if (_usualPointer == null)
                    _usualPointer = GetCursorFromFile("pointer.cur");
                return _usualPointer;
            }
        }

        public static Cursor LibraryClick
        {
            get
            {
                if (_libraryClick == null)
                    _libraryClick = GetCursorFromFile("hand.cur");
                return _libraryClick;
            }
        }

        public static Cursor Expand
        {
            get
            {
                if (_expand == null)
                    _expand = GetCursorFromFile("expand.cur");
                return _expand;
            }
        }

        public static Cursor Condense
        {
            get
            {
                if (_condense == null)
                    _condense = GetCursorFromFile("condense.cur");
                return _condense;
            }
        }

        public static Cursor RectangularSelection
        {
            get
            {
                if (_rectangularSelection == null)
                    _rectangularSelection = GetCursorFromFile("rectangular_selection.cur");
                return _rectangularSelection;
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
