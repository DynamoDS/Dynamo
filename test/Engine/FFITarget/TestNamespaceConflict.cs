using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFITarget
{
    namespace A
    {
        public class DupTargetTest
        {
            public DupTargetTest()
            {
                   
            }

            public DupTargetTest(int prop)
            {
                Prop = prop;
            }

            public int Foo()
            {
                return 1;
            }

            public int Prop { get; set; }



        }
    }


    namespace B
    {

        public class DupTargetTest
        {
            public DupTargetTest()
            {

            }

            public DupTargetTest(int prop)
            {
                Prop = prop;
            }

            public int Foo()
            {
                return 2;
            }

            public int Prop { get; set; }

        }
    }


    namespace C
    {
        namespace B
        {
            public class DupTargetTest
            {
                public DupTargetTest()
                {

                }

                public DupTargetTest(int prop)
                {
                    Prop = prop;
                }

                public int Foo()
                {
                    return 2;
                }

                public int Prop { get; set; }

            }
        
    }
}


}
