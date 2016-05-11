using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PluginManager
{
<<<<<<< HEAD
    public static class TextBoxPromptDialog
    {
        public static string ShowDialog(string text, string caption)
        {
            Form prompt = new Form()
            {
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Width = 150,
                Height = 150,
                Text = caption                
            };
            Label textLabel = new Label() { Left = 20, Top = 20, Text = text };
            TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 90};
            Button confirmation = new Button() { Text = "Ok", Left = 20, Width = 90, Top = 70, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
=======
   public static class TextBoxPromptDialog
    {
       public static string ShowDialog(string text, string caption)
       {
           Form prompt = new Form()
           {
               Width = 500,
               Height = 150,
               FormBorderStyle = FormBorderStyle.FixedDialog,
               Text = caption,
               StartPosition = FormStartPosition.CenterScreen
           };
           Label textLabel = new Label() { Left = 50, Top = 20, Text = text };
           TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 400 };
           Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 70, DialogResult = DialogResult.OK };
           confirmation.Click += (sender, e) => { prompt.Close(); };
           prompt.Controls.Add(textBox);
           prompt.Controls.Add(confirmation);
           prompt.Controls.Add(textLabel);
           prompt.AcceptButton = confirmation;

           return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
       }
>>>>>>> d0901afa26af77467b285c036ec39a57e9e14f02
    }
}
