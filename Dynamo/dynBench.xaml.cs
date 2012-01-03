//Copyright 2011 Ian Keough

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Dynamo.Connectors;
using Dynamo.Elements;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml.Serialization;
using System.IO;
using System.ComponentModel;
using System.Xml;
using Dynamo.Utilities;
using System.Collections.ObjectModel;

namespace Dynamo.Controls
{
    /// <summary>
    /// Interaction logic for DynamoForm.xaml
    /// </summary>
    public partial class dynBench : Window, INotifyPropertyChanged
    {

        double zoom = 1.0;
        double currentX;
        double currentY;
        double newX = 0.0;
        double newY = 0.0;
        double oldY = 0.0;
        double oldX = 0.0;
        List<dynElement> elements;
        //List<dynConnector> connectors;
        dynSelection selectedElements;
        bool isConnecting = false;
        dynConnector activeConnector;
        List<DependencyObject> hitResultsList = new List<DependencyObject>();
        bool isPanning = false;
        StringWriter sw;
        string logText;
        
        public dynToolFinder toolFinder;
        public event PropertyChangedEventHandler PropertyChanged;
        Hashtable userTypes = new Hashtable();
        //Hashtable builtinTypes = new Hashtable();
        SortedDictionary<string, TypeLoadData> builtinTypes = new SortedDictionary<string, TypeLoadData>();

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public string LogText
        {
            get { return logText; }
            set{
                logText = value;
                NotifyPropertyChanged("LogText");
            }
        }
        public double Zoom
        {
            get { return zoom; }
            set
            {
                zoom = value;
                NotifyPropertyChanged("Zoom");
            }
        }
        public double CurrentX
        {
            get { return currentX; }
            set 
            { 
                currentX = value;
                NotifyPropertyChanged("CurrentX");

            }
        }
        public double CurrentY
        {
            get { return currentY; }
            set 
            { 
                currentY = value;
                NotifyPropertyChanged("CurrentY");
            }
        }
        public List<dynElement> Elements
        {
            get{return elements;}
            set{elements = value;}
        }
        public dynSelection SelectedElements
        {
            get { return selectedElements; }
            set { selectedElements = value; }
        }
        public dynBench()
        {

            InitializeComponent();

            LoadBuiltinTypes();

            LoadUserTypes();

            //pass in the drawing canvas
            //this is where everything will be drawn
            dynElementSettings.SharedInstance.Workbench = workBench;
            dynElementSettings.SharedInstance.Bench = this;

            elements = new List<dynElement>();
            //connectors = new List<dynConnector>();
            selectedElements = new dynSelection();

            //DrawGrid();

            //create the stringWriter for logging
            sw = new StringWriter();
            Log("Welcome to Dynamo!");
            Log(String.Format("You are using build {0}.", Assembly.GetExecutingAssembly().GetName().Version.ToString()));
            Log("*****NEW IN THIS RELEASE*****");
            Log("-Nodes are now dragged using the left mouse button.");
            Log("-Added Matt's wicked button and logo :).");
        }
    
        private void LoadBuiltinTypes()
        {
            //setup the menu with all the types by reflecting
            //the DynamoElements.dll
            Assembly elementsAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            Type[] loadedTypes = elementsAssembly.GetTypes();

            foreach (Type t in loadedTypes)
            {
                //only load types that are in the right namespace, are not abstract
                //and have the elementname attribute
                object[] attribs = t.GetCustomAttributes(typeof(ElementNameAttribute), false);

                if (t.Namespace == "Dynamo.Elements" && 
                    !t.IsAbstract && 
                    attribs.Length>0 &&
                    t.IsSubclassOf(typeof(dynElement)))
                {
                    string typeName = (attribs[0] as ElementNameAttribute).ElementName;
                    builtinTypes.Add(typeName, new TypeLoadData(elementsAssembly, t));
                }
            }

            foreach (KeyValuePair<string,TypeLoadData> kvp in builtinTypes)
            {
                System.Windows.Controls.MenuItem mi = new System.Windows.Controls.MenuItem();
                mi.Header = kvp.Key;
                mi.Click += new RoutedEventHandler(AddElement_Click);
                AddMenu.Items.Add(mi);
            }
        }

