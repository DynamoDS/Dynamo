using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore.DSASM;

namespace ProtoCore.Utils
{
    public static class ClassUtils
    {
        /// <summary>
        /// Get the list of classes that this can be upcast to
        /// It includes the class itself
        /// </summary>
        /// <param name="cn"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static List<int> GetClassUpcastChain(ClassNode cn, Core core)
        {
            List<int> ret = new List<int>();

            //@TODO: Replace this with an ID
            ret.Add(core.ClassTable.ClassNodes.IndexOf(cn));

            ClassNode target = cn;
            while (target.baseList.Count > 0)
            {
                Validity.Assert(target.baseList.Count == 1, "Multiple Inheritence not yet supported, {F5DDC58D-F721-4319-854A-622175AC43F8}");
                ret.Add(target.baseList[0]);

                target = core.ClassTable.ClassNodes[target.baseList[0]];
            }

            if (!ret.Contains((int)(PrimitiveType.kTypeVar)))
                ret.Add((int)PrimitiveType.kTypeVar);


            return ret;
        }

        /// <summary>
        /// Get the number of upcasts that need to be performed to turn a class into another class in its upcast chain
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static int GetUpcastCountTo(ClassNode from, ClassNode to, Core core)
        {
            int fromID = core.ClassTable.ClassNodes.IndexOf(from);
            int toID = core.ClassTable.ClassNodes.IndexOf(to);

            List<int> upcastChain = GetClassUpcastChain(from, core);

            //Validity.Assert(upcastChain.Contains(toID), "Asked to upcast a class to a class that wasn't in its upcast chain");

            if (!upcastChain.Contains(toID))
                return int.MaxValue;

            return upcastChain.IndexOf(toID);


        }
    }
}
