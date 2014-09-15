using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class GlobalClass
{
    public GlobalClass() { }

    public GlobalClass(int a) 
    {
        Property = a;
    }

    public int Property { get; set; }

    public int Modify(int a)
    {
        Property += a;
        return Property;
    }
}

