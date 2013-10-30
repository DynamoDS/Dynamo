using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.Revit.DB;

namespace Dynamo.Nodes
{


    class Integrator
    {
        public Integrator(ParticleSystem s)
        {

        }

        public void step(double t)
        {
            t = 0;
        }
    }

    class VerletIntegrator
    {
        ParticleSystem s;

        public VerletIntegrator(ParticleSystem ps)
        {
            s = ps;
        }

        public void step(double t)
        {
            s.clearForces();
            s.applyForces();

            double halftt = 0.5 * t * t;
            double tt = t * t;

            for (int i = 0; i < s.numberOfParticles(); i++)
            {
                Particle p = s.getParticle(i);
                if (p.isFree())
                {
                    XYZ a = p.getForce() / p.getMass();
                    XYZ xmm = p.getOldPosition();
                    XYZ xm = p.getPosition();
                    XYZ x = xm.Add(xm - xmm) + a * tt;
                    XYZ vm = p.getVelocity();

                    p.setPosition(x);
                    p.setVelocity((x - xmm) / (2 * t));

                }

            }
        }
    }
}