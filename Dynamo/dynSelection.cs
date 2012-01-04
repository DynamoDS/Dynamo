using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Dynamo.Utilities;

namespace Dynamo.Elements
{
    [ElementName("Divided Surface by Selection")]
    [ElementDescription("An element which allows the user to select a divided surface.")]
    [RequiresTransaction(true)]
    public class dynSurfaceBySelection : dynElement, IDynamic
    {
        Form f;
        DividedSurfaceData dsd;
        //DataTree seedPts = new DataTree();

        public dynSurfaceBySelection()
        {
            
            this.topControl.Width = 300;

            OutPortData.Add(new Connectors.PortData(null, "srf", "The divided surface family instance(s)", typeof(dynElement)));
            OutPortData[0].Object = this.Tree;

            //add a button to the inputGrid on the dynElement
            System.Windows.Controls.Button paramMapButt = new System.Windows.Controls.Button();
            this.inputGrid.Children.Add(paramMapButt);
            paramMapButt.Margin = new System.Windows.Thickness(0, 0, 0, 0);
            paramMapButt.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            paramMapButt.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            paramMapButt.Click += new System.Windows.RoutedEventHandler(paramMapButt_Click);
            paramMapButt.Content = "Select";
            paramMapButt.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            paramMapButt.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            base.RegisterInputsAndOutputs();

        }

        void paramMapButt_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //clear the existing tree and the seed pts tree
            this.Tree.Clear();
            //seedPts.Clear();

            //foreach (Element el in this.Elements)
            //{
            //    dynElementSettings.SharedInstance.Doc.Document.Delete(el);
            //}

            f = SelectionHelper.RequestFormSelection(dynElementSettings.SharedInstance.Doc, "Select a face.", dynElementSettings.SharedInstance);
            dsd = f.GetDividedSurfaceData();
            if (dsd != null)
            {
                foreach (Reference r in dsd.GetReferencesWithDividedSurfaces())
                {
                    DividedSurface ds = dsd.GetDividedSurfaceForReference(r);

                    GridNode gn = new GridNode();

                    int u = 0;
                    while (u < ds.NumberOfUGridlines)
                    {
                        //add a new tree branch for every node
                        DataTreeBranch dtb = new DataTreeBranch();
                        this.Tree.Trunk.Branches.Add(dtb);
                        //DataTreeBranch seedBranch = new DataTreeBranch();
                        //seedPts.Trunk.Branches.Add(seedBranch);

                        gn.UIndex = u;

                        int v = 0;
                        while (v < ds.NumberOfVGridlines)
                        {
                            gn.VIndex = v;

                            if (ds.IsSeedNode(gn))
                            {
                                FamilyInstance fi
                                  = ds.GetTileFamilyInstance(gn, 0);

                                //put the family instance into the tree
                                dtb.Leaves.Add(fi);

                                ////add a reference point for the seed node
                                //Point p = ds.GetGridNodeReference(gn).GeometryObject as Point;
                                //if (p != null)
                                //{
                                //    ReferencePoint rp = dynElementSettings.SharedInstance.Doc.Document.FamilyCreate.NewReferencePoint(p.Coord);
                                //    seedBranch.Leaves.Add(rp);
                                //    Elements.Append(rp);
                                //}
                            }
                            v = v + 1;
                        }
                        u = u + 1;
                    }
                }
            }

            OnDynElementReadyToBuild(EventArgs.Empty);
        }

        public override void Update()
        {
            OnDynElementReadyToBuild(EventArgs.Empty);
        }

        public override void Destroy()
        {
            //don't call base destroy
            //base.Destroy();
        }
    }
}
