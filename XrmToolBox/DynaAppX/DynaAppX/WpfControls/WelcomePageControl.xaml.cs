using System;
using System.Windows;
using System.Windows.Controls;

namespace DynaAppX.WpfControls
{
    public partial class WelcomePageControl : UserControl
    {
        public event Action<string> OpenFeatureRequested;

        public WelcomePageControl()
        {
            InitializeComponent();
        }

        private void BtnAccessCheck_Click(object sender, RoutedEventArgs e)
        {
            OpenFeatureRequested?.Invoke("AccessCheck");
        }

        private void BtnInvokeFlow_Click(object sender, RoutedEventArgs e)
        {
            OpenFeatureRequested?.Invoke("InvokeFlow");
        }

        private void BtnGodPage_Click(object sender, RoutedEventArgs e)
        {
            OpenFeatureRequested?.Invoke("GodPage");
        }

        private void BtnMetadataBrowser_Click(object sender, RoutedEventArgs e)
        {
            OpenFeatureRequested?.Invoke("MetadataBrowser");
        }
    }
}
