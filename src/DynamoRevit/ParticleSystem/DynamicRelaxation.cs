using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Dynamo.Controls;
using Dynamo.FSchemeInterop;
using Dynamo.Models;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;

namespace Dynamo.Nodes
{
    public abstract class ParticleSystemBase : NodeModel
    {
        internal ParticleSystem ParticleSystem;

        internal ParticleSystemBase()
        {
            ParticleSystem = new ParticleSystem();
        }
    }

    [NodeName("Create Particle System")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_STRUCTURE)]
    [NodeDescription("A node which allows you to drive the position of elements via a particle system.")]
    class DynamicRelaxation :  ParticleSystemBase
    {
        private double _d;
        private double _m;
        private double _s;
        private double _r;
        private double _g;
        private int _fixPtCount;
        private bool _useRl;
        private double _rlf;
        private double _threshold;
        private bool _reset;
        private FSharpList<Value> _points;
        private FSharpList<Value> _curves;

        private readonly PortData _psPort = new PortData(
            "ps", "Particle System", typeof(ParticleSystem));

        private readonly PortData _forcesPort = new PortData(
            "forces", "Member forces.", typeof(Value.List));

        public DynamicRelaxation()
        {
            InPortData.Add(new PortData("points", "The points to use as fixed nodes.", typeof(Value.List)));
            InPortData.Add(new PortData("curves", "Curves to use as springs.", typeof(Value.List)));
            InPortData.Add(new PortData("d", "Dampening.", typeof(Value.Number)));
            InPortData.Add(new PortData("s", "Spring constant.", typeof(Value.Number)));
            InPortData.Add(new PortData("rl", "Rest length.", typeof(Value.Number)));
            InPortData.Add(new PortData("use_rl", "Should the rest length be considered (true), or should we use the rest length factor instead (false)?.", typeof(Value.Number)));
            InPortData.Add(new PortData("rlf", "Rest length factor. If use rest length factor is set to false," +
                " rest length will be ignored and this factor will be used to determine the rest length as a factor of the spring's initial size.", typeof(Value.Number)));
            InPortData.Add(new PortData("m", "Nodal Mass.", typeof(Value.Number)));
            InPortData.Add(new PortData("gravity", "Gravity in Z.", typeof(Value.Number)));
            InPortData.Add(new PortData("threshold", "The convergence threshold. When the maximum nodal velocity falls below this number, the particle system is flagged \"converged\".", typeof(Value.Number)));

            OutPortData.Add(_psPort);
            OutPortData.Add(_forcesPort);

            RegisterAllPorts();
        }

        void setupLineTest(int maxPartX, int maxPartY, double springDampening, double springRestLength, double springConstant, double mass)
        {
            XYZ partXYZ;
            double stepSize = 20;

            for (int j = 0; j < maxPartY; j++) // Y axis is outer loop
            {
                for (int i = 0; i < maxPartX; i++) // X axis is inner loop
                {
                    if (i == 0)
                    {
                        partXYZ = new XYZ(0, j*stepSize, 0);
                        Particle a = ParticleSystem.makeParticle(mass, partXYZ, true);
                    }
                    else
                    {
                        partXYZ = new XYZ(i * stepSize, j * stepSize, 0);
                        Particle b = ParticleSystem.makeParticle(mass, partXYZ, false);
                        ParticleSystem.makeSpring(ParticleSystem.getParticle((i - 1)+(j*maxPartX)), b, springRestLength, springConstant, springDampening);
                    }
                    if (i == maxPartX - 1)
                    {
                        ParticleSystem.getParticle(i + (j*maxPartX)).makeFixed();
                    }
                }

                if (j > 0) // thread multple chains together along Y axis
                {
                    for (int i = 0; i < maxPartX; i++)
                    {
                        Particle a = ParticleSystem.getParticle(i + (j * maxPartX));
                        Particle b = ParticleSystem.getParticle(i + ((j - 1 )*maxPartX));

                        ParticleSystem.makeSpring(a, b, springRestLength, springConstant, springDampening);
                    }
                }
            }
        }

        void CreateChainWithOneFixedEnd(ReferencePoint pt1, int numX, double springDampening, double springRestLength, double springConstant, double mass)
        {

            Particle p;

            XYZ partXYZ1 = pt1.Position;
            Particle fixedPart1 = ParticleSystem.makeParticleFromElementID(pt1.Id, mass, pt1.Position, true); // true means 'make fixed'

            XYZ partXYZ2 = partXYZ1 + new XYZ(10, 0, 0);
            //Line tempLine = this.UIDocument.Application.Application.Create.NewLineBound(partXYZ1, partXYZ2);
            XYZ vector = partXYZ2 - partXYZ1;
            XYZ step = vector.Divide(numX);

            for (int j = 0; j < numX; j++)//step along curve and evaluate at each step, making sure to thread in the existing fixed parts
            {
                //double curveParam = 0;
                XYZ pointOnLine;

                pointOnLine = partXYZ1 + step.Multiply(j);

                if (j == 0) // starting point
                {
                    //curveParam = (double)j / numX;
                    //pointOnLine = tempLine.Evaluate(curveParam, true);
                    p = ParticleSystem.makeParticle(mass, pointOnLine, true); // make first particle fixed
                }
                else // middle points
                {
                    //curveParam = (double)j / numX;
                    //pointOnLine = tempLine.Evaluate(curveParam, true);
                    p = ParticleSystem.makeParticle(mass, pointOnLine, false); // make a new particle along curve at j-th point on line
                    ParticleSystem.makeSpring(ParticleSystem.getParticle((j - 1)), p, springRestLength, springConstant, springDampening);//make a new spring and connect it to the last-made point
                }
            }

        }

        void CreateChainWithTwoFixedEnds(ReferencePoint pt1, ReferencePoint pt2, int numX, double springDampening, double springRestLength, double springConstant, double mass)
        {
            Particle p;
            Particle p2;
            XYZ partXYZ1 = pt1.Position;
            XYZ partXYZ2 = pt2.Position;
            XYZ lineVec = partXYZ2 - partXYZ1;

            double stepSize = lineVec.GetLength() / numX;

            for (int j = 0; j < numX; j++)//step along curve and evaluate at each step, making sure to thread in the existing fixed parts
            {
                //double curveParam = 0;
                XYZ pointOnLine;

                if (j == 0) // starting point
                {
                    p = ParticleSystem.makeParticle(mass, partXYZ1, true); // make first particle fixed
                }
                else if(j > 0 && j < numX-1) // middle points
                {
                    pointOnLine = partXYZ1 + lineVec.Normalize() * (j * stepSize);
                    p = ParticleSystem.makeParticle(mass, pointOnLine, false); // make a new particle along curve at j-th point on line
                    p2 = ParticleSystem.getParticle(j - 1);
                    ParticleSystem.makeSpring(p, p2, springRestLength, springConstant, springDampening);//make a new spring and connect it to the last-made point
                }
                else //last point - fixed
                {
                    p = ParticleSystem.getParticle(j - 1);
                    p2 = ParticleSystem.makeParticle(mass, partXYZ2, true); // make last particle fixed
                    ParticleSystem.makeSpring(p, p2, springRestLength, springConstant, springDampening);//make a new spring and connect the j-th point to the fixed point
                }
            }

        }

        private void CreateSpringsFromCurves(IEnumerable<Value> curves, IEnumerable<Value> points)
        {
            //create all the fixed points first
            foreach (var pt in points)
            {
                var xyz = (XYZ) ((Value.Container) pt).Item;
                ParticleSystem.makeParticle(_m, xyz, true);
            }

            //create all the springs, checking for existing particles
            foreach (var crv in curves)
            {
                var curve = (Curve) ((Value.Container) crv).Item;
                XYZ start = curve.get_EndPoint(0);
                XYZ end = curve.get_EndPoint(1);

                //find an existing particle to use
                Particle a = ParticleSystem.getParticleByXYZ(start);
                Particle b = ParticleSystem.getParticleByXYZ(end);

                //if not, create a particle
                if (a == null)
                    a = ParticleSystem.makeParticle(_m, start, false);
                if (b == null)
                    b = ParticleSystem.makeParticle(_m, end, false);

                if (_useRl)
                    ParticleSystem.makeSpring(a, b, _r, _s, _d);
                else
                {
                    double restLength = start.DistanceTo(end)*_rlf;
                    ParticleSystem.makeSpring(a, b, restLength, _s, _d);
                }
                    
            }

        }

        public override void Evaluate(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            _points = ((Value.List)args[0]).Item;//point list
            _curves = ((Value.List)args[1]).Item;//spring list
            _d = ((Value.Number)args[2]).Item;//dampening
            _s = ((Value.Number)args[3]).Item;//spring constant
            _r = ((Value.Number)args[4]).Item;//rest length
            _useRl = Convert.ToBoolean(((Value.Number)args[5]).Item);//use rest length
            _rlf = ((Value.Number)args[6]).Item;//rest length factor
            _m = ((Value.Number)args[7]).Item;//nodal mass
            _g = ((Value.Number)args[8]).Item;//gravity z component
            _threshold = ((Value.Number) args[9]).Item; //convergence threshold

            //if we are in the evaluate, this has been
            //marked dirty and we should set it to unconverged
            //in case one of the inputs has changed.
            ParticleSystem.setConverged(false);
            ParticleSystem.setGravity(_g);
            ParticleSystem.setThreshold(_threshold);

            //if the particle system has a different layout, then
            //clear it instead of updating
            if(ParticleSystem.numberOfParticles() == 0 ||
                _fixPtCount != _points.Count() ||
                _curves.Count() != ParticleSystem.numberOfSprings() ||
                _reset)
            {
                ResetSystem(_points, _curves);
            }
            else
            {
                UpdateSystem();
            }

            outPuts[_psPort] = Value.NewContainer(ParticleSystem);
            outPuts[_forcesPort] = Value.NewList(Utils.SequenceToFSharpList(
                ParticleSystem.Springs.Select(s => Value.NewNumber(s.getResidualForce()))));
        }

        private void UpdateSystem()
        {
            //update the spring values
            for (int j = 0; j < ParticleSystem.numberOfSprings(); j++)
            {
                ParticleSpring spring = ParticleSystem.getSpring(j);
                spring.setDamping(_d);
                if (!_useRl)
                    spring.setRestLength(_r);
                spring.setSpringConstant(_s);
            }
            for (int j = 0; j < ParticleSystem.numberOfParticles(); j++)
            {
                Particle p = ParticleSystem.getParticle(j);
                p.setMass(_m);
            }
        }

        private void ResetSystem(FSharpList<Value> points, FSharpList<Value> curves)
        {
            if (points == null || curves == null)
                return;

            //particleSystem.Clear();
            ParticleSystem = null;
            ParticleSystem = new ParticleSystem();

            _fixPtCount = points.Count();

            ParticleSystem.setConverged(false);
            ParticleSystem.setGravity(_g);
            ParticleSystem.setThreshold(_threshold);

            CreateSpringsFromCurves(curves, points);

            DispatchOnUIThread(dynSettings.Controller.RequestClearDrawables);

            _reset = false;
        }

        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            base.SetupCustomUIElements(nodeUI);
            
            var resetButt = new Button
            {
                Content = "Reset",
                ToolTip = "Resets the system to its initial state.",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top
            };

            resetButt.Click += delegate
                {
                    _reset = true;
                    RequiresRecalc = true;
                };

            nodeUI.inputGrid.Children.Add(resetButt);
        }
    }

    [NodeName("Create Particle System on Face")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_STRUCTURE)]
    [NodeDescription("A node which allows you to drive the position of elmenets via a particle system.")]
    class DynamicRelaxationOnFace : ParticleSystemBase
    {
        public DynamicRelaxationOnFace()
        {
            InPortData.Add(new PortData("face", "The face to use for distribution of particles.", typeof(Value.Container)));
            InPortData.Add(new PortData("d", "Dampening.", typeof(Value.Number)));
            InPortData.Add(new PortData("s", "Spring Constant.", typeof(Value.Number)));
            InPortData.Add(new PortData("r", "Rest Length.", typeof(Value.Number)));
            InPortData.Add(new PortData("m", "Nodal Mass.", typeof(Value.Number)));
            InPortData.Add(new PortData("numU", "Number of Particles in U.", typeof(Value.Number)));
            InPortData.Add(new PortData("numV", "Number of Particles in V.", typeof(Value.Number)));
            InPortData.Add(new PortData("gravity", "Gravity in Z.", typeof(Value.Number)));

            OutPortData.Add(new PortData("ps", "Particle System", typeof(Value.Container)));
            RegisterAllPorts();
        }

        void setupParticleSystem(Autodesk.Revit.DB.Face f, int uDiv, int vDiv, double springDampening, double springRestLength, double springConstant, double mass)
        {
            BoundingBoxUV bbox = f.GetBoundingBox();
            double uStep = (bbox.Max.U - bbox.Min.U)/uDiv;
            double vStep = (bbox.Max.V - bbox.Min.V)/vDiv;

            for (int j = 0; j <=uDiv; j++) // Y axis is outer loop
            {
                double u = bbox.Min.U + uStep * j;

                for (int i = 0; i <= vDiv; i++) // X axis is inner loop
                {
                    double v = bbox.Min.V + vStep * i;

                    Particle a = ParticleSystem.makeParticle(mass, f.Evaluate(new UV(u, v)), false);
                    if(i > 0)
                    {   
                        ParticleSystem.makeSpring(ParticleSystem.getParticle((i - 1) + (j * (vDiv+1))), a, springRestLength, springConstant, springDampening);
                    }

                    if (j > 0)
                    {
                        Particle b = ParticleSystem.getParticle(i + ((j - 1) * (vDiv+1)));
                        ParticleSystem.makeSpring(a, b, springRestLength, springConstant, springDampening);
                    }

                    if (i == 0 || i == vDiv || j==0 || j==uDiv)
                    {
                        a.makeFixed();
                    }
                }
            }
        }

        public override void Evaluate(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            object arg0 = ((Value.Container)args[0]).Item;
            Autodesk.Revit.DB.Face f = null;
            if (arg0 is Reference)
            {
                Reference faceRef = arg0 as Reference;
                f = dynRevitSettings.Doc.Document.GetElement(faceRef.ElementId).GetGeometryObjectFromReference(faceRef) as Autodesk.Revit.DB.Face;
            }

            double d = ((Value.Number)args[1]).Item;//dampening
            double s = ((Value.Number)args[2]).Item;//spring constant
            double r = ((Value.Number)args[3]).Item;//rest length
            double m = ((Value.Number)args[4]).Item;//nodal mass
            int numX = (int)((Value.Number)args[5]).Item;//number of particles in X
            int numY = (int)((Value.Number)args[6]).Item;//number of particles in Y
            double g = ((Value.Number)args[7]).Item;//gravity z component

            ParticleSystem.setIsFaceConstrained(true);
            ParticleSystem.setConstraintFace(f);

            ParticleSystem.Clear();

            setupParticleSystem(f, numX, numY, d, r, s, m);
            ParticleSystem.setGravity(g);

            outPuts[OutPortData[0]] = Value.NewContainer(ParticleSystem);
        }
    }

    [NodeName("Dynamic Relaxation Step")]
    [NodeDescription("Performs a step in the dynamic relaxation simulation for a particle system.")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_STRUCTURE)]
    [IsInteractive(true)]
    public class DynamicRelaxationStep : NodeModel
    {
        private readonly PortData _vMaxPort = new PortData(
            "vMax", "Maximum nodal velocity.", typeof(Value.Number));

        private readonly PortData _convergedPort = new PortData(
            "converged?",
            "Has the maximum nodal velocity dropped below the threshold set for the system?",
            typeof(Value.Number));

        public DynamicRelaxationStep()
        {
            InPortData.Add(new PortData("ps", "Particle System to simulate", typeof(Value.Container)));
            InPortData.Add(new PortData("step", "Time to step.", typeof(Value.Number)));
            InPortData.Add(new PortData("interval", "An execution interval.", typeof(Value.Number)));
            
            OutPortData.Add(_vMaxPort); 
            OutPortData.Add(_convergedPort);

            RegisterAllPorts();
        }

        public override void Evaluate(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            var partSys = (ParticleSystem)((Value.Container)args[0]).Item;
            double timeStep = ((Value.Number)args[1]).Item;
            partSys.step(timeStep);//in ms

            //trigger an intermittent update on the controller
            //this is useful for when this node is used in an infinite
            //loop and you need to draw its contents

            //throttle sending visualization updates.
            //_stepCount++;
            //if (_stepCount > 10)
            //{
                dynSettings.Controller.OnRequestsRedraw(this, EventArgs.Empty);
                //_stepCount = 0;
            //}

            outPuts[_vMaxPort] = Value.NewNumber(partSys.getMaxNodalVelocity());
            outPuts[_convergedPort] = Value.NewNumber(Convert.ToInt16(partSys.getConverged()));
        }
    }

    [NodeName("XYZs from Particle System")]
    [NodeDescription("Creates XYZs from a Particle System.")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_STRUCTURE)]
    public class XyZsFromPs : NodeWithOneOutput
    {
        public XyZsFromPs()
        {
            InPortData.Add(new PortData("ps", "Particle System", typeof(Value.Container)));
            OutPortData.Add(new PortData("XYZs", "XYZs.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var particleSystem = (ParticleSystem)((Value.Container)args[0]).Item;

            return Value.NewList(
                Utils.SequenceToFSharpList(
                    particleSystem.Particles.Select(p => Value.NewContainer(p.getPosition()))));
        }
    }

    [NodeName("Curves from Particle System")]
    [NodeDescription("Creates Curves from a Particle System.")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_STRUCTURE)]
    public class CurvesFromPs : NodeWithOneOutput
    {
        public CurvesFromPs()
        {
            InPortData.Add(new PortData("ps", "Particle System", typeof(Value.Container)));
            OutPortData.Add(new PortData("curves", "geometry curves.", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var particleSystem = (ParticleSystem)((Value.Container)args[0]).Item;

            var result = FSharpList<Value>.Empty;

            //create a geometry curve from each spring
            for (int i = 0; i < particleSystem.numberOfSprings(); i++)
            {
                ParticleSpring s = particleSystem.getSpring(i);
                Particle springEnd1 = s.getOneEnd();
                Particle springEnd2 = s.getTheOtherEnd();

                XYZ springXYZ1 = springEnd1.getPosition();
                XYZ springXYZ2 = springEnd2.getPosition();
                Line springLine = dynRevitSettings.Doc.Application.Application.Create.NewLineBound(springXYZ1, springXYZ2);

                result = FSharpList<Value>.Cons(Value.NewContainer(springLine), result);
            }

            return Value.NewList(result);
        }
    }
}
