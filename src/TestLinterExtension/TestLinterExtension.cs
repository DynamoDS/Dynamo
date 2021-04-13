using System.Collections.Generic;
using Dynamo.Graph.Nodes;
using Dynamo.TestLinterExtension.LinterRules;
using Dynamo.ViewModels;
using Dynamo.Extensions;
using Dynamo.Linting;
using Dynamo.Linting.Rules;
using Dynamo.Linting.Interfaces;

namespace Dynamo.TestLinterExtension
{
    public class TestLinterExtension : LinterExtensionBase
    {

        public override string UniqueId => "a7ad5249-10ea-4fbf-b2f6-7f9658773850";

        public override string Name => "Test Linter ViewExtension";

        #region Extension Lifecycle

        public override void Ready(ReadyParams sp)
        {
            base.Ready(sp);

            this.AddLinterRule(new NodesCantBeNamedFooRule());
            this.AddLinterRule(new InputNodesNotAllowedRule());
            this.AddLinterRule(new GraphNeedsOutputNodesRule());
        }

        public override void Dispose() { }
        public override void Shutdown() { }

        #endregion

    }
}
