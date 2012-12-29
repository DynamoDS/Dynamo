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
    [ElementDescription("A node which allows you to drive the position of elmenets via a particle system .")]
    [RequiresTransaction(false)]
    class dynDynamicRelaxation : dynNode
    {
        ParticleSystem particleSystem;

        public dynDynamicRelaxation()
        {
            InPortData.Add(new PortData("points", "The point to drive.", typeof(object)));
            InPortData.Add(new PortData("curves", "The curves to make into springs", typeof(object)));
            //InPortData.Add(new PortData("tim", "Timer results to trigger updates.", typeof(double)));
            InPortData.Add(new PortData("d", "Dampening.", typeof(double)));
            InPortData.Add(new PortData("s", "Spring Constant.", typeof(double)));
            InPortData.Add(new PortData("r", "Rest Length.", typeof(double)));
            InPortData.Add(new PortData("m", "Nodal Mass.", typeof(double)));
            InPortData.Add(new PortData("numX", "Number of Particles in X.", typeof(int)));
            InPortData.Add(new PortData("numY", "Number of Particles in Y.", typeof(int)));
            InPortData.Add(new PortData("Gravity", "Gravity in Z.", typeof(double)));

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

            setupLineTest(numX, numY, d, r, s, m);
            particleSystem.setGravity(g);

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
            var result = FSharpList<Expression>.Empty;

            particleSystem.step(timeStep);//in ms

            return Expression.NewContainer(particleSystem);
        }
    }

}
