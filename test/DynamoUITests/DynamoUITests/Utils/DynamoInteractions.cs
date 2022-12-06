using DynamoTests.DTO;
using DynamoTests.Elements;
using DynamoTests.FactoryElements;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using System;
using System.Linq;
using System.Threading;

namespace DynamoTests.Utils
{
    public class DynamoInteractions
    {
        protected WindowsDriver<WindowsElement> session;
        protected Actions dynamoActions;
        protected WorkspaceViewElement workspaceViewElement;
        protected LibraryElement libraryElement;

        public DynamoInteractions(WindowsDriver<WindowsElement> _session)
        {
            this.session = _session;
            ResetDynamoActions();
        }

        public void SetLibrary(LibraryElement _libraryElement)
        {
            libraryElement = _libraryElement;
        }

        public void SetWorkspaceView(WorkspaceViewElement _workspaceViewElement)
        {
            workspaceViewElement = _workspaceViewElement;
        }

        #region .: Library :.

        /// <summary>
        /// This method will be cleaning the SearchTextBox in the Library by double clicking (seleting the current text) and then press delete, then it will insert the new text
        /// </summary>
        /// <param name="text"></param>
        public void CleanLibrary(string text)
        {
            if (libraryElement != null)
            {                
                MoveTo(libraryElement.Library, libraryElement.SearchTextBoxInitialLocation);
                dynamoActions.Click();

                dynamoActions.SendKeys(Keys.Shift + Keys.End);
                dynamoActions.SendKeys(Keys.Delete);                
                dynamoActions.SendKeys(Keys.Shift);// release the shift in order to avoid impacts the next actions
                ExecuteActions();
                ScrollUpLibrary();
            }
            WriteTextOnLibrary(text);
        }

        public void ScrollUpLibrary()
        {
            LocationDTO scrollBarLocation = new LocationDTO
            {
                X = libraryElement.Library.Size.Width - 10,
                Y = 200
            };
            if (libraryElement != null)
            {
                MoveTo(libraryElement.Library, scrollBarLocation);
                dynamoActions.ClickAndHold();
                dynamoActions.MoveByOffset(0, -100);
                dynamoActions.Release();
                ExecuteActions();
            }

            libraryElement.Library.Click();
            dynamoActions.SendKeys(Keys.Home);
        }

        public void WriteTextOnLibrary(string text)
        {
            if (libraryElement != null)
            {
                libraryElement.Library.Click();
                SendKeys(Keys.Home);
                MoveTo(libraryElement.Library, libraryElement.TextLocation);
                dynamoActions.Click();
                dynamoActions.SendKeys(text);
                ExecuteActions();
                DynamoSleep(TimeSpan.FromSeconds(1));
            }
        }

        public void MoveToFirstElement()
        {
            if (libraryElement != null)
            {
                MoveTo(libraryElement.Library, libraryElement.FirstElementLocation);
                dynamoActions.Click();
                ExecuteActions();
            }
        }

        #endregion .: Library :.

        #region .: Make Nodes :.

        #region .: Test 05 :.
        public void MakeSaveAsElement(ref SaveAsElement _saveAsElement)
        {
            using (var factory = new SaveAsElementFactory(session))
            {
                _saveAsElement = factory.Build();
            }
        }
        #endregion .: Test 05 :.

        #region .: Test 08 :.
        #endregion .: Test 08 :.

