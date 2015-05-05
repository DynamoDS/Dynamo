using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoAssociative
{
    public struct StringConstants
    {
        public const string invalidNestedAssociativeBlock = "An associative language block is declared within an associative language block.";

        public const string classNameAsVariableError = "{0} is a class name, can't be used as a variable.";
        public const string memberVariableAlreadyDefined = "Member variable '{0}' is already defined in class {1}";
        public const string classCannotBeDefinedInsideLanguageBlock = "A class cannot be defined inside a language block.\n";
        public const string functionPointerNotAllowedAtBinaryExpression = "Function pointer is not allowed at binary expression other than assignment!";
        public const string functionAsVariableError = "\"{0}\" is a function and not allowed as a variable name";
    }
}
