using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace Dynamo.GraphMetadata.Controls
{
    /// <summary>
    /// Interaction logic for ImageSelector.xaml
    /// </summary>
    public partial class ImageSelector : UserControl
    {
        private static readonly string DEFAULT_FEEDBACK = Properties.Resources.ImageSelector_Message_DefaultFeedback;
        private static readonly string[] ALLOWED_EXTENSIONS = new[] { ".jpeg", ".jpg", ".png" };
        private readonly string extensionFilter;

        #region DependencyProperties

        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(
            nameof(Image),
            typeof(ImageSource),
            typeof(ImageSelector),
            new FrameworkPropertyMetadata(null,
             FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
             OnSourceImageChanged)
        );

        private static void OnSourceImageChanged(DependencyObject data, DependencyPropertyChangedEventArgs args)
        {
            var control = (ImageSelector)data;
            if (control is null || !(args.NewValue is ImageSource image))
            {
                return;
            }

            control.HasImage = control.Image != null;
        }

        public bool HasImage
        {
            get { return (bool)GetValue(HasImageProperty); }
            set { SetValue(HasImageProperty, value); }
        }

        public static readonly DependencyProperty HasImageProperty = DependencyProperty.Register(
            nameof(HasImage),
            typeof(bool),
            typeof(ImageSelector),
            new PropertyMetadata(false)
        );

        public bool IsDragging
        {
            get { return (bool)GetValue(IsDraggingProperty); }
            set { SetValue(IsDraggingProperty, value); }
        }

        public static readonly DependencyProperty IsDraggingProperty = DependencyProperty.Register(
            nameof(IsDragging),
            typeof(bool),
            typeof(ImageSelector),
            new PropertyMetadata(false)
        );

        public string UserFeedback
        {
            get { return (string)GetValue(UserFeedbackProperty); }
            set { SetValue(UserFeedbackProperty, value); }
        }

        public static readonly DependencyProperty UserFeedbackProperty = DependencyProperty.Register(
            nameof(UserFeedback),
            typeof(string),
            typeof(ImageSelector),
            new PropertyMetadata(DEFAULT_FEEDBACK)
        );

        #endregion DependencyProperties

        #region Constructors

        public ImageSelector()
        {
            InitializeComponent();
            this.AllowDrop = true;
            this.InitializeEvents();

            extensionFilter = String.Join(";", ALLOWED_EXTENSIONS.Select(ext => $"*{ext}"));
        }

        private void InitializeEvents()
        {
            this.Drop += OnDrop;
            this.DragEnter += this.OnDragEnter;
            this.DragLeave += this.OnDragLeave;
            this.btn_ImageSelection.Click += this.OnClick;
        }

        #endregion Constructors

        #region EventDeclaration

        public EventHandler<Image> ImageChanged { get; set; }

        protected void OnImageChanged(Image image) => this.ImageChanged?.Invoke(this, image);

        #endregion EventDeclaration

        #region EventHandling

        private void OnClick(object sender, RoutedEventArgs e)
        {
            var filter = $"Image files ({this.extensionFilter})|{extensionFilter}";
            var dialog = new OpenFileDialog
            {
                Filter = filter,
                Title = Properties.Resources.ImageSelector_Title_SelectImage
            };

            if (dialog.ShowDialog() == true && !String.IsNullOrEmpty(dialog.FileName))
            {
                this.LoadImage(dialog.FileName);
            }
        }

        private void OnDragLeave(object sender, DragEventArgs e)
        {
            this.UserFeedback = DEFAULT_FEEDBACK;
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            var path = this.GetFilePath(e);
            if (String.IsNullOrEmpty(path) || !ALLOWED_EXTENSIONS.Contains(System.IO.Path.GetExtension(path)))
            {
                this.UserFeedback = Properties.Resources.ImageSelector_Message_InvalidExtension;
            }

            else
            {
                this.UserFeedback = Properties.Resources.ImageSelector_Message_DropImage;
            }
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            var path = this.GetFilePath(e);
            if (!String.IsNullOrEmpty(path) && ALLOWED_EXTENSIONS.Contains(System.IO.Path.GetExtension(path)))
            {
                this.LoadImage(path);
            }

            this.OnDragLeave(sender, e);
        }

        #endregion EventHandling

        #region Helpers

        private string GetFilePath(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return ((string[])e.Data.GetData(DataFormats.FileDrop))?.FirstOrDefault();
            }

            return null;
        }

        private void LoadImage(string imagePath)
        {
            if (!Uri.TryCreate(imagePath, UriKind.RelativeOrAbsolute, out Uri uri))
            {
                this.Image = null;
                return;
            }

            try
            {
                this.Image = new BitmapImage(uri);
            }
            catch (Exception)
            {
                this.Image = null;
            }
        }

        #endregion Helpers
    }
}