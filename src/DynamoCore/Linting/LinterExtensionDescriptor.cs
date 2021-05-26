using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Linting
{
    public class LinterExtensionDescriptor
    {
        /// <summary>
        /// Id of linter extension
        /// </summary>
        public string Id { get; }

        public string Name { get; }

        public LinterExtensionDescriptor(string id, string name)
        {
            Id = id;
            Name = name;
        }

        /// <summary>
        /// Checks whether two LinterExtensionDescriptor are equal
        /// They are equal if their Name and Id are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is LinterExtensionDescriptor))
            {
                return false;
            }

            var other = obj as LinterExtensionDescriptor;
            if (other.Name == this.Name && other.Id == this.Id)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the hashcode for this LinterExtensionDescriptor
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Id.GetHashCode();
        }
    }
}
