using System;
//using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Events;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Data;
using TextBox = System.Windows.Controls.TextBox;
using System.Windows.Forms;
using Dynamo.Controls;
using Dynamo.Connectors;

namespace Dynamo.Elements
{
    public class dynElementSettings
    {
        Autodesk.Revit.UI.UIApplication revit;
        Autodesk.Revit.UI.UIDocument doc;
        Element dynX;
        Element dynY;
        Element dynZ;
        Element dynVolatile;
        Element dynPersistent;
        FamilySymbol defaultFraming;
        Level defaultLevel;
        DynamoWarningSwallower warningSwallower;
        dynBench bench;
        Dynamo.Controls.DragCanvas workBench;
        dynCollection dynColl;
        Transaction trans;

        public Autodesk.Revit.UI.UIApplication Revit
        {
            get { return revit; }
        }
        public Autodesk.Revit.UI.UIDocument Doc
        {
            get { return doc; }
        }
        public Element StyleVolatile
        {
            get { return dynVolatile; }
        }
        public Element StylePersistent
        {
            get { return dynPersistent; }
        }
        public Element StyleX
        {
            get{return dynX;}
        }
        public Element StyleY
        {
            get { return dynY; }
        }
        public Element StyleZ
        {
            get { return dynZ; }
        }
        public FamilySymbol DefaultFraming
        {
            get { return defaultFraming; }
        }
        public Level DefaultLevel
        {
            get { return defaultLevel; }
        }
        public DynamoWarningSwallower WarningSwallower
        {
            get { return warningSwallower; }
        }
        public Dynamo.Controls.DragCanvas WorkBench
        {
            get{return workBench;}
            set{workBench = value;}
        }
        public dynBench Bench
        {
            get { return bench; }
            set { bench = value; }
        }
        public dynCollection Collection
        {
            get { return dynColl; }
            set { dynColl = value; }
        }
        public Transaction MainTransaction
        {
            get { return trans; }
        }
        public dynElementSettings(Autodesk.Revit.UI.UIApplication app, Autodesk.Revit.UI.UIDocument doc,
            Element dynVolatile, Element dynPersistent, Element dynX, Element dynY, Element dynZ,
            FamilySymbol defaultFraming, Level defaultLevel, DynamoWarningSwallower warningSwallower, Transaction t)
        {
            this.dynX = dynX;
            this.dynY = dynY;
            this.dynZ = dynZ;
            this.dynPersistent = dynPersistent;
            this.dynVolatile = dynVolatile;
            this.revit = app;
            this.doc = doc;
            this.defaultFraming = defaultFraming;
            this.defaultLevel = defaultLevel;
            this.warningSwallower = warningSwallower;
            this.trans = t;
        }
    }

    #region interfaces
    public interface IDynamic
    {
        void Draw();
        void Destroy();
    }
    #endregion

    #region dynamic value types
    //public class dynDouble : dynElement
    //{
    //    double d;
    //    public double D
    //    {
    //        get { return d; }
    //        set
    //        {
    //            if (double.IsNaN((double)value))
    //            {
    //                d = 0.0;
    //            }
    //            else
    //            {
    //                d = value;
    //            }
    //        }
    //    }

    //    public dynDouble(dynElementSettings settings, string nickName)
    //        : base(settings, nickName)
    //    {
    //        this.d = 0.0;
    //        base.Outputs.Add(this);
    //        OutPortData.Add(new PortData("D", "Double value.", typeof(dynDouble)));
    //        //base.RegisterInputsAndOutputs();
    //    }

    //    public dynDouble(dynElementSettings settings):base(settings)
    //    {
    //    }

    //    public override void Draw()
    //    {
    //        base.Draw();
    //    }

    //    public override void Update()
    //    {
    //        //raise the event for the base class
    //        //to build, sending this as the 
    //        OnDynElementReadyToBuild(EventArgs.Empty);
    //    }
    //}

    //public class dynDoubleSimple : dynDouble, IDynamic
    //{
    //    public dynDoubleSimple(dynElementSettings settings)
    //        : base(settings)
    //    {
    //        this.D = 0.0;
    //        base.RegisterInputsAndOutputs();
    //    }
    //}
    
    //public class dynDoubleInput : dynDouble
    //{
    //    TextBox tb;

    //    public dynDoubleInput(dynElementSettings settings, string nickName)
    //        : base(settings, nickName)
    //    {

    //        //add a text box to the input grid of the control
    //        tb = new System.Windows.Controls.TextBox();
    //        tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
    //        tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
    //        inputGrid.Children.Add(tb);
    //        System.Windows.Controls.Grid.SetColumn(tb, 0);
    //        System.Windows.Controls.Grid.SetRow(tb, 0);
    //        tb.Text = "10.0";
    //        this.D = 10.0;

    //        //tb.TextChanged += new TextChangedEventHandler(tb_TextChanged);
    //        tb.KeyDown += new System.Windows.Input.KeyEventHandler(tb_KeyDown);

    //        base.RegisterInputsAndOutputs();
    //    }

    //    void tb_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    //    {
    //        //if enter is pressed, update the value
    //        if (e.Key == System.Windows.Input.Key.Enter)
    //        {
    //            TextBox tb = sender as TextBox;

    //            try
    //            {
    //                this.D = Convert.ToDouble(tb.Text);
    //            }
    //            catch
    //            {
    //                this.D = 0.0;
    //            }

    //            //base.UpdateOutputs();
    //            OnDynElementReadyToBuild(EventArgs.Empty);
    //        }

      
    //    }

    //    void tb_TextChanged(object sender, KeyEventArgs e)
    //    {
            
    //        TextBox tb = sender as TextBox;

    //        try
    //        {
    //            this.D = Convert.ToDouble(tb.Text);
    //        }
    //        catch
    //        {
    //            this.D = 0.0;
    //        }
    //    }
    //}

    //public class dynDoubleSlider : dynDouble
    //{
    //    Slider slide;

    //    public dynDoubleSlider(dynElementSettings settings, string nickName)
    //        : base(settings, nickName)
    //    {

    //        //add a slider control
    //        slide = new System.Windows.Controls.Slider();
    //        slide.Minimum = 0.0;
    //        slide.Maximum = 10.0;
    //        slide.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
    //        slide.VerticalAlignment = System.Windows.VerticalAlignment.Center;
    //        inputGrid.Children.Add(slide);
    //        System.Windows.Controls.Grid.SetColumn(slide, 0);
    //        System.Windows.Controls.Grid.SetRow(slide, 0);

    //        slide.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(slide_ValueChanged);

    //        base.RegisterInputsAndOutputs();
    //    }

    //    void slide_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
    //    {
    //        this.D = e.NewValue;
    //    }

    //}

    //public abstract class dynDoubleArray : dynElementArray
    //{
    //    dynElementArray arr_in;

    //    public dynElementArray Array
    //    {
    //        get { return arr_in; }
    //        set { arr_in = value; }
    //    }

    //    public dynDoubleArray(dynElementSettings settings, string nickName)
    //        : base(settings, nickName)
    //    {
    //        Inputs.Add(null);
    //        InPortData.Add(new PortData("Arr", "The dynElementArray to evaluate.", typeof(dynElementArray)));

