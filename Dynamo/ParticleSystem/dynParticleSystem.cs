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
using System.Text;
using System.Collections;

using Autodesk.Revit.DB;


namespace Dynamo.Elements
{


    /* The ParticleSystem Class.  

    High level data structure holding all particles and forces.  Manages drag and
    gravity.  Manages stepping the system and the integrator type.

    Overall architecture is largely borrowed from that suggested in MIT's 6.837 
    course, which is also quite similar to that found in Traer Physics.
    */

     class ParticleSystem
    {
        protected List<Particle> particles;
        protected List<ParticleSpring> springs;

        protected const double DEFAULT_GRAVITY = -9.8f;
        protected const double DEFAULT_DRAG = 0.15f;

        protected VerletIntegrator integrator;

        protected XYZ gravity;
        protected double drag;

        protected bool hasDeadParticles;


        public ParticleSystem()
        {
            hasDeadParticles = false;
            integrator = new VerletIntegrator(this);

            particles = new List<Particle>();
            springs = new List<ParticleSpring>();

            gravity = new XYZ(0, 0, DEFAULT_GRAVITY);
            drag = DEFAULT_DRAG;

        }

        public enum IntegratorType { Verlet, RK4 };

        public void step(double t)
        {
            integrator.step(t); //.0002 should be stable
        }

        //public void SetIntegrator(IntegratorType integrator )
        //{
        //    switch ( integrator )
        //    {
        //        case VERLET:
        //            this.integrator = new VerletIntegrator( this );
        //            break;
        //        case RK4: // TODO: this is a placeholder until we implement rk4!
        //            this.integrator = new VerletIntegrator( this );
        //            break;
        //        default:
        //            this.integrator = new VerletIntegrator( this );
        //            break;
        //    }
        //}

        public void setGravity(double g)
        {
            gravity = new XYZ(0, 0, g);
        }

        void setDrag(double d)
        {
            drag = d;
        }

        public Particle makeParticle(double mass, XYZ position, bool fix)
        {
            Particle p = new Particle(mass, position, fix);
            particles.Add(p);
            return p;
        }

        public Particle makeParticleFromElementID(ElementId eid, double mass, XYZ position, bool fix)
        {
            bool found = false;
            for (int i = 0; i < particles.Count(); ++i)
            {
                if (eid != null  && (particles[i].getElementID() != null))
                {
                    if (eid == particles[i].getElementID())
                    {
                        found = true;
                        return particles[i];
                       
                    }
                }
            }
            if (found == false)//if we did not find one make a new one
            {
                Particle part = new Particle(eid, .5, position, fix);
                particles.Add(part);
                return part;
            }

            return null;

        }

        public Particle makeParticleFromXYZ(ElementId eid, double mass, XYZ position, bool fix)
        {
            bool found = false;
            for (int i = 0; i < particles.Count(); ++i)
            {
                if (eid != null && (particles[i].getElementID() != null)&& position!=null)
                {
                    if (position.IsAlmostEqualTo(particles[i].getPosition()))
                    {
                        found = true;
                        return particles[i];

                    }
                }
            }
            if (found == false)//if we did not find one make a new one
            {
                Particle part = new Particle(eid, .5, position, fix);
                particles.Add(part);
                return part;
            }

            return null;

        }
        
         public ParticleSpring makeSpring(Particle a, Particle b, double restLength, double springConstant, double damping)
        {

            ParticleSpring s = new ParticleSpring(a, b, restLength, springConstant, damping);
            springs.Add(s);
            return s;
        }

        public ParticleSpring makeSpringFromElementID(ElementId eid, Particle a, Particle b, double restLength, double springConstant, double damping)
        {
            bool found = false;
            for (int i = 0; i < springs.Count(); ++i)
            {
                if (eid != null && (springs[i].getElementID() != null))
                {
                    if (eid == springs[i].getElementID())
                    {
                        found = true;
                        return springs[i];
                        
                    }
                }
            }
            if (found == false)
            {
                ParticleSpring s = new ParticleSpring(eid, a, b, restLength, springConstant, damping);
                springs.Add(s);
                return s;
            }
            return null;
        }

        public void applyForces()
        {

            for (int i = 0; i < particles.Count(); ++i)
            {
                particles[i].addForce(gravity);
                particles[i].addForce(particles[i].getVelocity() * -drag);
            }

            /*for (int i = 0; i < particles.Count(); ++i)
            {

                Particle p = particles[i];
                p.addForce(p.getVelocity() * -drag);

            }*/

            for (int i = 0; i < springs.Count(); i++)
            {
                ParticleSpring f = springs[i];
                f.apply();
            }
        }

        public void clearForces()
        {
            for (int i = 0; i < particles.Count(); i++)
            {
                particles[i].clearForce();
            }
        }

        public int numberOfParticles()
        {
            return particles.Count();
        }

        public int numberOfSprings()
        {
            return springs.Count();
        }

        public Particle getParticle(int i)
        {
            return particles[i];
        }

        public ParticleSpring getSpring(int i)
        {
            return springs[i];
        }

        public Particle getParticleByElementID(ElementId eid)
        {
            for (int i = 0; i < particles.Count(); ++i)
            {
                if (eid != null && particles.Count > 0 && (particles[i].getElementID() != null))
                {
                    if (eid == particles[i].getElementID())
                    {
                        return particles[i];
                    }
                }
            }
            return null;
        }

        public bool deleteParticleByElementID(ElementId eid)
        {
            for (int i = 0; i < particles.Count(); ++i)
            {
                if (eid != null && particles.Count > 0 && (particles[i].getElementID() != null))
                {
                    if (eid == particles[i].getElementID())
                    {
                        particles.Remove(particles[i]);
                        return true; //found and deleted
                    }
                }
            }
            return false; //did not delete
        }

        public ParticleSpring getSpringByElementID(ElementId eid)
        {
            for (int i = 0; i < springs.Count(); ++i)
            {
                if (eid != null && springs.Count > 0 && (springs[i].getElementID() != null))
                {
                    if (eid == springs[i].getElementID())
                    {
                        return springs[i];
                    }
                }
            }
            return null;
        }

        public bool deleteSpringByElementID(ElementId eid)
        {
            for (int i = 0; i < springs.Count(); ++i)
            {
                if (eid != null && springs.Count > 0 && (springs[i].getElementID() != null))
                {
                    if (eid == springs[i].getElementID())
                    {
                        springs.Remove(springs[i]);
                        return true; //found and deleted
                    }
                }
            }
            return false; //did not delete
        }

        public Particle getParticleByXYZ(XYZ xyz)
        {

            for (int i = 0; i < particles.Count(); ++i)
            {
                if (xyz != null && particles.Count > 0 && (particles[i].getElementID() != null))
                {
                    if (xyz.IsAlmostEqualTo(particles[i].getPosition()))
                    {
                        return particles[i];

                    }

                }
               
            }

            return null;

        }

        public void Clear()
        {
            particles.Clear();
            springs.Clear();
        }

    }
}


