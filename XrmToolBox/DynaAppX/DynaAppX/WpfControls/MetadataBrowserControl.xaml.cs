using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace DynaAppX.WpfControls
{
    public partial class MetadataBrowserControl : UserControl
    {
        private IOrganizationService _service;
        private string _crmUrl = "";

        public event Action<string> CrmUrlRequest;

        public MetadataBrowserControl()
        {
            InitializeComponent();
            this.Loaded += MetadataBrowserControl_Loaded;
        }

        public void SetService(IOrganizationService service)
        {
            _service = service;
            // Request URL from parent
            CrmUrlRequest?.Invoke(this);
        }

        public void SetCrmUrl(string crmUrl)
        {
            _crmUrl = crmUrl.TrimEnd('/');
            txtStatus.Text = string.IsNullOrEmpty(_crmUrl)
                ? "CRM URL not available. Connect to CRM first."
                : "Ready. Click a button to open the Metadata Browser.";
        }

        private void MetadataBrowserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_crmUrl))
            {
                txtStatus.Text = "Waiting for CRM connection...";
            }
        }

        private string GetCrmUrl()
        {
            return _crmUrl;
        }

        private void BtnOpenMetadataBrowser_Click(object sender, RoutedEventArgs e)
        {
            OpenMetadataBrowserUrl();
        }

        private void BtnOpenEntityMetadataBrowser_Click(object sender, RoutedEventArgs e)
        {
            OpenEntityMetadataBrowserUrl();
        }

        private void OpenMetadataBrowserUrl()
        {
            var baseUrl = GetCrmUrl();
            if (string.IsNullOrEmpty(baseUrl))
            {
                txtStatus.Text = "Error: CRM URL not available. Please connect to CRM.";
                MessageBox.Show(
                    "CRM URL is not available. Please connect to a CRM organization first.",
                    "CRM URL Not Set",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var url = $"{baseUrl}/WebResources/dax_/metadatabrowser/MetaDataBrowser.htm";
            OpenUrl(url);
            txtStatus.Text = $"Opened: {url}";
        }

        private void OpenEntityMetadataBrowserUrl()
        {
            var baseUrl = GetCrmUrl();
            if (string.IsNullOrEmpty(baseUrl))
            {
                txtStatus.Text = "Error: CRM URL not available. Please connect to CRM.";
                MessageBox.Show(
                    "CRM URL is not available. Please connect to a CRM organization first.",
                    "CRM URL Not Set",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var url = $"{baseUrl}/WebResources/dax_/metadatabrowser/EntityMetaDataBrowser.htm";
            OpenUrl(url);
            txtStatus.Text = $"Opened: {url}";
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error opening URL: {ex.Message}";
                MessageBox.Show(
                    $"Failed to open URL:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}