    //        //base.RegisterInputsAndOutputs();
    //    }
    //}

    //public class dynDoubleInputArray : dynDoubleArray
    //{
    //    dynDouble double_in;

    //    public dynDoubleInputArray(dynElementSettings settings, string nickName)
    //        : base(settings, nickName)
    //    {
    //        Inputs.Add(null);
    //        InPortData.Add(new PortData("D", "The dynDouble value to evaluate.", typeof(dynDouble)));

    //        base.RegisterInputsAndOutputs();
    //    }
    //    public override void Draw()
    //    {
    //        Array = Inputs[0] as dynElementArray;
    //        double_in = Inputs[1] as dynDouble;
    //        if (double_in != null)
    //        {
    //            foreach (dynElement el in Array.Elements)
    //            {
    //                Elements.Add(double_in);
    //            }
    //        }
    //        //base.Draw();
    //    }
    //    public override void Destroy()
    //    {
    //        base.Destroy();
    //    }

    //    public override void Update()
    //    {
    //        OnDynElementReadyToBuild(EventArgs.Empty);
    //    }
    //}

    //public class dynDoubleDistanceArray: dynDoubleArray
    //{
    //    dynFamilySymbolBySelection fs_in;

    //    public dynDoubleDistanceArray(dynElementSettings settings, string nickName)
    //        : base(settings, nickName)
    //    {
    //        Inputs.Add(null);
    //        InPortData.Add(new PortData("FS", "The family instance to measure to.", typeof(dynFamilySymbolBySelection)));

    //        base.RegisterInputsAndOutputs();
    //    }

    //    public override void Draw()
    //    {
    //        Array = Inputs[0] as dynElementArray;
    //        fs_in = Inputs[1] as dynFamilySymbolBySelection;

    //        if (fs_in != null && Array != null)
    //        {
    //            if (fs_in.FamilyInst != null)
    //            {

    //                LocationPoint lp = fs_in.FamilyInst.Location as LocationPoint;

    //                if (lp != null)
    //                {
    //                    foreach (dynElement el in Array.Elements)
    //                    {
    //                        dynPoint p = el as dynPoint;
    //                        if (p != null)
    //                        {
    //                            dynDoubleSimple doubSimp = new dynDoubleSimple(Settings);
    //                            doubSimp.D = lp.Point.DistanceTo(p.point);

    //                            Elements.Add(doubSimp);
    //                        }
    //                    }
    //                }
    //                else
    //                {
    //                    //create placeholder distances
    //                    foreach (dynElement el in Array.Elements)
    //                    {
    //                        dynDoubleSimple doubSimp = new dynDoubleSimple(Settings);
    //                        doubSimp.D = 0.0;

    //                        Elements.Add(doubSimp);
    //                    }

    //                }
    //            }
    //        }

    //        //base.Draw();
    //    }

    //    public override void Destroy()
    //    {
    //        base.Destroy();
    //    }

    //    public override void Update()
    //    {
    //        OnDynElementReadyToBuild(EventArgs.Empty);
    //    }
    //}

    ////public class dynDoubleClamp : dynDouble
    ////{
    ////    dynDouble value_in;

    ////    public dynDoubleClamp(dynElementSettings settings, string nickName)
    ////        : base(settings, nickName)
    ////    {

    ////        Inputs.Add(null);
    ////        InPortData.Add(new PortData("D", "Value.", typeof(dynDouble)));

    ////        //add a text box to the input grid of the control
    ////        tb = new System.Windows.Controls.TextBox();
    ////        tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
    ////        tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
    ////        inputGrid.Children.Add(tb);
    ////        System.Windows.Controls.Grid.SetColumn(tb, 0);
    ////        System.Windows.Controls.Grid.SetRow(tb, 0);
    ////        tb.Text = "10.0";
    ////        this.D = 10.0;

    ////        //tb.TextChanged += new TextChangedEventHandler(tb_TextChanged);
    ////        tb.KeyDown += new System.Windows.Input.KeyEventHandler(tb_KeyDown);

    ////        base.RegisterInputsAndOutputs();
    ////    }

    ////    void tb_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    ////    {
    ////        //if enter is pressed, test the value
    ////        if (e.Key == System.Windows.Input.Key.Enter)
    ////        {
    ////            TextBox tb = sender as TextBox;

    ////            try
    ////            {
    ////                this.D = Convert.ToDouble(tb.Text);
    ////            }
    ////            catch
    ////            {
    ////                this.D = 0.0;
    ////            }

    ////            //base.UpdateOutputs();
    ////            OnDynElementReadyToBuild(EventArgs.Empty);
    ////        }


    ////    }
    ////}

    #endregion


    #region dynamic point types

    /// <summary>
    /// Base type for all dynamic point types
    /// </summary>
    public abstract class dynPoint : dynElement, IDynamic
    {
        public XYZ point;
        public XYZ up = new XYZ(0.0, 0.0, 1.0);
        public XYZ xAxis = new XYZ(1.0, 0.0, 0.0);
        public XYZ yAxis = new XYZ(0.0, 1.0, 0.0);
        public Plane xyPlane;
        public Plane yzPlane;
        public Plane xzPlane;
        public SketchPlane topSketch;
        public SketchPlane fbSketch;
        public SketchPlane lrSketch;
        protected ModelLine ll1;
        protected ModelLine ll2;
        protected ModelLine ll3;

        dynPlane xyPlane_out;
        dynPlane yzPlane_out;
        dynPlane xzPlane_out;

        #region accessors
        //public ModelLine LL1
        //{
        //    get { return ll1; }
        //}
        //public ModelLine LL2
        //{
        //    get { return ll2; }
        //}
        //public ModelLine LL3
        //{
        //    get { return ll3; }
        //}
        //public XYZ Up
        //{
        //    get { return up; }
        //}
        //public XYZ XAxis
        //{
        //    get { return xAxis; }
        //}
        //public XYZ YAxis
        //{
        //    get { return yAxis; }
        //}
        //public Plane XYPlane
        //{
        //    get { return xyPlane; }
        //}
        //public Plane YZPlane
        //{
        //    get { return yzPlane; }
        //}
        //public Plane XZPlane
        //{
        //    get { return xzPlane; }
        //}
        //public SketchPlane Top
        //{
        //    get { return topSketch; }
        //}
        //public SketchPlane FrontBack
        //{
        //    get { return fbSketch; }
        //}
        //public SketchPlane LeftRight
        //{
        //    get { return lrSketch; }
        //}
        //public XYZ Point
        //{
        //    get { return point; }
        //    set { point = value; }
        //}
        #endregion

        public dynPoint(dynElementSettings settings, string nickName)
            : base(settings, nickName)
        {
            point = new XYZ();
            xyPlane_out = new dynPlane(settings);
            yzPlane_out = new dynPlane(settings);
            xzPlane_out = new dynPlane(settings);

            Outputs.Add(this); 
            OutPortData.Add(new PortData("Pt", "The dynPoint.", typeof(dynPoint)));
            Outputs.Add(xyPlane_out); 
            OutPortData.Add(new PortData("Pl", "The XY plane of the point.", typeof(dynPlane)));
            Outputs.Add(yzPlane_out); 
            OutPortData.Add(new PortData("Pl", "The YZ plane of the point.", typeof(dynPlane)));
            Outputs.Add(xzPlane_out); 
            OutPortData.Add(new PortData("Pl", "The XZ plane of the point.", typeof(dynPlane)));

        }

