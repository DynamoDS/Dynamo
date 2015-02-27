using System;
using ProtoCore.DSASM;

namespace ProtoCore.Lang
{
    public class Obj
    {

        /// <summary>
        /// This represents a tieback value into the DSASM execution engine
        /// </summary>
        public StackValue DsasmValue { get; private set; }

        public Obj(StackValue dsasmValue)
        {
            DsasmValue = dsasmValue;
        }

        public override string ToString()
        {
            return DsasmValue.ToString();
        }

        public Obj()
        {

        }

        //Payloads are objects, or Array

        public Object Payload;
        public ProtoCore.Type Type;

    }
}
