using McTools.Xrm.Connection;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using XrmToolBox.Extensibility;
using DynaAppX.WpfControls;
using DynaAppX.Services;
using WpfTabControl = System.Windows.Controls.TabControl;
using WpfTabItem = System.Windows.Controls.TabItem;
using WpfUserControl = System.Windows.Controls.UserControl;

namespace DynaAppX
{
    public partial class MyPluginControl : PluginControlBase
    {
        private Settings mySettings;
        private IOrganizationService currentService;
        private WpfTabItem welcomeTab;
        private readonly Dictionary<Guid, WpfTabItem> openTabs = new Dictionary<Guid, WpfTabItem>();
        private readonly Dictionary<Guid, WpfUserControl> tabContents = new Dictionary<Guid, WpfUserControl>();
        private string crmWebAppUrl;

        public MyPluginControl()
        {
            InitializeComponent();
        }

        private void MyPluginControl_Load(object sender, EventArgs e)
        {
            ShowInfoNotification("DynaAppX - XrmToolBox Plugin", new Uri("https://github.com/iamhoho/DynaAppX"));

            InitializeTabControl();

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
            var wpfTabControl = new WpfTabControl
            {
                Background = System.Windows.Media.Brushes.White,
                BorderThickness = new System.Windows.Thickness(0)
            };

            var welcomeControl = new WelcomePageControl();
            welcomeControl.OpenFeatureRequested += OnOpenFeatureRequested;

            var welcomeHeader = new System.Windows.Controls.StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                Background = System.Windows.Media.Brushes.Transparent
            };
            welcomeHeader.Children.Add(new System.Windows.Controls.TextBlock
            {
                Text = "Welcome",
                Foreground = System.Windows.Media.Brushes.Black,
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            });

            welcomeTab = new WpfTabItem
            {
                Header = welcomeHeader,
                Content = welcomeControl
            };

            wpfTabControl.Items.Add(welcomeTab);
            wpfTabControl.SelectedItem = welcomeTab;

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

            WpfUserControl content;
            switch (featureName)
            {
                case "AccessCheck":
                    content = CreateAccessCheckControl(tabId);
                    break;
                case "InvokeFlow":
                    content = CreateInvokeFlowControl(tabId);
                    break;
                case "GodPage":
                    content = CreateGodPageControl(tabId);
                    break;
                case "MetadataBrowser":
                    content = CreateMetadataBrowserControl(tabId);
                    break;
                default:
                    throw new ArgumentException($"Unknown feature: {featureName}");
            }

            var tabItem = new WpfTabItem
            {
                Header = CreateTabHeader(featureName, tabId),
                Content = content,
                Tag = tabId
            };

            var elementHost = this.Controls[0] as ElementHost;
            if (elementHost?.Child is WpfTabControl tabControl)
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
                Tag = tabId,
                Background = System.Windows.Media.Brushes.Transparent
            };

            var label = new System.Windows.Controls.TextBlock
            {
                Text = featureName,
                Margin = new System.Windows.Thickness(0, 0, 10, 0),
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Foreground = System.Windows.Media.Brushes.Black
            };

            var closeButton = new System.Windows.Controls.Button
            {
                Content = "x",
                Width = 18,
                Height = 18,
                FontSize = 10,
                Padding = new System.Windows.Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = tabId,
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xE0, 0xE0, 0xE0)),
                Foreground = System.Windows.Media.Brushes.Black
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

            if (tabContents.TryGetValue(tabId, out var content) && content is ITabContent tabContent)
            {
                tabContent.OnTabClosing();
            }

            var elementHost = this.Controls[0] as ElementHost;
            if (elementHost?.Child is WpfTabControl wpfTabControl)
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
            if (mySettings != null)
            {
                SettingsManager.Instance.Save(GetType(), mySettings);
            }

            SharedMetadataCache.Instance.Clear();
            if (newService != null)
            {
                SharedMetadataCache.Instance.Initialize(newService);
            }

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
            if (reference == null) return;

            if (reference.StartsWith("role:"))
            {
                var roleIdStr = reference.Substring(5);
                if (Guid.TryParse(roleIdStr, out var roleId))
                    OpenRecordInCRM("role", roleId);
            }
            else if (reference.StartsWith("team:"))
            {
                var teamIdStr = reference.Substring(5);
                if (Guid.TryParse(teamIdStr, out var teamId))
                    OpenRecordInCRM("team", teamId);
            }
        }

        private void OpenRecordInCRM(string entityName, Guid recordId)
        {
            try
            {
                if (string.IsNullOrEmpty(mySettings?.LastUsedOrganizationWebappUrl))
                {
                    MessageBox.Show("CRM URL not available. Please connect to a CRM instance first.",
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
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
            var elementHost = this.Controls[0] as ElementHost;
            if (elementHost?.Child is WpfTabControl tabControl)
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
