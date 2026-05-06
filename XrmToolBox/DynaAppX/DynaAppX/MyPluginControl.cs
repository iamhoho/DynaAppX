using McTools.Xrm.Connection;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using XrmToolBox.Extensibility;
using DynaAppX.WpfControls;
using DynaAppX.Services;

namespace DynaAppX
{
    public partial class MyPluginControl : PluginControlBase
    {
        private Settings mySettings;
        private IOrganizationService currentService;
        private TabControl tabControl;
        private TabItem welcomeTab;
        private readonly Dictionary<Guid, TabItem> openTabs = new Dictionary<Guid, TabItem>();
        private readonly Dictionary<Guid, UserControl> tabContents = new Dictionary<Guid, UserControl>();
        private string crmWebAppUrl;

        public MyPluginControl()
        {
            InitializeComponent();
        }

        private void MyPluginControl_Load(object sender, EventArgs e)
        {
            ShowInfoNotification("DynaAppX - XrmToolBox Plugin", new Uri("https://github.com/iamhoho/DynaAppX"));

            // Initialize TabControl
            InitializeTabControl();

            // Initialize SharedMetadataCache
            SharedMetadataCache.Instance.Initialize(currentService);

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
        }

        private void InitializeTabControl()
        {
            // Create WPF TabControl hosted in ElementHost
            var wpfTabControl = new System.Windows.Controls.TabControl
            {
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x1E, 0x1E, 0x1E)),
                BorderThickness = new System.Windows.Thickness(0)
            };

            // Create Welcome tab
            var welcomeControl = new WelcomePageControl();
            welcomeControl.OpenFeatureRequested += OnOpenFeatureRequested;

            welcomeTab = new System.Windows.Controls.TabItem
            {
                Header = "Welcome",
                Content = welcomeControl
            };

            wpfTabControl.Items.Add(welcomeTab);

            var elementHost = new ElementHost
            {
                Dock = DockStyle.Fill,
                Child = wpfTabControl
            };