        public void LoadUserTypes()
        {
            string directory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string pluginsPath = System.IO.Path.Combine(directory, "plugins");
            if (System.IO.Directory.Exists(pluginsPath))
            {
                string[] filePaths = Directory.GetFiles(pluginsPath, "*.dll");
                for (int i = 0; i < filePaths.Length; i++)
                {
                    Assembly currAss = Assembly.LoadFrom(filePaths[i]);
                    Type[] loadedTypes = currAss.GetTypes();
                    foreach (Type t in loadedTypes)
                    {
                        //only load types that are in the right namespace, are not abstract
                        //and have the elementname attribute
                        object[] attribs = t.GetCustomAttributes(typeof(ElementNameAttribute), false);

                        if (t.Namespace == "Dynamo.Elements" && 
                            !t.IsAbstract && 
                            attribs.Length > 0 &&
                            t.IsSubclassOf(typeof(dynElement)))
                        {
                            string typeName = (attribs[0] as ElementNameAttribute).ElementName;
                            System.Windows.Controls.MenuItem mi = new System.Windows.Controls.MenuItem();
                            mi.Header = typeName;
                            mi.Click += new RoutedEventHandler(AddElement_Click);
                            AddMenu.Items.Add(mi);

                            userTypes.Add(typeName, new TypeLoadData(currAss, t));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method adds dynElements when selected in the menu
        /// </summary>
        /// <param name="elementType"></param>
        /// <param name="nickName"></param>
        /// <param name="guid"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public dynElement AddDynElement(Type elementType, Assembly assem, string nickName, Guid guid, double x, double y)
        {
            try
            {
                //http://msdn.microsoft.com/en-us/library/ms173139.aspx
                //http://stackoverflow.com/questions/4993098/wpf-control-throwing-resource-identified-by-the-uri-missing-exception
                //http://www.matthidinger.com/archive/2008/10/12/managed-addin-framework-system.addin-with-wpf.aspx

                //create a new object from a type
                //that is passed in
                //dynElement el = (dynElement)Activator.CreateInstance(elementType, new object[] { nickName });
                System.Runtime.Remoting.ObjectHandle obj = Activator.CreateInstanceFrom(assem.Location, elementType.FullName);
                dynElement el = (dynElement)obj.Unwrap();

                el.GUID = guid;

                //store the element in the elements list
                elements.Add(el);
                
                workBench.Children.Add(el);

                Canvas.SetLeft(el, x);
                Canvas.SetTop(el, y);

                //create an event on the element itself
                //to update the elements ports and connectors
                el.PreviewMouseRightButtonDown += new MouseButtonEventHandler(UpdateElement);

                return el;
            }
            catch (Exception e)
            {
                dynElementSettings.SharedInstance.Bench.Log(e.Message);
                return null;
            }
        }

        /// <summary>
        /// This method adds dynElements when opening from a file
        /// </summary>
        /// <param name="elementType"></param>
        /// <param name="nickName"></param>
        /// <param name="guid"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public dynElement AddDynElement(Type elementType, string nickName, Guid guid, double x, double y)
        {
            try
            {
                //create a new object from a type
                //that is passed in
                //dynElement el = (dynElement)Activator.CreateInstance(elementType, new object[] { nickName });
                dynElement el = (dynElement)Activator.CreateInstance(elementType);

                if (!string.IsNullOrEmpty(nickName))
                {
                    el.NickName = nickName;
                }
                else
                {
                    ElementNameAttribute elNameAttrib = this.GetType().GetCustomAttributes(typeof(ElementNameAttribute), true)[0] as ElementNameAttribute;
                    if (elNameAttrib != null)
                    {
                        el.NickName = elNameAttrib.ElementName;
                    }
                }
                el.GUID = guid;

                //store the element in the elements list
                elements.Add(el);

                workBench.Children.Add(el);

                Canvas.SetLeft(el, x);
                Canvas.SetTop(el, y);

                //create an event on the element itself
                //to update the elements ports and connectors
                el.PreviewMouseRightButtonDown += new MouseButtonEventHandler(UpdateElement);

                return el;
            }
            catch (Exception e)
            {
                dynElementSettings.SharedInstance.Bench.Log("Could not create an instance of the selected type.");
                return null;
            }
        }

        public void SelectElement(dynElement sel)
        {
            if (!selectedElements.Contains(sel))
            {
                //set all other items to the unselected state
                foreach (dynElement el in selectedElements)
                {
                    el.Deselect();
                }

                selectedElements.Clear();
                selectedElements.Add(sel);
                sel.Select();

            }
        }

        public void ClearSelection()
        {
            //set all other items to the unselected state
            foreach (dynElement el in selectedElements)
            {
                el.Deselect();
            }
            selectedElements.Clear();
        }

        void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double scale = .001;
            double newValue = Convert.ToDouble(e.Delta)*scale;

            if (Zoom + newValue <= 1 && Zoom + newValue >= .001)
            {
                Zoom += newValue;
            }
            
            //if(this.zoomSlider.Value + newValue <= zoomSlider.Maximum &&
            //    this.zoomSlider.Value + newValue >= zoomSlider.Minimum)

            //this.zoomSlider.Value += newValue;
        }

        bool HasParentType(Type t, Type testType)
        {
            while (t != typeof(object))
            {
                t = t.BaseType;
                if (t.Equals(testType)) return true;
            }
            return false;
        }
        
        void UpdateElement(object sender, MouseButtonEventArgs e)
        {
            dynElement el = sender as dynElement;
            foreach (dynPort p in el.InPorts)
            {
                p.Update();
            }
            foreach (dynPort p in el.OutPorts)
            {
                p.Update();
            }
        }

        /// <summary>
        /// Find the user control of type 'testType' by traversing the tree.
        /// </summary>
        /// <returns></returns>
        public UIElement ElementClicked(DependencyObject depObj, Type testType)
        {
            UIElement foundElement = null;

            //IInputElement el = Mouse.DirectlyOver;
            //FrameworkElement fe = el as FrameworkElement;
            //DependencyObject depObj = fe.Parent;

            //walk up the tree to see whether the element is part of a port
            //then get the port's parent object
            while (depObj != null)
            {   
                // If the current object is a UIElement which is a child of the
                // Canvas, exit the loop and return it.
                UIElement elem = depObj as UIElement;

                if (elem != null)
                {
                    Type t = elem.GetType();

                    if (HasParentType(t, testType))
                    {
                        foundElement = elem;
                        return foundElement;
                    }

                    if (elem != null && t.Equals(testType))
                    {
                        foundElement = elem;
                        return foundElement;
                    }
                }

                // VisualTreeHelper works with objects of type Visual or Visual3D.
                // If the current object is not derived from Visual or Visual3D,
                // then use the LogicalTreeHelper to find the parent element.
                if (depObj is Visual)
                    depObj = VisualTreeHelper.GetParent(depObj);
                else
                    depObj = LogicalTreeHelper.GetParent(depObj);
            }

            return foundElement;
        }

        void TestClick(Point pt)
        {
            // Set up a callback to receive the hit test result enumeration.
            VisualTreeHelper.HitTest(workBench, null,
                new HitTestResultCallback(MyHitTestResult),
                new PointHitTestParameters(pt));

        }

        // Return the result of the hit test to the callback.
        public HitTestResultBehavior MyHitTestResult(HitTestResult result)
        {
            // Add the hit test result to the list that will be processed after the enumeration.
            hitResultsList.Add(result.VisualHit);

            // Set the behavior to return visuals at all z-order levels.
            return HitTestResultBehavior.Continue;
        }

        void DrawGrid()
        {
            //clear the canvas's children
            workBench.Children.Clear();
            double gridSpacing = 100.0;

            for (double i = 0.0; i < workBench.Width; i += gridSpacing)
            {
                Line xLine = new Line();
                xLine.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
                xLine.X1 = i;
                xLine.Y1 = 0;
                xLine.X2 = i;
                xLine.Y2 = workBench.Height;
                xLine.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                xLine.VerticalAlignment = VerticalAlignment.Center;
                xLine.StrokeThickness = 1;
                workBench.Children.Add(xLine);
                Dynamo.Controls.DragCanvas.SetCanBeDragged(xLine, false);
            }
            for (double i = 0.0; i < workBench.Height; i += gridSpacing)
            {
                Line yLine = new Line();
                yLine.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
                yLine.X1 = 0;
                yLine.Y1 = i;
                yLine.X2 = workBench.Width;
                yLine.Y2 = i;
                yLine.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                yLine.VerticalAlignment = VerticalAlignment.Center;
                yLine.StrokeThickness = 1;
                workBench.Children.Add(yLine);
                Dynamo.Controls.DragCanvas.SetCanBeDragged(yLine, false);
            }

        }

        public void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (isConnecting && activeConnector != null)
            {
                activeConnector.Redraw(e.GetPosition(workBench));
            }
            if (workBench.isDragInProgress)
            {
                dynElement el = workBench.elementBeingDragged as dynElement;
                if (el != null)
                {
                    foreach (dynPort p in el.InPorts)
                    {
                        p.Update();
                    }
                    foreach (dynPort p in el.OutPorts)
                    {
                        p.Update();
                    }
                    foreach (dynPort p in el.StatePorts)
                    {
                        p.Update();
                    }
                }
            }

            if (isPanning)
            {

                if (oldX == 0.0)
                {
                    oldX = e.GetPosition(border).X;
                    oldY = e.GetPosition(border).Y;
                }
                else
                {
                    newX = e.GetPosition(border).X;
                    newY = e.GetPosition(border).Y;
                    this.CurrentX += newX - oldX;
                    this.CurrentY += newY - oldY;
                    oldX = newX;
                    oldY = newY;
                }
                
            }
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            //string xmlPath = "C:\\test\\myWorkbench.xml";
            string xmlPath = "";

            System.Windows.Forms.SaveFileDialog saveDialog = new SaveFileDialog();
            if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                xmlPath = saveDialog.FileName;
            }

            if (!string.IsNullOrEmpty(xmlPath))
            {
                if (!SaveWorkbench(xmlPath))
                {
                    //MessageBox.Show("Workbench could not be saved.");
                    Log("Workbench could not be saved.");
                }
            }

        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                isPanning = true;
            }

            if (toolFinder != null)
            {
                //close the tool finder if the user
                //has clicked anywhere else on the workbench
                workBench.Children.Remove(toolFinder);
                toolFinder = null;
            }
            
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                isPanning = false;

                oldX = 0.0;
                oldY = 0.0;
                newX = 0.0;
                newY = 0.0;
            }
        }

