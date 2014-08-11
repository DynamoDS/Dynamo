using System;
using System.Windows.Controls;
using Dynamo.Models;

namespace Dynamo.UI.Views
{
    /// <summary>
    /// Interaction logic for LibraryView.xaml
    /// </summary>
    public partial class LibraryView : UserControl
    {
        private readonly Random random = new Random();
        private readonly LibraryModel libraryData = new LibraryModel();

        public LibraryView()
        {
            InitializeComponent();
            CreateInitialData();
            CategoryListView.ItemsSource = libraryData.Categories;
        }

        private void CreateInitialData()
        {
            // Create some categories...
            for (int c = 0; c < 10; c++)
            {
                var category = new Category(string.Format("Category #{0}", c));
                libraryData.Add(category);

                // Generate some classes.
                var objects = random.Next(10, 20);
                for (int o = 0; o < objects; o++)
                {
                    var classObject = new ClassObject(string.Format("Class #{0}", o));
                    category.Add(classObject);

                    // Generate some data members.
                    var members = random.Next(1, 6);
                    for (int m = 0; m < members; m++)
                    {
                        classObject.CreateMembers.Add(new ClassMember(string.Format("CMember #{0}", m)));
                    }
                    members = random.Next(1, 6);
                    for (int m = 0; m < members; m++)
                    {
                        classObject.ActionMembers.Add(new ClassMember(string.Format("AMember #{0}", m)));
                    }
                    members = random.Next(1, 6);
                    for (int m = 0; m < members; m++)
                    {
                        classObject.QueryMembers.Add(new ClassMember(string.Format("QMember #{0}", m)));
                    }
                }
            }
        }
    }
}
