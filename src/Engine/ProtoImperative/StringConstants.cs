using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoImperative
{
    public struct StringConstants
    {
        public const string invalidNestedImperativeBlock = "An imperative language block is declared within an imperative language block.";
        public const string onlyIdentifierOrIdentifierListCanBeOnLeftSide = "Only identifier or identifier list can appear on the left hand side of assignment.";
        public const string arraySizeOverflow = "Array size overflow";
        public const string constantExpectedInArrayDeclaration = "Array declaration expected constant expression";
        public const string classNameAsVariableError = "{0} is a class name, can't be used as a variable.";
        public const string identifierRedefinition = "redefinition of identifier '{0}'";
        public const string invalidArrayInitializer = "array initializer must be an expression list";
        public const string functionPointerNotAllowedAtBinaryExpression = "Function pointer is not allowed at binary expression other than assignment!";
        public const string functionAsVaribleError = "\"{0}\"is a function and not allowed as a variable name";
    }
}