        #region .: Test 09 :.
        public void MakeCodeBlock(ref CodeBlockNode codeBlockNode, string textForLibrary, string textForCodeBlock)
        {
            CleanLibrary(textForLibrary);
            MoveToFirstElement();
            
            using (var factory = new CodeBlockNodeFactory(textForCodeBlock, session))
            {
                codeBlockNode = factory.Build();
            }
        }
        public void MakeString(ref StringNode stringNode)
        {
            CleanLibrary("String");
            MoveToFirstElement();

            using (var factory = new StringNodeFactory(session))
            {
                stringNode = factory.Build();
            }
        }
        public void MakeSlider(ref SliderNode sliderNode, string text, int minValue, int maxValue, int step)
        {
            CleanLibrary(text);
            MoveToFirstElement();

            using (var factory = new SliderNodeFactory(minValue, maxValue, step, session))
            {
                sliderNode = factory.Build();
            }
        }
        public void MakeRange(ref RangeNode rangeNode)
        {
            CleanLibrary("Range");
            MoveToFirstElement();

            using (var factory = new RangeNodeFactory(session))
            {
                rangeNode = factory.Build();
            }
        }
        public void MakeListCreate(ref ListCreateNode listCreateNodee)
        {
            CleanLibrary("List Create");
            MoveToFirstElement();

            using (var factory = new ListCreateFactory(session))
            {
                listCreateNodee = factory.Build();
            }
        }
        public void MakeFileFromPath(ref FileFromPathNode fileFromPathNode)
        {
            CleanLibrary("File From Path");
            MoveToFirstElement();

            using (var factory = new FileFromPathNodeFactory(session))
            {
                fileFromPathNode = factory.Build();
            }
        }
        public void MakeImageReadFromFile(ref ImageReadFromFileNode imageReadFromFileNode)
        {
            CleanLibrary("ReadFromFile");
            MoveToFirstElement();

            using (var factory = new ImageReadFromFileNodeFactory(session))
            {
                imageReadFromFileNode = factory.Build();
            }
        }
        public void MakeWatchImage(ref WatchImageNode watchImageNode)
        {
            CleanLibrary("Watch Image");
            MoveToFirstElement();

            using (var factory = new WatchImageNodeFactory(session))
            {
                watchImageNode = factory.Build();
            }
        }
        public void MakeBoolean(ref BooleanNode booleanNode)
        {
            CleanLibrary("Boolean");
            MoveToFirstElement();

            using (var factory = new BooleanNodeFactory(session))
            {
                booleanNode = factory.Build();
            }
        }


        #endregion .: Test 09 :.

        #region .: Test 11 :.

        public void MakeNumber(ref NumberNode _numberNode)
        {
            CleanLibrary("Number");
            MoveToFirstElement();

            using (var factory = new NumberNodeFactory(session))
            {
                _numberNode = factory.Build();
            }
        }

        public void MakeNumber(ref NumberNode _numberNode, IWebElement elementFound)
        {
            using (var factory = new NumberNodeFactory(session, elementFound))
            {
                _numberNode = factory.Build();
            }
        }

        public void MakeAddOperatorNode(ref AddOperatorNode _addOperatorNode)
        {
            CleanLibrary("Add");
            MoveToFirstElement();

            using (var factory = new AddOperatorNodeFactory(session))
            {
                _addOperatorNode = factory.Build();
            }
        }

        public void MakeAddOperatorNode(ref AddOperatorNode _addOperatorNode, IWebElement elementFound)
        {
            using (var factory = new AddOperatorNodeFactory(session, elementFound))
            {
                _addOperatorNode = factory.Build();
            }
        }

        public void MakeWatchNode(ref WatchNode _watchNode)
        {
            CleanLibrary("Watch");
            MoveToFirstElement();

            using (var factory = new WatchNodeFactory(session))
            {
                _watchNode = factory.Build();
            }
        }

        public void MakeWatchNode(ref WatchNode _watchNode, IWebElement elementFound)
        {
            using (var factory = new WatchNodeFactory(session, elementFound))
            {
                _watchNode = factory.Build();
            }
        }

        public void MakeOutputNode(ref OutputNode _outputNode)
        {
            CleanLibrary("Output");
            MoveToFirstElement();

            using (var factory = new OutputNodeFactory(session))
            {
                _outputNode = factory.Build();
            }
        }

        public void MakeOutputNode(ref OutputNode _outputNode, IWebElement elementFound)
        {
            using (var factory = new OutputNodeFactory(session, elementFound))
            {
                _outputNode = factory.Build();
            }
        }