        private void AddElement_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem mi = e.Source as System.Windows.Controls.MenuItem;

            TypeLoadData tld = builtinTypes[mi.Header.ToString()] as TypeLoadData;
            if(tld!=null)
            {
                dynElement newEl = AddDynElement(tld.t, tld.assembly, mi.Header.ToString(), Guid.NewGuid(), 0.0, 0.0);
                if (newEl != null)
                {
                    newEl.CheckInputs();
                    return;
                }
            }

            tld = userTypes[mi.Header.ToString()] as TypeLoadData;
            if(tld!=null)
            {
                dynElement newEl = AddDynElement(tld.t,tld.assembly,mi.Header.ToString(), Guid.NewGuid(), 0.0, 0.0);
                if (newEl != null)
                {
                    newEl.CheckInputs();
                    return;
                }
            }
        }

        bool SaveWorkbench(string xmlPath)
        {
            Log("Saving workbench " + xmlPath + "...");
            try
            {
                //create the xml document
                //create the xml document
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.CreateXmlDeclaration("1.0", null, null);

                XmlElement root = xmlDoc.CreateElement(this.GetType().ToString());  //write the root element
                xmlDoc.AppendChild(root);

                XmlElement elementList = xmlDoc.CreateElement("dynElements");  //write the root element
                root.AppendChild(elementList);

                foreach (dynElement el in elements)
                {
                    Point relPoint = el.TransformToAncestor(workBench).Transform(new Point(0, 0)); 

                    XmlElement dynEl = xmlDoc.CreateElement(el.GetType().ToString());
                    elementList.AppendChild(dynEl);

                    //set the type attribute
                    dynEl.SetAttribute("type", el.GetType().ToString());
                    dynEl.SetAttribute("guid", el.GUID.ToString());
                    dynEl.SetAttribute("nickname", el.NickName);
                    dynEl.SetAttribute("x", Canvas.GetLeft(el).ToString());
                    dynEl.SetAttribute("y", Canvas.GetTop(el).ToString());

                    foreach (PortData pd in el.OutPortData)
                    {
                        if (pd.Object != null)
                        {
                            //only write the numeric values
                            if (pd.Object.GetType() == typeof(System.Double) ||
                                pd.Object.GetType() == typeof(System.Int32))
                            {
                                Debug.WriteLine(pd.Object.GetType().ToString());
                                XmlElement outEl = xmlDoc.CreateElement(pd.Object.GetType().ToString());
                                outEl.SetAttribute("value", pd.Object.ToString());
                                dynEl.AppendChild(outEl);
                            }
                        }
                    }

                }

                //write only the output connectors
                XmlElement connectorList = xmlDoc.CreateElement("dynConnectors");  //write the root element
                root.AppendChild(connectorList);

                foreach(dynElement el in elements)
                {
                    foreach (dynPort p in el.OutPorts)
                    {
                        foreach (dynConnector c in p.Connectors)
                        {
                            if (c.End.Owner.GetType() == typeof(dynInstanceParameterMapper))
                                continue;

                            XmlElement connector = xmlDoc.CreateElement(c.GetType().ToString());
                            connectorList.AppendChild(connector);
                            connector.SetAttribute("start", c.Start.Owner.GUID.ToString());
                            connector.SetAttribute("start_index", c.Start.Index.ToString());
                            connector.SetAttribute("end", c.End.Owner.GUID.ToString());
                            connector.SetAttribute("end_index", c.End.Index.ToString());
                            
                            if (c.End.PortType == PortType.INPUT)
                                connector.SetAttribute("portType", "0");
                            else if (c.End.PortType == PortType.STATE)
                                connector.SetAttribute("portType", "1");
                        }
                    }

                }

                xmlDoc.Save(xmlPath);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
                return false;
            }

            return true;
        }

