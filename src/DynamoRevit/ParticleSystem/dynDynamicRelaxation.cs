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

            for(int i=0; i<particleSystem.numberOfParticles(); i++) 
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

                if(i*2+1 < this.RenderDescription.lines.Count())
                {
                    this.RenderDescription.lines[i*2] = new Point3D(pos1.X, pos1.Y, pos1.Z);
                    this.RenderDescription.lines[i*2+1] = new Point3D(pos2.X, pos2.Y, pos2.Z);
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
        int oldNumX = -1;
        int oldNumY = -1;

        public dynDynamicRelaxation()
        {
            InPortData.Add(new PortData("points", "The points to use as fixed nodes.", typeof(Value.List)));
            InPortData.Add(new PortData("d", "Dampening.", typeof(Value.Number)));
            InPortData.Add(new PortData("s", "Spring Constant.", typeof(Value.Number)));
            InPortData.Add(new PortData("r", "Rest Length.", typeof(Value.Number)));
            InPortData.Add(new PortData("m", "Nodal Mass.", typeof(Value.Number)));
            InPortData.Add(new PortData("numX", "Number of Particles in X.", typeof(Value.Number)));
            InPortData.Add(new PortData("numY", "Number of Particles in Y.", typeof(Value.Number)));
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

        // geometric test, fairly expensive. obsoleted by CreateParticleByElementID and GetParticleFromElementID
        //public ReferencePoint FindRefPointFromXYZ(XYZ xyz)         
        //{
        //    Element el;
        //    ReferencePoint rp;

        //    foreach(ElementId id in this.Elements)
        //    {

        //        dynUtils.TryGetElement(id, out el);
        //        rp = el as ReferencePoint;

        //        if (rp != null && rp.Position.IsAlmostEqualTo(xyz))// note this is not gauranteed to be unique. there may be mulitple coincident refpoints and this utill will only return the first found
        //        {
        //            return rp;

        //        }
        //    }

        //    return null;
        //}

        // geometric test, fairly expensive. 
        //public ReferencePoint FindRefPointWithCoincidentXYZ(ReferencePoint rp)
        //{
        //    Element el;
        //    ReferencePoint rp2;

        //    foreach (ElementId id in this.Elements) // compare to inputed points
        //    {
        //        dynUtils.TryGetElement(id, out el);
        //        rp2 = el as ReferencePoint;

        //        if (rp != null && rp2 != null && rp.Position.IsAlmostEqualTo(rp2.Position))// note this is not gauranteed to be unique. there may be mulitple coincident refpoints and this utill will only return the first found
        //        {
        //            return rp2; // found a match
        //        }
        //    }

        //    return null;
        //}

        public override Value Evaluate(FSharpList<Value> args)
        {

            var input = args[0];//point list
            double d = ((Value.Number)args[1]).Item;//dampening
            double s = ((Value.Number)args[2]).Item;//spring constant
            double r = ((Value.Number)args[3]).Item;//rest length
            double m = ((Value.Number)args[4]).Item;//nodal mass
            int numX = (int)((Value.Number)args[5]).Item;//number of particles in X
            int numY = (int)((Value.Number)args[6]).Item;//number of particles in Y
            double g = ((Value.Number)args[7]).Item;//gravity z component

            //if the particle system has a different layout, then
            //clear it instead of updating
            bool reset = false;
            if(oldNumX == -1 || 
                oldNumY == -1 || 
                oldNumX != numX || 
                oldNumY != numY ||
                particleSystem.numberOfParticles() == 0)
            {
                reset = true;
                particleSystem.Clear();
                oldNumX = numX;
                oldNumY = numY;

                //if we have an existing RenderDescription
                //then clear it
                if (dynSettings.Controller.UIDispatcher != null)
                {
                    dynSettings.Controller.UIDispatcher.Invoke(new Action(
                       delegate
                       {
                           if (this.RenderDescription != null)
                               this.RenderDescription.ClearAll();
                       }
                    ));
                }
            }

            particleSystem.setGravity(g);

            ReferencePoint pt1;
            ReferencePoint pt2;

            //If we are receiving a list, we must create fixed particles for each reference point in the list.
            if (input.IsList)
            {
                var pointList = (input as Value.List).Item;

                Array pointArray = pointList.ToArray();

                for (int i = 0; i < pointArray.Length-1; i++)
                {

                    pt1 = (ReferencePoint)((Value.Container)pointArray.GetValue(i)).Item as ReferencePoint;
                    pt2 = (ReferencePoint)((Value.Container)pointArray.GetValue(i + 1)).Item as ReferencePoint;

                    (dynSettings.Controller as DynamoController_Revit).Updater.RegisterChangeHook(pt1.Id, ChangeTypeEnum.Modify, UpdateStart);
                    (dynSettings.Controller as DynamoController_Revit).Updater.RegisterChangeHook(pt2.Id, ChangeTypeEnum.Modify, UpdateEnd);

                    if (reset)
                    {
                        CreateChainWithTwoFixedEnds(pt1, pt2, numX, d, r, s, m);
                    }
                    else
                    {
                        //update the spring values
                        for (int j = 0; j < particleSystem.numberOfSprings(); j++)
                        {
                            ParticleSpring spring = particleSystem.getSpring(j);
                            spring.setDamping(d);
                            spring.setRestLength(r);
                            spring.setSpringConstant(s);
                        }
                        for (int j = 0; j < particleSystem.numberOfParticles(); j++)
                        {
                            Particle p = particleSystem.getParticle(j);
                            p.setMass(m);
                        }

                        particleSystem.getParticle(0).setPosition(pt1.Position);
                        particleSystem.getParticle(particleSystem.numberOfParticles()-1).setPosition(pt2.Position);
                    }
                }

                return Value.NewContainer(particleSystem);
            }
            else
            {
                throw new Exception("You must pass in a list of reference points.");
            }

            //setupLineTest(numX, numY, d, r, s, m);
            //return Value.NewContainer(particleSystem);
        }

        public void UpdateStart(List<ElementId> updated)
        {
            ReferencePoint rp = dynRevitSettings.Doc.Document.GetElement(updated[0]) as ReferencePoint;
            if (rp != null)
            {
                Particle p = particleSystem.getParticle(0);
                p.setPosition(rp.Position);
            }
        }

        public void UpdateEnd(List<ElementId> updated)
        {
            ReferencePoint rp = dynRevitSettings.Doc.Document.GetElement(updated[0]) as ReferencePoint;
            if (rp != null)
            {
                Particle p = particleSystem.getParticle(particleSystem.numberOfParticles()-1);
                p.setPosition(rp.Position);
            }
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
    [NodeCategory(BuiltinNodeCategories.ANALYZE_STRUCTURE)]
    [IsInteractive(true)]
    public class dynDynamicRelaxationStep: dynNodeWithOneOutput
    {
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
