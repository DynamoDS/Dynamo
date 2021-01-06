using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFITarget
{
    public class ClassWithMigratedMembers
    {

        public static ClassWithMigratedMembers ByNothing()
        {
            return new ClassWithMigratedMembers();
        }

        //we test by opening a file that references a node with three params
        public string AMemberWithTwoParams(string a, string b)
        {
            return a + b;
        }
        public string AMemberWithThreeParams(string a, string b,string c ="abc")
        {
            return "migrated";
        }

        //we test a file that uses the node AMemberWithAChangedName0
        public string AMemberWithAChangedName2(string a)
        {
            return a;
        }
      /*  public string AMemberWithAChangedName1(string a)
        {
            return a;
        }*/
    }
}
