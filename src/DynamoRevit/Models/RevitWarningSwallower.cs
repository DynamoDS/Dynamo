using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace Dynamo.Utilities
{
    public class RevitWarningSwallower : IFailuresPreprocessor
    {
        public FailureProcessingResult PreprocessFailures(FailuresAccessor a)
        {
            // inside event handler, get all warnings

            IList<FailureMessageAccessor> failures = a.GetFailureMessages();

            foreach (FailureMessageAccessor f in failures)
            {
                // check failure definition ids
                // against ones to dismiss:

                FailureDefinitionId id = f.GetFailureDefinitionId();

                if (BuiltInFailures.InaccurateFailures.InaccurateLine == id
                    || BuiltInFailures.OverlapFailures.DuplicateInstances == id
                    || BuiltInFailures.InaccurateFailures.InaccurateCurveBasedFamily == id
                    || BuiltInFailures.InaccurateFailures.InaccurateBeamOrBrace == id
                    || BuiltInFailures.InaccurateFailures.InaccurateLine == id)
                    a.DeleteWarning(f);
                else
                    a.RollBackPendingTransaction();
            }
            return FailureProcessingResult.Continue;
        }
    }
}