        public dynPoint(dynElementSettings settings):base(settings)
        {
            //empty constructor for non-gui point types
        }

        //override the destroy method for all point objects
        public override void Destroy()
        {
            try
            {
                if (ll1 != null)
                {
                    Settings.Doc.Document.Delete(ll1);
                    Settings.Doc.Document.Delete(ll2);
                    Settings.Doc.Document.Delete(ll3);
                    Settings.Doc.Document.Delete(topSketch);
                    Settings.Doc.Document.Delete(fbSketch);
                    Settings.Doc.Document.Delete(lrSketch);
                    ll1 = null;
                    ll2 = null;
                    ll3 = null;
                    topSketch = null;
                    fbSketch = null;
                    lrSketch = null;
                }
                if (xyPlane != null)
                {
                    point = null;
                    xyPlane = null;
                    yzPlane = null;
                    xzPlane = null;
                }
            }
            catch
            {
                ll1 = null;
                ll2 = null;
                ll3 = null;
                topSketch = null;
                fbSketch = null;
                lrSketch = null;
                point = null;
                xyPlane = null;
                yzPlane = null;
                xzPlane = null;
            }

        }

        public override void Draw()
        {
            //dynDouble x = Inputs[0] as dynDouble;
            //dynDouble y = Inputs[1] as dynDouble;
            //dynDouble z = Inputs[2] as dynDouble;
            
            //if (x != null && y != null && z != null)
            //{
            //    point = new XYZ(x.D, y.D, z.D);
            //}
            //else
            //{
            //    point = null;
            //}

            if (point != null)
            {

                //generate the planes
                xyPlane = this.Settings.Revit.Application.Create.NewPlane(up, point);
                yzPlane = this.Settings.Revit.Application.Create.NewPlane(xAxis, point);
                xzPlane = this.Settings.Revit.Application.Create.NewPlane(yAxis, point);
                
                //generate the tick lines
                Line l1 = this.Settings.Revit.Application.Create.NewLineBound(point, point + up);
                Line l2 = this.Settings.Revit.Application.Create.NewLineBound(point, point + xAxis);
                Line l3 = this.Settings.Revit.Application.Create.NewLineBound(point, point + yAxis);

                //generate the sketch planes
                topSketch = this.Settings.Doc.Document.Create.NewSketchPlane(xyPlane);
                lrSketch = this.Settings.Doc.Document.Create.NewSketchPlane(yzPlane);
                fbSketch = this.Settings.Doc.Document.Create.NewSketchPlane(xzPlane);

                ll1 = this.Settings.Doc.Document.Create.NewModelCurve(l1, lrSketch) as ModelLine;
                ll2 = this.Settings.Doc.Document.Create.NewModelCurve(l2, topSketch) as ModelLine;
                ll3 = this.Settings.Doc.Document.Create.NewModelCurve(l3, topSketch) as ModelLine;

                if (xyPlane_out != null && yzPlane_out != null && xzPlane_out != null)
                {
                    //update the outputs
                    xyPlane_out.P = xyPlane;
                    yzPlane_out.P = yzPlane;
                    xzPlane_out.P = xzPlane;
                }
            }
        }

    }

    public abstract class dynElementArray : dynElement, IDynamic
    {
        List<dynElement> elements;

        public List<dynElement> Elements
        {
            get { return elements; }
            set
            {
                elements = value;
            }
        }

        public dynElementArray(dynElementSettings settings, string nickName)
            : base(settings, nickName)
        {
            elements = new List<dynElement>();

            Outputs.Add(this);
            OutPortData.Add(new PortData("Arr","The array of dynElements.", typeof(dynElementArray)));
        }

        public override void Destroy()
        {
            //foreach (Element e in elements)
            //{
            //    Settings.Doc.Document.Delete(e);
            //}
            foreach (dynElement e in elements)
            {
                e.Destroy();
            }
            elements.Clear();
        }

        public override void Draw()
        {
            foreach (dynElement e in elements)
            {
                e.Draw();
            }
        }

    }

    /// <summary>
    /// Dynamic point created by X,Y, and Z elements
    /// </summary>
    public class dynPointByXYZSimple : dynPoint, IDynamic
    {
        dynDouble x;
        dynDouble y;
        dynDouble z;

        public dynPointByXYZSimple(dynElementSettings settings, string nickName):base(settings, nickName)
        {

            Inputs.Add(x); 
            InPortData.Add(new PortData("X", "The X component of the point.", typeof(dynDouble)));
            Inputs.Add(y); 
            InPortData.Add(new PortData("Y", "The Y component of the point.", typeof(dynDouble)));
            Inputs.Add(z); 
            InPortData.Add(new PortData("Z", "The Z component of the point.", typeof(dynDouble)));

            base.RegisterInputsAndOutputs();
        }

        public override void Destroy()
        {
            base.Destroy();
        }
        
        public override void Draw()
        {
            if (Inputs.Count == 3)
            {
                x = Inputs[0] as dynDouble;
                y = Inputs[1] as dynDouble;
                z = Inputs[2] as dynDouble;

                if (x != null && y != null && z != null)
                {
                    this.point = new XYZ(x.D, y.D, z.D);
                    base.Draw();
                }
            }

        }
        
        public override void Update()
        {
            //raise the event for the base class
            //to build
            OnDynElementReadyToBuild(EventArgs.Empty);
        }

    }

    public class dynPointSimple : dynPoint, IDynamic
    {
        
        public dynPointSimple(dynElementSettings settings):base(settings)
        {

        }
    }

    public class dynPointByParameterAlongCurve : dynPoint, IDynamic
    {
        dynDouble t_in;
        dynCurve curve_in;

        public dynPointByParameterAlongCurve(dynElementSettings settings, string nickName)
            : base(settings, nickName)
        {
            Inputs.Add(t_in);
            InPortData.Add(new PortData("t", "The parameter along the curve.", typeof(dynDouble)));
            Inputs.Add(curve_in);
            InPortData.Add(new PortData("Crv", "The input curve.", typeof(dynCurve)));

            base.RegisterInputsAndOutputs();
        }

        public override void Destroy()
        {
            base.Destroy();
        }

        public override void Draw()
        {
         
            t_in = Inputs[0] as dynDouble;
            curve_in = Inputs[1] as dynCurve;

            if (curve_in != null && t_in != null)
            {
               this.point = curve_in.Curve.Evaluate(t_in.D, true);

               //compute the derivatives and set the axes of the point
               Transform t = curve_in.Curve.ComputeDerivatives(t_in.D, true);
               XYZ xaxis = t.BasisX.Normalize();
               XYZ yaxis = t.BasisY.Normalize();

               if (xaxis.Y == 1.0)
               {
                   yaxis = new XYZ(1.0, 0.0, 0.0);
               }
               else if (xaxis.Y == -1.0)
               {
                   yaxis = new XYZ(-1.0, 0.0, 0.0);
               }
               else if (xaxis.X == -1)
               {
                   yaxis = new XYZ(0.0, 1.0, 0.0);
               }
               else if (xaxis.X == 1)
               {
                   yaxis = new XYZ(0.0, -1.0, 0.0);
               }

               this.xAxis = xaxis;
               this.yAxis = yaxis;
               this.up = xAxis.CrossProduct(yaxis);

               base.Draw();
            }
        }

