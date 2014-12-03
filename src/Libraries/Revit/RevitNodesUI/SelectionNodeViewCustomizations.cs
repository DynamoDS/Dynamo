using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.Wpf;

namespace DSRevitNodesUI
{
    // TODO(Peter): These classes implement NodeViewCustomizations for various Revit selection nodes.
    // Because SelectionBaseNodeViewCustomization is a generic class and NodeViewCustomizations
    // may not be generic, we need to provide this (fairly dumb) implementation.  In the future,
    // we should consider extending the NodeViewCustomizationLibrary for cases like this, but this
    // is likely a non-trivial operation.

    public class ElementSelectionNodeViewCustomization :
        SelectionBaseNodeViewCustomization<Element, Element>,
        INodeViewCustomization<ElementSelection<Element>>
    {
        public void CustomizeView(ElementSelection<Element> model, NodeView nodeView)
        {
            base.CustomizeView(model, nodeView);
        }
    }

    public class DSDividedSurfaceFamiliesSelectionNodeViewCustomization :
        SelectionBaseNodeViewCustomization<DividedSurface, Element>,
        INodeViewCustomization<ElementSelection<DividedSurface>>
    {
        public void CustomizeView(ElementSelection<DividedSurface> model, NodeView nodeView)
        {
            base.CustomizeView(model, nodeView);
        }
    }

    public class ReferenceSelectionNodeViewCustomization :
        SelectionBaseNodeViewCustomization<Reference, Reference>,
        INodeViewCustomization<ReferenceSelection>
    {
        public void CustomizeView(ReferenceSelection model, NodeView nodeView)
        {
            base.CustomizeView(model, nodeView);
        }
    }

}
