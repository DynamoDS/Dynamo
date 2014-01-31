using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoCore.DSASM
{
    public class FunctionCounter
    {
        public int functionIndex;
        public int classScope;
        public string name;
        public int times { get; set; }
        //public int sharedCounter { get; set; }


        public FunctionCounter(int index, int scope, string name, int sharedCounter)
        {
            functionIndex = index;
            classScope = scope;
            this.times = times;
            this.name = name;
            //this.sharedCounter = sharedCounter;
        }
        public FunctionCounter(int index,int scope, int counter,string name,int sharedCounter)
        {
            functionIndex = index;
            classScope = scope;
            this.times = counter;
            this.name = name;
            //this.sharedCounter = sharedCounter;
        }
    }   
}