        public override void Update()
        {
            //raise the event for the base class
            //to build
            OnDynElementReadyToBuild(EventArgs.Empty);
        }

    }

    public class dynPointsByFaceGrid : dynElementArray, IDynamic
    {
        dynFaceBySelection face_in;
        dynDouble u_in;
        dynDouble v_in;
        dynDouble offset_in;

        public dynPointsByFaceGrid(dynElementSettings settings, string nickName)
            : base(settings, nickName)
        {
            //add the null inputs
            Inputs.Add(face_in);
            InPortData.Add(new PortData("F", "Face.", typeof(dynFaceBySelection)));
            Inputs.Add(u_in);
            InPortData.Add(new PortData("U", "Subdivisions in U direction", typeof(dynDouble)));
            Inputs.Add(v_in);
            InPortData.Add(new PortData("V", "Subdivisions in V direction.", typeof(dynDouble)));
            Inputs.Add(offset_in);
            InPortData.Add(new PortData("Off", "Face offset.", typeof(dynDouble)));

            base.RegisterInputsAndOutputs();

        }

        public override void Destroy()
        {
            base.Destroy();
        }
        public override void Draw()
        {
            face_in = Inputs[0] as dynFaceBySelection;
            u_in = Inputs[1] as dynDouble;
            v_in = Inputs[2] as dynDouble;
            offset_in = Inputs[3] as dynDouble;

            if (face_in != null &&
                u_in != null &&
                v_in != null && offset_in!=null)
            {
                //Use 0 for u coordinate and 1 for v coordinate.
                
                BoundingBoxUV bbuv = face_in.Face.GetBoundingBox();
                double uPer = bbuv.Max.U;
                double vPer = bbuv.Max.V;

                //convert the doubles to ints
                int uDiv = Convert.ToInt16(u_in.D);
                int vDiv = Convert.ToInt16(v_in.D);

                double perDivU = uPer / uDiv;
                double perDivV = vPer / vDiv;

                for (int i = 0; i <=uDiv; i++)
                {
                    for (int j = 0; j <= vDiv; j++)
                    {
                        UV uv1 = new UV(i * perDivU, j * perDivV);
                        Transform t = face_in.Face.ComputeDerivatives(uv1);
                        XYZ pt1 = face_in.Face.Evaluate(uv1) + t.BasisZ.Normalize() * offset_in.D;

                        dynPointSimple pt = new dynPointSimple(Settings);
                        pt.point = pt1;
                        pt.xAxis = t.BasisX.Normalize();
                        pt.yAxis = t.BasisY.Normalize();
                        pt.up = t.BasisZ.Normalize();

                        if (pt != null)
                        {
                            this.Elements.Add(pt);
                        }
                    }
                }

                base.Draw();
            }
        }
        
        public override void Update()
        {
            //raise the event for the base class
            //to build, sending this as the 
            OnDynElementReadyToBuild(EventArgs.Empty);
        }
    }

    //public class dynPointArrayStadium : dynElementArray, IDynamic
    //{
    //    //dynDouble D1_in;  //horiz. distance from APS to first row eye
    //    //dynDouble He_in;  //eye height of seated person typ
    //    //dynDouble E1_in;  //elevation of eye level at first row above APS
    //    //dynDouble C_in;    //sight line clearance over row in front
    //    //dynDouble T_in;    //depth of tread
    //    //dynDouble Rows;     //# of rows

    //    dynPoint focus_in;
    //    dynPoint basePt_in;

    //    //dynXYZArray stadiumArray_out;

    //    //local variables
    //    //double Dn;  //horiz. distance from row n eye position to APS
    //    //double En;  //elev. of eye at row n above APS
    //    //double Dcalc;

    //    public dynPointArrayStadium(dynElementSettings settings, string nickName)
    //        : base(settings, nickName)
    //    {

    //        //Inputs.Add(dy);
    //        //Inputs.Add(He_in);
    //        //Inputs.Add(E1_in);
    //        //Inputs.Add(C_in);
    //        //Inputs.Add(T_in);

    //        //Inputs.Add(Rows);

    //        Inputs.Add(focus_in); InTips.Add("dynPoint : Focal point.");
    //        Inputs.Add(basePt_in);  InTips.Add("dynPoint : Base point.");
    //        InTypes.Add(typeof(dynPoint));
    //        InTypes.Add(typeof(dynPoint));
    //        //Outputs.Add(stadiumArray_out);

    //        base.RegisterInputsAndOutputs();
    //    }

    //    public override void Destroy()
    //    {
    //        base.Destroy();
    //    }
        
    //    public override void Draw()
    //    {
    //        focus_in = Inputs[0] as dynPoint;
    //        basePt_in = Inputs[1] as dynPoint;

    //        List<UV> eyePoints = new List<UV>();
    //        List<UV> treadPoints = new List<UV>();
    //        List<double> RnMap = new List<double>();

    //        double He = 44.0/12;
    //        int Rows = 10;
    //        double T = 33.0;
    //        double C = 2.25/12;

    //        if (focus_in != null && basePt_in != null)
    //        {
    //            BowlHelper.GenerateSettingOutFromFocalPoints(focus_in.yzPlane, focus_in.point, ref eyePoints, ref treadPoints, ref RnMap, basePt_in.point.Z, He, T, C, Rows, 6.0);

    //            Transform t = Transform.Identity;
    //            t.Origin = focus_in.yzPlane.Origin;
    //            t.BasisX = focus_in.yzPlane.XVec;
    //            t.BasisY = focus_in.yzPlane.YVec;
    //            t.BasisZ = focus_in.yzPlane.Normal;

    //            for (int i = 0; i < treadPoints.Count - 1; i++)
    //            {
    //                UV treadPoint1 = treadPoints[i];
    //                UV treadPoint2 = treadPoints[i + 1];
    //                XYZ pt1 = BowlHelper.TransformPoint(new XYZ(treadPoint1.U, treadPoint1.V, 0.0), t);
    //                XYZ pt2 = BowlHelper.TransformPoint(new XYZ(treadPoint2.U, treadPoint2.V, 0.0), t);

    //                Line l = Settings.Revit.Application.Create.NewLineBound(pt1, pt2);
    //                ModelCurve mc = Settings.Doc.Document.Create.NewModelCurve(l, focus_in.lrSketch);
    //                Elements.Add(mc);
    //            }
    //        }

    //        //base.Draw();
    //    }
        
    //    public override void Update()
    //    {
    //        //raise the event for the base class
    //        //to build
    //        OnDynElementReadyToBuild(EventArgs.Empty);
    //    }
    //}

    #endregion

    #region dynamic geometric types
    public class dynVector : dynElement
    {
        public dynVector(dynElementSettings settings)
            : base(settings)
        {
        }
    }

    public class dynPlane : dynElement
    {
        Plane p;

