using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFITarget
{
    public class EnumReferencingClass
    {
        public string TestEnumDefaultVal(Days day = Days.Friday)
        {
            return $"{day} is the best";
        }
    }
}
