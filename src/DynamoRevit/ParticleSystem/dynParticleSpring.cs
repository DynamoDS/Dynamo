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

using Autodesk.Revit.DB;

namespace Dynamo.Nodes
{
    class ParticleSpring
    {
        protected Particle m_particleA;
        protected Particle m_particleB;
        protected double m_restLength;
        protected double m_springConstant;
        protected double m_Damping;
        protected ElementId m_ElementId;
        //protected double m_residualForce;
        protected int m_ID;

        public ParticleSpring(int ID, Particle particleA, Particle particleB, double restLength, double springConstant, double damping)
        {
            m_ID = ID;
            m_particleA = particleA;
            m_particleB = particleB;
            m_restLength = restLength;
            m_springConstant = springConstant;
            m_Damping = damping;
        }

        public ParticleSpring(int ID, ElementId eid, Particle particleA, Particle particleB, double restLength, double springConstant, double damping)
        {
            m_ID = ID;
            m_ElementId = eid;
            m_particleA = particleA;
            m_particleB = particleB;
            m_restLength = restLength;
            m_springConstant = springConstant;
            m_Damping = damping;
        }

       

        public double getLength()
        {
            return (m_particleB.getPosition() - m_particleA.getPosition()).GetLength();
        }

        public Particle getOneEnd()
        {
            return m_particleA;
        }

        public Particle getTheOtherEnd()
        {
            return m_particleB;
        }

        public double getRestLength()
        {
            return m_restLength;
        }

        public double getDamping()
        {
            return m_Damping;
        }

        public double getResidualForce()
        {
            return getSpringConstant() * Math.Abs(getRestLength() - getLength());
        }

        public void setSpringConstant(double springConstant)
        {
            m_springConstant = springConstant;
        }

        public void setRestLength(double restLength)
        {
            m_restLength = restLength;
        }

        public void setDamping(double damping)
        {
            m_Damping = damping;
        }

        public double getSpringConstant()
        {
            return m_springConstant;
        }

        public void apply()
        {

            if (m_particleA.isFree() || m_particleB.isFree())
            {

                try
                {
                    XYZ a2b = m_particleA.getPosition().Subtract(m_particleB.getPosition());

                    double a2bDistance = Math.Abs(Math.Sqrt(a2b.X * a2b.X + a2b.Y * a2b.Y + a2b.Z * a2b.Z)); // MDJ vector norm http://mathworld.wolfram.com/VectorNorm.html != a2b.Normalize();

                    if (a2bDistance == 0 )
                    {
                        a2b = new XYZ(0, 0, 0);
                    }
                    else
                    {
                        a2b = a2b / a2bDistance;
                    }

                    double springForce = -(a2bDistance - m_restLength) * m_springConstant;

                    XYZ Va2b = m_particleA.getVelocity() - m_particleB.getVelocity();

                    double dampingForce = -m_Damping * (a2b.DotProduct(Va2b));

                    // forceB is same as forceA in opposite direction
                    double r = springForce + dampingForce;

                    a2b = a2b * r;

                    if (m_particleA.isFree())
                        m_particleA.addForce(a2b);
                    if (m_particleB.isFree())
                        m_particleB.addForce(-a2b);
                }
                catch (Exception e)
                {
                    DynamoLogger.Instance.Log(e.Message);
                    DynamoLogger.Instance.Log(e.StackTrace);
                }
            }

        }

        public ElementId getElementID()
        {
            return m_ElementId;
        }

        public void setElementID(ElementId eid)
        {
            m_ElementId = eid;
        }

        public int ID()
        {
            return m_ID;
        }

      
    }
}
