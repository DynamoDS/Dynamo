using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.Utilities;

namespace Dynamo.Revit
{
    public partial class dynRevitTransactionNode : dynNodeModel, IDrawable
    {
        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            base.SetupCustomUIElements(nodeUI);

            var mi = new MenuItem
            {
                Header = "Show Elements"
            };

            mi.Click += new System.Windows.RoutedEventHandler(mi_Click);

            nodeUI.MainContextMenu.Items.Add(mi);

        }

        void mi_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (AllElements.Count == 0)
                return;

            //select the elements
            dynRevitSettings.Doc.Selection.Elements.Clear();
            AllElements.ForEach(x=>dynRevitSettings.Doc.Selection.Elements.Add(dynRevitSettings.Doc.Document.GetElement(x)));

            //show the elements
            dynRevitSettings.Doc.ShowElements(Elements);
        }
    }
}
