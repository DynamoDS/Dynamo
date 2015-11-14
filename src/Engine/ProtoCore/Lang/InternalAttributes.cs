using System;
using System.Collections.Generic;
using System.Diagnostics;
using ProtoCore.BuildData;
using ProtoCore.DSASM;
using ProtoCore.Utils;
using ProtoCore.Properties;
using System.Linq;
using System.Text;

namespace ProtoCore
{
    /// <summary>
    /// These are DS defined class attributes 
    /// These attributes are used internally by the compiler
    /// </summary>
    public class InternalAttributes
    {
        private const string kAttributeSuffix = "Attribute";
        public const string kInternalClassName = "InternalClass";

        public ProtoCore.DSASM.ClassTable ClassTable { get; private set; }

        public InternalAttributes(ProtoCore.DSASM.ClassTable classTable)
        {
            this.ClassTable = classTable;
            BuildInteralAttributes();
        }

        private void BuildInteralAttributes()
        {
            Validity.Assert(ClassTable != null);

            ClassNode cnode = null;

            cnode = new ClassNode { Name = kInternalClassName + kAttributeSuffix };
            ClassTable.Append(cnode);
        }
    }
}
