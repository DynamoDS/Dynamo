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

     class dynParticleSystem
    {
        protected List<dynParticle> particles;
        protected List<dynParticleSpring> springs;

        protected const double DEFAULT_GRAVITY = -9.8f;
        protected const double DEFAULT_DRAG = 0.15f;

        protected VerletIntegrator integrator;

        protected XYZ gravity;
        protected double drag;

        protected bool hasDeadParticles;


        public dynParticleSystem()
        {
            hasDeadParticles = false;
            integrator = new VerletIntegrator(this);

            particles = new List<dynParticle>();
            springs = new List<dynParticleSpring>();


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

        void setGravity(double g)
        {
            gravity = new XYZ(0, 0, g);
        }

        void setDrag(double d)
        {
            drag = d;
        }

        public dynParticle makeParticle(double mass, XYZ position, bool fix)
        {
            dynParticle p = new dynParticle(mass, position, fix);
            particles.Add(p);
            return p;
        }

        public dynParticleSpring makeSpring(dynParticle a, dynParticle b, double restLength, double springConstant, double damping)
        {
            dynParticleSpring s = new dynParticleSpring(a, b, restLength, springConstant, damping);
            springs.Add(s);
            return s;
        }

        public void applyForces()
        {

            for (int i = 0; i < particles.Count(); ++i)
            {

                particles[i].addForce(gravity);

            }

            for (int i = 0; i < particles.Count(); ++i)
            {

                dynParticle p = particles[i];
                p.addForce(p.getVelocity() * -drag);

            }

            for (int i = 0; i < springs.Count(); i++)
            {

                dynParticleSpring f = springs[i];
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

        public dynParticle getParticle(int i)
        {
            return particles[i];
        }

        public dynParticleSpring getSpring(int i)
        {
            return springs[i];
        }

    }
}


