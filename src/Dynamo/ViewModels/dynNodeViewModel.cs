//Copyright 2013 Ian Keough

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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Collections.ObjectModel;
using Dynamo.Connectors;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.Selection;
using Microsoft.Practices.Prism.Commands;

namespace Dynamo.Controls
{
    /// <summary>
    /// Interaction logic for dynControl.xaml
    /// </summary>
    

    public partial class dynNodeViewModel : dynViewModelBase
    {
        #region delegates
        public delegate void SetToolTipDelegate(string message);
        public delegate void UpdateLayoutDelegate(FrameworkElement el);
        #endregion

        #region private members

        ObservableCollection<dynPortViewModel> inPorts = new ObservableCollection<dynPortViewModel>();
        ObservableCollection<dynPortViewModel> outPorts = new ObservableCollection<dynPortViewModel>();
        
        dynNode nodeLogic;
        
        int preferredHeight = 30;
        private bool isFullyConnected = false;
        #endregion

        #region public members

        public bool IsFullyConnected
        {
            get { return isFullyConnected; }
            set
            {
                isFullyConnected = value;
                RaisePropertyChanged("IsFullyConnected");
            }
        }
        
        public LacingStrategy ArgumentLacing
        {
            get { return nodeLogic.ArgumentLacing; }
        }

        public dynNode NodeLogic
        {
            get { return nodeLogic; }
        }

        public string ToolTipText
        {
            get { return nodeLogic.ToolTipText; }
        }
        
        public ObservableCollection<dynPortViewModel> InPorts
        {
            get { return inPorts; }
            set
            {
                inPorts = value;
                RaisePropertyChanged("InPorts");
            }
        }

        public ObservableCollection<dynPortViewModel> OutPorts
        {
            get { return outPorts; }
            set
            {
                outPorts = value;
                RaisePropertyChanged("OutPorts");
            }
        }

        public string NickName
        {
            get { return nodeLogic.NickName; }
        }

        public ElementState State
        {
            get { return nodeLogic.State; }
        }

        public int PreferredHeight
        {
            get
            {
                return preferredHeight;
            }
            set
            {
                preferredHeight = value;
                RaisePropertyChanged("PreferredHeight");
            }
        }
        #endregion

        #region commands

        public DelegateCommand DeleteCommand { get; set; }
        public DelegateCommand<string> SetLacingTypeCommand { get; set; }
        public DelegateCommand SetStateCommand { get; set; }
        public DelegateCommand SelectCommand { get; set; }

        #endregion

        #region constructors
        /// <summary>
        /// dynElement constructor for use by workbench in creating dynElements
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="nickName"></param>
        public dynNodeViewModel(dynNode logic)
        {
            nodeLogic = logic;

            //respond to collection changed events to add
            //and remove port model views
            logic.InPorts.CollectionChanged += inports_collectionChanged;
            logic.OutPorts.CollectionChanged += outports_collectionChanged;

            this.IsSelected = false;

            State = ElementState.DEAD;

            logic.PropertyChanged += logic_PropertyChanged;

            DeleteCommand = new DelegateCommand(new Action(DeleteNode()), CanDeleteNode);
            SetLacingTypeCommand = new DelegateCommand<string>(new Action<string>(SetLacingType), CanSetLacingType);
        }

