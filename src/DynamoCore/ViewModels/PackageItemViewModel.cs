﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Dynamo.Nodes;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.PackageManager.UI
{
    public abstract class PackageItemViewModel : NotificationObject
    {
        public abstract ObservableCollection<PackageItemViewModel> Items { get; set; }

        /// <summary>
        /// The height of the element in search
        /// </summary>
        private int _height = 30;
        public int Height
        {
            get { return _height; }
            set
            {
                _height = value;
                RaisePropertyChanged("Height");
            }
        }

        /// <summary>
        ///     Adds an element as a child of this one, while updating its parent and oldparent field
        /// </summary>
        /// <param name="elem">The element in question</param>
        public void AddChild(PackageItemInternalViewModel elem)
        {
            if (elem.Parent != null)
                elem.Parent.Items.Remove(elem);

            elem.Parent = this;
            this.Items.Add(elem);
        }

        /// <summary>
        /// Whether the item is visible or not
        /// </summary>
        private Visibility _visibility = Visibility.Visible;
        public Visibility Visibility
        {
            get
            {
                return _visibility;
            }
            set
            {
                _visibility = value;
                RaisePropertyChanged("Visibility");
            }
        }

        /// <summary>
        /// Whether the item is selected or not
        /// </summary>
        private bool _isSelected = false;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }

        /// <summary>
        /// Is the element expanded in the browser
        /// </summary>
        private bool _isExpanded = false;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                RaisePropertyChanged("IsExpanded");
            }
        }

        public ToggleIsExpandedCommand _toggleIsExpanded;
        public ToggleIsExpandedCommand ToggleIsExpanded
        {
            get
            {
                if (_toggleIsExpanded == null)
                    _toggleIsExpanded = new ToggleIsExpandedCommand(this);
                return _toggleIsExpanded;
            }
        }

        public class ToggleIsExpandedCommand : ICommand
        {
            private PackageItemViewModel _viewModel;

            public ToggleIsExpandedCommand(PackageItemViewModel i)
            {
                this._viewModel = i;
            }

            public void Execute(object parameters)
            {
                var endState = !_viewModel.IsExpanded;
                _viewModel.IsExpanded = endState;
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public bool CanExecute(object parameters)
            {
                return true;
            }
        }

        /// <summary>
        /// Name property </summary>
        /// <value>
        /// The name of the node </value>
        public string Name
        {
            get
            {
                if (DependencyType == DependencyType.CustomNode)
                {
                    return Definition.WorkspaceModel.Name;
                }
                return Assembly.FullName;
            }
        }

        /// <summary>
        /// Enumerate the dependencies of this item as its children.  The 
        /// </summary>
        public void BuildDependencies( HashSet<object> discoveredDeps )
        {
            this.Items.Clear();

            if (DependencyType == DependencyType.CustomNode)
            {
                foreach (var dep in Definition.DirectDependencies)
                {
                    var discovered = discoveredDeps.Contains(dep);
                    var packDep = new PackageItemInternalViewModel(dep, this)
                        {
                            AlreadyDiscovered = discovered
                        };
                    if (!discovered)
                    {
                        discoveredDeps.Add(this);
                        packDep.BuildDependencies(discoveredDeps);
                    }
                    this.Items.Add(packDep);
                }
            }
            else if (DependencyType == DependencyType.Assembly)
            {
                foreach (var dep in Assembly.GetReferencedAssemblies())
                {
                    var depAss = Assembly.Load(dep);
                    var discovered = discoveredDeps.Contains(dep);
                    var packDep = new PackageItemInternalViewModel(depAss, this)
                    {
                        AlreadyDiscovered = discovered
                    };
                    if (!discovered)
                    {
                        discoveredDeps.Add(this);
                        packDep.BuildDependencies(discoveredDeps);
                    }
                    this.Items.Add(packDep);
                }
            }
        }

        public bool AlreadyDiscovered { get; set; }
        public DependencyType DependencyType { get; protected set; }
        public Assembly Assembly { get; protected set; }
        public FunctionDefinition Definition { get; protected set; }

    }
}