        public void MakeCustomNodeProperties(ref CustomNodePropertiesElement _customNodePropertiesElement)
        {
            using (var factory = new CustomNodePropertiesFactory(session))
            {
                _customNodePropertiesElement = factory.Build();
            }
        }

        public void GetLastCustomNode(ref CustomNode _customNode)
        {
            using (var factory = new CustomNodeFactory(session))
            {
                _customNode = factory.Build();
            }
        }

        public void MakeCircleByCenterPointRadius(ref CircleByCenterPointRadiusNode _circleNode)
        {
            CleanLibrary("ByCenterPointRadius");
            MoveToFirstElement();

            using (var factory = new CircleByCenterPointRadiusNodeFactory(session))
            {
                _circleNode = factory.Build();
            }
        }

        public void MakeCircleByCenterPointRadiusNormal(ref CircleByCenterPointRadiusNormalNode _circleNode)
        {
            CleanLibrary("ByCenterPointRadiusNormal");
            MoveToFirstElement();

            using (var factory = new CircleByCenterPointRadiusNormalNodeFactory(session))
            {
                _circleNode = factory.Build();
            }
        }

        public void MakeSphereByCenterPointRadius(ref SphereByCenterPointRadiusNode _sphereNode)
        {
            CleanLibrary("Sphere.ByCenterPointRadius");
            MoveToFirstElement();

            using (var factory = new SphereByCenterPointRadiusNodeFactory(session))
            {
                _sphereNode = factory.Build();
            }
        }

        public void MakeCustomNode(ref CustomNode _customNode, string customNodeName)
        {
            CleanLibrary(customNodeName);
            MoveToFirstElement();

            using (var factory = new CustomNodeFactory(session))
            {
                _customNode = factory.Build();
            }
        }

        public void MakeCustomNode(ref CustomNode _customNode, IWebElement elementFound)
        {
            using (var factory = new CustomNodeFactory(session, elementFound))
            {
                _customNode = factory.Build();
            }
        }

        public void GetLastGroup(ref GroupElement _groupElement)
        {
            using (var factory = new GroupElementFactory(session))
            {
                _groupElement = factory.Build();
            }
        }

        public void GetLastNote(ref NoteElement noteElement)
        {
            using (var factory = new NoteElementFactory(session))
            {
                noteElement = factory.Build();
            }
        }

        #endregion .: Test 11 :.

        #region .: Test 12 :.
        public void MakePackageSearch(ref PackageSearchElement _packageSearchElement)
        {
            using (var factory = new PackageSearchElementFactory(session))
            {
                _packageSearchElement = factory.Build();
            }
        }

        public void MakePackageManager(ref PackageManagerElement _packageManagerElement)
        {
            using (var factory = new PackageManagerElementFactory(session))
            {
                _packageManagerElement = factory.Build();
            }
        }

        public void MakePointByCoordinates(ref PointByCoordinates _pointByCoordinatesElement, 
                                               IWebElement _elementFound = null)
        {
            using (var factory = new PointByCoordinatesNodeFactory(session, _elementFound))
            {
                _pointByCoordinatesElement = factory.Build();
            }
        }

        #endregion .: Test 12 :.

        #region .: Test 13 :.
        #endregion .: Test 13 :.

        #region .: Test 14 :.
        #endregion .: Test 14 :.

        #region .: Test 15 :.

        public void MakeImportLibrary(ref ImportLibraryElement _importLibraryElement)
        {
            using (var factory = new ImportLibraryElementFactory(session))
            {
                _importLibraryElement = factory.Build();
            }
        }

        #endregion .: Test 15 :.

        #region .: Test 16 :.
        public void MakePythonScriptNode(ref PythonScriptNode _pythonNode, IWebElement elementFound = null)
        {
            if (elementFound == null)
            {
                CleanLibrary("Python");
                MoveToFirstElement();
            }

            using (var factory = new PythonScriptNodeFactory(session, elementFound))
            {
                _pythonNode = factory.Build();
            }
        }

