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
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.FSharp.Collections;
using System.IO.Ports;
using Dynamo.Connectors;
using Dynamo.Utilities;
using Dynamo.FSchemeInterop;
using Value = Dynamo.FScheme.Value;
using Autodesk.Revit.DB;
using System.Timers;
using System.Threading;
using Dynamo.Nodes;
using Dynamo.Revit;


namespace Dynamo.Nodes
{
    [NodeName("Create Particle System")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE)]
    [NodeDescription("A node which allows you to drive the position of elmenets via a particle system.")]
    class dynDynamicRelaxation : dynRevitTransactionNodeWithOneOutput
    {
        ParticleSystem particleSystem;
        //private int extraParticlesCounter = 0;
        //private int extraSpringCounter = 0;

        public dynDynamicRelaxation()
        {
            InPortData.Add(new PortData("points", "The points to use as fixed nodes.", typeof(object)));
            InPortData.Add(new PortData("curves", "The curves to make into spring chains", typeof(object)));
            InPortData.Add(new PortData("d", "Dampening.", typeof(double)));
            InPortData.Add(new PortData("s", "Spring Constant.", typeof(double)));
            InPortData.Add(new PortData("r", "Rest Length.", typeof(double)));
            InPortData.Add(new PortData("m", "Nodal Mass.", typeof(double)));
            InPortData.Add(new PortData("numX", "Number of Particles in X.", typeof(int)));
            InPortData.Add(new PortData("numY", "Number of Particles in Y.", typeof(int)));
            InPortData.Add(new PortData("gravity", "Gravity in Z.", typeof(double)));

            OutPortData.Add(new PortData("ps", "Particle System", typeof(ParticleSystem)));
            
            RegisterAllPorts();

            particleSystem = new ParticleSystem();

        }

        void setupLineTest(int maxPartX, int maxPartY, double springDampening, double springRestLength, double springConstant, double mass)
        {

            double stepSize = 20;

            for (int j = 0; j < maxPartY; j++) // Y axis is outer loop
            {
                for (int i = 0; i < maxPartX; i++) // X axis is inner loop
                {
                    if (i == 0)
                    {
                        Particle a = particleSystem.makeParticle(mass, new XYZ(0, j*stepSize, 0), true);
                    }
                    else
                    {
                        Particle b = particleSystem.makeParticle(mass, new XYZ(i * stepSize, j*stepSize, 0), false);
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
            Line tempLine = this.UIDocument.Application.Application.Create.NewLineBound(partXYZ1, partXYZ2);

            for (int j = 0; j < numX; j++)//step along curve and evaluate at each step, making sure to thread in the existing fixed parts
            {
                double curveParam = 0;
                XYZ pointOnLine;

                if (j == 0) // starting point
                {
                    curveParam = (double)j / numX;
                    pointOnLine = tempLine.Evaluate(curveParam, true);
                    p = particleSystem.makeParticle(mass, pointOnLine, true); // make first particle fixed
                }
                else // middle points
                {
                    curveParam = (double)j / numX;
                    pointOnLine = tempLine.Evaluate(curveParam, true);
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
            Particle fixedPart1 = particleSystem.makeParticleFromElementID(pt1.Id, mass, pt1.Position, true); // true means 'make fixed'

            XYZ partXYZ2 = pt2.Position;
            Line tempLine = this.UIDocument.Application.Application.Create.NewLineBound(partXYZ1, partXYZ2);

            for (int j = 0; j < numX; j++)//step along curve and evaluate at each step, making sure to thread in the existing fixed parts
            {
                double curveParam = 0;
                XYZ pointOnLine;

                if (j == 0) // starting point
                {
                    curveParam = (double)j / numX;
                    pointOnLine = tempLine.Evaluate(curveParam, true);
                    p = particleSystem.makeParticle(mass, pointOnLine, true); // make first particle fixed
                }
                else // middle points
                {
                    curveParam = (double)j / numX;
                    pointOnLine = tempLine.Evaluate(curveParam, true);
                    p = particleSystem.makeParticle(mass, pointOnLine, false); // make a new particle along curve at j-th point on line
                    particleSystem.makeSpring(particleSystem.getParticle((j - 1)), p, springRestLength, springConstant, springDampening);//make a new spring and connect it to the last-made point
                }
                if (j == numX - 1) //last free point, connect with fixed end point
                {
                    curveParam = (double)(j + 1) / numX; // last point 
                    pointOnLine = tempLine.Evaluate(curveParam, true);
                    p2 = particleSystem.makeParticle(mass, pointOnLine, true); // make last particle fixed
                    particleSystem.makeSpring(p, p2, springRestLength, springConstant, springDampening);//make a new spring and connect the j-th point to the fixed point

                }
            }

        }

        // geometric test, fairly expensive. obsoleted by CreateParticleByElementID and GetParticleFromElementID
        public ReferencePoint FindRefPointFromXYZ(XYZ xyz)         
        {
            Element el;
            ReferencePoint rp;

            foreach(ElementId id in this.Elements)
            {

                dynUtils.TryGetElement(id, out el);
                rp = el as ReferencePoint;

                if (rp != null && rp.Position.IsAlmostEqualTo(xyz))// note this is not gauranteed to be unique. there may be mulitple coincident refpoints and this utill will only return the first found
                {
                    return rp;

                }
            }

            return null;
        }

        // geometric test, fairly expensive. 
        public ReferencePoint FindRefPointWithCoincidentXYZ(ReferencePoint rp)
        {
            Element el;
            ReferencePoint rp2;

            foreach (ElementId id in this.Elements) // compare to inputed points
            {
                dynUtils.TryGetElement(id, out el);
                rp2 = el as ReferencePoint;

                if (rp != null && rp2 != null && rp.Position.IsAlmostEqualTo(rp2.Position))// note this is not gauranteed to be unique. there may be mulitple coincident refpoints and this utill will only return the first found
                {
                    return rp2; // found a match
                }
            }

            return null;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];//point list
            //var curves = args[1];//curves
            double d = ((Value.Number)args[2]).Item;//dampening
            double s = ((Value.Number)args[3]).Item;//spring constant
            double r = ((Value.Number)args[4]).Item;//rest length
            double m = ((Value.Number)args[5]).Item;//nodal mass
            int numX = (int)((Value.Number)args[6]).Item;//number of particles in X
            int numY = (int)((Value.Number)args[7]).Item;//number of particles in Y
            double g = ((Value.Number)args[8]).Item;//gravity z component


            particleSystem.Clear();
            particleSystem.setGravity(g);

            ReferencePoint pt1;
            ReferencePoint pt2;
            //Particle fixedPart1;
            //XYZ partXYZ1;
            //XYZ partXYZ2;
            //ParticleSpring sp;
            //Line tempLine;
            //Particle p;
            //Particle p2;


            //If we are receiving a list, we must create fixed particles for each reference point in the list.
            if (input.IsList)
            {
                var pointList = (input as Value.List).Item;

                if (pointList.Count() > 1)
                {
                    Array pointArray = pointList.ToArray();

                    for (int i = 0; i < pointArray.Length-1; i++)
                    {

                        pt1 = (ReferencePoint)((Value.Container)pointArray.GetValue(i)).Item as ReferencePoint;
                        pt2 = (ReferencePoint)((Value.Container)pointArray.GetValue(i+1)).Item as ReferencePoint;
                        CreateChainWithTwoFixedEnds(pt1, pt2, numX, d, r, s, m);
                    }

                }
                else // just one point (still in a list)
                {
                    pt1 = (ReferencePoint)((Value.Container)pointList.ElementAt(0)).Item as ReferencePoint;
                    CreateChainWithOneFixedEnd(pt1,numX, d, r, s, m);

                }

                return Value.NewContainer(particleSystem);
            }
            //If we're not receiving a list, we will just assume we don't care about the point input and will just run our test function.
            else
            {
                setupLineTest(numX, numY, d, r, s, m);
            }
        


            return Value.NewContainer(particleSystem);
        }
    }

    [NodeName("Create Particle System on Face")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE)]
    [NodeDescription("A node which allows you to drive the position of elmenets via a particle system.")]
    class dynDynamicRelaxationOnFace: dynNodeWithOneOutput
    {
        ParticleSystem particleSystem;

        public dynDynamicRelaxationOnFace()
        {
            InPortData.Add(new PortData("face", "The face to use for distribution of particles.", typeof(object)));
            InPortData.Add(new PortData("d", "Dampening.", typeof(double)));
            InPortData.Add(new PortData("s", "Spring Constant.", typeof(double)));
            InPortData.Add(new PortData("r", "Rest Length.", typeof(double)));
            InPortData.Add(new PortData("m", "Nodal Mass.", typeof(double)));
            InPortData.Add(new PortData("numU", "Number of Particles in U.", typeof(int)));
            InPortData.Add(new PortData("numV", "Number of Particles in V.", typeof(int)));
            InPortData.Add(new PortData("gravity", "Gravity in Z.", typeof(double)));

            OutPortData.Add(new PortData("ps", "Particle System", typeof(ParticleSystem)));
            RegisterAllPorts();

            particleSystem = new ParticleSystem();
        }

        void setupParticleSystem(Face f, int uDiv, int vDiv, double springDampening, double springRestLength, double springConstant, double mass)
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
            Face f = null;
            if (arg0 is Reference)
            {
                Reference faceRef = arg0 as Reference;
                f = dynRevitSettings.Doc.Document.GetElement(faceRef.ElementId).GetGeometryObjectFromReference(faceRef) as Face;
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
    [NodeCategory(BuiltinNodeCategories.ANALYZE)]
    public class dynDynamicRelaxationStep: dynNodeWithOneOutput
    {
        public dynDynamicRelaxationStep()
        {
            InPortData.Add(new PortData("ps", "Particle System to simulate", typeof(ParticleSystem)));
            InPortData.Add(new PortData("step", "Time to step.", typeof(double)));
            InPortData.Add(new PortData("exec", "Execution interval.", typeof(object)));
            OutPortData.Add(new PortData("geom", "Relaxation data.", typeof(object)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            ParticleSystem particleSystem = (ParticleSystem)((Value.Container)args[0]).Item;
            double timeStep = ((Value.Number)args[1]).Item;
            //var result = FSharpList<Value>.Empty;

            particleSystem.step(timeStep);//in ms

            return Value.NewContainer(particleSystem);
        }
    }

    [NodeName("XYZs from Particle System")]
    [NodeDescription("Creates XYZs from a Particle System.")]
    [NodeCategory(BuiltinNodeCategories.REVIT)]
    public class dynXYZsFromPS: dynNodeWithOneOutput
    {
        public dynXYZsFromPS()
        {
            InPortData.Add(new PortData("ps", "Particle System", typeof(ParticleSystem)));
            OutPortData.Add(new PortData("XYZs", "XYZs.", typeof(object)));

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
    [NodeCategory(BuiltinNodeCategories.REVIT)]
    public class dynCurvesFromPS: dynNodeWithOneOutput
    {
        public dynCurvesFromPS()
        {
            InPortData.Add(new PortData("ps", "Particle System", typeof(ParticleSystem)));
            OutPortData.Add(new PortData("curves", "geometry curves.", typeof(object)));

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
