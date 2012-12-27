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
    [RequiresTransaction(true)]
    class dynDynamicRelaxation : dynNode
    {
        ParticleSystem particleSystem;
        Dictionary<ElementId, ElementId> pointDictionary;
        List<ReferencePoint> rPoints;

        public dynDynamicRelaxation()
        {
            InPortData.Add(new PortData("points", "The point to drive.", typeof(ReferencePoint)));
            InPortData.Add(new PortData("curves", "The curves to make into springs", typeof(IList<CurveByPoints>)));
            InPortData.Add(new PortData("tim", "Timer results to trigger updates.", typeof(double)));
            InPortData.Add(new PortData("d", "Dampening.", typeof(double)));
            InPortData.Add(new PortData("s", "Spring Constant.", typeof(double)));
            InPortData.Add(new PortData("r", "Rest Length.", typeof(double)));

            OutPortData = new PortData("ps", "Particle System", typeof(ParticleSystem));
            base.RegisterInputsAndOutputs();

            particleSystem = new ParticleSystem();

            // need a dictionary to hold reference between old points and new to handle spring mapping and mutation
            // curve input is curve by point which references two original ref points. in order to make a new curve by point 
            // from the old i need to know which two new points correspond to these two old points.
            pointDictionary = new Dictionary<ElementId, ElementId>();


            rPoints = new List<ReferencePoint>();


            
        }

        void setupLineTest()
        {

            double maxLength = 200;
            int maxPart = 20;
            double stepSize = maxLength / maxPart;
            double mass = 1;
            double springDampening = 1;
            double springRestLength = stepSize / 1;
            double springConstant = 2500;

            for (int i = 0; i < maxPart; i++)
            {
                if (i == 0)
                {
                    Particle a = particleSystem.makeParticle(mass, new XYZ(0, 0, 0), true);
                }
                else
                {
                    Particle b = particleSystem.makeParticle(mass, new XYZ(i * stepSize, 0, 0), false);
                    particleSystem.makeSpring(particleSystem.getParticle(i - 1), b, springRestLength, springConstant, springDampening);
                }
                if (i == maxPart - 1)
                {
                    particleSystem.getParticle(i).makeFixed();
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

        public override bool IsDirty { get { return true; } set { } } 

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
            double timer = ((Expression.Number)args[2]).Item;//timer in ms
            double ms = timer / 1000;
            //double d = ((Expression.Number)args[3]).Item;//dampening
            //double s = ((Expression.Number)args[4]).Item;//spring constant
            //double r = ((Expression.Number)args[5]).Item;//rest length

            particleSystem.Clear();
            
            //ReferencePointArray refPtArr = new ReferencePointArray();
            //ReferencePointArray tempRefPtArr = new ReferencePointArray();

            setupLineTest();


            return Expression.NewContainer(particleSystem);
        }
    }

    [ElementName("Dynamic Relaxation Step")]
    [ElementDescription("Performs a step in the dynamic relaxation simulation for a particle system.")]
    [ElementCategory(BuiltinElementCategories.MISC)]
    [RequiresTransaction(true)]
    public class dynDynamicRelaxationStep : dynNode
    {
        public dynDynamicRelaxationStep()
        {
            InPortData.Add(new PortData("ps", "Particle System to simulate", typeof(ParticleSystem)));
            InPortData.Add(new PortData("step", "Time to step.", typeof(double)));
            OutPortData = new PortData("geom", "Relaxation data.", typeof(object));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            ParticleSystem particleSystem = (ParticleSystem)((Expression.Container)args[0]).Item;
            double timeStep = ((Expression.Number)args[1]).Item;
            var result = FSharpList<Expression>.Empty;

            particleSystem.step(timeStep);

            Particle p;
            ParticleSpring s;
            XYZ pt;

            Particle springEnd1;
            Particle springEnd2;
            XYZ springXYZ1;
            XYZ springXYZ2;
            Line springLine;

            //draw points as XYZs
            for (int i = 0; i < particleSystem.numberOfParticles(); i++)
            {
                p = particleSystem.getParticle(i);
                pt = new XYZ(p.getPosition().X, p.getPosition().Y, p.getPosition().Z);
                //result = FSharpList<Expression>.Cons(Expression.NewContainer(pt), result);
            }

            //draw curves as geometry curves
            for (int i = 0; i < particleSystem.numberOfSprings(); i++)
            {
                s = particleSystem.getSpring(i);
                springEnd1 = s.getOneEnd();
                springEnd2 = s.getTheOtherEnd();

                springXYZ1 = new XYZ(springEnd1.getPosition().X, springEnd1.getPosition().Y, springEnd1.getPosition().Z);
                springXYZ2 = new XYZ(springEnd2.getPosition().X, springEnd2.getPosition().Y, springEnd2.getPosition().Z);
                springLine = this.UIDocument.Application.Application.Create.NewLineBound(springXYZ1, springXYZ2);

                result = FSharpList<Expression>.Cons(Expression.NewContainer(springLine), result);
            }

            return Expression.NewList(result);
        }
    }



    //[ElementName("Timer")]
    //[ElementCategory(BuiltinElementCategories.MISC)]
    //[ElementDescription("A node which represents a stopwatch.")]
    //[RequiresTransaction(false)]
    //public class dynTimer : dynNode
    //{
    //    Stopwatch sw;
    //    bool timing = false;
    //    System.Timers.Timer timer; 

    //    public dynTimer()
    //    {
    //        InPortData.Add(new PortData("n", "How often to receive updates in milliseconds.", typeof(double)));
    //        InPortData.Add(new PortData("i/o", "Turn the timer on or off", typeof(dynBool)));
    //        OutPortData = new PortData("tim", "The timer, counting in milliseconds.", typeof(int));

    //        base.RegisterInputsAndOutputs();

    //        sw = new Stopwatch();
    //        //timer = new System.Timers.Timer();


    //    }

    //    void StartTimer(int interval)
    //    {

    //        timer = new System.Timers.Timer(interval); 

    //        timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
    //        timer.Enabled = true; // Enable it
    //    }

    //    static void _timer_Elapsed(object sender, ElapsedEventArgs e)
    //    {
    //       //figure out how to trigger Evaluate of the Timer node
    //       // Dynamo.Utilities.dynElementSettings.SharedInstance.Workbench
    //    }

    //    int KeepTime(int interval)
    //    {

    //        if (sw.ElapsedMilliseconds > interval)
    //        {
    //            sw.Stop();
    //            sw.Reset();
    //            sw.Start();
    //            return interval;

    //        }
    //        else
    //        {
    //            return 0;
    //        }

    //        //return (int)sw.ElapsedMilliseconds;

    //    }

    //    public override Expression Evaluate(FSharpList<Expression> args)
    //    {
            
    //        double result = 0;
    //        int interval = Convert.ToInt16(((Expression.Number)args[0]).Item);


    //        bool isTiming = Convert.ToBoolean(((Expression.Number)args[1]).Item);


    //        if (timing)
    //        {
    //            if (!isTiming)  //if you are timing and we turn off the timer
    //            {
    //                timing = false; //stop
    //                sw.Stop();
    //                sw.Reset();
    //            }
    //        }
    //        else
    //        {
    //            if (isTiming)
    //            {
    //                timing = true;  //if you are not timing and we turn on the timer
    //                sw.Start();
    //                StartTimer(interval);
    //                while (timing)
    //                {
    //                    result = KeepTime(interval);
    //                    break;
    //                }
    //            }
    //        }

    //        IsDirty = true;// stephen suggesting hitting this with a larger hammer. 
    //        return Expression.NewNumber(result);
    //    }
    //}

}
