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
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Dynamo.Connectors;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;

using Value = Dynamo.FScheme.Value;
using Dynamo.FSchemeInterop;
using Dynamo.Revit;

namespace Dynamo.Nodes
{
    [NodeName("Drafting View")]
    [NodeCategory(BuiltinNodeCategories.REVIT_VIEW)]
    [NodeDescription("Creates a drafting view.")]
    public class dynDraftingView: dynNodeWithOneOutput
    {
        public dynDraftingView()
        {
            InPortData.Add(new PortData("name", "Name", typeof(Value.String)));
            OutPortData.Add(new PortData("v", "Drafting View", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {

            ViewDrafting vd = null;
            string viewName = ((Value.String)args[0]).Item;

            if (!string.IsNullOrEmpty(viewName))
            {
                //if we've already found the view
                //and it's the same one, get out
                if (vd != null && vd.Name == viewName)
                {
                    return Value.NewContainer(vd);
                }

                FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
                fec.OfClass(typeof(ViewDrafting));

                IList<Element> els = fec.ToElements();

                var vds = from v in els
                            where ((ViewDrafting)v).Name == viewName
                            select v;

                if (vds.Count() == 0)
                {
                    try
                    {
                        //create the view
                        vd = dynRevitSettings.Doc.Document.Create.NewViewDrafting();
                        if (vd != null)
                        {
                            vd.Name = viewName;
                        }
                    }
                    catch
                    {
                        dynSettings.Controller.DynamoViewModel.Log(string.Format("Could not create view: {0}", viewName));
                    }
                }
                else
                {
                    vd = vds.First() as ViewDrafting;
                }
            }

            return Value.NewContainer(vd);
        }
    }

    [NodeName("Isometric View")]
    [NodeCategory(BuiltinNodeCategories.REVIT_VIEW)]
    [NodeDescription("Creates a drafting view.")]
    public class dynIsometricView : dynRevitTransactionNodeWithOneOutput
    {
        public dynIsometricView()
        {
            InPortData.Add(new PortData("eye", "The eye position point.", typeof(Value.Container)));
            InPortData.Add(new PortData("up", "The up direction of the view.", typeof(Value.Container)));
            InPortData.Add(new PortData("forward", "The view direction - the vector pointing from the eye towards the model.", typeof(Value.Container)));
            OutPortData.Add(new PortData("v", "The newly created 3D view.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            View3D view = null;
            XYZ eye = (XYZ)((Value.Container)args[0]).Item;
            XYZ userUp = (XYZ)((Value.Container)args[1]).Item;
            XYZ direction = (XYZ)((Value.Container)args[2]).Item;

            XYZ side = new XYZ();
            if (direction.IsAlmostEqualTo(userUp) || direction.IsAlmostEqualTo(userUp.Negate()))
                side = XYZ.BasisZ.CrossProduct(direction);
            else
                side = userUp.CrossProduct(direction);
            XYZ up = side.CrossProduct(direction);

            if (this.Elements.Any())
            {
                Element e;
                if (dynUtils.TryGetElement(this.Elements[0], typeof(View3D), out e))
                {
                    view = e as View3D;
                    if (!view.ViewDirection.IsAlmostEqualTo(direction))
                    {
                        ViewOrientation3D orient = new ViewOrientation3D(eye, up, direction);
                        view.SetOrientation(orient);
                        view.SaveOrientationAndLock();
                        view.SetOrientation(orient);
                    }
                }
                else
                {
                    //create a new view
                    view = Create3DView(eye, up, direction);
                    Elements[0] = view.Id;
                }
            }
            else
            {
                view = Create3DView(eye, up, direction);
                Elements.Add(view.Id);
            }

            return Value.NewContainer(view);
        }

        private static View3D Create3DView(XYZ eye, XYZ up, XYZ direction)
        {
            //http://adndevblog.typepad.com/aec/2012/05/viewplancreate-method.html

            IEnumerable<ViewFamilyType> viewFamilyTypes = from elem in new
              FilteredElementCollector(dynRevitSettings.Doc.Document).OfClass(typeof(ViewFamilyType))
                                                          let type = elem as ViewFamilyType
                                                          where type.ViewFamily == ViewFamily.ThreeDimensional
                                                          select type;

            //create a new view
            View3D view = View3D.CreateIsometric(dynRevitSettings.Doc.Document, viewFamilyTypes.First().Id);
            ViewOrientation3D orient = new ViewOrientation3D(eye, up, direction);
            view.SetOrientation(orient);
            view.SaveOrientationAndLock();
            return view;
        }
    }

    [NodeName("Bounding Box XYZ")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_TRANSFORM)]
    [NodeDescription("Create a bounding box.")]
    public class dynBoundingBoxXYZ : dynNodeWithOneOutput
    {
        public dynBoundingBoxXYZ()
        {
            InPortData.Add(new PortData("trans", "The coordinate system of the box.", typeof(Value.Container)));
            InPortData.Add(new PortData("x size", "The size of the bounding box in the x direction of the local coordinate system.", typeof(Value.Number)));
            InPortData.Add(new PortData("y size", "The size of the bounding box in the y direction of the local coordinate system.", typeof(Value.Number)));
            InPortData.Add(new PortData("z size", "The size of the bounding box in the z direction of the local coordinate system.", typeof(Value.Number)));
            OutPortData.Add(new PortData("bbox", "The bounding box.", typeof(Value.Container)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            BoundingBoxXYZ bbox = new BoundingBoxXYZ();
            
            Transform t = (Transform)((Value.Container)args[0]).Item;
            double x = (double)((Value.Number)args[1]).Item;
            double y = (double)((Value.Number)args[2]).Item;
            double z = (double)((Value.Number)args[3]).Item;

            bbox.Transform = t;
            bbox.Min = new XYZ(0, 0, 0);
            bbox.Max = new XYZ(x, y, z);
            return Value.NewContainer(bbox);
        }

    }

    [NodeName("Section View")]
    [NodeCategory(BuiltinNodeCategories.REVIT_VIEW)]
    [NodeDescription("Creates a drafting view.")]
    public class dynSectionView : dynRevitTransactionNodeWithOneOutput
    {
        public dynSectionView()
        {
            InPortData.Add(new PortData("bbox", "The bounding box of the view.", typeof(Value.Container)));
            OutPortData.Add(new PortData("v", "The newly created section view.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            ViewSection view = null;
            BoundingBoxXYZ bbox = (BoundingBoxXYZ)((Value.Container)args[0]).Item;

            //recreate the view. it does not seem possible to update a section view's orientation
            if (this.Elements.Any())
            {
                //create a new view
                view = CreateSectionView(bbox);
                Elements[0] = view.Id;
            }
            else
            {
                view = CreateSectionView(bbox);
                Elements.Add(view.Id);
            }

            return Value.NewContainer(view);
        }

        private static ViewSection CreateSectionView(BoundingBoxXYZ bbox)
        {
            //http://adndevblog.typepad.com/aec/2012/05/viewplancreate-method.html

            IEnumerable<ViewFamilyType> viewFamilyTypes = from elem in new
              FilteredElementCollector(dynRevitSettings.Doc.Document).OfClass(typeof(ViewFamilyType))
                                                          let type = elem as ViewFamilyType
                                                          where type.ViewFamily == ViewFamily.Section
                                                          select type;

            //create a new view
            ViewSection view = ViewSection.CreateSection(dynRevitSettings.Doc.Document, viewFamilyTypes.First().Id, bbox);
            return view;
        }
    }
}
