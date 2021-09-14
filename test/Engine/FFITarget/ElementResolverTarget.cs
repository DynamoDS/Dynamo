using Autodesk.DesignScript.Runtime;

namespace FFITarget
{
    public class ElementResolverTarget
    {
        public static ElementResolverTarget Create()
        {
            return new ElementResolverTarget();
        }

        public static ElementResolverTarget Create(ElementResolverTarget target)
        {
            return null;
        }

        public static ElementResolverTarget StaticProperty { get; set; }

        public static ElementResolverTarget StaticProperty2 { get; set; }

        public ElementResolverTarget Property { get; set; }

        public ElementResolverTarget Method()
        {
            return null;
        }

        public ElementResolverTarget Method(ElementResolverTarget target)
        {
            return null;
        }

        public static int StaticMethod(
            [DefaultArgumentAttribute("ElementResolverTarget.Create().StaticProperty")] ElementResolverTarget ert)
        {
            return 999;
        }

        public static int StaticMethod2(
            [DefaultArgumentAttribute(
                "FFITarget.ElementResolverTarget.StaticProperty.StaticProperty2.Method(FFITarget.ElementResolverTarget.Create())"
                )] ElementResolverTarget ert)
        {
            return 1999;
        }
    }

    namespace NameSpaceA
    {
        namespace NameSpaceB
        {
            namespace NameSpaceC
            {
                public class NestedResolverTarget
                {
                    public static ElementResolverTarget Property { get; set; }
                }

                public class NameSpaceC
                {
                }
            }
    }
}
}