        public Plane P
        {
            get { return p; }
            set 
            { 
                p = value;
                //OnDynElementUpdated(EventArgs.Empty);
            }
        }

        public dynPlane(dynElementSettings settings):base(settings)
        {

        }

    }

    public class dynXYZ : dynElement
    {
        XYZ pt;

        public XYZ Pt
        {
            get { return pt; }
            set
            {
                pt = value;
                //OnDynElementUpdated(EventArgs.Empty);
            }
        }

        public dynXYZ(dynElementSettings settings)
            : base(settings)
        {
        }
    }

    public class dynXYZArray : dynElement
    {
        List <dynXYZ> a;

        public List<dynXYZ> Array
        {
            get { return a; }
            set
            {
                a = value;
                //OnDynElementUpdated(EventArgs.Empty);
            }
        }

        public dynXYZArray(dynElementSettings settings)
            : base(settings)
        {
        }

    }

    #endregion

    #region dynamic curve types
    public abstract class dynCurve : dynElement, IDynamic
    {
        protected Curve curve;
        protected ModelCurve modelCurve;
        protected SketchPlane sp;
        //protected Plane plane;
        protected dynPlane plane_in;

        public SketchPlane Sketch
        {
            get { return sp; }
            set { sp = value; }
        }
        public Curve Curve
        {
            get { return curve; }
            set { curve = value; }
        }

        #region old accessors
        //public Curve Curve
        //{
        //    get { return curve; }
        //    set { curve = value; }
        //}
        //public ModelCurve ModelCurve
        //{
        //    get { return modelCurve; }
        //    set { modelCurve = value; }
        //}
        //public dynPlane Plane
        //{
        //    get { return plane_in; }
        //    set { plane_in = value;}
        //}
        //public SketchPlane SketchPlane
        //{
        //    get { return sp; }
        //    set { sp = value; }
        //}
        #endregion
 
        public dynCurve(dynElementSettings settings, string nickName)
            : base(settings, nickName)
        {
            Inputs.Add(plane_in); 
            InPortData.Add(new PortData("Pl", "The plane on which to create the curve.", typeof(dynPlane)));
            Outputs.Add(this); 
            OutPortData.Add(new PortData("Crv", "The curve.", typeof(dynCurve)));
        }

        public dynCurve(dynElementSettings settings) : base(settings)
        {

        }

        public override void Draw()
        {
            //sp = Settings.Doc.Document.Create.NewSketchPlane(plane_in.P);
            if (sp != null)
            {
                modelCurve = Settings.Doc.Document.Create.NewModelCurve(this.curve, sp);
            }
        }

        public override void Destroy()
        {
            if (modelCurve != null)
            {
                Settings.Doc.Document.Delete(modelCurve);
                Settings.Doc.Document.Delete(sp);
                modelCurve = null;
                curve = null;
            }
        }

    }

    public class dynCurveSimple : dynCurve, IDynamic
    {
        public dynCurveSimple(dynElementSettings settings):base(settings)
        {

        }
    }

    public class dynArcByCenterAndAngles : dynCurve, IDynamic
    {
        dynDouble angle1_in;
        dynDouble angle2_in;
        dynDouble radius_in;
 
        public dynArcByCenterAndAngles(dynElementSettings settings, string nickName)
            : base(settings, nickName)
        {
            //add the null inputs
            Inputs.Add(angle1_in);
            InPortData.Add(new PortData("A", "Angle 1.", typeof(dynDouble)));
            Inputs.Add(angle2_in);
            InPortData.Add(new PortData("A", "Angle 1.", typeof(dynDouble)));
            Inputs.Add(radius_in);
            InPortData.Add(new PortData("D", "The radius of the arc.", typeof(dynDouble)));
            base.RegisterInputsAndOutputs();

        }

        public override void Destroy()
        {
            base.Destroy();
        }
        public override void Draw()
        {
            angle1_in = Inputs[1] as dynDouble;
            angle2_in = Inputs[2] as dynDouble;
            radius_in = Inputs[3] as dynDouble;
            plane_in = Inputs[0] as dynPlane;

            if (plane_in != null && 
                plane_in.P != null && 
                angle1_in != null && 
                angle2_in != null && 
                radius_in != null)
            {
                if (angle2_in.D - angle1_in.D > 1 && radius_in.D > 0)
                {
                    //convert these to radians
                    double a1 = angle1_in.D * Math.PI / 180;
                    double a2 = angle2_in.D * Math.PI / 180;

                    curve = Settings.Revit.Application.Create.NewArc(plane_in.P, radius_in.D, angle1_in.D, angle2_in.D);

                    base.Draw();
                }
            }
        }
        public override void Update()
        {
            //raise the event for the base class
            //to build, sending this as the 
            OnDynElementReadyToBuild(EventArgs.Empty);
        }
    }

    public class dynCurvesByFaceGrid : dynElementArray, IDynamic
    {
        dynFaceBySelection face_in;
        dynDouble u_in;
        dynDouble v_in;
        dynDouble offset_in;

        public dynCurvesByFaceGrid(dynElementSettings settings, string nickName)
            : base(settings, nickName)
        {
            //add the null inputs
            Inputs.Add(face_in);
            InPortData.Add(new PortData("F", "Face.", typeof(dynFaceBySelection)));
            Inputs.Add(u_in);
            InPortData.Add(new PortData("U", "Subdivisions in U direction", typeof(dynDouble)));
            Inputs.Add(v_in);
            InPortData.Add(new PortData("V", "Subdivisions in V direction.", typeof(dynDouble)));
            Inputs.Add(offset_in);
            InPortData.Add(new PortData("Off", "Face offset.", typeof(dynDouble)));

            base.RegisterInputsAndOutputs();

        }

