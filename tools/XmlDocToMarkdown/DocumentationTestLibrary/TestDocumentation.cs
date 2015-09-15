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
    #region Interface
    /// <summary>
    ///  Has an event that produces items.
    /// </summary>
    /// <typeparam name="TItem">Type of items produced.</typeparam>
    public interface ISource<out TItem>
    {
        /// <summary>
        ///     Produces items, potentially asynchronously.
        /// </summary>
        event Action<TItem> ItemProduced;
    }

    #endregion

    public class TestDocumentation
    {


        #region properties

        /// <summary>
        /// Testing property
        /// </summary>
        public int property1 { get; set; }

        #endregion

        #region events
        public delegate void ChangedEventHandler(object sender, EventArgs e);

        /// <summary>
        /// Testing Events
        /// </summary>
        public event ChangedEventHandler Changed;

        #endregion

        /// <summary> 
        /// Mks the array.
        /// </summary>        
        /// <param name="n">The index.</param>
        /// <returns>New Array</returns>
        /// <search> Testing Search Tag </search>          
        /// <remarks>Testing remarks tag</remarks>        
        /// <typeparam name="T">The element type of the array</typeparam>
        /// <api_stability>2</api_stability>       
        public T[] TestingGenerics<T>(int n)
        {
            return new T[n];
        }

        /// <summary>
        /// Tests the hyper link.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns></returns>
        public Person TestHyperLink(Person p)
        {
            return null;
        }
    }

    public class Person
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the age.
        /// </summary>
        /// <value>
        /// The age.
        /// </value>
        public int age { get; set; }
    }
}
