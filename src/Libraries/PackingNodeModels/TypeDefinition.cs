using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackingNodeModels
{
    /// <summary>
    /// Defines a Type by its name and its properties.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class TypeDefinition
    {
        public string Name { get; set; }

        public Dictionary<string, PropertyType> Properties { get; set; }
    }

    /// <summary>
    /// Define a Type as a string name and whether it is a collection or not.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class PropertyType
    {
        public string Type { get; set; }
        public bool IsCollection { get; set; }

        public override string ToString()
        {
            return Type + (IsCollection ? "[]" : "");
        }
    }

    /// <summary>
    /// Typescript-like parser to create TypeDefinitions out of formatted strings.
    /// i.e.:
    /// "Type MyType {
    ///    property1 : String,
    ///    property2 : Float64,
    ///    property3 : Point[]
    ///  }"
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    internal class TypeDefinitionParser
    {
        private const string OpeningKeyword = "Type";

        private static readonly Parser<string> TypeKeyword =
            from leading in Parse.WhiteSpace.Many()
            from keyword in Parse.String(OpeningKeyword)
            from trailing in Parse.WhiteSpace.Many()
            select new string(keyword.ToArray());

        private static readonly Parser<string> TypeName =
            from type in Parse.CharExcept(new[] { '{', '}', ',', '[', ']' }).AtLeastOnce()
            from trailing in Parse.WhiteSpace.Many()
            select new string(type.ToArray()).Trim();

        private static readonly Parser<KeyValuePair<string, PropertyType>> Property =
            from leading in Parse.WhiteSpace.Many()
            from firstLetter in Parse.Letter.AtLeastOnce()
            from rest in Parse.LetterOrDigit.Many()
            from seperator in Colon
            from type in TypeName
            from isCollection in Parse.String("[]").Optional()
            from trailing in Parse.WhiteSpace.Many()
            select new KeyValuePair<string, PropertyType>(new string(firstLetter.ToArray()) + new string(rest.ToArray()), new PropertyType { Type = new string(type.ToArray()).Trim(), IsCollection = !isCollection.IsEmpty });

        private static readonly Parser<char> OpeningCurlyBracket = Operator('{');
        private static readonly Parser<char> ClosingCurlyBracket = Operator('}');
        private static readonly Parser<char> Colon = Operator(':');
        private static readonly Parser<char> Comma = Operator(',');

        private static Parser<char> Operator(char op)
        {
            return
                from leading in Parse.WhiteSpace.Many()
                from bracket in Parse.Char(op)
                from trailing in Parse.WhiteSpace.Many()
                select bracket;
        }

        private static Parser<TypeDefinition> TypeDefinition =
            from keyword in TypeKeyword
            from name in TypeName
            from oB in OpeningCurlyBracket
            from properties in Property.DelimitedBy(Comma)
            from cB in ClosingCurlyBracket.End()
            select new TypeDefinition { Name = name, Properties = properties.ToDictionary(pair => pair.Key, pair => pair.Value) };

        /// <summary>
        /// Parses a string and returns the corresponding TypeDefinition or throws a Sprache.ParseException.
        /// </summary>
        /// <param name="text">Formatted string definining a TypeDefinition</param>
        /// <returns>The parsed TypeDefinition</returns>
        public static TypeDefinition ParseType(string text)
        {
            return TypeDefinition.Parse(text);
        }
    }
}
