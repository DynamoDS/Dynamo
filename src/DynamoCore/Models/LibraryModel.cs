using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

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
        private bool _focusable = false;
        public bool Focusable 
        {
            get { return _focusable; }
            set { _focusable = value; } 
        }
    }

    public class ClassObject : ClassObjectBase
    {
        private ObservableCollection<ClassMember> createMembers = null;
        private ObservableCollection<ClassMember> actionMembers = null;
        private ObservableCollection<ClassMember> queryMembers = null;

        internal ClassObject(string name)
        {
            this.Name = name;

            this.createMembers = new ObservableCollection<ClassMember>();
            this.actionMembers = new ObservableCollection<ClassMember>();
            this.queryMembers = new ObservableCollection<ClassMember>();

            this.Focusable = true;
        }

        public string Name { get; private set; }

        public BitmapImage SmallIcon
        {
            get
            {
                //TODO: remove loading image from defined path
                string startupPath = Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName).FullName;
                MemoryStream memory = new MemoryStream();
                Bitmap bitmap = new Bitmap(startupPath + @"\src\DynamoCore\Resources\Icons\Saturation.png");
                BitmapImage bitmapImage = new BitmapImage();
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        public int Index { get; set; }

        public ObservableCollection<ClassMember> CreateMembers
        {
            get { return this.createMembers; }
        }

        public ObservableCollection<ClassMember> ActionMembers
        {
            get { return this.actionMembers; }
        }

        public ObservableCollection<ClassMember> QueryMembers
        {
            get { return this.queryMembers; }
        }
    }

    public class ClassDetails : ClassObjectBase
    {
        internal ClassDetails()
        {
            // Class details is by default hidden.
            ClassDetailsVisibility = false;
        }

        public bool ClassDetailsVisibility { get; set; }

        public ObservableCollection<ClassMember> CreateMembers { get; private set; }
        public ObservableCollection<ClassMember> ActionMembers { get; private set; }
        public ObservableCollection<ClassMember> QueryMembers { get; private set; }

        public void AddCreateMembers(ObservableCollection<ClassMember> members)
        {
            this.CreateMembers = members;
        }
        public void AddActionMembers(ObservableCollection<ClassMember> members)
        {
            this.ActionMembers = members;
        }
        public void AddQueryMembers(ObservableCollection<ClassMember> members)
        {
            this.QueryMembers = members;
        }
    }

    public class ClassMember
    {
        internal ClassMember(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; }

        public BitmapImage SmallIcon
        {
            get
            {
                //TODO: remove loading image from defined path, use LibraryCustomizationServices
                string startupPath = Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName).FullName;
                MemoryStream memory = new MemoryStream();
                Bitmap bitmap = new Bitmap(startupPath + @"\src\DynamoCore\Resources\Icons\Hue.png");
                BitmapImage bitmapImage = new BitmapImage();
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
            set { }
        }
    }
}