        public void MakePythonScriptEditor(ref PythonScriptEditorElement _pythonEditor)
        {
            using (var factory = new PythonScriptEditorElementFactory(session))
            {
                _pythonEditor = factory.Build();
            }
        }

        public void EditPythonScriptNode(PythonScriptNode pythonNode, string script)
        {
            MoveTo(pythonNode.Node,
                new LocationDTO()
                {
                    X = pythonNode.Node.Center().X,
                    Y = pythonNode.Node.Center().Y + 50
                });

            DoubleClick();

            PythonScriptEditorElement editor = null;
            MakePythonScriptEditor(ref editor);

            MoveTo(editor.TextArea, editor.TextArea.Center());
            Click();

            Tools.SetClipboard(script);

            SendKeys(Keys.Control + "a" + Keys.Control);
            SendKeys(Keys.Control + "v" + Keys.Control);

            MoveTo(editor.SaveButton, editor.SaveButton.Center());
            Click();
            CloseFocusedWindow();
        }
        #endregion .: Test 16 :.

        #region . : Custom Node Tests : .
        public void MakeShortcutToolbar(ref ShortcutToolbarElement _shortcutToolbar)
        {
            using (var factory = new ShortcutToolbarElementFactory(session))
            {
                _shortcutToolbar = factory.Build();
            }
        }

        public void MakeInputNode(ref InputNode _inputNode, IWebElement elementFound = null)
        {
            if (elementFound == null)
            {
                CleanLibrary("Input");
                MoveToFirstElement();
            }

            using (var factory = new InputNodeFactory(session, elementFound))
            {
                _inputNode = factory.Build();
            }
        }

        public void MakeMultiplication(ref MultiplyOperatorNode _multiplyOperatorNode, IWebElement elementFound = null)
        {
            if (elementFound == null)
            {
                CleanLibrary("Multiply");
                MoveToFirstElement();
            }

            using (var factory = new MultiplyOperatorNodeFactory(session, elementFound))
            {
                _multiplyOperatorNode = factory.Build();
            }
        }
        #endregion

        #endregion .: Make Nodes :.

        #region .: Utils :.

        public void ClickMenus(MenuDTO menu)
        {
            if (!string.IsNullOrEmpty(menu.AccessibilityId))
            {
                session.FindElementByAccessibilityId(menu.AccessibilityId).Click();
            }
            else
            {
                session.FindElementByName(menu.Name).Click();
            }

            foreach (var menuItem in menu.Menus)
            {
                IWebElement menuElement = null;

                if (!string.IsNullOrEmpty(menuItem.AccessibilityId))
                {
                    menuElement = session.FindElementByAccessibilityId(menuItem.AccessibilityId);
                }
                else
                {
                    menuElement = session.FindElementByName(menuItem.Name);
                }

                MoveTo(menuElement, new LocationDTO { X = 10, Y = 10 });

                if (menuItem.SubMenus != null && menuItem.SubMenus.Count > 0)
                {
                    foreach (var subMenuItem in menuItem.SubMenus)
                    {
                        IWebElement subMenuElement = null;

                        if (!string.IsNullOrEmpty(subMenuItem.AccessibilityId))
                        {
                            subMenuElement = session.FindElementByAccessibilityId(subMenuItem.AccessibilityId);
                        }
                        else
                        {
                            subMenuElement = session.FindElementByName(subMenuItem.Name);
                        }

                        MoveTo(subMenuElement, new LocationDTO { X = 10, Y = 10 });
                        Click();
                    }
                }
                else
                {
                    Click();
                }
            }
            
        }

        public void ClickSubMenu(MenuDTO menu, string name)
        {
            if (!string.IsNullOrEmpty(menu.AccessibilityId))
            {
                session.FindElementByAccessibilityId(menu.AccessibilityId).Click();
            }
            else
            {
                session.FindElementByName(menu.Name).Click();
            }

            var preferencesMenuItem = (from menuItem
                    in menu.Menus
                where menuItem.Text.Contains(name)
                select menuItem).First();
            session.FindElementByAccessibilityId(preferencesMenuItem.AccessibilityId).Click();
        }

