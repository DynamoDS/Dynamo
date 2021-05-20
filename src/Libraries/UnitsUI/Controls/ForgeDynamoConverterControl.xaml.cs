using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using CoreNodeModels;
using Dynamo.Controls;
using DynamoUnits;
using UnitsUI;

namespace UnitsUI.Controls
{
    /// <summary>
    /// Interaction logic for ConverterControl.xaml
    /// </summary>
    public partial class ForgeDynamoConverterControl : UserControl
    {
        public ForgeDynamoConverterControl(ForgeDynamoConvert Model, NodeView nodeView)
        {
            InitializeComponent();          
        }      
    }
}
