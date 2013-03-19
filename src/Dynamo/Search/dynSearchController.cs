using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dynamo.Commands;
using Dynamo.Controls;

namespace Dynamo.Nodes
{
    public class dynSearchController
    {

        private static SearchDictionary<dynNodeUI> _searchDict = new SearchDictionary<dynNodeUI>();

        private static List<dynNode> localNodes = new List<dynNode>();

        private static ObservableCollection<dynNodeUI> visibleNodes = new ObservableCollection<dynNodeUI>();
        public ObservableCollection<dynNodeUI> VisibleNodes { get { return visibleNodes; } }

        private dynSearchUI _view;
        public dynSearchUI View { get { return _view;  } }

        private dynBench _bench;
        public dynBench Bench { get { return _bench; } }

        public dynSearchController( dynBench bench )
        {
            this._bench = bench;
            this._view = new dynSearchUI(this);
        }

        internal void SearchAndUpdateUI(string search)
        {
            visibleNodes.Clear();

            if (search == "") return;

            foreach (var node in this.Search(search))
            {
                visibleNodes.Add(node);
            }
        }

        internal List<dynNodeUI> Search(string search)
        {
            return _searchDict.FuzzySearch(search, 10);
        }

        public void SendFirstResultToWorkspace()
        {
            View.Visibility = Visibility.Collapsed;
            
            if (visibleNodes.Count == 0) return;

            DynamoCommands.CreateNodeCmd.Execute(new Dictionary<string, object>()
                {
                    {"name", visibleNodes[0].NickName},
                    {"x", Bench.ActualWidth/2.0},
                    {"y", Bench.ActualHeight/2.0}
                   
                });
        }
        

        public void Add(Type type, string name)
        {

            dynNode newNode = null;

            try
            {
                var obj = Activator.CreateInstance(type);
                newNode = (dynNode)obj;
            }
            catch (Exception e)
            {
                Bench.Log("Error creating search element for: " + name);
                Bench.Log(e.InnerException);
                return;
            }

            var nodeUI = newNode.NodeUI;
            nodeUI.DisableInteraction();
            nodeUI.Margin = new Thickness(5, 30, 5, 5);
            nodeUI.LayoutTransform = new ScaleTransform(0.8, 0.8);

            nodeUI.MouseDown += delegate
            {
                Bench.BeginDragElement(nodeUI, name, Mouse.GetPosition(nodeUI));
                nodeUI.Visibility = System.Windows.Visibility.Hidden;
            };

            nodeUI.GUID = new Guid();

            _searchDict.Add(nodeUI, name.Split(' ').Where(x => x.Length > 0));
            _searchDict.Add(nodeUI, name);
            _searchDict.AddName(nodeUI, name);

            localNodes.Add(newNode);

        }

    }
}