        /// <summary>
        /// This method will allow to click a submenu item
        /// </summary>
        /// <param name="menu">Menu name e.g. Dynamo, Edit, View, Help</param>
        /// <param name="menuItemName">MenuItem name inside the Menu e.g. Dynamo->Preferences</param>
        /// <param name="subMenuItemName">SubMenuItem name inside the MenuItem e.g. Help->Interactive Guides->Get Started</param>
        public void ClickMenuSubMenu(MenuDTO menu, string menuItemName, string subMenuItemName)
        {
            if (!string.IsNullOrEmpty(menu.AccessibilityId))
            {
                session.FindElementByAccessibilityId(menu.AccessibilityId).Click();
            }
            else
            {
                session.FindElementByName(menu.Name).Click();
            }

            var menuItem = (from item
                              in menu.Menus
                           where item.Text.Contains(menuItemName)
                          select item).First();

            var subMenuItem = session.FindElementByAccessibilityId(menuItem.AccessibilityId);
            subMenuItem.Click();

            subMenuItem.FindElementByName(subMenuItemName).Click();
        }

        public void SelectElementsInWorkspace(LocationDTO from, LocationDTO to)
        {
            if (workspaceViewElement != null)
            {
                MoveTo(workspaceViewElement.WorkspaceView, from);
                dynamoActions.ClickAndHold();
                ExecuteActions();
                MoveTo(workspaceViewElement.WorkspaceView, to);
                dynamoActions.Release();
                ExecuteActions();
            }
        }

        public void LinkElements(IWebElement from, IWebElement to)
        {
            MoveTo(from, from.Center());
            dynamoActions.Click();
            ExecuteActions();

            MoveTo(to, to.Center());
            dynamoActions.Click();
            ExecuteActions();
        }

        public void MoveTo(IWebElement windowsElement, LocationDTO location = null)
        {
            if (location == null)
            {
                location = new LocationDTO { X = 0, Y = 0 };
            }

            dynamoActions.MoveToElement(windowsElement, location.X, location.Y);
            ExecuteActions();
        }

        public void Click()
        {
            dynamoActions.Click();
            ExecuteActions();
        }

        public void DoubleClick()
        {
            dynamoActions.DoubleClick();
            ExecuteActions();
        }

        public void ClickAndHold()
        {
            dynamoActions.ClickAndHold();
            ExecuteActions();
        }

        public void Release()
        {
            dynamoActions.Release();
            ExecuteActions();
        }

        public void ContextClick()
        {
            dynamoActions.ContextClick();
            ExecuteActions();
        }

        public void SendKeys(string keysToSend)
        {
            dynamoActions.SendKeys(keysToSend);
            ExecuteActions();
        }

        /// <summary>
        /// Deletes a node from the Workspace.
        /// </summary>
        /// <param name="node">Node to be deleted</param>
        public void DeleteNode(NodeBase node)
        {
            dynamoActions.MoveToElement(node.NameBlock);
            dynamoActions.Click();

            dynamoActions.SendKeys(Keys.Delete);

            ExecuteActions();
        }

        public void DeleteAllNodes()
        {
            if (workspaceViewElement != null)
            {
                dynamoActions.MoveToElement(workspaceViewElement.WorkspaceView);
                Click();

                //Select all Nodes
                dynamoActions.KeyDown(Keys.Control);
                dynamoActions.SendKeys("a");
                dynamoActions.KeyUp(Keys.Control);
                ExecuteActions();

                //Delete all nodes
                dynamoActions.SendKeys(Keys.Delete);
                ExecuteActions();

                DynamoSleep(TimeSpan.FromSeconds(10));
            }
        }

