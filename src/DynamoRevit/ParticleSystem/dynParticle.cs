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
    class Particle
    {
       protected XYZ m_Position;
       protected XYZ m_oldPosition;
       protected XYZ m_Velocity;
       protected XYZ m_Force;

       protected double m_Mass;
       protected bool m_Fixed;
       protected ElementId m_ElementId = null;
       protected int m_ID;


       public Particle(int ID, double mass, XYZ position, bool fix) // added to handle maintaining/updating chains non-destructively
       {
           m_ID = ID;
           m_Position = position;
           m_oldPosition = position;
           m_Velocity = new XYZ(0, 0, 0);
           m_Force = new XYZ(0, 0, 0);
           m_Mass = mass;
           m_Fixed = fix;

       }

       public Particle(int ID, ElementId eid, double mass, XYZ position, bool fix) // added to handle backfinding the right refpoint without an expensive search
       {
           m_ID = ID;
           m_ElementId = eid;
           m_Position = position;
           m_oldPosition = position;
           m_Velocity = new XYZ(0, 0, 0);
           m_Force = new XYZ(0, 0, 0);
           m_Mass = mass;
           m_Fixed = fix;
       }

        public void makeFixed()
        {
            m_Fixed = true;
            m_Velocity = new XYZ(0, 0, 0);
        }


        public bool isFree()
        {
            return !m_Fixed;
        }

        public XYZ getForce()
        {
            return m_Force;
        }
        public void addForce(XYZ f)
        {
            m_Force = m_Force.Add(f);
        }

        public void setForce(XYZ f)
        {
            m_Force = f;
        }

        public void addVelocity(XYZ v)
        {
            m_Velocity = m_Velocity.Add(v);
        }

        public XYZ getVelocity()
        {
            return m_Velocity;
        }

        public void setVelocity(XYZ v)
        {
            m_Velocity = v;
        }

        public XYZ getPosition()
        {
            return m_Position;
        }

        public XYZ getOldPosition()
        {
            return m_oldPosition;
        }

        public void setPosition(XYZ p)
        {
            m_oldPosition = m_Position;
            m_Position = p;
        }

        public void addPosition(XYZ m)
        {
            m_oldPosition = m_Position;
            m_Position = m_Position.Add(m);
        }

        public double getMass()
        {
            return m_Mass;
        }

        public void setMass(double mass)
        {
            m_Mass = mass;
        }

        public void clearForce()
        {
            m_Force = new XYZ(0, 0, 0);
        }

        public void reset()
        {
            m_Position = new XYZ(0, 0, 0);
            m_Force = new XYZ(0, 0, 0);
            m_Velocity = new XYZ(0, 0, 0);
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
