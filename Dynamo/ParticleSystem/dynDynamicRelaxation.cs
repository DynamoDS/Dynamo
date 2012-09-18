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
    [ElementName("Dynamic Relaxation")]
    [ElementCategory(BuiltinElementCategories.MISC)]
    [ElementDescription("A node which allows you to drive the position of elmenets via a particle system .")]
    [RequiresTransaction(true)]

    class dynDynamicRelaxation : dynNode
    {
        dynParticleSystem particleSystem;
        Dictionary<ElementId, ElementId> pointDictionary;
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

            // need a dictionary to hold reference between old points and new to handle spring mapping and mutation
            // curve input is curve by point which references two original ref points. in order to make a new curve by point 
            // from the old i need to know which two new points correspond to these two old points.
            pointDictionary = new Dictionary<ElementId, ElementId>();


            rPoints = new List<ReferencePoint>();
            //setupLineTest();

            
        }

        void setupLineTest()
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

        // geomtric test, fairly expensive. obsoleted by CreateParticleByElementID and GetParticleFromElementID
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


        // geomtric test, fairly expensive. 
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
            
            dynParticle p;
            dynParticleSpring a;
            
            ReferencePointArray refPtArr = new ReferencePointArray();
            ReferencePointArray tempRefPtArr = new ReferencePointArray();


            

            //process refpoint inputs and convert to dynParticles in particlesystem. for now we will copy the ref points and only update the position of the copied points.

            //If we are receiving a list of reference points, we must create a new ref point and a dynParticle for each refpoint in the input list.


            //If we are receiving a list of reference points, we must create a new ref point and a dynParticle for each refpoint in the input list.
            //if (points.IsList)
            //{
            //    var pointList = (points as Expression.List).Item;

            //    //Counter to keep track of how many ref points we've made. We'll use this to delete old
            //    //elements later.
            //    int count = 0;

            //    //We create our output by...
            //    var resultPoints = Utils.convertSequence(
            //       pointList.Select(
            //        //..taking each element in the list and...
            //          delegate(Expression x)
            //          {
            //              ReferencePoint pt;
            //              //...if we already have elements made by this node in a previous run...
            //              if (this.Elements.Count > count)
            //              {
            //                  Element e;
            //                  //...we attempt to fetch it from the document...
            //                  if (dynUtils.TryGetElement(this.Elements[count], out e))
            //                  {
            //                      //...and if we're successful, update it's properties as needed

            //                      pt = e as ReferencePoint;
                                  
            //                      // - find the matching dynParticle (just updated dynParticle to keep it's refpoint id)
            //                      // - the rest is a no-op beause the dynParticle will update this refpoints position after the next step.
            //                      dynParticle part = particleSystem.getParticleByElementID(e.Id);


            //                  }
            //                  else
            //                  {
            //                      //...otherwise, we can make a new reference point and replace it in the list of
            //                      //previously created points.
            //                      ReferencePoint origPoint = (ReferencePoint)((Expression.Container)x).Item;
            //                      pt = this.UIDocument.Document.FamilyCreate.NewReferencePoint(
            //                         origPoint.Position
            //                      );
            //                      this.Elements[count] = pt.Id;
            //                      pointDictionary.Add(origPoint.Id, pt.Id);// we are going to key off the old point to find the new point

            //                      dynParticle part = particleSystem.getParticleByElementID(e.Id);
            //                      //if we find this ref point does not have a matching particle, make a new particle
            //                      if (part == null)
            //                      {
            //                          particleSystem.makeParticleFromElementID(e.Id, 0.5, pt.Position, false);
            //                      }
            //                      else
            //                      {
            //                          part.setElementID(pt.Id);//we found a particle that matches the original element id but not the new point we just made, add new point id to part
            //                      }
            //                  }
            //              }
            //              //...otherwise...
            //              else
            //              {
            //                  //...we create a new point...
            //                      ReferencePoint origPoint = (ReferencePoint)((Expression.Container)x).Item;
            //                      pt = this.UIDocument.Document.FamilyCreate.NewReferencePoint(
            //                         origPoint.Position
            //                      );
            //                  //...and store it in the element list for future runs.
            //                  this.Elements.Add(pt.Id);
            //                  pointDictionary.Add(origPoint.Id, pt.Id);// we are going to key off the old point to find the new point

            //                  //make a new particle that matches the new point we just made
            //                  particleSystem.makeParticleFromElementID(pt.Id, 0.5, pt.Position, false);


            //              }
            //              //Finally, we update the counter, and return a new Expression containing the reference point.
            //              //This Expression will be placed in the Expression.List that will be passed downstream from this
            //              //node.
            //              count++;
            //              return Expression.NewContainer(pt);
            //          }
            //       )
            //    );

            //    //Now that we've created all the Reference Points and particles from this run, we delete all of the
            //    //extra ones from the previous run.
            //    foreach (var e in this.Elements.Skip(count))
            //    {
            //        this.DeleteElement(e);
            //        particleSystem.deleteParticleByElementID(e);
            //        //if(pointDictionary.ContainsValue(e))
            //        //{
            //        //    pointDictionary.Remove
            //        //}
            //    }

            //    //Fin
            //    //return Expression.NewList(result);
            //}


            //process curve inputs and convert to dynParticleSprings in particlesystem. for now we will copy the curves and only update the position of the copied points.

            
            if (curves.IsList)
            {
                var curvesList = (curves as Expression.List).Item;

                //Counter to keep track of how many curves we've made. We'll use this to delete old
                //elements later.
                int count = 0;

                //We create our output by...
                var resultCurves = Utils.convertSequence(
                   curvesList.Select(
                    //..taking each element in the list and...
                      delegate(Expression x)
                      {
                          CurveByPoints c;

 
                          //...if we already have cpb elements made by this node in a previous run...
                          if (this.Elements.Count > count)
                          {
                              Element e;
                              //...we attempt to fetch it from the document...
                              if (dynUtils.TryGetElement(this.Elements[count], out e) && e is CurveByPoints)
                              {

                                    //...and if we're successful, set properties of the spring
                                    c = e as CurveByPoints;
                                    dynParticleSpring spring = particleSystem.getSpringByElementID(e.Id);
                                    spring.setDamping(d);
                                    spring.setRestLength(r);
                                    spring.setSpringConstant(s);

                                  // - find the matching dynParticleSpring (just updated dynParticleSpring to keep it's curvebypoint id)
                                  // - the rest is a no-op because the dynParticleSpring will update based on the refpoints position after the next step.

                              }
                              else
                              {
                                  //if (e is CurveByPoints)
                                  //{
                                      //...otherwise, we can make a new curve by points using the passed-in CBPs points and replace it in the list of
                                      //previously created curve by points.

                                      // todo - ensure that we don't make coincident points
                                      // check each point extracted from cbp against all other points in element list, there should be a match for each element id.
                                      // 
                                      tempRefPtArr.Clear();
                                      tempRefPtArr = (ReferencePointArray)((CurveByPoints)((Expression.Container)x).Item).GetPoints();

                                      c = this.UIDocument.Document.FamilyCreate.NewCurveByPoints(

                                          tempRefPtArr

                                      );
                                      this.Elements[count] = c.Id;

                                      dynParticleSpring spring = particleSystem.getSpringByElementID(c.Id);
                                      //if we find this cbp does not have a matching spring, make a new spring
                                      if (spring == null)
                                      {
                                          try
                                          {


                                              //dynParticle partA = particleSystem.getParticleByElementID(pointDictionary[tempRefPtArr.get_Item(0).Id]);// we have the old point value here we need to find the new
                                              //dynParticle partB = particleSystem.getParticleByElementID(pointDictionary[tempRefPtArr.get_Item(1).Id]);
                                              // old point id, need to find new

                                              ReferencePoint oldRefPointA = tempRefPtArr.get_Item(0);
                                              ReferencePoint oldRefPointB = tempRefPtArr.get_Item(1);

                                              dynParticle partA = particleSystem.makeParticleFromElementID(oldRefPointA.Id, .5, oldRefPointA.Position, false);
                                              dynParticle partB = particleSystem.makeParticleFromElementID(oldRefPointB.Id, .5, oldRefPointA.Position, false);
                                              particleSystem.makeSpringFromElementID(c.Id, partA, partB, r, s, d);

                                              //ReferencePoint rpA = FindRefPointWithCoincidentXYZ(oldRefPointA);
                                              //ReferencePoint rpB = FindRefPointWithCoincidentXYZ(oldRefPointB);


                                              ////try and find the particles that are associated with these refpoints. 

                                              ////For some reason the dictionary is not working
                                              ////dynParticle partA = particleSystem.getParticleByElementID(pointDictionary[tempRefPtArr.get_Item(0).Id]);// we have the old point value here we need to find the new
                                              ////dynParticle partB = particleSystem.getParticleByElementID(pointDictionary[tempRefPtArr.get_Item(1).Id]);

                                              //dynParticle partA = particleSystem.getParticleByXYZ(rpA.Position); 
                                              //dynParticle partB = particleSystem.getParticleByXYZ(rpB.Position);

                                              //if (partA != null && partB != null)
                                              //{
                                              //    particleSystem.makeSpringFromElementID(c.Id, partA, partB, r, s, d);
                                              //}
                                              //else
                                              //{
                                              //    partA = particleSystem.makeParticleFromElementID(oldRefPointA.Id, .5, oldRefPointA.Position, false);
                                              //    partB = particleSystem.makeParticleFromElementID(oldRefPointB.Id, .5, oldRefPointA.Position, false);
                                              //    particleSystem.makeSpringFromElementID(c.Id, partA, partB, r, s, d);
                                              //}

                                          }
                                          catch (Exception ex)
                                          {
                                          }
                                              
                                      }
                                      else
                                      {
                                          spring.setElementID(c.Id);//we found a spring that matches the original element id but not the new cbp we just made, add new spring id to part
                                      }
                                  //}
                              }
                          }
                          //...otherwise...
                          else
                          {
                              //...we create a new cbp...
                              tempRefPtArr.Clear();
                              tempRefPtArr = (ReferencePointArray)((CurveByPoints)((Expression.Container)x).Item).GetPoints();

                              c = this.UIDocument.Document.FamilyCreate.NewCurveByPoints(

                                  tempRefPtArr

                              );
                              this.Elements.Add(c.Id);

                              //dynParticleSpring spring = particleSystem.getSpringByElementID(c.Id);
                              ReferencePoint oldRefPointA = tempRefPtArr.get_Item(0);
                              ReferencePoint oldRefPointB = tempRefPtArr.get_Item(1);

                              dynParticle partA = particleSystem.makeParticleFromElementID(oldRefPointA.Id, .5, oldRefPointA.Position, false);
                              dynParticle partB = particleSystem.makeParticleFromElementID(oldRefPointB.Id, .5, oldRefPointA.Position, false);
                              particleSystem.makeSpringFromElementID(c.Id, partA, partB, r, s, d);


                          }
                          //Finally, we update the counter, and return a new Expression containing the reference point.
                          //This Expression will be placed in the Expression.List that will be passed downstream from this
                          //node.
                          count++;
                          return Expression.NewContainer(c);
                      }
                   )
                );

                //Now that we've created all the Reference Points and particles from this run, we delete all of the
                //extra ones from the previous run.
                foreach (var e in this.Elements.Skip(count))
                {
                    this.DeleteElement(e);
                    particleSystem.deleteSpringByElementID(e);
                }

                //Fin
                //return Expression.NewList(result);
            }








            //actually run the simulation, TODO this should probaly move out of evaluate and into the timer event callback. 

            particleSystem.step(.004); // step size - .004 is fairly stable, TODO - generalize to take double ms in in the future. 








            //temporarily create ref points and curves for visualization

            //foreach (var el in this.Elements)
            //{
            //    this.DeleteElement(el);
            //}



            ////update ref points - purely destructive for now

            //for (int i = 0; i < particleSystem.numberOfParticles(); i++)
            //{
            //    p = particleSystem.getParticle(i);
            //    ReferencePoint rp = this.UIDocument.Document.FamilyCreate.NewReferencePoint(p.getPosition()); // update ref point position based on current particle position.
            //    this.Elements.Add(rp.Id);
            //    refPtArr.Append(rp);
            //}

            ////update lines - purely destructive for now

            //if (particleSystem.numberOfParticles() == refPtArr.Size)
            //{
            //    for (int i = 0; i < particleSystem.numberOfSprings(); i++) // make lines between points in pendul
            //    {

            //        a = particleSystem.getSpring(i);
            //        a.setDamping(d);
            //        a.setRestLength(r);
            //        a.setSpringConstant(s);

            //        tempRefPtArr.Clear();
            //        ReferencePoint end1 = FindRefPointFromXYZ(a.getOneEnd().getPosition());
            //        ReferencePoint end2 = FindRefPointFromXYZ(a.getTheOtherEnd().getPosition());
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
    [ElementName("Timer")]
    [ElementCategory(BuiltinElementCategories.MISC)]
    [ElementDescription("A node which represents a stopwatch.")]
    [RequiresTransaction(false)]

    public class dynTimer : dynNode
    {
        Stopwatch sw;
        bool timing = false;
        System.Timers.Timer timer; 

        public dynTimer()
        {
            InPortData.Add(new PortData("n", "How often to receive updates in milliseconds.", typeof(double)));
            InPortData.Add(new PortData("i/o", "Turn the timer on or off", typeof(dynBool)));
            OutPortData = new PortData("tim", "The timer, counting in milliseconds.", typeof(int));

            base.RegisterInputsAndOutputs();

            sw = new Stopwatch();
            //timer = new System.Timers.Timer();


        }

        void StartTimer(int interval)
        {

            timer = new System.Timers.Timer(interval); 

            timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
            timer.Enabled = true; // Enable it
        }

        static void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
           //figure out how to trigger Evaluate of the Timer node
           // Dynamo.Utilities.dynElementSettings.SharedInstance.Workbench
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
                    StartTimer(interval);
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
