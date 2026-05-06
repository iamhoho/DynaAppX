using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DynaAppX.WpfControls
{
    public partial class MetadataBrowserControl : UserControl
    {
        private string _crmUrl;

        public MetadataBrowserControl()
        {
            InitializeComponent();
        }

        public void SetCrmUrl(string crmUrl)
        {
            _crmUrl = crmUrl;
        }

        private void btnOpenEntityMetadata_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_crmUrl))
            {
                MessageBox.Show("CRM URL not available. Please connect to a CRM instance first.",
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var url = $"{_crmUrl}/WebResources/dax_/metadatabrowser/EntityMetaDataBrowser.htm";
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening metadata browser: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnOpenMetadata_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_crmUrl))
            {
                MessageBox.Show("CRM URL not available. Please connect to a CRM instance first.",
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var url = $"{_crmUrl}/WebResources/dax_/metadatabrowser/MetaDataBrowser.htm";
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening metadata browser: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
