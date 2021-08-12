using System.Windows;

namespace Dynamo.Wpf.UI.GuidedTour
{
    public class StepUIAutomation
    {
        public double Sequence { get; set; }
        public string ControlType { get; set; }
        public string Name { get; set; }
        public string Action { get; set; }
        public UIElement UIElementAutomation { get; set; }
    }
}
