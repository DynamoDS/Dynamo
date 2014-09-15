using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Dynamo.TestInfrastructure
{
    public class RunAllTests : INotifyPropertyChanged
    {
        private string name = "Run all tests";

        public string Name
        {
            get { return name; }
        }

        private bool isSelected = true;

        public virtual bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                NotifyPropertyChanged("IsSelected");

                if (IsUpdateNeeded)
                    CheckChange(value);
            }
        }

        private bool isUpdateNeeded = true;

        public bool IsUpdateNeeded
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


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        private void CheckChange(bool selected)
        {
            List<AbstractMutator> mutators = new List<AbstractMutator>();
            foreach (CollectionContainer container in MutatorDriver.Instance.Collection)
            {
                var objs = container.Collection.OfType<AbstractMutator>();
                if (objs != null)
                    mutators.AddRange(objs);
            }

            foreach (var mutator in mutators)
            {
                mutator.IsUpdateNeeded = false;
                mutator.IsSelected = selected;
                mutator.IsUpdateNeeded = true;
            }
        }
    }
}
