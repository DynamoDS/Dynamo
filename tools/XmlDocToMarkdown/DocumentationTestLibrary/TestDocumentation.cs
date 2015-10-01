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
    /// Interface with generics
    /// </summary>
    /// <typeparam name="TItem">Type of items produced.</typeparam>
    public interface ISource<out TItem>
    {
        /// <summary>
        ///     Produces items, potentially asynchronously.
        /// </summary>
        event Action<TItem> ItemProduced;
    }

    /// <summary>
    /// Interface without generics
    /// </summary>
    public interface IPackage
    {
        /// <summary>
        /// Tests the method in interface.
        /// </summary>
        void TestMethodInInterface();
    }

    #endregion

    public abstract class TestDocumentation
    {
        public enum TaskPriority
        {
            Critical,
            Highest,          
        }

        #region properties

        /// <summary>
        /// Testing property
        /// </summary>
        public int property1
        {
            get { return 0; }
        }

        /// <summary>
        /// Gets the priority.
        /// </summary>       
        public abstract TaskPriority Priority { get; }

        /// <summary>
        /// Gets or sets the abstract property.
        /// </summary>
        /// <value>
        /// The abstract property.
        /// </value>
        public abstract string AbstractProperty { get; set; }

        #endregion

        #region events
        /// <summary>
        /// Delegate
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public delegate void ChangedEventHandler(object sender, EventArgs e);

        /// <summary>
        /// Testing Events
        /// </summary>
        public event ChangedEventHandler Changed;

        #endregion

        /// <summary>
        /// Testing Constructor without params.
        /// </summary>
        public TestDocumentation()
        {

        }

        /// <summary>
        /// Testing Constructor with params.
        /// </summary>
        /// <param name="test">The test.</param>
        public TestDocumentation(int test)
        {

        }

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

        //<summary>
        //Tests the method that has no param set.
        //</summary>       
        public void TestMethodWithNoParaminXML(object o)
        {

        }

        /// <summary>
        /// Tests the method without parameters.
        /// </summary>
        public void TestMethodWithoutParameters()
        {

        }

        /// <summary>
        /// Tests the with multiple parameters.
        /// </summary>
        /// <param name="test1">The test1.</param>
        /// <param name="test2">The test2.</param>
        /// <param name="test3">The test3.</param>
        public void TestWithMultipleParameters(int test1, int test2, int test3)
        {
            
        }

        /// <summary>
        /// Testings the abstract method.
        /// </summary>
        public abstract void TestingAbstractMethod();

        /// <summary>
        /// Testings the virtual method.
        /// </summary>
        public virtual void TestingVirtualMethod()
        {
            
        }

        /// <summary>
        /// Tests the method with collections.
        /// </summary>
        /// <returns>List of items</returns>
        public List<Person> TestMethodWithCollections()
        {
            return null;
        }

        /// <summary>
        /// Tests the type of the method with dictionary return.
        /// </summary>
        /// <returns>Key value pair</returns>
        public Dictionary<Person, String> TestMethodWithDictReturnType()
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

        /// <summary>
        /// Gets or sets the list persons.
        /// </summary>
        /// <value>
        /// The list persons.
        /// </value>
        public List<Person> ListPersons { get; set; }

        /// <summary>
        /// Gets or sets the virtual property.
        /// </summary>
        /// <value>
        /// The virtual property.
        /// </value>
        public virtual int VirtualProperty { get; set; }

        
        /// <summary>
        /// Initializes a new instance of the <see cref="Person"/> class.
        /// </summary>
        public Person()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Person"/> class.
        /// </summary>
        /// <param name="anothertest">The anothertest.</param>
        public Person(int anothertest)
        {

        }

    }    
}
