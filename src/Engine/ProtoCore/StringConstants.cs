﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoCore
{
    public struct StringConstants
    {
        public const string kAssingToThis = "'this' is readonly and cannot be assigned to.";
        public const string kCallingNonStaticProperty = "'{0}.{1}' is not a static property.";
        public const string kCallingNonStaticMethod = "'{0}.{1}()' is not a static method.";
        public const string kMethodHasInvalidArguments = "'{0}()' has some invalid arguments.";
        public const string kInvalidStaticCyclicDependency = "Cyclic dependency detected at '{0}' and '{1}'.";
        public const string KCallingConstructorOnInstance = "Cannot call constructor '{0}()' on instance.";
        public const string kPropertyIsInaccessible = "Property '{0}' is inaccessible.";
        public const string kMethodIsInaccessible = "Method '{0}()' is inaccessible.";
        public const string kCallingConstructorInConstructor = "Cannot call constructor '{0}()' in itself.";
        public const string kPropertyNotFound = "Property '{0}' not found.";
        public const string kMethodNotFound = "Method '{0}()' not found.";
        public const string kStaticMethodNotFound = "Cannot find static method or constructor {0}.{1}().";
        public const string kUnboundIdentifierMsg = "Variable '{0}' hasn't been defined yet.";
        public const string kFunctionNotReturnAtAllCodePaths = "Method '{0}()' doesn't return at all code paths.";
        public const string kRangeExpressionWithStepSizeZero = "The step size of range expression should not be 0.";
        public const string kRangeExpressionWithInvalidStepSize = "The step size of range expression is invalid.";
        public const string kRangeExpressionWithNonIntegerStepNumber = "The step number of range expression should be integer.";
        public const string kRangeExpressionWithNegativeStepNumber = "The step number of range expression should be greater than 0.";
        public const string kRangeExpressionWithInvalidAmount = "The amount of step is invalid.";
        public const string kRangeExpressionConflictOperator = "The amount operator cannot be used together with step operator.";
        public const string kTypeUndefined = "Type '{0}' is not defined.";
        public const string kMethodAlreadyDefined = "Method '{0}()' is already defined.";
        public const string kReturnTypeUndefined = "Return type '{0}' of method '{1}()' is not defined.";
        public const string kExceptionTypeUndefined = "Exception type '{0}' is not defined.";
        public const string kArgumentTypeUndefined = "Type '{0}' of argument '{1}' is not defined.";
        public const string kInvalidBreakForFunction = "Statement break causes function to abnormally return null.";
        public const string kInvalidContinueForFunction = "Statement continue cause function to abnormally return null.";
        public const string kUsingThisInStaticFunction = "'this' cannot be used in static method.";
        public const string kInvalidThis = "'this' can only be used in member methods.";
        public const string kUsingNonStaticMemberInStaticContext = "'{0}' is not a static property, so cannot be assigned to static properties or used in static methods.";
        public const string kFileNotFound = "File : '{0}' not found";
        public const string kAlreadyImported = "File : '{0}' is already imported";
        public const string kMultipleSymbolFound = "Multiple definitions for '{0}' are found as {1}";
        public const string kMultipleSymbolFoundFromName = "Multiple definitions for '{0}' are found as {1}";


        public const string kArrayOverIndexed = "Variable is over indexed.";
        public const string kIndexOutOfRange = "Index is out of range.";
        public const string kSymbolOverIndexed = "'{0}' is over indexed.";
        public const string kStringOverIndexed = "String is over indexed.";
        public const string kStringIndexOutOfRange = "The index to string is out of range";
        public const string kAssignNonCharacterToString = "Only character can be assigned to a position in a string.";
        public const string kInvokeMethodOnInvalidObject = "Method '{0}()' is invoked on invalid object.";
        public const string kMethodStackOverflow = "Stack overflow caused by calling method '{0}()' recursively.";
        public const string kCyclicDependency = "Cyclic dependency detected at '{0}' and '{1}'.";
        public const string kFFIFailedToObtainThisObject = "Failed to obtain this object for '{0}.{1}'.";
        public const string kFFIFailedToObtainObject = "Failed to obtain object '{0}' for '{1}.{2}'.";
        public const string kFFIInvalidCast = "'{0}' is being cast to '{1}', but the allowed range is [{2}..{3}].";
        public const string kDeferencingNonPointer = "Dereferencing a non-pointer.";
        public const string kFailToConverToPointer = "Converting other things to pointer is not allowed.";
        public const string kFailToConverToNull = "Converting other things to null is not allowed.";
        public const string kFailToConverToFunction = "Converting other things to function pointer is not allowed.";
        public const string kConvertDoubleToInt = "Converting double to int will cause possible information loss.";
        public const string kArrayRankReduction = "Type conversion would cause array rank reduction. This is not permitted outside of replication. {511ED65F-FB66-4709-BDDA-DCD5E053B87F}";
        public const string kConvertArrayToNonArray = "Converting an array to {0} would cause array rank reduction and is not permitted.";
        public const string kConvertNonConvertibleTypes = "Asked to convert non-convertible types.";
        public const string kFunctionNotFound = "No candidate function could be found.";
        public const string kAmbigousMethodDispatch = "Candidate function could not be located on final replicated dispatch GUARD {FDD1F347-A9A6-43FB-A08E-A5E793EC3920}.";
        public const string kInvalidArguments = "Argument is invalid.";
        public const string kInvalidArgumentsInRangeExpression = "The value that used in range expression should be either integer or double.";
        public const string kInvalidAmountInRangeExpression = "The amount in range expression should be an positive integer.";
        public const string kNoStepSizeInAmountRangeExpression = "No step size is specified in amount range expression.";
        public const string kPropertyOfClassNotFound = "Class '{0}' does not have a property '{1}'.";
        public const string kPropertyInaccessible = "Property '{0}' is inaccessible.";
        public const string kMethodResolutionFailure = "Method resolution failure on: {0}() - 0CD069F4-6C8A-42B6-86B1-B5C17072751B.";
        public const string kMethodResolutionFailureWithTypes = "One or more of the input types are not matching, please check that the right variable types are being passed to the inputs. Couldn't find a version of {0} that takes arguments of type {1}.";
        public const string kMethodResolutionFailureForOperator = "Operator '{0}' cannot be applied to operands of type '{1}' and '{2}'.";
        public const string kConsoleWarningMessage = "> Runtime warning: {0}\n - \"{1}\" <line: {2}, col: {3}>";

        public const string FUNCTION_GROUP_RESOLUTION_FAILURE =
            "No function called {0} could be found. Please check the name of the function.";

        public const string tooManyCharacters = "Too many characters in character literal";

        public const string returnStatementIsNotAllowedInConstructor = "return statement is not allowed in constructor";
        public const string unknownAttribute = "Unknown attribute {0}";
        public const string attributeArgMustBeConstant = "An attribute argument must be a constant expression";
        public const string noConstructorForAttribute = "No constructors for Attribute '{0}' takes {1} arguments";
        public const string modifierBlockNotSupported = "Modifier Blocks are not supported currently.";
        public const string importStatementNotSupported = "Import statements are not supported in CodeBlock Nodes.";
        public const string failedToImport = "Failed to import {0}";

        public const string noSuchFileOrDirectoryToImport = "Cannot import file: '{0}': No such file or directory";
        public const string keywordCantBeUsedAsIdentifier = "\"{0}\" is a keyword, identifier expected";
        public const string invalidLanguageBlockIdentifier = "\"{0}\" is not a valid language block identifier, do you mean \"Associative\" or \"Imperative\"?";
        public const string keywordCannotBeUsedAsConstructorName = "\"{0}\" is a keyword, can't be used as constructor name";
        public const string emptyCharacterLiteral = "Empty character literal.";
        public const string functionCallCannotBeAtLeftSide = "function call is not allowed at the left hand side of an assignment";

        public const string useInlineConditional = "'{0}' statement can only be used in imperative language block, consider using an inline conditional instead?";
        public const string validForImperativeBlockOnly = "'{0}' statement can only be used in imperative language block.";
        public const string semiColonExpected = "';' is expected.";
        public const string closeBracketExpected = @"')' expected - Imcomplete Closure";
        public const string invalidSymbol = "Syntax Error: invalid symbol '{0}'. (Did you mean to use Modifier Stack \" => \")";
        public const string baseIsExpectedToCallBaseConstructor = "'base' is expected here to call base constructor.";
        public const string invalidReturnStatement = "Return statement is invalid. Do you mean: return = {0} ?";
    }
}