using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

using Autodesk.Revit.DB;

using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Utilities;
using RevitServices.Persistence;

namespace Dynamo.Revit
{
    public partial class RevitTransactionNode
    {
        public void SetupCustomUIElements(dynNodeView nodeUI)
        {
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
            DocumentManager.GetInstance().CurrentUIDocument.Selection.Elements.Clear();

            var existingElements = new List<Element>();

            foreach (var id in AllElements)
            {
                Element el;
                if (dynUtils.TryGetElement(id, out el))
                {
                    existingElements.Add(el);
                }
            }

            existingElements.ForEach(x => DocumentManager.GetInstance().CurrentUIDocument.Selection.Elements.Add(x));

            //show the elements
            DocumentManager.GetInstance().CurrentUIDocument.ShowElements(existingElements.Select(x => x.Id).ToList());
        }
    }
}