        void logic_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "NickName":
                    RaisePropertyChanged("NickName");
                    break;
                case "ArgumentLacing":
                    RaisePropertyChanged("ArgumentLacing");
                    break;
            }
        }

        private bool CanDeleteNode()
        {
            return true;
        }

        private Action DeleteNode()
        {
            foreach (var port in OutPorts)
            {
                for (int j = port.Connectors.Count - 1; j >= 0; j--)
                {
                    port.Connectors[j].Kill();
                }
            }

            foreach (dynPort p in InPorts)
            {
                for (int j = p.Connectors.Count - 1; j >= 0; j--)
                {
                    p.Connectors[j].Kill();
                }
            }

            NodeLogic.DisableReporting();
            NodeLogic.Destroy();
            NodeLogic.Cleanup();

            DynamoSelection.Instance.Selection.Remove(this);
            dynSettings.Controller.Nodes.Remove(NodeLogic);
            //dynSettings.Workbench.Children.Remove(node);
        }

        void SetLacingType(string parameter)
        {
            if (parameter == "Single")
            {
                NodeLogic.ArgumentLacing = LacingStrategy.Single;
            }
            else if (parameter == "Longest")
            {
                NodeLogic.ArgumentLacing = LacingStrategy.Longest;
            }
            else if (parameter == "Shortest")
            {
                NodeLogic.ArgumentLacing = LacingStrategy.Shortest;
            }
            else
                NodeLogic.ArgumentLacing = LacingStrategy.Single;

            ReportPropertyChanged("Lacing");
        }

        bool CanSetLacingType(string parameter)
        {
            return true;
        }

        void inports_collectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //The visual height of the node is bound to preferred height.
            PreferredHeight = Math.Max(inPorts.Count * 20 + 10, outPorts.Count * 20 + 10); //spacing for inputs + title space + bottom space

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                //create a new port view model
                foreach (var item in e.NewItems)
                {
                    InPorts.Add(new dynPortViewModel(item as dynPortModel));
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                //remove the port view model whose model item
                //is the one passed in
                foreach (var item in e.OldItems)
                {
                    InPorts.Remove(InPorts.ToList().First(x => x.PortModel == item));
                }
            }
        }

        void outports_collectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //The visual height of the node is bound to preferred height.
            PreferredHeight = Math.Max(inPorts.Count * 20 + 10, outPorts.Count * 20 + 10); //spacing for inputs + title space + bottom space

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                //create a new port view model
                foreach (var item in e.NewItems)
                {
                    OutPorts.Add(new dynPortViewModel(item as dynPortModel));
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                //remove the port view model whose model item is the
                //one passed in
                foreach (var item in e.OldItems)
                {
                    OutPorts.Remove(OutPorts.ToList().Where(x => x.PortModel == item));
                }
            }
        }

        #endregion

        public void UpdateConnections()
        {
            foreach (var p in nodeLogic.InPorts.Concat(nodeLogic.OutPorts))
                p.Update();
        }

        private Dictionary<UIElement, bool> enabledDict
            = new Dictionary<UIElement, bool>();

        internal void DisableInteraction()
        {
            enabledDict.Clear();

            foreach (UIElement e in inputGrid.Children)
            {
                enabledDict[e] = e.IsEnabled;

                e.IsEnabled = false;
            }
            State = ElementState.DEAD;
        }

        internal void EnableInteraction()
        {
            foreach (UIElement e in inputGrid.Children)
            {
                if (enabledDict.ContainsKey(e))
                    e.IsEnabled = enabledDict[e];
            }
            ValidateConnections();
        }

        public string Description
        {
            get
            {
                Type t = NodeLogic.GetType();
                object[] rtAttribs = t.GetCustomAttributes(typeof(NodeDescriptionAttribute), true);
                return ((NodeDescriptionAttribute)rtAttribs[0]).ElementDescription;
            }
        }

        public List<string> Tags
        {
            get
            {
                Type t = NodeLogic.GetType();
                object[] rtAttribs = t.GetCustomAttributes(typeof(NodeSearchTagsAttribute), true);

                if (rtAttribs.Length > 0)
                    return ((NodeSearchTagsAttribute)rtAttribs[0]).Tags;
                else
                    return new List<string>();

            }
        }

        
        #region junk
        //public void CallUpdateLayout(FrameworkElement el)
        //{
        //    el.UpdateLayout();
        //}

        //public void SetTooltip(string message)
        //{
        //    this.ToolTipText = message;
        //}
        #endregion
    }
}

