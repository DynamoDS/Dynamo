//Copyright © Autodesk, Inc. 2012. All rights reserved.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Connectors;
using Dynamo.FSchemeInterop;
using Dynamo.Revit;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
using System.Windows.Media.Media3D;

namespace Dynamo.Nodes
{

    public abstract class dynParticleSystemBase : dynNodeWithOneOutput, IDrawable
    {
        internal ParticleSystem particleSystem;

        internal dynParticleSystemBase()
        {
            dynSettings.Controller.RequestsRedraw += Controller_RequestsRedraw;
        }

        void Controller_RequestsRedraw(object sender, EventArgs e)
        {
            Draw();
        }

        public RenderDescription RenderDescription
        {
            get;
            set;
        }

        public void Draw()
        {
            if (this.RenderDescription == null)
                this.RenderDescription = new Nodes.RenderDescription();

            if (particleSystem == null)
                return;

            for (int i = 0; i < particleSystem.numberOfParticles(); i++)
            {
                Particle p = particleSystem.getParticle(i);
                XYZ pos = p.getPosition();
                if (i < this.RenderDescription.points.Count())
                {
                    this.RenderDescription.points[i] = new Point3D(pos.X, pos.Y, pos.Z);
                }
                else
                {
                    Point3D pt = new System.Windows.Media.Media3D.Point3D(pos.X, pos.Y, pos.Z);
                    this.RenderDescription.points.Add(pt);
                }
            }

            for (int i = 0; i < particleSystem.numberOfSprings(); i++)
            {
                ParticleSpring ps = particleSystem.getSpring(i);
                XYZ pos1 = ps.getOneEnd().getPosition();
                XYZ pos2 = ps.getTheOtherEnd().getPosition();

                if (i * 2 + 1 < this.RenderDescription.lines.Count())
                {
                    this.RenderDescription.lines[i * 2] = new Point3D(pos1.X, pos1.Y, pos1.Z);
                    this.RenderDescription.lines[i * 2 + 1] = new Point3D(pos2.X, pos2.Y, pos2.Z);
                }
                else
                {
                    Point3D pt1 = new System.Windows.Media.Media3D.Point3D(pos1.X, pos1.Y, pos1.Z);
                    Point3D pt2 = new System.Windows.Media.Media3D.Point3D(pos2.X, pos2.Y, pos2.Z);

                    this.RenderDescription.lines.Add(pt1);
                    this.RenderDescription.lines.Add(pt2);
                }
            }
        }
    }