        void SerializeWorkbench(string xmlPath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<dynElement>));
            TextWriter tw = new StreamWriter(xmlPath);
            serializer.Serialize(tw, this.elements);
            tw.Close(); 

        }

        bool OpenWorkbench(string xmlPath)
        {
            Log("Opening workbench " + xmlPath + "...");
            CleanWorkbench();
                
                try
                {
                    #region read xml file
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(xmlPath);

                    XmlNodeList elNodes = xmlDoc.GetElementsByTagName("dynElements");
                    XmlNodeList cNodes = xmlDoc.GetElementsByTagName("dynConnectors");

                    XmlNode elNodesList = elNodes[0] as XmlNode;
                    XmlNode cNodesList = cNodes[0] as XmlNode;

                    foreach (XmlNode elNode in elNodesList.ChildNodes)
                    {
                        XmlAttribute typeAttrib = elNode.Attributes[0];
                        XmlAttribute guidAttrib = elNode.Attributes[1];
                        XmlAttribute nicknameAttrib = elNode.Attributes[2];
                        XmlAttribute xAttrib = elNode.Attributes[3];
                        XmlAttribute yAttrib = elNode.Attributes[4];

                        string typeName = typeAttrib.Value.ToString();
                        Guid guid = new Guid(guidAttrib.Value.ToString());
                        string nickname = nicknameAttrib.Value.ToString();

                        double x = Convert.ToDouble(xAttrib.Value.ToString());
                        double y = Convert.ToDouble(yAttrib.Value.ToString());

                        Type t = Type.GetType(typeName);

                        dynElement el = AddDynElement(t, nickname, guid, x, y);

                        //read the sub elements
                        //set any numeric values 
                        foreach (XmlNode subNode in elNode.ChildNodes)
                        {
                            if (subNode.Name == "System.Double")
                            {
                                double val = Convert.ToDouble(subNode.Attributes[0].Value);
                                el.OutPortData[0].Object = val;
                                el.Update();
                            }
                            else if (subNode.Name == "System.Int32")
                            {
                                int val = Convert.ToInt32(subNode.Attributes[0].Value);
                                el.OutPortData[0].Object = val;
                                el.Update();
                            }
                        }

                    }
                
                    dynElementSettings.SharedInstance.Workbench.UpdateLayout();

                    foreach (XmlNode connector in cNodesList.ChildNodes)
                    {
                        XmlAttribute guidStartAttrib = connector.Attributes[0];
                        XmlAttribute intStartAttrib = connector.Attributes[1];
                        XmlAttribute guidEndAttrib = connector.Attributes[2];
                        XmlAttribute intEndAttrib = connector.Attributes[3];
                        XmlAttribute portTypeAttrib = connector.Attributes[4];

                        Guid guidStart = new Guid(guidStartAttrib.Value.ToString());
                        Guid guidEnd = new Guid(guidEndAttrib.Value.ToString());
                        int startIndex = Convert.ToInt16(intStartAttrib.Value.ToString());
                        int endIndex = Convert.ToInt16(intEndAttrib.Value.ToString());
                        int portType = Convert.ToInt16(portTypeAttrib.Value.ToString());

                        //find the elements to connect
                        dynElement start = null;
                        dynElement end = null;

                        foreach (dynElement e in dynElementSettings.SharedInstance.Bench.Elements)
                        {
                            if (e.GUID == guidStart)
                            {
                                start = e;
                            }
                            else if (e.GUID == guidEnd)
                            {
                                end = e;
                            }
                            if (start != null && end != null)
                            {
                                break;
                            }
                        }

                        //don't connect if the end element is an instance map
                        //those have a morphing set of inputs
                        //dynInstanceParameterMap endTest = end as dynInstanceParameterMap;

                        //if (endTest != null)
                        //{
                        //    continue;
                        //}
                    
                        if (start != null && end != null && start != end)
                        {
                            dynConnector newConnector = new dynConnector(start, end, startIndex,
                                endIndex, portType);

                            //connectors.Add(newConnector);
                        }
                    

                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    Log("There was an error opening the workbench.");
                    Log(ex.Message);
                    Log(ex.StackTrace);
                    Debug.WriteLine(ex.Message + ":" + ex.StackTrace);
                    CleanWorkbench();
                    return false;
                }
            return true;
        }

        private void CleanWorkbench()
        {
            Log("Clearing workflow...");

            #region clear the existing workflow
            foreach (dynElement el in elements)
            {
                foreach (dynPort p in el.InPorts)
                {
                    for(int i=p.Connectors.Count-1;i>=0;i--)
                    {
                        p.Connectors[i].Kill();
                    }
                }
                foreach (dynPort p in el.OutPorts)
                {
                    for (int i = p.Connectors.Count - 1; i >= 0; i--)
                    {
                        p.Connectors[i].Kill();
                    }
                }
                foreach (dynPort p in el.StatePorts)
                {
                    for (int i = p.Connectors.Count - 1; i >= 0; i--)
                    {
                        p.Connectors[i].Kill();
                    }
                }

                //remove the element from the workbench
                dynElementSettings.SharedInstance.Workbench.Children.Remove(el);
            }

            elements.Clear();

            #endregion
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            //string xmlPath = "C:\\test\\myWorkbench.xml";
            string xmlPath = "";

            System.Windows.Forms.OpenFileDialog openDialog = new OpenFileDialog();
            if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                xmlPath = openDialog.FileName;
            }

            if (!string.IsNullOrEmpty(xmlPath))
            {
                if (!OpenWorkbench(xmlPath))
                {
                    //MessageBox.Show("Workbench could not be opened.");
                    Log("Workbench could not be opened.");

                    dynElementSettings.SharedInstance.Writer.WriteLine("Workbench could not be opened.");
                    dynElementSettings.SharedInstance.Writer.WriteLine(xmlPath);
                }
            }
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            if (sw != null)
            {
                sw.Close();
                dynElementSettings.SharedInstance.Writer.WriteLine("Dynamo ended " + System.DateTime.Now.ToString());
                dynElementSettings.SharedInstance.Writer.Close();
            }

            //end the transaction 
            dynElementSettings.SharedInstance.MainTransaction.Commit();
        }

        public void Log(string message)
        {
            sw.WriteLine(message);
            LogText = sw.ToString();
            //LogScroller.ScrollToEnd();

            dynElementSettings.SharedInstance.Writer.WriteLine(message);
        }

        void OnPreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //Keyboard.Focus(this);

            hitResultsList.Clear();
            TestClick(e.GetPosition(workBench));

            dynPort p = null;
            DragCanvas dc = null;
            dynElement element = null;

            //figure out which element is hit
            //HACK: put the tests with break in highest to
            //lowest z order 
            if (hitResultsList.Count > 0)
            {
                foreach (DependencyObject depObj in hitResultsList)
                {
                    //traverse the tree through all the
                    //hit elements to see if you get a port
                    p = ElementClicked(depObj, typeof(dynPort)) as dynPort;
                    if (p != null)
                    {
                        break;
                    }

                    //traverse the tree through all the
                    //hit elements to see if you get an element
                    element = ElementClicked(depObj, typeof(dynElement)) as dynElement;
                    if (element != null)
                    {
                        break;
                    }

                    //traverse the tree through all the
                    //hit elements to see if you get a port
                    dc = ElementClicked(depObj, typeof(DragCanvas)) as DragCanvas;
                    if (dc != null)
                    {
                        break;
                    }
                }

            }

            #region test for a port
            if (p != null)
            {
                Debug.WriteLine("Port clicked");

                if (!isConnecting)
                {
                    //test if port already has a connection if so grab it
                    //and begin connecting to somewhere else
                    //don't allow the grabbing of the start connector
                    if (p.Connectors.Count > 0 && p.Connectors[0].Start != p)
                    {
                        activeConnector = p.Connectors[0];
                        activeConnector.Disconnect(p);
                        isConnecting = true;
                        workBench.isConnecting = true;
                    }
                    else
                    {

                        try
                        {

                            //you've begun creating a connector
                            dynConnector c = new dynConnector(p, workBench, e.GetPosition(workBench));
                            activeConnector = c;
                            isConnecting = true;
                            workBench.isConnecting = true;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }

                }
                else
                {
                    //attempt a connection between the port
                    //and the connector
                    if (!activeConnector.Connect(p))
                    {
                        activeConnector.Kill();
                        isConnecting = false;
                        workBench.isConnecting = false;
                        activeConnector = null;

                    }
                    else
                    {
                        //you've already started connecting
                        //now you're going to stop
                        isConnecting = false;
                        workBench.isConnecting = false;
                        activeConnector = null;
                    }
                }

                //set the handled flag so that the element doesn't get dragged
                e.Handled = true;
            }
            else
            {
                //if you click on the canvas and you're connecting
                //then drop the connector, otherwise do nothing
                if (activeConnector != null)
                {
                    activeConnector.Kill();
                    isConnecting = false;
                    workBench.isConnecting = false;
                    activeConnector = null;
                }

            }
            #endregion

            if (element != null)
            {
                Debug.WriteLine("Element clicked");
                SelectElement(element);
            }

            if (dc != null)
            {
                Debug.WriteLine("Canvas clicked");
                ClearSelection();
            }


        }

        //void OnMouseRightButtonDown(object sender, System.Windows.Input.MouseEventArgs e)
        //{
        //    hitResultsList.Clear();
        //    TestClick(e.GetPosition(workBench));

        //    dynElement dynEl = null;
        //    if (hitResultsList.Count > 0)
        //    {
        //        foreach (DependencyObject depObj in hitResultsList)
        //        {
        //            //traverse the tree through all the
        //            //hit elements to see if you get a port
        //            dynEl = ElementClicked(depObj, typeof(dynElement)) as dynElement;
        //            if (dynEl != null)
        //            {
        //                break;
        //            }
        //        }
        //    }

        //    //start dragging the element
        //    if (dynEl != null)
        //    {
        //        //this.statusText.Text = "DynElement selected...";
        //        //hold off on setting the isDragInProcess
        //        workBench.isDragInProgress = true;
        //        workBench.elementBeingDragged = dynEl;
        //        workBench.DragElement();
        //    }

        //}

        //bubbling
        //from element up to root
        private void OnKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            
           
        }

        //tunneling
        //from root down to element
        private void OnPreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            
        }

        private void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //handle key presses for the bench in the bubbling event
            //if no other element has already handled this event it will 
            //start at the bench and move up to root, not raising the event
            //on any other elements

            //if the key down is 'b' open the build window
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.B))
            {
                //get the mouse position

                toolFinder = new dynToolFinder();
                dynElementSettings.SharedInstance.Workbench.Children.Add(toolFinder);
                toolFinder.ToolFinderFinished += new ToolFinderFinishedHandler(toolFinder_ToolFinderFinished);

                Canvas.SetLeft(toolFinder, 100);
                Canvas.SetTop(toolFinder, 100);
                e.Handled = true;
            }
            //changed the delete key combination so as not to interfere with
            //keyboard events
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.Back) ||
                Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.Delete))
            {
                //don't do this if an input element has focus
                //this keeps us from deleting nodes when the
                //user is deleting text

                for (int i = selectedElements.Count - 1; i >= 0; i--)
                {
                    DeleteElement(selectedElements[i]);
                }
                e.Handled = true;
            }

            
        }

        private void DeleteElement(dynElement el)
        {
            foreach (dynPort p in el.OutPorts)
            {
                for(int i=p.Connectors.Count-1; i>=0; i--)
                {
                    p.Connectors[i].Kill();
                }
            }
            foreach (dynPort p in el.InPorts)
            {
                for (int i = p.Connectors.Count - 1; i >= 0; i--)
                {
                    p.Connectors[i].Kill();
                }
            }

            selectedElements.Remove(el);
            elements.Remove(el);
            dynElementSettings.SharedInstance.Workbench.Children.Remove(el);
            el = null;
            
        }
        
        void toolFinder_ToolFinderFinished(object sender, EventArgs e)
        {
            dynElementSettings.SharedInstance.Workbench.Children.Remove(toolFinder);
            toolFinder = null;
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            CleanWorkbench();
        }

    }

    public class dynSelection : ObservableCollection<dynElement>
    {
        public dynSelection()
            : base()
        {

        }
    }

    public class TypeLoadData
    {
        public Assembly assembly;
        public Type t;

        public TypeLoadData(Assembly assemblyIn, Type typeIn)
        {
            assembly = assemblyIn;
            t = typeIn;

        }
    }

}
