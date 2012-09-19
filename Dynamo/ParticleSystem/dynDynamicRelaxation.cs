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
using Microsoft.FSharp.Collections;
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

            int max = 10;

            XYZ basePoint = new XYZ(0, 0, 0);
            Particle a = particleSystem.makeParticle(0.5, basePoint, true);
            a.makeFixed();

            List<Particle> particles = new List<Particle>();
            particles.Add(a);

            for (int i = 1; i < max ; i++)
            {
                Particle s = particles[i - 1];

                //XYZ pendPoint = new XYZ(0, ((double)i) * Math.Sin(0.5), ((double)i) * Math.Sin(0.5));
                XYZ pendPoint = new XYZ((double)100*i, 0, 0); // straight line in x axis
                Particle b = particleSystem.makeParticle(0.5, pendPoint, false);

                if (i == max - 1)
                {
                    b.makeFixed();
                }

                particles.Add(b);


                particleSystem.makeSpring(s, b, 1, 800, 0.8); // dynParticleSpring(dynParticle particleA, dynParticle particleB, double restLength, double springConstant, double damping)
            
            
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
            var points = args[0];//points
            var curves = args[1];//curves
            double timer = ((Expression.Number)args[2]).Item;//timer in ms
            double ms = timer / 1000;
            double d = ((Expression.Number)args[3]).Item;//dampening
            double s = ((Expression.Number)args[4]).Item;//spring constant
            double r = ((Expression.Number)args[5]).Item;//rest length

            particleSystem.Clear();
            
            CurveByPoints existingCurve;
            
            ReferencePointArray refPtArr = new ReferencePointArray();
            ReferencePointArray tempRefPtArr = new ReferencePointArray();

            setupLineTest();

            //if (points.IsList)
            //{
            //    var pointsList = (points as Expression.List).Item;

            //    int count = 0;

            //    //We create our output by...
            //    var resultPoints = Utils.convertSequence(
            //       pointsList.Select(
            //        //..taking each element in the list and...
            //          delegate(Expression x)
            //          {
            //              ReferencePoint p = ((ReferencePoint)((Expression.Container)x).Item);

            //              refPtArr.Append(p);

            //              Particle partA;
            //              partA = particleSystem.makeParticleFromElementID(p.Id, .5, p.Position, false);
            //              //partA = particleSystem.getParticleByXYZ(p.Position);

            //              if (partA != null)
            //              {
                              
            //                  string name = p.Name;
            //                  //if (name.Contains("Fixed"))
            //                  //{
            //                  //    partA.makeFixed();
            //                  //}

            //                  p.Name = count.ToString(); // mark the point
            //              }

            //              count++;
            //              return Expression.NewContainer(p);
            //          }

            //        )
            //    );


            //}


            //for (int i = 0; i < refPtArr.Size; i++)
            //{
            //    if (i == 0 || i == refPtArr.Size-1) // set the first and the last to fixed (test that will only work for linear things now)
            //    {
            //        ReferencePoint p = refPtArr.get_Item(i);
            //        p.CoordinatePlaneVisibility = (CoordinatePlaneVisibility)2;
            //        Particle part = particleSystem.getParticleByElementID(p.Id);
            //        if (part != null)
            //        {
            //            part.makeFixed();
            //        }
            //    }

            //}

            ////process curve inputs and convert to dynParticleSprings and dynParticles in particlesystem.
            ////Note this now will NOT make any user visible elements, just populate the particlesystem

            //if (curves.IsList)
            //{
            //    var curvesList = (curves as Expression.List).Item;

            //    //We create our output by...
            //    var resultCurves = Utils.convertSequence(
            //       curvesList.Select(
            //        //..taking each element in the list and...
            //          delegate(Expression x)
            //          {

            //              // check each point.position xyz extracted from cbp against all other particles position
            //              // if there is a match use existing.

            //              tempRefPtArr.Clear();
            //              existingCurve = ((CurveByPoints)((Expression.Container)x).Item);
            //              tempRefPtArr = existingCurve.GetPoints();

            //              try
            //              {


            //                  ReferencePoint oldRefPointA = tempRefPtArr.get_Item(0);
            //                  ReferencePoint oldRefPointB = tempRefPtArr.get_Item(1);

            //                  Particle partA;
            //                  Particle partB;

            //                  partA = particleSystem.makeParticleFromXYZ(oldRefPointA.Id, .5, oldRefPointA.Position, false);
            //                  partB = particleSystem.makeParticleFromXYZ(oldRefPointB.Id, .5, oldRefPointB.Position, false);

            //                  //if (partA == null)
            //                  //{
            //                  //    partA = particleSystem.makeParticleFromElementID(tempRefPtArr.get_Item(0).Id, .5, tempRefPtArr.get_Item(0).Position, false);
            //                  //}

            //                  //if (partB == null)
            //                  //{
            //                  //    partB = particleSystem.makeParticleFromElementID(tempRefPtArr.get_Item(1).Id, .5, tempRefPtArr.get_Item(0).Position, false);

            //                  //}
            //                  particleSystem.makeSpringFromElementID(existingCurve.Id, partA, partB, r, s, d);


            //              }
            //              catch (Exception ex)
            //              {
            //              }

            //              return Expression.NewContainer(existingCurve);
            //          }
            //       )
            //    );
            //}

            


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
            OutPortData = new PortData("", "Success?", typeof(bool));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            ParticleSystem particleSystem = (ParticleSystem)((Expression.Container)args[0]).Item;
            double timeStep = ((Expression.Number)args[1]).Item;

            particleSystem.step(timeStep);

            Particle p;
            ParticleSpring s;
            ReferencePointArray refPtArr = new ReferencePointArray();
            ReferencePointArray tempRefPtArr = new ReferencePointArray();

            //temporarily create ref points and curves for visualization

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

            ////update lines - purely destructive for now

            //if (particleSystem.numberOfParticles() == refPtArr.Size)
            //{
            //    for (int i = 0; i < particleSystem.numberOfSprings(); i++) // make lines between points
            //    {

            //        s = particleSystem.getSpring(i);
            //        //s.setDamping(d);
            //        //s.setRestLength(r);
            //        //s.setSpringConstant(s);

            //        tempRefPtArr.Clear();
            //        ReferencePoint end1 = this.UIDocument.Document.FamilyCreate.NewReferencePoint(s.getOneEnd().getPosition());
            //        this.Elements.Add(end1.Id);
            //        ReferencePoint end2 = this.UIDocument.Document.FamilyCreate.NewReferencePoint(s.getTheOtherEnd().getPosition());
            //        this.Elements.Add(end2.Id);
            //        if (end1 != null && end2 != null)
            //        {
            //            tempRefPtArr.Append(end1);

            //            tempRefPtArr.Append(end2);

            //           CurveByPoints c = this.UIDocument.Document.FamilyCreate.NewCurveByPoints(tempRefPtArr); // update curve position based on current particle position.
            //           this.Elements.Add(c.Id);
            //        }
            //    }

            //}

            return Expression.NewNumber(1);
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
