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
using Expression = Dynamo.FScheme.Expression;
using Autodesk.Revit.DB;
using System.Threading;

namespace Dynamo.Elements
{
    [ElementName("Dynamic Relaxation")]
    [ElementCategory(BuiltinElementCategories.MISC)]
    [ElementDescription("A node which allows you to drive the position of elmenets via a particle system .")]
    [RequiresTransaction(true)]

    class dynDynamicRelaxation : dynNode
    {
        dynParticleSystem particleSystem;
        List<ReferencePoint> rPoints;

        public dynDynamicRelaxation()
        {
            InPortData.Add(new PortData("points", "The point to drive.", typeof(dynReferencePointByXYZ)));
            InPortData.Add(new PortData( "curves", "The curves to make into springs", typeof(dynCurveByPoints)));
            InPortData.Add(new PortData("tim", "Timer results to trigger updates.", typeof(double)));
            InPortData.Add(new PortData("d", "Dampening.", typeof(double)));
            InPortData.Add(new PortData("s", "Spring Constant.", typeof(double)));
            InPortData.Add(new PortData("r", "Rest Length.", typeof(double)));

            OutPortData = new PortData("points", "the outputed points", typeof(dynReferencePointByXYZ));
            base.RegisterInputsAndOutputs();

            particleSystem = new dynParticleSystem();

            rPoints = new List<ReferencePoint>();
            setupPendulum();

            
        }

        void setupPendulum()
        {


            XYZ basePoint = new XYZ(0, 0, 0);
            dynParticle a = particleSystem.makeParticle(0.5, basePoint, true);
            a.makeFixed();

            List<dynParticle> particles = new List<dynParticle>();
            particles.Add(a);

            for (int i = 1; i < 10; i++)
            {
                dynParticle s = particles[i - 1];

                //XYZ pendPoint = new XYZ(0, ((double)i) * Math.Sin(0.5), ((double)i) * Math.Sin(0.5));
                XYZ pendPoint = new XYZ((double)100*i, 0, 0); // straight line in x axis
                dynParticle b = particleSystem.makeParticle(0.5, pendPoint, false);


                particles.Add(b);


                particleSystem.makeSpring(s, b, 1, 800, 0.8); // dynParticleSpring(dynParticle particleA, dynParticle particleB, double restLength, double springConstant, double damping)
            }

            

        }

        public ReferencePoint FindRefPointFromXYZ(XYZ xyz)
        {
            Element el;
            ReferencePoint rp;

            foreach(ElementId id in this.Elements)
            {
                dynUtils.TryGetElement(id, out el);
                rp = el as ReferencePoint;

                if (rp != null && rp.Position.IsAlmostEqualTo(xyz))
                {
                    return rp;

                }
            }

            return null;
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var points = args[0];//points
            var curves = args[1];//curves
            double timer = ((Expression.Number)args[2]).Item;//timer in ms
            double ms = timer / 1000;
            double d = ((Expression.Number)args[3]).Item;//dampening
            double s = ((Expression.Number)args[4]).Item;//spring constant
            double r = ((Expression.Number)args[5]).Item;//rest length
            
            dynParticle p;
            dynParticleSpring a;
            dynParticleSpring b;
            ReferencePointArray refPtArr = new ReferencePointArray();
            ReferencePointArray tempRefPtArr = new ReferencePointArray();
            

            particleSystem.step(.004); //ms


            foreach (var el in this.Elements)
            {
                this.DeleteElement(el);
            }

            //update ref points - purely destructive for now

            for (int i = 0; i < particleSystem.numberOfParticles(); i++)
            {
                p = particleSystem.getParticle(i);
                ReferencePoint rp = this.UIDocument.Document.FamilyCreate.NewReferencePoint(p.getPosition()); // update ref point position based on current particle position.
                this.Elements.Add(rp.Id);
                refPtArr.Append(rp);
            }

            //update lines - purely destructive for now

            if (particleSystem.numberOfParticles() == refPtArr.Size)
            {
                for (int i = 0; i < particleSystem.numberOfSprings(); i++) // make lines between points in pendul
                {

                    a = particleSystem.getSpring(i);
                    a.setDamping(d);
                    a.setRestLength(r);
                    a.setSpringConstant(s);

                    tempRefPtArr.Clear();
                    ReferencePoint end1 = FindRefPointFromXYZ(a.getOneEnd().getPosition());
                    ReferencePoint end2 = FindRefPointFromXYZ(a.getTheOtherEnd().getPosition());
                    if (end1 != null && end2 != null)
                    {
                        tempRefPtArr.Append(end1);

                        tempRefPtArr.Append(end2);

                       CurveByPoints c = this.UIDocument.Document.FamilyCreate.NewCurveByPoints(tempRefPtArr); // update curve position based on current particle position.
                       this.Elements.Add(c.Id);
                    }
                }
                
            }


            return Expression.NewNumber(1);
        }
    }
    [ElementName("Timer")]
    [ElementCategory(BuiltinElementCategories.MISC)]
    [ElementDescription("A node which represents a stopwatch.")]
    [RequiresTransaction(false)]

    public class dynTimer : dynNode
    {
        Stopwatch sw;
        bool timing = false;

        public dynTimer()
        {
            InPortData.Add(new PortData("n", "How often to receive updates in milliseconds.", typeof(double)));
            InPortData.Add(new PortData("i/o", "Turn the timer on or off", typeof(dynBool)));
            OutPortData = new PortData("tim", "The timer, counting in milliseconds.", typeof(int));

            base.RegisterInputsAndOutputs();

            sw = new Stopwatch();


        }

        int KeepTime(int interval)
        {

            if (sw.ElapsedMilliseconds > interval)
            {
                sw.Stop();
                sw.Reset();
                sw.Start();
                return interval;

            }
            else
            {
                return 0;
            }

            //return (int)sw.ElapsedMilliseconds;

        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            bool update = false;
            double result = 0;
            int interval = Convert.ToInt16(((Expression.Number)args[0]).Item);


            bool isTiming = Convert.ToBoolean(((Expression.Number)args[1]).Item);


            if (timing)
            {
                if (!isTiming)  //if you are timing and we turn off the timer
                {
                    timing = false; //stop
                    sw.Stop();
                    sw.Reset();
                }
            }
            else
            {
                if (isTiming)
                {
                    timing = true;  //if you are not timing and we turn on the timer
                    sw.Start();
                    while (timing)
                    {
                        result = KeepTime(interval);
                        break;
                    }
                }
            }

            return Expression.NewNumber(result);
        }
    }

}
