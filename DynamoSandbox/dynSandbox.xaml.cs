using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using Dynamo.Elements;
using Dynamo.Controls;
using Dynamo;

namespace DynamoSandbox
{
    /// <summary>
    /// Interaction logic for dynSandbox.xaml
    /// </summary>
    public partial class dynSandbox : Window
    {
        SortedDictionary<string, TypeLoadData> builtinTypes = new SortedDictionary<string, TypeLoadData>();

        public dynSandbox()
        {
            InitializeComponent();
            LoadBuiltInTypes();
        }

        public void LoadBuiltInTypes()
        {
            //setup the menu with all the types by reflecting
            //the DynamoElements.dll
            Assembly elementsAssembly = Assembly.LoadFrom(@".\DynamoElements.dll");

            Type[] loadedTypes = elementsAssembly.GetTypes();

            foreach (Type t in loadedTypes)
            {
                //only load types that are in the right namespace, are not abstract
                //and have the elementname attribute
                object[] attribs = t.GetCustomAttributes(typeof(ElementNameAttribute), false);

                if (t.Namespace == "Dynamo.Elements" &&
                    !t.IsAbstract &&
                    attribs.Length > 0 &&
                    t.IsSubclassOf(typeof(dynNode)))
                {
                    string typeName = (attribs[0] as ElementNameAttribute).ElementName;
                    builtinTypes.Add(typeName, new TypeLoadData(elementsAssembly, t));
                }
            }
        }
    }
}