    [NodeName("Create Particle System")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_STRUCTURE)]
    [NodeDescription("A node which allows you to drive the position of elmenets via a particle system.")]
    class dynDynamicRelaxation :  dynParticleSystemBase
    {
        private double _d;
        private double _m;
        private double _s;
        private double _r;
        private double _g;
        private int _fixPtCount;
        private bool _use_rl;
        private double _rlf;

        public dynDynamicRelaxation()
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

            OutPortData.Add(new PortData("ps", "Particle System", typeof(ParticleSystem)));
            
            RegisterAllPorts();

            particleSystem = new ParticleSystem();

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
                        Particle a = particleSystem.makeParticle(mass, partXYZ, true);
                    }
                    else
                    {
                        partXYZ = new XYZ(i * stepSize, j * stepSize, 0);
                        Particle b = particleSystem.makeParticle(mass, partXYZ, false);
                        particleSystem.makeSpring(particleSystem.getParticle((i - 1)+(j*maxPartX)), b, springRestLength, springConstant, springDampening);
                    }
                    if (i == maxPartX - 1)
                    {
                        particleSystem.getParticle(i + (j*maxPartX)).makeFixed();
                    }
                }

                if (j > 0) // thread multple chains together along Y axis
                {
                    for (int i = 0; i < maxPartX; i++)
                    {
                        Particle a = particleSystem.getParticle(i + (j * maxPartX));
                        Particle b = particleSystem.getParticle(i + ((j - 1 )*maxPartX));

                        particleSystem.makeSpring(a, b, springRestLength, springConstant, springDampening);
                    }
                }
            }
        }

        void CreateChainWithOneFixedEnd(ReferencePoint pt1, int numX, double springDampening, double springRestLength, double springConstant, double mass)
        {

            Particle p;

            XYZ partXYZ1 = pt1.Position;
            Particle fixedPart1 = particleSystem.makeParticleFromElementID(pt1.Id, mass, pt1.Position, true); // true means 'make fixed'

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
                    p = particleSystem.makeParticle(mass, pointOnLine, true); // make first particle fixed
                }
                else // middle points
                {
                    //curveParam = (double)j / numX;
                    //pointOnLine = tempLine.Evaluate(curveParam, true);
                    p = particleSystem.makeParticle(mass, pointOnLine, false); // make a new particle along curve at j-th point on line
                    particleSystem.makeSpring(particleSystem.getParticle((j - 1)), p, springRestLength, springConstant, springDampening);//make a new spring and connect it to the last-made point
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
                    p = particleSystem.makeParticle(mass, partXYZ1, true); // make first particle fixed
                }
                else if(j > 0 && j < numX-1) // middle points
                {
                    pointOnLine = partXYZ1 + lineVec.Normalize() * (j * stepSize);
                    p = particleSystem.makeParticle(mass, pointOnLine, false); // make a new particle along curve at j-th point on line
                    p2 = particleSystem.getParticle(j - 1);
                    particleSystem.makeSpring(p, p2, springRestLength, springConstant, springDampening);//make a new spring and connect it to the last-made point
                }
                else //last point - fixed
                {
                    p = particleSystem.getParticle(j - 1);
                    p2 = particleSystem.makeParticle(mass, partXYZ2, true); // make last particle fixed
                    particleSystem.makeSpring(p, p2, springRestLength, springConstant, springDampening);//make a new spring and connect the j-th point to the fixed point
                }
            }

        }

        private void CreateSpringsFromCurves(IEnumerable<Value> curves, IEnumerable<Value> points)
        {
            //create all the fixed points first
            foreach (var pt in points)
            {
                var xyz = (XYZ) ((Value.Container) pt).Item;
                particleSystem.makeParticle(_m, xyz, true);
            }

            //create all the springs, checking for existing particles
            foreach (var crv in curves)
            {
                var curve = (Curve) ((Value.Container) crv).Item;
                XYZ start = curve.get_EndPoint(0);
                XYZ end = curve.get_EndPoint(1);

                //find an existing particle to use
                Particle a = particleSystem.getParticleByXYZ(start);
                Particle b = particleSystem.getParticleByXYZ(end);

                //if not, create a particle
                if (a == null)
                    a = particleSystem.makeParticle(_m, start, false);
                if (b == null)
                    b = particleSystem.makeParticle(_m, end, false);

                if (_use_rl)
                    particleSystem.makeSpring(a, b, _r, _s, _d);
                else
                {
                    double restLength = start.DistanceTo(end)*_rlf;
                    particleSystem.makeSpring(a, b, restLength, _s, _d);
                }
                    
            }

        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var points = ((Value.List)args[0]).Item;//point list
            var curves = ((Value.List)args[1]).Item;//spring list
            _d = ((Value.Number)args[2]).Item;//dampening
            _s = ((Value.Number)args[3]).Item;//spring constant
            _r = ((Value.Number)args[4]).Item;//rest length
            _use_rl = Convert.ToBoolean(((Value.Number)args[5]).Item);//use rest length
            _rlf = ((Value.Number)args[6]).Item;//rest length factor
            _m = ((Value.Number)args[7]).Item;//nodal mass
            _g = ((Value.Number)args[8]).Item;//gravity z component

            //if the particle system has a different layout, then
            //clear it instead of updating
            bool reset = false;
            if(particleSystem.numberOfParticles() == 0 ||
                _fixPtCount != points.Count() ||
                curves.Count() != particleSystem.numberOfSprings())
            {
                reset = true;
                particleSystem.Clear();
                if (dynSettings.Controller.UIDispatcher != null)
                {
                    dynSettings.Controller.UIDispatcher.Invoke(new Action(()=> RenderDescription.ClearAll()));
                }
                _fixPtCount = points.Count();
            }

            particleSystem.setGravity(_g);

            if (reset)
            {
                CreateSpringsFromCurves(curves, points);
            }
            else
            {
                //update the spring values
                for (int j = 0; j < particleSystem.numberOfSprings(); j++)
                {
                    ParticleSpring spring = particleSystem.getSpring(j);
                    spring.setDamping(_d);
                    if(!_use_rl)
                        spring.setRestLength(_r);
                    spring.setSpringConstant(_s);
                }
                for (int j = 0; j < particleSystem.numberOfParticles(); j++)
                {
                    Particle p = particleSystem.getParticle(j);
                    p.setMass(_m);
                }
            }

            return Value.NewContainer(particleSystem);

        }

    }

    [NodeName("Create Particle System on Face")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_STRUCTURE)]
    [NodeDescription("A node which allows you to drive the position of elmenets via a particle system.")]
    class dynDynamicRelaxationOnFace : dynParticleSystemBase
    {
        public dynDynamicRelaxationOnFace()
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

            particleSystem = new ParticleSystem();
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

                    Particle a = particleSystem.makeParticle(mass, f.Evaluate(new UV(u, v)), false);
                    if(i > 0)
                    {   
                        particleSystem.makeSpring(particleSystem.getParticle((i - 1) + (j * (vDiv+1))), a, springRestLength, springConstant, springDampening);
                    }

                    if (j > 0)
                    {
                        Particle b = particleSystem.getParticle(i + ((j - 1) * (vDiv+1)));
                        particleSystem.makeSpring(a, b, springRestLength, springConstant, springDampening);
                    }

                    if (i == 0 || i == vDiv || j==0 || j==uDiv)
                    {
                        a.makeFixed();
                    }
                }
            }
        }

        public override Value Evaluate(FSharpList<Value> args)
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

            particleSystem.setIsFaceConstrained(true);
            particleSystem.setConstraintFace(f);

            particleSystem.Clear();

            setupParticleSystem(f, numX, numY, d, r, s, m);
            particleSystem.setGravity(g);

            return Value.NewContainer(particleSystem);
        }
    }

    [NodeName("Dynamic Relaxation Step")]
    [NodeDescription("Performs a step in the dynamic relaxation simulation for a particle system.")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_STRUCTURE)]
    [IsInteractive(true)]
    public class dynDynamicRelaxationStep: dynNodeWithOneOutput
    {
        private ParticleSystem particleSystem;

        public dynDynamicRelaxationStep()
        {
            InPortData.Add(new PortData("ps", "Particle System to simulate", typeof(Value.Container)));
            InPortData.Add(new PortData("step", "Time to step.", typeof(Value.Number)));
            InPortData.Add(new PortData("interval", "An execution interval.", typeof(Value.Number)));
            OutPortData.Add(new PortData("data", "Relaxation data.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var particleSystem = (ParticleSystem)((Value.Container)args[0]).Item;
            double timeStep = ((Value.Number)args[1]).Item;
            particleSystem.step(timeStep);//in ms

            //trigger an intermittent update on the controller
            //this is useful for when this node is used in an infinite
            //loop and you need to draw its contents
            dynSettings.Controller.OnRequestsRedraw(this, EventArgs.Empty);

            return Value.NewList(Utils.MakeFSharpList<Value>(
                new Value[]{Value.NewContainer(particleSystem),Value.NewNumber(particleSystem.getMaxResidualForce())})
                );
        }

    }

    [NodeName("XYZs from Particle System")]
    [NodeDescription("Creates XYZs from a Particle System.")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_STRUCTURE)]
    public class dynXYZsFromPS: dynNodeWithOneOutput
    {
        public dynXYZsFromPS()
        {
            InPortData.Add(new PortData("ps", "Particle System", typeof(Value.Container)));
            OutPortData.Add(new PortData("XYZs", "XYZs.", typeof(Value.Container)));

            RegisterAllPorts();
        }
        public override Value Evaluate(FSharpList<Value> args)
        {

            ParticleSystem particleSystem = (ParticleSystem)((Value.Container)args[0]).Item;

            var result = FSharpList<Value>.Empty;

            Particle p;
            XYZ pt;

            //create an XYZ from each Particle
            for (int i = 0; i < particleSystem.numberOfParticles(); i++)
            {
                p = particleSystem.getParticle(i);
                pt = new XYZ(p.getPosition().X, p.getPosition().Y, p.getPosition().Z);
                result = FSharpList<Value>.Cons(Value.NewContainer(pt), result);
            }

            return Value.NewList(result);
        }
    }

    [NodeName("Curves from Particle System")]
    [NodeDescription("Creates Curves from a Particle System.")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_STRUCTURE)]
    public class dynCurvesFromPS: dynNodeWithOneOutput
    {
        public dynCurvesFromPS()
        {
            InPortData.Add(new PortData("ps", "Particle System", typeof(Value.Container)));
            OutPortData.Add(new PortData("curves", "geometry curves.", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {

            ParticleSystem particleSystem = (ParticleSystem)((Value.Container)args[0]).Item;

            var result = FSharpList<Value>.Empty;

            ParticleSpring s;
            Particle springEnd1;
            Particle springEnd2;
            XYZ springXYZ1;
            XYZ springXYZ2;
            Line springLine;

            //create a geometry curve from each spring
            for (int i = 0; i < particleSystem.numberOfSprings(); i++)
            {
                s = particleSystem.getSpring(i);
                springEnd1 = s.getOneEnd();
                springEnd2 = s.getTheOtherEnd();

                springXYZ1 = springEnd1.getPosition();
                springXYZ2 = springEnd2.getPosition();
                springLine = dynRevitSettings.Doc.Application.Application.Create.NewLineBound(springXYZ1, springXYZ2);

                result = FSharpList<Value>.Cons(Value.NewContainer(springLine), result);
            }

            return Value.NewList(result);
        }
    }
}
