using System.Collections.ObjectModel;
using System.Windows;

namespace Dynamo.Models
{
    public class LibraryModel
    {
        private ObservableCollection<Category> categories = null;

        internal LibraryModel()
        {
        }

        internal void Add(Category category)
        {
            if (this.categories == null)
                this.categories = new ObservableCollection<Category>();

            this.categories.Add(category);
        }

        public ObservableCollection<Category> Categories
        {
            get { return this.categories; }
        }
    }

    public class Category
    {
        private ObservableCollection<ClassObjectBase> classes = null;

        internal Category(string name)
        {
            this.Name = name;
        }

        internal void Add(ClassObjectBase classObjectBase)
        {
            if (this.classes == null)
            {
                this.classes = new ObservableCollection<ClassObjectBase>
                {
                    // When we first create the list of ClassObject, we also 
                    // create an additional ClassDetails class so that during 
                    // data binding, we get the additional StandardPanel for 
                    // this item.
                    // 
                    new ClassDetails()
                };
            }

            this.classes.Add(classObjectBase);
        }

        public string Name { get; private set; }

        public ObservableCollection<ClassObjectBase> Classes
        {
            get { return this.classes; }
        }
    }

    public class ClassObjectBase
    {
        // Dummy base class so "ClassObject" and "ClassDetails" 
        // can be bound to the list view in a single collection.
    }

    public class ClassObject : ClassObjectBase
    {
        private ObservableCollection<ClassMember> members = null;

        internal ClassObject(string name)
        {
            this.Name = name;
        }

        internal void Add(ClassMember member)
        {
            if (this.members == null)
                this.members = new ObservableCollection<ClassMember>();

            this.members.Add(member);
        }

        public string Name { get; private set; }

        public ObservableCollection<ClassMember> Members
        {
            get { return this.members; }
        }
    }

    public class ClassDetails : ClassObjectBase
    {
        internal ClassDetails()
        {
            // Class details is by default hidden.
            ClassDetailsVisibility = Visibility.Hidden;
        }

        // TODO: Add a method to set the list of "ClassMember" here so the 
        // data template (i.e. StandardPanel) gets updated accordingly.

        public Visibility ClassDetailsVisibility { get; set; }
    }

    public class ClassMember
    {
        internal ClassMember(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; }
    }
}