        /// <summary>
        /// Changes the value of a node's control that accepts text or numbers.
        /// </summary>
        /// <param name="textControl">Control to be modified</param>
        /// <param name="newValue">Desired input to be entered</param>
        public void ChangeTextValue(IWebElement textControl, string newValue)
        {
            DynamoSleep(TimeSpan.FromSeconds(1));
            dynamoActions.MoveToElement(textControl);
            DynamoSleep(TimeSpan.FromSeconds(1));
            dynamoActions.Click();

            dynamoActions.SendKeys(newValue.ToString());
            dynamoActions.SendKeys(Keys.Enter);

            ExecuteActions();
        }

        public void MoveNodeInWorkspace(NodeBase node, LocationDTO location)
        {
            if (workspaceViewElement != null)
            {
                MoveTo(node.NameBlock, node.NameBlock.Center());
                ClickAndHold();
                MoveTo(workspaceViewElement.WorkspaceView, location);
                Release();
            }   
        }

        public void MoveElementInWorkspace(IWebElement element, LocationDTO location)
        {
            if (workspaceViewElement != null)
            {
                MoveTo(element, element.Center());
                ClickAndHold();
                MoveTo(workspaceViewElement.WorkspaceView, location);
                Release();
            }
        }

        /// <summary>
        /// Verifies that the Preview control of a node contains the given text.
        /// (Could be changed to a method in the NodeBase class)
        /// </summary>
        /// <param name="node">Node to validate</param>
        /// <param name="expectedValue">Text the control should contain</param>
        /// <returns></returns>
        public bool ValidatePreviewControlText(NodeBase node, string expectedValue)
        {
            MoveTo(node.Node, node.Node.Center());
            DynamoSleep(TimeSpan.FromSeconds(1.5));

            IWebElement previewControl = node.Node.FindElementByName(expectedValue);

            return previewControl.Text.Equals(expectedValue);
        }

        public int NodesCount()
        {
            return session.FindElementsByAccessibilityId("topControl").Count;
        }

        public void CloseFocusedWindow()
        {
            SendKeys(Keys.Alt + Keys.F4 + Keys.Alt);
        }

        private void ExecuteActions()
        {
            dynamoActions.Build().Perform();
            ResetDynamoActions();
        }

        private void ResetDynamoActions()
        {
            dynamoActions = new Actions(session);
        }

        private void DynamoSleep(TimeSpan timeSpan)
        {
            Thread.Sleep(timeSpan);
        }

        public static IWebElement FindElement(ElementBaseDTO elementDTO, WindowsDriver<WindowsElement> targetSession)
        {
            IWebElement result = null;

            if (!string.IsNullOrEmpty(elementDTO.AccessibilityId))
            {
                try
                {
                    result = targetSession.FindElementByAccessibilityId(elementDTO.AccessibilityId);
                }
                catch (WebDriverException)
                {
                    return null;
                }
            }

            if (!string.IsNullOrEmpty(elementDTO.Name))
            {
                try
                {
                    result = targetSession.FindElementByName(elementDTO.Name);
                }
                catch (WebDriverException)
                {
                    return null;
                }
            }

            return result;
        }

        public void CtrlDragNodeByOffset(NodeBase node, int xOffset, int yOffset)
        {
            dynamoActions
                .SendKeys(Keys.Control)
                .DragAndDropToOffset(node.Node, xOffset, yOffset)
                .SendKeys(Keys.Control);
            ExecuteActions();
        }

        /// <summary>
        /// accept the package manager TOU - requires a root desktop session, not the Dynamo main window handle
        /// Because the TOU window is a seperate window.
        /// </summary>
        /// <param name="desktopSession"></param>
        public void AcceptPackageSearchTOU(WindowsDriver<WindowsElement> desktopSession)
        {
            Thread.Sleep(TimeSpan.FromSeconds(2));
            WindowsElement acceptTermsButton = desktopSession.FindElementByName("I Accept");

            var actions = new Actions(desktopSession);
            actions.MoveToElement(acceptTermsButton);
            actions.Click();
            actions.Build().Perform();
        }

        #endregion .: Utils :.
    }
}
