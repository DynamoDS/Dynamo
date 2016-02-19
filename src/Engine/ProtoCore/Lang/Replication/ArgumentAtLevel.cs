using ProtoCore.DSASM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtoCore.Lang.Replication
{
    public class ElementAtLevel 
    {
        public List<int> Indices { get; private set; }
        public StackValue Element { get; private set; }

        public ElementAtLevel(StackValue element)
        {
            Indices = new List<int>();
            Element = element;
        }

        public ElementAtLevel(StackValue element, List<int> indices)
        {
            Indices = indices;
            Element = element;
        }
    }

    public class ArgumentAtLevel
    {
        public List<List<int>> Indices { get; private set; }
        public StackValue Argument { get; private set; }

        public bool IsDominant { get; private set; }

        public ArgumentAtLevel(StackValue argument)
        {
            Indices = new List<List<int>>();
            Argument = argument;
            IsDominant = false;
        }

        public ArgumentAtLevel(StackValue argument, List<List<int>> indices, bool isDominant)
        {
            Indices = indices;
            Argument = argument;
            IsDominant = isDominant;
        }
    }
}
