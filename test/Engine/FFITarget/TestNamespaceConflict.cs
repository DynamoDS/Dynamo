using Autodesk.DesignScript.Runtime;

public class DupTargetTest : FFITarget.C.B.DupTargetTest
{
    public DupTargetTest()
    {

    }

    public string Bar()
    {
        return "GlobalClass";
    }
}

namespace FFITarget
{
    namespace DSCore
    {
        public class List
        {
            public int Count()
            {
                return 999;
            }
        }
    }

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
            [IsVisibleInDynamoLibrary(false)]
            public class DupTargetTest
            {
                [IsVisibleInDynamoLibrary(true)]
                public DupTargetTest()
                {

                }

                [IsVisibleInDynamoLibrary(true)]
                public DupTargetTest(int prop)
                {
                    Prop = prop;
                }

                [IsVisibleInDynamoLibrary(true)]
                public int Foo()
                {
                    return 4;
                }

                [IsVisibleInDynamoLibrary(true)]
                public int Prop { get; set; }

            }

        }
    }
}