        public override void Destroy()
        {
            base.Destroy();
        }
        public override void Draw()
        {
            face_in = Inputs[0] as dynFaceBySelection;
            u_in = Inputs[1] as dynDouble;
            v_in = Inputs[2] as dynDouble;
            offset_in = Inputs[3] as dynDouble;

            if (face_in != null &&
                u_in != null &&
                v_in != null && offset_in!=null)
            {
                //Use 0 for u coordinate and 1 for v coordinate.
                
                BoundingBoxUV bbuv = face_in.Face.GetBoundingBox();
                double uPer = bbuv.Max.U;
                double vPer = bbuv.Max.V;

                //convert the doubles to ints
                int uDiv = Convert.ToInt16(u_in.D);
                int vDiv = Convert.ToInt16(v_in.D);

                double perDivU = uPer / uDiv;
                double perDivV = vPer / vDiv;

                for (int i = 0; i <= uDiv-1; i++)
                {
                    for (int j = 0; j <= vDiv; j++)
                    {
                        UV uv1 = new UV(i * perDivU, j * perDivV);
                        UV uv2 = new UV((i+1) * perDivU, j * perDivV);

                        XYZ normal1 = face_in.Face.ComputeNormal(uv1).Normalize();
                        XYZ normal2 = face_in.Face.ComputeNormal(uv2).Normalize();

                        XYZ pt1 = face_in.Face.Evaluate(uv1) + normal1 * offset_in.D;
                        XYZ pt2 = face_in.Face.Evaluate(uv2) + normal2 * offset_in.D;

                        //SketchPlane sp = dynUtils.CreateSketchPlaneForModelCurve(Settings.Revit, Settings.Doc,
                          //  pt1, pt2);
                        XYZ xAxis = (pt2 - pt1).Normalize();
                        XYZ planeNorm = xAxis.CrossProduct(normal1);
                        SketchPlane sp = Settings.Doc.Document.Create.NewSketchPlane(new Plane(planeNorm, pt1));
                        Curve c = Settings.Revit.Application.Create.NewLineBound(pt1, pt2);
                        //ModelCurve mc = Settings.Doc.Document.Create.NewModelCurve(c, sp);
                        dynCurveSimple cv = new dynCurveSimple(Settings);
                        cv.Curve = c;
                        cv.Sketch = sp;

                        //if (mc != null)
                        //{
                        //    this.Elements.Add(mc);
                        //}
                        if (cv != null)
                        {
                            this.Elements.Add(cv);
                        }

                    }
                }
                for (int i = 0; i <= uDiv; i++)
                {
                    for (int j = 0; j <= vDiv - 1; j++)
                    {
                        UV uv1 = new UV(i * perDivU, j * perDivV);
                        UV uv2 = new UV(i*perDivU, (j+1) * perDivV);

                        XYZ normal1 = face_in.Face.ComputeNormal(uv1).Normalize();
                        XYZ normal2 = face_in.Face.ComputeNormal(uv2).Normalize();

                        XYZ pt1 = face_in.Face.Evaluate(uv1) + normal1 * offset_in.D;
                        XYZ pt2 = face_in.Face.Evaluate(uv2) + normal2 * offset_in.D;

                        //SketchPlane sp = dynUtils.CreateSketchPlaneForModelCurve(Settings.Revit, Settings.Doc,
                        //    pt1, pt2);
                        XYZ xAxis = (pt2 - pt1).Normalize();
                        XYZ planeNorm = xAxis.CrossProduct(normal1);
                        SketchPlane sp = Settings.Doc.Document.Create.NewSketchPlane(new Plane(planeNorm, pt1));
                        Curve c = Settings.Revit.Application.Create.NewLineBound(pt1, pt2);
                        //ModelCurve mc = Settings.Doc.Document.Create.NewModelCurve(c, sp);
                        dynCurveSimple cv = new dynCurveSimple(Settings);
                        cv.Curve = c;
                        cv.Sketch = sp;

                        //if (mc != null)
                        //{
                        //    this.Elements.Add(mc);
                        //}
                        if (cv != null)
                        {
                            this.Elements.Add(cv);
                        }
                    }
                }

                base.Draw();
            }
        }
        public override void Update()
        {
            //raise the event for the base class
            //to build, sending this as the 
            OnDynElementReadyToBuild(EventArgs.Empty);
        }
    }
    #endregion

    #region dynamic selection types
    public class dynCurveBySelection : dynElement, IDynamic
    {
        protected Curve curve;

        public Curve Curve
        {
            get { return curve; }
        }
        
        public dynCurveBySelection(dynElementSettings settings, string nickName)
            : base(settings, nickName)
        {
            //add a button to the inputGrid on the dynElement
            System.Windows.Controls.Button curveSelectButt = new System.Windows.Controls.Button();
            this.inputGrid.Children.Add(curveSelectButt);
            curveSelectButt.Margin = new System.Windows.Thickness(0, 0, 0, 0);
            curveSelectButt.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            curveSelectButt.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            curveSelectButt.Click += new System.Windows.RoutedEventHandler(curveSelectButt_Click);
            curveSelectButt.Content = "Get Curve";

            //get the model curve selection
            //this.curve = SelectionHelper.RequestModelCurveSelection(Settings.Doc, "Select your a model curve.");

            this.Outputs.Add(this); //OutTips.Add("dynCurve");
            OutPortData.Add(new PortData("Crv", "The selected curve.", typeof(dynCurveBySelection)));
            //there are no inputs to this function, only the base curve output
            base.RegisterInputsAndOutputs();

        }

        void curveSelectButt_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            curve = SelectionHelper.RequestModelCurveSelection(Settings.Doc, "Select a model curve.", Settings);
        }

