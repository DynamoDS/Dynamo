using Dynamo.Models;
using Dynamo.ViewModels;
using System;

namespace MutationTestInfrastructure
{
    class DynamoViewModelMutation : DynamoViewModel
    {
        public DynamoViewModelMutation(StartConfiguration startConfiguration)
            : base(startConfiguration)
        {
        }

        override protected void OnModelCommandCompleted(DynamoModel.RecordableCommand command)
        {
            var name = command.GetType().Name;
            switch (name)
            {
                case "MutateTestCommand":
                    var mutatorDriver = new Dynamo.MutationInfastructure.MutatorDriver(this);
                    mutatorDriver.RunMutationTests();
                    break;
            }

            base.OnModelCommandCompleted(command);
        }
    }
}
