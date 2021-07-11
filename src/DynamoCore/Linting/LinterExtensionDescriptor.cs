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
            
            if (!(obj is LinterExtensionDescriptor other))
            {
                return false;
            }

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

        private const string NONE_DESCRIPTOR_GUID = "7b75fb44-43fd-4631-a878-29f4d5d8399a";
        
        internal static LinterExtensionDescriptor DefaultDescriptor => new LinterExtensionDescriptor(NONE_DESCRIPTOR_GUID, Properties.Resources.NoneLinterDescriptorName);
    }
}
