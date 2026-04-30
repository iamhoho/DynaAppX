using System;
using System.Windows;
using System.Windows.Controls;

namespace DynaAppX.WpfControls
{
    public partial class WpfTestControl : UserControl
    {
        public WpfTestControl()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var message = txtMessage.Text;
            lstLog.Items.Insert(0, new ListBoxItem { Content = $"Button clicked at {DateTime.Now}: {message}" });
            txtMessage.Clear();
        }
    }
}
