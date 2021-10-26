using Dynamo.Wpf.UI.GuidedTour;
using System;
using Dynamo.Wpf.Properties;


namespace Dynamo.Wpf.ViewModels.GuidedTour
{
    public class ExitGuideWindowViewModel
    {
        private double height = 128;
        private double width = 400;
        private string title = "PackagesGuideExitTitle";
        private string formattedText;

        public double Height { get => height; set => height = value; }
        public double Width { get => width; set => width = value; }
        public string Title { get => title; set => title = value; }
        public string FormattedText
        {
            get => formattedText;
            set
            {
                //Because we are reading the value from a Resource, the \n is converted to char escaped and we need to replace it by the special char
                if (value != null)
                    formattedText = value.Replace("\\n", Environment.NewLine);
            }
        }

        public ExitGuideWindowViewModel(ExitGuide exitGuide)
        {
            if (exitGuide != null)
            {
                Height = exitGuide.Height;
                Width = exitGuide.Width;
                Title = Resources.ResourceManager.GetString(exitGuide.Title);
                FormattedText = Resources.ResourceManager.GetString(exitGuide.FormattedText);
            }
            else
            {
                Title = Resources.ResourceManager.GetString(Title);
            }
        }
    }
}
