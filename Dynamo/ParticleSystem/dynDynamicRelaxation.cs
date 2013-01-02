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
using Expression = Dynamo.FScheme.Expression;
using Autodesk.Revit.DB;
using System.Timers;
using System.Threading;


namespace Dynamo.Elements
{
    [ElementName("Create Particle System")]
    [ElementCategory(BuiltinElementCategories.MISC)]
    [ElementDescription("A node which allows you to drive the position of elmenets via a particle system.")]
    [RequiresTransaction(false)]
    class dynDynamicRelaxation : dynNode
    {
        ParticleSystem particleSystem;
        private int extraParticlesCounter = 0;
        private int extraSpringCounter = 0;

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

            OutPortData = new PortData("ps", "Particle System", typeof(ParticleSystem));
            base.RegisterInputsAndOutputs();

            particleSystem = new ParticleSystem();

        }


        void setupLineTest(int maxPartX, int maxPartY, double springDampening, double springRestLength, double springConstant, double mass)
        {

            //double maxLength = 200;
            double stepSize = 20;
            //int maxPart = 20;
            //double stepSize = maxLength / maxPart;
            //double mass = 1;
            //double springDampening = 1;
            //double springRestLength = stepSize / 1;
            //double springConstant = 2500;

            

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

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            //var points = args[0];//points
            //var curves = args[1];//curves
            double d = ((Expression.Number)args[2]).Item;//dampening
            double s = ((Expression.Number)args[3]).Item;//spring constant
            double r = ((Expression.Number)args[4]).Item;//rest length
            double m = ((Expression.Number)args[5]).Item;//nodal mass
            int numX = (int)((Expression.Number)args[6]).Item;//number of particles in X
            int numY = (int)((Expression.Number)args[7]).Item;//number of particles in Y
            double g = ((Expression.Number)args[8]).Item;//gravity z component


            particleSystem.Clear();
            particleSystem.setGravity(g);
            ReferencePoint pt1;
            ReferencePoint pt2;
            Particle fixedPart1;
            Particle fixedPart2;
            XYZ partXYZ1;
            XYZ partXYZ2;
            ParticleSpring sp;
            Line tempLine;
            Particle p;
            Particle p2;

            


            var input = args[0];

            //If we are receiving a list, we must create fixed particles for each reference point in the list.
            if (input.IsList)
            {
                var pointList = (input as Expression.List).Item;

                // for each point in collection, make a fixed particle
                // create a geom line between subsequent pairs of points in list
                // divide/evaluate lines into maxPart number, create a Particle for each
                // create springs between each pair of particles in each chain

                ////We create our output by...
                //var result = Utils.convertSequence(
                //   pointList.Select(
                //    //..taking each element in the list and...
                //      delegate(Expression x)
                //      {
                //          XYZ fixedXYZ;

                //          pt = (ReferencePoint)(((Expression.Container)x).Item);
                //          fixedXYZ = pt.Position;
                //          fixedPart1 = particleSystem.makeParticleFromElementID(pt.Id, m, pt.Position, true); // true means 'make fixed'
                //          return Expression.NewContainer(fixedXYZ);
                //      }
                //   )
                //);






                if (pointList.Count() > 1)
                {
                    Array pointArray = pointList.ToArray();
                    //foreach (var elem in pointList)

                    for (int i = 0; i < pointArray.Length-1; i++)
                    {
                        
                        //create temp  geomlines 
                        //var pair = pointList.Take(2);
                        //pt1 = (ReferencePoint)((Expression.Container)pair.ElementAt(0)).Item as ReferencePoint;
                        //partXYZ1 = pt1.Position;
                        //pt2 = (ReferencePoint)((Expression.Container)pair.ElementAt(1)).Item as ReferencePoint;
                        //partXYZ2 = pt2.Position;

                        pt1 = (ReferencePoint)((Expression.Container)pointArray.GetValue(i)).Item as ReferencePoint;
                        partXYZ1 = pt1.Position;
                        pt2 = (ReferencePoint)((Expression.Container)pointArray.GetValue(i+1)).Item as ReferencePoint;
                        partXYZ2 = pt2.Position;

                        tempLine = this.UIDocument.Application.Application.Create.NewLineBound(partXYZ1, partXYZ2);

                        //divide up geom lines into a chain
                        if (tempLine != null)
                        {

                            for (int j = 0; j < numX; j++)//step along curve and evaluate at each step, making sure to thread in the existing fixed parts
                            {
                                double curveParam = 0;
                                XYZ pointOnLine;

                                if (j == 0) // starting point
                                {
                                    curveParam = (double)j / numX;
                                    pointOnLine = tempLine.Evaluate(curveParam, true);
                                    p = particleSystem.makeParticle(m, pointOnLine, true); // make first particle fixed
                                }
                                else // middle points
                                {
                                    curveParam = (double)j / numX;
                                    pointOnLine = tempLine.Evaluate(curveParam, true);
                                    p = particleSystem.makeParticle(m, pointOnLine, false); // make a new particle along curve at j-th point on line
                                    particleSystem.makeSpring(particleSystem.getParticle((j - 1)), p, r, s, d);//make a new spring and connect it to the last-made point
                                }
                                if (j == numX - 1) //last free point, connect with fixed end point
                                {
                                    curveParam = (double)(j+1) / numX; // last point 
                                    pointOnLine = tempLine.Evaluate(curveParam, true);
                                    p2 = particleSystem.makeParticle(m, pointOnLine, true); // make last particle fixed
                                    particleSystem.makeSpring(p, p2, r, s, d);//make a new spring and connect the j-th point to the fixed point

                                }
                            }

                        }
                    }

                }
                else // just one point
                {
                    pt1 = (ReferencePoint)((Expression.Container)pointList.ElementAt(0)).Item as ReferencePoint;
                    partXYZ1 = pt1.Position;
                    fixedPart1 = particleSystem.makeParticleFromElementID(pt1.Id, m, pt1.Position, true); // true means 'make fixed'

                    partXYZ2 = partXYZ1 + new XYZ(10, 0, 0);
                    tempLine = this.UIDocument.Application.Application.Create.NewLineBound(partXYZ1, partXYZ2);

                    for (int j = 0; j < numX - 1; j++) //step along curve and evaluate at each step
                    {
                        p = particleSystem.makeParticle(m, tempLine.Evaluate(j / numX, true), false);
                        p2 = particleSystem.makeParticle(m, tempLine.Evaluate((j + 1) / numX, true), false);
                        particleSystem.makeSpring(p, p2, r, s, d);

                        extraParticlesCounter++;
                        extraSpringCounter++;
                    }
                }


                


                //Fin
                return Expression.NewContainer(particleSystem);
            }
            //If we're not receiving a list, we will just assume we don't care about the point input and will just run our test function.
            else
            {
                setupLineTest(numX, numY, d, r, s, m);
            }
        


            return Expression.NewContainer(particleSystem);
        }
    }

    [ElementName("Dynamic Relaxation Step")]
    [ElementDescription("Performs a step in the dynamic relaxation simulation for a particle system.")]
    [ElementCategory(BuiltinElementCategories.MISC)]
    [RequiresTransaction(false)]
    public class dynDynamicRelaxationStep : dynNode
    {
        public dynDynamicRelaxationStep()
        {
            InPortData.Add(new PortData("ps", "Particle System to simulate", typeof(ParticleSystem)));
            InPortData.Add(new PortData("step", "Time to step.", typeof(double)));
            InPortData.Add(new PortData("exec", "Execution interval.", typeof(object)));
            OutPortData = new PortData("geom", "Relaxation data.", typeof(object));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            ParticleSystem particleSystem = (ParticleSystem)((Expression.Container)args[0]).Item;
            double timeStep = ((Expression.Number)args[1]).Item;
            //var result = FSharpList<Expression>.Empty;

            particleSystem.step(timeStep);//in ms

            return Expression.NewContainer(particleSystem);
        }
    }

    [ElementName("XYZs from Particle System")]
    [ElementDescription("Creates XYZs from a Particle System.")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [RequiresTransaction(false)]
    public class dynXYZsFromPS : dynNode
    {
        public dynXYZsFromPS()
        {
            InPortData.Add(new PortData("ps", "Particle System", typeof(ParticleSystem)));
            OutPortData = new PortData("XYZs", "XYZs.", typeof(object));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {

            ParticleSystem particleSystem = (ParticleSystem)((Expression.Container)args[0]).Item;

            var result = FSharpList<Expression>.Empty;

            Particle p;
            XYZ pt;

            //create an XYZ from each Particle
            for (int i = 0; i < particleSystem.numberOfParticles(); i++)
            {
                p = particleSystem.getParticle(i);
                pt = new XYZ(p.getPosition().X, p.getPosition().Y, p.getPosition().Z);
                result = FSharpList<Expression>.Cons(Expression.NewContainer(pt), result);
            }

            return Expression.NewList(result);
        }
    }

    [ElementName("Curves from Particle System")]
    [ElementDescription("Creates Curves from a Particle System.")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [RequiresTransaction(false)]
    public class dynCurvesFromPS : dynNode
    {
        public dynCurvesFromPS()
        {
            InPortData.Add(new PortData("ps", "Particle System", typeof(ParticleSystem)));
            OutPortData = new PortData("curves", "geometry curves.", typeof(object));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {

            ParticleSystem particleSystem = (ParticleSystem)((Expression.Container)args[0]).Item;

            var result = FSharpList<Expression>.Empty;

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
                springLine = this.UIDocument.Application.Application.Create.NewLineBound(springXYZ1, springXYZ2);

                result = FSharpList<Expression>.Cons(Expression.NewContainer(springLine), result);
            }

            return Expression.NewList(result);
        }
    }
}
