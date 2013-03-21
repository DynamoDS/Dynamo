using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.Revit.DB;

using Microsoft.FSharp.Collections;
using Expression = Dynamo.FScheme.Expression;
using Value = Dynamo.FScheme.Value;

namespace Dynamo.Utilities
{
    public static class dynRevitUtils
    {
        public static ReferenceArrayArray ConvertFSharpListListToReferenceArrayArray(FSharpList<Value> lstlst)
        {
            ReferenceArrayArray refArrArr = new ReferenceArrayArray();
            foreach (Value v in lstlst)
            {
                ReferenceArray refArr = new ReferenceArray();
                FSharpList<Value> lst = (v as Value.List).Item;

                foreach (Value vInner in lst)
                {
                    var mc = (ModelCurve)(vInner as Value.Container).Item;
                    refArr.Append(mc.GeometryCurve.Reference);
                }
                refArrArr.Append(refArr);
            }

            return refArrArr;
        }
    }
}
