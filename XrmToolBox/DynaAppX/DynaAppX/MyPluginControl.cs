using McTools.Xrm.Connection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using XrmToolBox.Extensibility;
using DynaAppX.WpfControls;

namespace DynaAppX
{
    public partial class MyPluginControl : PluginControlBase
    {
        private Settings mySettings;
        private AccessCheckControl accessCheckControl;
        private InvokeFlowControl invokeFlowControl;
        private GodPageControl godPageControl;
        private MetadataBrowserControl metadataBrowserControl;
        private IOrganizationService currentService;
        private UserControl _currentWpfControl;

        public MyPluginControl()
        {
            InitializeComponent();
        }

        private void MyPluginControl_Load(object sender, EventArgs e)
        {
            ShowInfoNotification("DynaAppX - Access Check Tool", new Uri("https://github.com/iamhoho/DynaAppX"));

            // Loads or creates the settings for the plugin
            if (!SettingsManager.Instance.TryLoad(GetType(), out mySettings))
            {
                mySettings = new Settings();
                LogWarning("Settings not found => a new settings file has been created!");
            }
            else
            {
                LogInfo("Settings found and loaded");
            }

            // Auto-click AccessCheck button to show it by default
            btnAccessCheck.PerformClick();
        }

        private void ShowControl(UserControl control)
        {
            if (_currentWpfControl == control) return;
            _currentWpfControl = control;
            elementHostMain.Child = control;
        }

        private void btnAccessCheck_Click(object sender, EventArgs e)
        {
            if (accessCheckControl == null)
            {
                accessCheckControl = new AccessCheckControl();
                accessCheckControl.OpenRecordRequested += OnOpenRecordRequested;
                accessCheckControl.SetService(currentService);
            }
            ShowControl(accessCheckControl);
        }

        private void btnInvokeFlow_Click(object sender, EventArgs e)
        {
            if (invokeFlowControl == null)
            {
                invokeFlowControl = new InvokeFlowControl();
                invokeFlowControl.SetService(currentService);
            }
            ShowControl(invokeFlowControl);
        }

        private void btnGodPage_Click(object sender, EventArgs e)
        {
            if (godPageControl == null)
            {
                godPageControl = new GodPageControl();
                godPageControl.SetService(currentService);
            }
            ShowControl(godPageControl);
        }

        private void btnMetadata_Click(object sender, EventArgs e)
        {
            if (metadataBrowserControl == null)
            {
                metadataBrowserControl = new MetadataBrowserControl();
                metadataBrowserControl.CrmUrlRequest += () =>
                {
                    metadataBrowserControl.SetCrmUrl(mySettings?.LastUsedOrganizationWebappUrl ?? "");
                };
                metadataBrowserControl.SetService(currentService);
            }
            ShowControl(metadataBrowserControl);
        }

        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail, string actionName, object parameter)
        {
            base.UpdateConnection(newService, detail, actionName, parameter);

            if (detail != null)
            {
                if (mySettings == null) mySettings = new Settings();
                mySettings.LastUsedOrganizationWebappUrl = detail.WebApplicationUrl;
                LogInfo("Connection has changed to: {0}", detail.WebApplicationUrl);
            }
            currentService = newService;

            // Pass service to all WPF controls
            if (accessCheckControl != null && newService != null)
            {
                accessCheckControl.SetService(newService);
            }
            if (invokeFlowControl != null && newService != null)
            {
                invokeFlowControl.SetService(newService);
            }
            if (godPageControl != null && newService != null)
            {
                godPageControl.SetService(newService);
            }
            if (metadataBrowserControl != null && newService != null)
            {
                metadataBrowserControl.SetService(newService);
            }
        }

        private void OnOpenRecordRequested(string reference)
        {
            if (reference.StartsWith("role:"))
            {
                var roleId = reference.Substring(5);
                OpenRecordInCRM("role", new Guid(roleId));
            }
            else if (reference.StartsWith("team:"))
            {
                var teamId = reference.Substring(5);
                OpenRecordInCRM("team", new Guid(teamId));
            }
        }

        private void OpenRecordInCRM(string entityName, Guid recordId)
        {
            if (mySettings?.LastUsedOrganizationWebappUrl == null)
            {
                MessageBox.Show("No CRM connection URL available. Please connect first.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Escape entity name and record ID for URL
                var safeEntityName = Uri.EscapeDataString(entityName);
                var safeRecordId = Uri.EscapeDataString(recordId.ToString());
                var url = $"{mySettings.LastUsedOrganizationWebappUrl}/main.aspx?etn={safeEntityName}&id={safeRecordId}&pagetype=entityrecord";
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening record: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void tsbClose_Click(object sender, EventArgs e)
        {
            CloseTool();
        }

        private void MyPluginControl_OnCloseTool(object sender, EventArgs e)
        {
            if (mySettings != null)
            {
                SettingsManager.Instance.Save(GetType(), mySettings);
            }
        }
    }
}
