using System;
using System.Collections.Generic;
using System.Linq;

namespace Dynamo.Engine
{
    /// <summary>
    ///     A group of overloaded functions
    /// </summary>
    public class FunctionGroup
    {
        private readonly List<FunctionDescriptor> functions;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionGroup"/> class.
        /// </summary>
        /// <param name="qualifiedName">Qualified name.</param>
        public FunctionGroup(string qualifiedName)
        {
            functions = new List<FunctionDescriptor>();
            QualifiedName = qualifiedName;
        }

        /// <summary>
        /// Returns qualified name of the corresponding functions
        /// </summary>
        public string QualifiedName { get; private set; }

        /// <summary>
        /// Returns collection of functions with common qualified name
        /// </summary>
        public IEnumerable<FunctionDescriptor> Functions
        {
            get { return functions; }
        }

        /// <summary>
        ///     Add a function descriptor to the group
        /// </summary>
        /// <param name="function"><see cref="FunctionDescriptor"/> object to add</param>
        /// <returns>True if descriptor has been added</returns>
        internal bool AddFunctionDescriptor(FunctionDescriptor function)
        {
            if (!QualifiedName.Equals(function.QualifiedName) || functions.Contains(function))
                return false;

            functions.Add(function);

            if (functions.Count > 1)
            {
                functions[0].IsOverloaded = true;
                functions[functions.Count - 1].IsOverloaded = true;
            }

            return true;
        }

        /// <summary>
        ///     Returns function descriptor from mangled function name
        /// </summary>
        /// <param name="managledName"></param>
        /// <returns></returns>
        internal FunctionDescriptor GetFunctionDescriptor(string managledName)
        {
            if (null == managledName)
                throw new ArgumentNullException();

            if (functions.Count == 0)
                return null;

            FunctionDescriptor func = functions.FirstOrDefault(f => f.MangledName.EndsWith(managledName));
            if (func == null)
            {
                string[] split = managledName.Split('@');
                string[] inputTypes = split.Length > 1 ? split[1].Split(',') : new string[]{};
                return functions.OrderByDescending(f =>
                {
                    return f.Parameters.Select(p => p.Type.ToString())
                                       .Intersect(inputTypes)
                                       .Count();
                }).First();
            }
            return func;
        }

        /// <summary>
        /// Overrides equality check of two <see cref="FunctionGroup"/> objects
        /// </summary>
        /// <param name="obj"><see cref="FunctionGroup"/> object to compare 
        /// with the current one</param>
        /// <returns>Returns true if two <see cref="FunctionGroup"/> objects 
        /// are equals</returns>
        public override bool Equals(object obj)
        {
            if (null == obj || GetType() != obj.GetType())
                return false;

            return QualifiedName.Equals((obj as FunctionGroup).QualifiedName);
        }

        /// <summary>
        ///     Overrides computing the hash code for the <see cref="FunctionGroup"/>
        /// </summary>
        /// <returns>The hash code for this <see cref="FunctionGroup"/></returns>
        public override int GetHashCode()
        {
            return QualifiedName.GetHashCode();
        }
    }
}