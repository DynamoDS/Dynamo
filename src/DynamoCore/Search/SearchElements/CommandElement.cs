using System.Collections.Generic;
using System.Windows.Input;
using String = System.String;

namespace Dynamo.Search.SearchElements
{
    /// <summary>
    /// A search element representing a command to execute</summary>
    public class CommandElement : SearchElementBase
    {

        #region Properties

        /// <summary>
        /// Command property </summary>
        /// <value>
        /// The command to be executed by this search element </value>
        public ICommand Command { get; private set; }

        /// <summary>
        /// Type property </summary>
        /// <value>
        /// A string describing the type of object </value>
        private string _type;
        public override string Type { get { return _type; } }

        /// <summary>
        /// Name property </summary>
        /// <value>
        /// The name of the node </value>
        private string _name;
        public override string Name { get { return _name; } }

        /// <summary>
        /// Description property </summary>
        /// <value>
        /// A string describing what the node does</value>
        private string _description;
        public override string Description { get { return _description; } }

        public override bool Searchable { get { return true; } }

        /// <summary>
        /// Weight property </summary>
        /// <value>
        /// Number defining the relative importance of the element in search.  Higher weight means closer to the top. </value>
        public override double Weight { get; set; }

        /// <summary>
        /// Keywords property </summary>
        /// <value>
        /// Joined set of keywords </value>
        public override string Keywords { get; set; }

        #endregion

        /// <summary>
        ///     The class constructor.
        /// </summary>
        /// <param name="name">The name of the command as shown in search</param>
        /// <param name="description">A description of what the command does, to be shown in search.</param>
        /// <param name="tags">Some descriptive terms to be shown in search.</param>
        /// <param name="command">The command to be execute in the Execute() method - with no parameters</param>
        public CommandElement(string name, string description, IEnumerable<string> tags, ICommand command)
        {
            Command = command;
            _name = name;
            Weight = 1.2;
            Keywords = String.Join(" ", tags);
            _type = "Command";
            _description = description;
        }

        /// <summary>
        /// Executes the element in search, this is what happens when the user 
        /// hits enter in the SearchView.</summary>
        public override void Execute()
        {
            //DynamoSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(this.Command, null));
            //DynamoSettings.Controller.ProcessCommandQueue();

            Command.Execute(null);
        }

    }

}
