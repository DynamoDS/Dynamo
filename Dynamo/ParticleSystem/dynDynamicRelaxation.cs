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
using Microsoft.FSharp.Collections;
using System.IO.Ports;
using Dynamo.Connectors;
using Expression = Dynamo.FScheme.Expression;
using Autodesk.Revit.DB;

namespace Dynamo.Elements
{
    [ElementName("Dynamic Relaxation")]
    [ElementCategory(BuiltinElementCategories.MISC)]
    [ElementDescription("A node which allows you to drive the position of elmenets via a particle system .")]
    [RequiresTransaction(true)]

    class dynDynamicRelaxation : dynNode
    {
        dynParticleSystem particleSystem;

        public dynDynamicRelaxation()
        {
            InPortData.Add(new PortData("points", "The point to drive.", typeof(dynReferencePointByXYZ)));
            InPortData.Add(new PortData( "curves", "The curves to drive", typeof(dynCurveByPoints)));
            //InPortData.Add(new PortData("tim", "How often to receive updates.", typeof(double)));

            OutPortData = new PortData("points", "the outputed points", typeof(dynReferencePointByXYZ));
            base.RegisterInputsAndOutputs();

            particleSystem = new dynParticleSystem();
            setupPendulum();


        }

        void setupPendulum()
        {

            dynParticle a = particleSystem.makeParticle(0.5, new XYZ(0, 0, 0), true);

            List<dynParticle> particles = new List<dynParticle>();
            particles.Add(a);

            for (int i = 1; i < 10; i++)
            {
                dynParticle s = particles[i - 1];
                dynParticle b = particleSystem.makeParticle(0.5, new XYZ(0, ((double)i) * Math.Sin(0.5), ((double)i) * Math.Sin(0.5)), false);
                particles.Add(b);
                particleSystem.makeSpring(s, b, 1, 800, 0.2);
            }

            

        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            //particleSystem.applyForces();
            particleSystem.step();

            return Expression.NewNumber(1);
        }
    }
}
