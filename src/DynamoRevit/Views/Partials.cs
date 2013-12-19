using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

using Autodesk.Revit.DB;

using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Utilities;

namespace Dynamo.Revit
{
    public partial class RevitTransactionNode : NodeModel
    {
        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            base.SetupCustomUIElements(nodeUI);

            var mi = new MenuItem
            {
                Header = "Show Elements"
            };

            mi.Click += mi_Click;

            nodeUI.MainContextMenu.Items.Add(mi);

        }

        void mi_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!AllElements.Any())
                return;

            //select the elements
            dynRevitSettings.Doc.Selection.Elements.Clear();

            var existingElements = new List<Element>();

            foreach (var id in AllElements)
            {
                Element el;
                if (dynUtils.TryGetElement(id, out el))
                {
                    existingElements.Add(el);
                }
            }

            existingElements.ForEach(x => dynRevitSettings.Doc.Selection.Elements.Add(x));

            //show the elements
            dynRevitSettings.Doc.ShowElements(existingElements.Select(x => x.Id).ToList());
        }
    }
}
