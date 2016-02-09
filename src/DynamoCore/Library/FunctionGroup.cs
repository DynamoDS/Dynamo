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

        public FunctionGroup(string qualifiedName)
        {
            functions = new List<FunctionDescriptor>();
            QualifiedName = qualifiedName;
        }

        public string QualifiedName { get; private set; }

        public IEnumerable<FunctionDescriptor> Functions
        {
            get { return functions; }
        }

        /// <summary>
        ///     Add a function descriptor to the group
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
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
        ///     Get function descriptor from mangled function name
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

        public override bool Equals(object obj)
        {
            if (null == obj || GetType() != obj.GetType())
                return false;

            return QualifiedName.Equals((obj as FunctionGroup).QualifiedName);
        }

        public override int GetHashCode()
        {
            return QualifiedName.GetHashCode();
        }
    }
}