            this.Controls.Clear();
            this.Controls.Add(elementHost);
        }

        private void OnOpenFeatureRequested(string featureName)
        {
            CreateFeatureTab(featureName);
        }

        private void CreateFeatureTab(string featureName)
        {
            var tabId = Guid.NewGuid();

            UserControl content = featureName switch
            {
                "AccessCheck" => CreateAccessCheckControl(tabId),
                "InvokeFlow" => CreateInvokeFlowControl(tabId),
                "GodPage" => CreateGodPageControl(tabId),
                "MetadataBrowser" => CreateMetadataBrowserControl(tabId),
                _ => throw new ArgumentException($"Unknown feature: {featureName}")
            };

            var tabItem = new System.Windows.Controls.TabItem
            {
                Header = CreateTabHeader(featureName, tabId),
                Content = content,
                Tag = tabId
            };

            // Find the TabControl (it's inside the ElementHost)
            var elementHost = this.Controls[0] as ElementHost;
            if (elementHost?.Child is System.Windows.Controls.TabControl tabControl)
            {
                tabControl.Items.Add(tabItem);
                tabControl.SelectedItem = tabItem;
            }

            openTabs[tabId] = tabItem;
            tabContents[tabId] = content;
        }

        private System.Windows.Controls.StackPanel CreateTabHeader(string featureName, Guid tabId)
        {
            var headerPanel = new System.Windows.Controls.StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                Tag = tabId
            };

            var label = new System.Windows.Controls.TextBlock
            {
                Text = featureName,
                Margin = new System.Windows.Thickness(0, 0, 10, 0),
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };

            var closeButton = new System.Windows.Controls.Button
            {
                Content = "x",
                Width = 18,
                Height = 18,
                FontSize = 10,
                Padding = new System.Windows.Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = tabId
            };
            closeButton.Click += BtnCloseTab_Click;

            headerPanel.Children.Add(label);
            headerPanel.Children.Add(closeButton);

            return headerPanel;
        }

        private void BtnCloseTab_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is Guid tabId)
            {
                CloseTab(tabId);
            }
        }

        private void CloseTab(Guid tabId)
        {
            if (!openTabs.ContainsKey(tabId)) return;

            var tabItem = openTabs[tabId];

            // Call OnTabClosing if the content supports it
            if (tabContents.TryGetValue(tabId, out var content) && content is ITabContent tabContent)
            {
                tabContent.OnTabClosing();
            }

            // Find the TabControl
            var elementHost = this.Controls[0] as ElementHost;
            if (elementHost?.Child is System.Windows.Controls.TabControl wpfTabControl)
            {
                wpfTabControl.Items.Remove(tabItem);
            }

            openTabs.Remove(tabId);
            tabContents.Remove(tabId);
        }

        private AccessCheckControl CreateAccessCheckControl(Guid tabId)
        {
            var control = new AccessCheckControl();
            control.SetService(currentService);
            control.OpenRecordRequested += reference => OnOpenRecordRequested(reference, tabId);
            return control;
        }

        private InvokeFlowControl CreateInvokeFlowControl(Guid tabId)
        {
            var control = new InvokeFlowControl();
            control.SetService(currentService);
            return control;
        }

        private GodPageControl CreateGodPageControl(Guid tabId)
        {
            var control = new GodPageControl();
            control.SetService(currentService);
            return control;
        }

        private MetadataBrowserControl CreateMetadataBrowserControl(Guid tabId)
        {
            var control = new MetadataBrowserControl();
            control.SetCrmUrl(crmWebAppUrl);
            return control;
        }

        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail, string actionName, object parameter)
        {
            base.UpdateConnection(newService, detail, actionName, parameter);

            if (mySettings != null && detail != null)
            {
                mySettings.LastUsedOrganizationWebappUrl = detail.WebApplicationUrl;
                crmWebAppUrl = detail.WebApplicationUrl;
                LogInfo("Connection has changed to: {0}", detail.WebApplicationUrl);
            }
            currentService = newService;

            // Clear and reinitialize the cache
            SharedMetadataCache.Instance.Clear();
            if (newService != null)
            {
                SharedMetadataCache.Instance.Initialize(newService);
            }

            // Update all open tabs with new service
            foreach (var kvp in tabContents)
            {
                if (kvp.Value is AccessCheckControl accessControl)
                {
                    accessControl.SetService(newService);
                }
                else if (kvp.Value is InvokeFlowControl flowControl)
                {
                    flowControl.SetService(newService);
                }
                else if (kvp.Value is GodPageControl godControl)
                {
                    godControl.SetService(newService);
                }
                else if (kvp.Value is MetadataBrowserControl metaControl)
                {
                    metaControl.SetCrmUrl(crmWebAppUrl);
                }
            }
        }

        private void OnOpenRecordRequested(string reference, Guid tabId)
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
            try
            {
                var url = $"{mySettings.LastUsedOrganizationWebappUrl}/main.aspx?etn={entityName}&id={recordId}&pagetype=entityrecord";
                System.Diagnostics.Process.Start(url);
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

        private void tsbWelcome_Click(object sender, EventArgs e)
        {
            // Switch to welcome tab or create if doesn't exist
            var elementHost = this.Controls[0] as ElementHost;
            if (elementHost?.Child is System.Windows.Controls.TabControl tabControl)
            {
                tabControl.SelectedItem = welcomeTab;
            }
        }

        private void tsbAccessCheck_Click(object sender, EventArgs e)
        {
            CreateFeatureTab("AccessCheck");
        }

        private void tsbInvokeFlow_Click(object sender, EventArgs e)
        {
            CreateFeatureTab("InvokeFlow");
        }

        private void tsbGodPage_Click(object sender, EventArgs e)
        {
            CreateFeatureTab("GodPage");
        }

        private void tsbMetadataBrowser_Click(object sender, EventArgs e)
        {
            CreateFeatureTab("MetadataBrowser");
        }

        private void MyPluginControl_OnCloseTool(object sender, EventArgs e)
        {
            SettingsManager.Instance.Save(GetType(), mySettings);
        }
    }

    public interface ITabContent
    {
        void OnTabClosing();
    }
}