        public override void Destroy()
        {
            //base.Destroy();
        }
        public override void Draw()
        {
            //base.Draw();
        }
        public override void Update()
        {
            //raise the event for the base class
            //to build, sending this as the 
            OnDynElementReadyToBuild(EventArgs.Empty);
        }

    }

    public class dynFaceBySelection : dynElement, IDynamic
    {
        protected Face face;

        public Face Face
        {
            get { return face; }
        }

        public dynFaceBySelection(dynElementSettings settings, string nickName)
            : base(settings, nickName)
        {
            //add a button to the inputGrid on the dynElement
            System.Windows.Controls.Button faceSelectButt = new System.Windows.Controls.Button();
            this.inputGrid.Children.Add(faceSelectButt);
            faceSelectButt.Margin = new System.Windows.Thickness(0, 0, 0, 0);
            faceSelectButt.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            faceSelectButt.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            faceSelectButt.Click += new System.Windows.RoutedEventHandler(faceSetlect_buttClick);
            faceSelectButt.Content = "Get Face";

            this.Outputs.Add(this);
            OutPortData.Add(new PortData("F", "The selected face.", typeof(dynFaceBySelection)));
            //there are no inputs to this function, only the base curve output
            base.RegisterInputsAndOutputs();
        }

        void faceSetlect_buttClick(object sender, System.Windows.RoutedEventArgs e)
        {
            face = SelectionHelper.RequestFaceSelection(Settings.Doc, "Select a solid face.", Settings);
            
           
            if (face != null)
            {
                base.UpdateOutputs();
            }
        }

        public override void Destroy()
        {
            //base.Destroy();
        }
        public override void Draw()
        {
            //base.Draw();
        }
        public override void Update()
        {
            //raise the event for the base class
            //to build, sending this as the 
            OnDynElementReadyToBuild(EventArgs.Empty);
        }
    }

    public class dynFamilySymbolBySelection : dynElement, IDynamic
    {
        FamilySymbol fs;
        FamilyInstance fi;

        public FamilySymbol FamilySymb
        {
            get { return fs; }
        }
        public FamilyInstance FamilyInst
        {
            get { return fi; }
        }
        
        public dynFamilySymbolBySelection(dynElementSettings settings, string nickName)
            : base(settings, nickName)
        {
            //add a button to the inputGrid on the dynElement
            System.Windows.Controls.Button elementSelectButt = new System.Windows.Controls.Button();
            this.inputGrid.Children.Add(elementSelectButt);
            elementSelectButt.Margin = new System.Windows.Thickness(0, 0, 0, 0);
            elementSelectButt.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            elementSelectButt.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            elementSelectButt.Click += new System.Windows.RoutedEventHandler(elementSelectButt_Click);
            elementSelectButt.Content = "Symbol";

            this.Outputs.Add(this);
            OutPortData.Add(new PortData("FS", "The selected family symbol.", typeof(dynFamilySymbolBySelection)));

            base.RegisterInputsAndOutputs();
        }

        void elementSelectButt_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            fs  = SelectionHelper.RequestFamilyInstanceSelection(Settings.Doc, "Select a family instance.", Settings, ref fi);

            if (fs != null)
            {
                base.UpdateOutputs();
            }
        }
    }

    #endregion

    #region dynamic event arguments
    public class dynElementUpdateEventArgs : EventArgs
    {
        dynElement element;

        public dynElement Element
        {
            get { return element; }
        }
        public dynElementUpdateEventArgs(dynElement element)
        {
            this.element = element;
        }
    }
    #endregion

    #region dynamic instance and parameter types
    public class dynInstanceParameterMap : dynElement, IDynamic
    {
        dynFamilySymbolBySelection fs_in;
        dynElementArray array_in;
        List<dynParamData> map;

        public dynElementArray Array
        {
            get { return array_in; }
        }
        public dynFamilySymbolBySelection SelectedFamilySymbol
        {
            get { return fs_in; }
        }
        public List<dynParamData> ParameterMap
        {
            get { return map; }
        }

        public dynInstanceParameterMap(dynElementSettings settings, string nickName)
            : base(settings, nickName)
        {
            map = new List<dynParamData>();

            Inputs.Add(fs_in);
            InPortData.Add(new PortData("FS", "The Family Symbol", typeof(dynFamilySymbolBySelection)));
            Inputs.Add(array_in);
            InPortData.Add(new PortData("Arr", "The dynElementArray which defines this map.", typeof(dynElementArray)));

            Outputs.Add(this);
            OutPortData.Add(new PortData("PM", "A map of parameters on the instance.", typeof(dynInstanceParameterMap)));
            
            //add a button to the inputGrid on the dynElement
            System.Windows.Controls.Button paramMapButt = new System.Windows.Controls.Button();
            this.inputGrid.Children.Add(paramMapButt);
            paramMapButt.Margin = new System.Windows.Thickness(0, 0, 0, 0);
            paramMapButt.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            paramMapButt.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            paramMapButt.Click += new System.Windows.RoutedEventHandler(paramMapButt_Click);
            paramMapButt.Content = "Map";

            base.RegisterInputsAndOutputs();
 
        }

        void paramMapButt_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            fs_in = Inputs[0] as dynFamilySymbolBySelection;
            array_in = Inputs[1] as dynElementArray;

            if (fs_in != null && fs_in.FamilyInst != null)
            {
                map.Clear();

                //clear all the inputs but the first one
                //which is the family instance
                //first kill all the connectors
                for (int i = 2; i < InPorts.Count; i++)
                {
                    dynPort p = InPorts[i];
                    
                    //must remove the connectors iteratively
                    //do not use a foreach here!
                    while(p.Connectors.Count > 0)
                    {
                        dynConnector c = p.Connectors[p.Connectors.Count-1] as dynConnector;
                        c.Kill();
                    }
                }

                //then remove all the ports
                while (InPorts.Count > 2)
                {
                    InPorts.RemoveAt(InPorts.Count - 1);
                    InPortData.RemoveAt(InPorts.Count - 1);
                    Inputs.RemoveAt(InPorts.Count - 1);
                }

                while (gridLeft.RowDefinitions.Count > 2)
                {
                    //remove the port from the children list
                    gridLeft.Children.RemoveAt(gridLeft.RowDefinitions.Count - 1);
                    portNamesLeft.Children.RemoveAt(gridLeft.RowDefinitions.Count - 1);

                    gridLeft.RowDefinitions.RemoveAt(gridLeft.RowDefinitions.Count - 1);
                    portNamesLeft.RowDefinitions.RemoveAt(gridLeft.RowDefinitions.Count - 1);
                }

                foreach (Parameter p in fs_in.FamilyInst.Parameters)
                {
                    if (p.StorageType == StorageType.Double)
                    {
                        Inputs.Add(null);   //add a placeholder for the dynDouble
                        //InPortData.Add(new PortData(p.Definition.Name[0].ToString(), p.Definition.Name, typeof(dynDouble)));
                        InPortData.Add(new PortData(p.Definition.Name[0].ToString(), p.Definition.Name, typeof(dynElementArray)));
                    }
                }

                //resize this thing
                base.ResizeElementForInputs();

                //base.RegisterInputs();
                //add back new ports
                double yDiv = this.Height / Inputs.Count;
                //foreach (dynElement o in inputs)
                for(int i=2; i<Inputs.Count; i++)
                {
                    dynElement o = Inputs[i] as dynElement;

                    RowDefinition rd = new RowDefinition();
                    gridLeft.RowDefinitions.Add(rd);

                    RowDefinition nameRd = new RowDefinition();
                    portNamesLeft.RowDefinitions.Add(nameRd);

                    //add a port for each input
                    //distribute the ports along the 
                    //edges of the icon
                    AddPort(o, true, InPortData[i].NickName, i);
                }

                base.SetToolTips();
               
            }
        }

        public override void Draw()
        {
            fs_in = Inputs[0] as dynFamilySymbolBySelection;
            array_in = Inputs[1] as dynElementArray;

            //refresh the parameter map
            map.Clear();

            if (array_in != null)
            {
                int ptCount = 0;
                foreach (dynElement el in array_in.Elements)
                {
                    dynParamData dpd = new dynParamData();
                    for(int i=2; i<InPortData.Count;i++)
                    {
                        dynPoint p = el as dynPoint;
                        if (p != null)
                        {
                            dynDoubleArray d = Inputs[i] as dynDoubleArray;
                            
                            dpd.ParamNames.Add(InPortData[i].NickName);

                            if (d != null)
                            {
                                dynDouble dd = d.Elements[ptCount] as dynDouble;
                                if (dd != null)
                                {
                                    dpd.ParamValues.Add(dd.D);
                                }
                            }
                            else
                            {
                                dpd.ParamValues.Add(0.0);
                            }
                        }
                    }
                    map.Add(dpd);
                    ptCount++;
                }
            }

            //base.Draw();
        }
        
        public override void Update()
        {
            OnDynElementReadyToBuild(EventArgs.Empty);
        }
    }

    public class dynInstanceArray : dynElement, IDynamic
    {
        List<FamilyInstance> instances;
        public List<FamilyInstance> Instances
        {
            get { return instances; }
        }

        public dynInstanceArray(dynElementSettings settings, string nickName)
            : base(settings, nickName)
        {
            instances = new List<FamilyInstance>();

            Inputs.Add(null);
            InPortData.Add(new PortData("PM", "The parameter map which defines the parameter values for these instances.", 
                typeof(dynInstanceParameterMap)));

            Inputs.Add(null);
            InPortData.Add(new PortData("Face", "The face used to orient these instances.", typeof(dynFaceBySelection)));

            Outputs.Add(this);
            OutPortData.Add(new PortData("InstArr", "The dynInstanceArray created.", typeof(dynInstanceArray)));

            base.RegisterInputsAndOutputs();
        }
        
        public override void Destroy()
        {
            foreach (FamilyInstance fi in instances)
            {
                Settings.Doc.Document.Delete(fi);
            }

            instances.Clear();

            base.Destroy();
        }
        
        public override void Draw()
        {
            dynInstanceParameterMap map = Inputs[0] as dynInstanceParameterMap;
            dynFaceBySelection selFace = Inputs[1] as dynFaceBySelection;

            if (map.Array != null && map.SelectedFamilySymbol.FamilySymb != null)
            {
                int ptCount = 0;
                foreach (dynElement de in map.Array.Elements)
                {
                    dynPoint dp = de as dynPoint;
                    if (dp != null)
                    {
                        FamilyInstance fi = null;

                        if (selFace == null)
                        {
                            //create an instance at this point
                            //FamilyInstance fi = Settings.Doc.Document.Create.NewFamilyInstance(dp.point, symb.FamilySymb, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                            fi = Settings.Doc.Document.Create.NewFamilyInstance(dp.point, map.SelectedFamilySymbol.FamilySymb, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                        }
                        else
                        {
                            IntersectionResult iPt = selFace.Face.Project(dp.point);
                            Transform t = selFace.Face.ComputeDerivatives(iPt.UVPoint);
                            
                            fi = Settings.Doc.Document.Create.NewFamilyInstance(selFace.Face,dp.point,t.BasisX.Normalize(), map.SelectedFamilySymbol.FamilySymb);
                        }
                        
                        //add the instance to the array of family instances
                        instances.Add(fi);

                        //TODO:
                        //set the transform on the instance to align with the point
                        
                        //if the parameter map != null
                        if (map != null)
                        {
                            //iterate through the map's lists, the number of these is
                            //equal to the number of parameters
                            dynParamData dpd = map.ParameterMap[ptCount];

                            //get the value at the index for the current point
                            for (int j = 0; j < dpd.ParamNames.Count; j++)
                            {
                                double pValue = dpd.ParamValues[j];
                                string pName = dpd.ParamNames[j];
                                
                                Parameter p = fi.get_Parameter(pName);
                                if (p != null && p.StorageType == StorageType.Double)
                                {
                                    p.Set(pValue);
                                }
                            }
                            
                        }
                    }
                    ptCount++;
                }
            }
        }
        
        public override void Update()
        {
            OnDynElementReadyToBuild(EventArgs.Empty);
        }
        
    }
    #endregion

    #region delegates
    public delegate void dynElementUpdatedHandler(object sender, EventArgs e);
    public delegate void dynElementDestroyedHandler(object sender, EventArgs e);
    public delegate void dynElementReadyToBuildHandler(object sender, EventArgs e);
    public delegate void dynElementReadyToDestroyHandler(object sender, EventArgs e);
    #endregion

    public class DynamoWarningSwallower : IFailuresPreprocessor
    {
        public FailureProcessingResult PreprocessFailures(
          FailuresAccessor a)
        {
            // inside event handler, get all warnings

            IList<FailureMessageAccessor> failures
              = a.GetFailureMessages();

            foreach (FailureMessageAccessor f in failures)
            {
                // check failure definition ids 
                // against ones to dismiss:

                FailureDefinitionId id
                  = f.GetFailureDefinitionId();

                //BuiltInFailures.JoinElementsFailures.CannotKeepJoined == id ||
                //    BuiltInFailures.JoinElementsFailures.CannotJoinElementsError == id ||
                //    BuiltInFailures.JoinElementsFailures.CannotJoinElementsStructural == id ||
                //    BuiltInFailures.JoinElementsFailures.CannotJoinElementsStructuralError == id ||
                //    BuiltInFailures.JoinElementsFailures.CannotJoinElementsWarn == id

                if (BuiltInFailures.InaccurateFailures.InaccurateLine == id ||
                    BuiltInFailures.OverlapFailures.DuplicateInstances == id ||
                    BuiltInFailures.InaccurateFailures.InaccurateCurveBasedFamily == id ||
                    BuiltInFailures.InaccurateFailures.InaccurateBeamOrBrace == id
                    )
                {
                    a.DeleteWarning(f);
                }
                //else if(BuiltInFailures.CurveFailures.LineTooShortError == id ||
                //    BuiltInFailures.CurveFailures.LineTooShortWarning == id
                //    )
                //{
                //    a.RollBackPendingTransaction();
                //}
                else
                {
                    a.RollBackPendingTransaction();
                }
                
            }
            return FailureProcessingResult.Continue;
        }
    }

    public class dynParamData
    {
        List<string> paramNames;
        List<double> paramValues;

        public List<string> ParamNames
        {
            get { return paramNames; }
            set { paramNames = value; }
        }
        public List<double> ParamValues
        {
            get { return paramValues; }
            set { paramValues = value; }
        }

        public dynParamData()
        {
            paramNames = new List<string>();
            paramValues = new List<double>();
        }
    }
    public class SelectionHelper

    {
        public static Curve RequestModelCurveSelection(UIDocument doc, string message, dynElementSettings settings)
        {
            try
            {
                ModelCurve c = null;
                Curve cv = null;

                Selection choices = doc.Selection;

                choices.Elements.Clear();

                //MessageBox.Show(message);
                settings.Bench.Log(message);

                Reference curveRef = doc.Selection.PickObject(ObjectType.Element);

                c = curveRef.Element as ModelCurve;
                if (c != null)
                {
                    cv = c.GeometryCurve;
                }
                return cv;
            }
            catch (Exception ex)
            {
                settings.Bench.Log(ex.Message);
                return null;
            }
        }

        public static Face RequestFaceSelection(UIDocument doc, string message, dynElementSettings settings)
        {
            try
            {
                Face f = null;

                Selection choices = doc.Selection;

                choices.Elements.Clear();

                //MessageBox.Show(message);
                settings.Bench.Log(message);

                //create some geometry options so that we computer references
                Autodesk.Revit.DB.Options opts = new Options();
                opts.ComputeReferences = true;
                opts.DetailLevel = DetailLevels.Medium;
                opts.IncludeNonVisibleObjects = false;

                Reference faceRef = doc.Selection.PickObject(ObjectType.Face);

                if (faceRef != null)
                {
                    f = faceRef.GeometryObject as Face;
                    GeometryElement geom = faceRef.Element.get_Geometry(opts);
                    foreach (GeometryObject geob in geom.Objects)
                    {
                        Solid faceSolid = geob as Solid;

                        if(faceSolid != null)
                        {
                            foreach(Face testFace in faceSolid.Faces)
                            {
                                if(testFace.Area==f.Area)
                                {
                                    f=testFace;
                                }
                            }
                        }
                    }
                }
                return f;
            }
            catch (Exception ex)
            {
                settings.Bench.Log(ex.Message);
                return null;
            }

           
        }

        public static FamilySymbol RequestFamilyInstanceSelection(UIDocument doc, string message, 
            dynElementSettings settings, ref FamilyInstance fi)
        {
            try
            {
                FamilySymbol fs = null;

                Selection choices = doc.Selection;

                choices.Elements.Clear();

                //MessageBox.Show(message);
                settings.Bench.Log(message);

                Reference fsRef = doc.Selection.PickObject(ObjectType.Element);

                if (fsRef != null)
                {
                    fi = fsRef.Element as FamilyInstance;

                    if (fi != null)
                    {
                        return fi.Symbol;
                    }
                    else return null;
                }
                else return null;
            }
            catch (Exception ex)
            {
                settings.Bench.Log(ex.Message);
                return null;
            }
        }
    }
}
