using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFITarget
{
    public class ElementResolverTarget
    {
        public static ElementResolverTarget Create()
        {
            return null;
        }

        public static ElementResolverTarget Create(ElementResolverTarget target)
        {
            return null;
        }

        public static ElementResolverTarget StaticProperty { get; set; }

        public ElementResolverTarget Property { get; set; }

        public ElementResolverTarget Method()
        {
            return null;
        }

        public ElementResolverTarget Method(ElementResolverTarget target)
        {
            return null;
        }
    }

}
