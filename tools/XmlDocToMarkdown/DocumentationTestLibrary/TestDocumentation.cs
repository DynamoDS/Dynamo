using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;

namespace Dynamo.DocumentationTestLibrary
{
    public class TestDocumentation
    {
        /// <summary>
        /// The persons
        /// </summary>
        /// <value>
        /// The persons.
        /// </value>
        public List<int> Persons { get; set; }

        /// <summary> 
        /// Mks the array.
        /// </summary>        
        /// <param name="n">The index.</param>
        /// <returns>New Array</returns>
        /// <search>Testing</search>          
        /// <remarks>Testing remarks tag</remarks>        
        /// <typeparam name="T">The element type of the array</typeparam>
        public T[] mkArray<T>(int n)
        {
            return new T[n];
        }
    }

    //public class Person
    //{
    //    /// <summary>
    //    /// The name
    //    /// </summary>
    //    public string name;

    //    /// <summary>
    //    /// The age
    //    /// </summary>
    //    public int age;
    //}
}
