using Dynamo.Models;
using Dynamo.ViewModels;
using System;
using System.IO;
using System.ComponentModel;
using System.Linq;
using Microsoft.Practices.Prism.ViewModel;
using System.Collections.Generic;
using System.Windows.Data;

namespace Dynamo.TestInfrastructure
{
    public abstract class AbstractMutator : INotifyPropertyChanged
    {
        //Convenience state, the presence of this state cache means that
        //usage of this mutator should be short lived
        protected DynamoViewModel DynamoViewModel;
        protected DynamoModel DynamoModel;

        protected AbstractMutator(DynamoViewModel dynamoViewModel)
        {
            this.DynamoViewModel = dynamoViewModel;
            this.DynamoModel = dynamoViewModel.Model;
        }

        /// <summary>
        /// Returns the number of undoable operations that have been performed 
        /// </summary>
        /// <returns></returns>
        public abstract int Mutate(NodeModel node);

        /// <summary>
        /// Returns the test result flag
        /// </summary>
        /// <returns></returns>
        public abstract bool RunTest(NodeModel node, StreamWriter writer);

        public virtual Type GetNodeType()
        {
            return typeof(NodeModel);
        }

        /// <summary>
        /// Number of runs
        /// </summary>
        /// <returns></returns>
        public virtual int NumberOfLaunches
        {
            get { return 1000; }
        }

        private string name = string.Empty;

        public string Name
        {
            get
            {
                return ((MutationTestAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(MutationTestAttribute))).Caption;
            }
        }

        private bool isUpdateNeeded = true;

        public bool IsRunAllUpdateNeeded
        {
            get
            {
                return isUpdateNeeded;
            }
            set
            {
                isUpdateNeeded = value;
            }
        }

        private bool isSelected = true;

        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                isSelected = value;
                NotifyPropertyChanged("IsSelected");

                if (IsRunAllUpdateNeeded && !isSelected)
                    CheckChange();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        private void CheckChange()
        {
            List<RunAllTests> runAllTestObj = new List<RunAllTests>();
            foreach (CollectionContainer container in DynamoViewModel.MutatorDriver.Collection)
            {
                var objs = container.Collection.OfType<RunAllTests>();
                if (objs != null)
                    runAllTestObj.AddRange(objs);
            }

            foreach (var item in runAllTestObj)
            {
                item.IsMutatorsUpdateNeeded = false;
                item.IsSelected = isSelected;
                item.IsMutatorsUpdateNeeded = true;
            }
        }
    }
}
