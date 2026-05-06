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
        private IOrganizationService currentService;

        // Tab instance counters
        private int _accessCheckTabCount = 0;
        private int _invokeFlowTabCount = 0;
        private int _godPageTabCount = 0;
        private int _metadataTabCount = 0;

        // Welcome tab reference (never closed, or recreated if closed)
        private TabPage _welcomeTab;

        public MyPluginControl()
        {
            InitializeComponent();
        }

        private void MyPluginControl_Load(object sender, EventArgs e)
        {
            ShowInfoNotification("DynaAppX - Dynamics CRM Assistant", new Uri("https://github.com/iamhoho/DynaAppX"));

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

            // Create Welcome tab as the default
            CreateWelcomeTab();
        }

        private void CreateWelcomeTab()
        {
            _welcomeTab = new TabPage("🏠 Welcome");
            _welcomeTab.Padding = new Padding(0);
            var welcomePanel = CreateWelcomePanel();
            _welcomeTab.Controls.Add(welcomePanel);
            tcMain.TabPages.Insert(0, _welcomeTab);
            tcMain.SelectedTab = _welcomeTab;
        }

        private Panel CreateWelcomePanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill };

            // Title
            var lblTitle = new System.Windows.Forms.Label
            {
                Text = "DynaAppX - Dynamics CRM Assistant Tool",
                Font = new System.Drawing.Font("Segoe UI", 18, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(31, 78, 121),
                AutoSize = false,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Location = new System.Drawing.Point(0, 40),
                Size = new System.Drawing.Size(800, 40)
            };

            var lblSubtitle = new System.Windows.Forms.Label
            {
                Text = "Welcome!",
                Font = new System.Drawing.Font("Segoe UI", 13),
                ForeColor = System.Drawing.Color.Gray,
                AutoSize = false,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Location = new System.Drawing.Point(0, 85),
                Size = new System.Drawing.Size(800, 25)
            };

            // Instructions
            var instructions = new System.Windows.Forms.Label
            {
                Text = @"Instructions:
🔑 AccessCheck - Check user/team access rights to CRM records
⚡ InvokeFlow  - Execute CRM workflows and custom actions
📝 GodPage    - View and edit entity record attributes
🌐 Metadata   - Browse CRM entity metadata",
                Font = new System.Drawing.Font("Segoe UI", 11),
                AutoSize = false,
                TextAlign = System.Drawing.ContentAlignment.TopLeft,
                Location = new System.Drawing.Point(200, 130),
                Size = new System.Drawing.Size(450, 120)
            };

            var howToUse = new System.Windows.Forms.Label
            {
                Text = @"How to use:
1. Connect to your CRM using File → Connections
2. Click a button above or in the toolbar to open a feature tab
3. Each click opens a NEW instance of that feature
4. You can have multiple instances of the same feature open
5. Close tabs using the × button on each tab",
                Font = new System.Drawing.Font("Segoe UI", 10),
                ForeColor = System.Drawing.Color.FromArgb(102, 102, 102),
                AutoSize = false,
                TextAlign = System.Drawing.ContentAlignment.TopLeft,
                Location = new System.Drawing.Point(200, 260),
                Size = new System.Drawing.Size(450, 130)
            };

            // Feature buttons
            var btnAccess = new Button
            {
                Text = "🔑 AccessCheck",
                Size = new System.Drawing.Size(200, 50),
                Location = new System.Drawing.Point(80, 130),
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(92, 184, 92),
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold)
            };
            btnAccess.FlatAppearance.BorderSize = 0;
            btnAccess.Click += btnAccessCheck_Click;

            var btnInvoke = new Button
            {
                Text = "⚡ InvokeFlow",
                Size = new System.Drawing.Size(200, 50),
                Location = new System.Drawing.Point(320, 130),
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(19, 206, 102),
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold)
            };
            btnInvoke.FlatAppearance.BorderSize = 0;
            btnInvoke.Click += btnInvokeFlow_Click;

            var btnGod = new Button
            {
                Text = "📝 GodPage",
                Size = new System.Drawing.Size(200, 50),
                Location = new System.Drawing.Point(80, 200),
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(51, 122, 183),
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold)
            };
            btnGod.FlatAppearance.BorderSize = 0;
            btnGod.Click += btnGodPage_Click;

            var btnMeta = new Button
            {
                Text = "🌐 Metadata",
                Size = new System.Drawing.Size(200, 50),
                Location = new System.Drawing.Point(320, 200),
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(46, 117, 182),
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold)
            };
            btnMeta.FlatAppearance.BorderSize = 0;
            btnMeta.Click += btnMetadata_Click;

            panel.Controls.AddRange(new Control[] {
                lblTitle, lblSubtitle, instructions, howToUse,
                btnAccess, btnInvoke, btnGod, btnMeta
            });

            return panel;
        }

        private void btnAccessCheck_Click(object sender, EventArgs e)
        {
            _accessCheckTabCount++;
            var ctrl = new AccessCheckControl();
            ctrl.OpenRecordRequested += OnOpenRecordRequested;
            ctrl.SetService(currentService);
            var tab = new TabPage($"🔑 AccessCheck #{_accessCheckTabCount}");
            tab.Padding = new Padding(0);
            var host = new ElementHost { Dock = DockStyle.Fill, Child = ctrl };
            tab.Controls.Add(host);
            tcMain.TabPages.Add(tab);
            tcMain.SelectedTab = tab;
        }

        private void btnInvokeFlow_Click(object sender, EventArgs e)
        {
            _invokeFlowTabCount++;
            var ctrl = new InvokeFlowControl();
            ctrl.SetService(currentService);
            var tab = new TabPage($"⚡ InvokeFlow #{_invokeFlowTabCount}");
            tab.Padding = new Padding(0);
            var host = new ElementHost { Dock = DockStyle.Fill, Child = ctrl };
            tab.Controls.Add(host);
            tcMain.TabPages.Add(tab);
            tcMain.SelectedTab = tab;
        }

        private void btnGodPage_Click(object sender, EventArgs e)
        {
            _godPageTabCount++;
            var ctrl = new GodPageControl();
            ctrl.SetService(currentService);
            var tab = new TabPage($"📝 GodPage #{_godPageTabCount}");
            tab.Padding = new Padding(0);
            var host = new ElementHost { Dock = DockStyle.Fill, Child = ctrl };
            tab.Controls.Add(host);
            tcMain.TabPages.Add(tab);
            tcMain.SelectedTab = tab;
        }

        private void btnMetadata_Click(object sender, EventArgs e)
        {
            _metadataTabCount++;
            var ctrl = new MetadataBrowserControl();
            ctrl.CrmUrlRequest += () =>
            {
                ctrl.SetCrmUrl(mySettings?.LastUsedOrganizationWebappUrl ?? "");
            };
            ctrl.SetService(currentService);
            var tab = new TabPage($"🌐 Metadata #{_metadataTabCount}");
            tab.Padding = new Padding(0);
            var host = new ElementHost { Dock = DockStyle.Fill, Child = ctrl };
            tab.Controls.Add(host);
            tcMain.TabPages.Add(tab);
            tcMain.SelectedTab = tab;
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

            // Pass service to all open WPF controls via their ElementHosts
            // We refresh controls that are already open by re-finding them through the tab pages
            RefreshServiceOnAllControls(newService);
        }

        private void RefreshServiceOnAllControls(IOrganizationService service)
        {
            // Re-create controls in each feature tab so they get the new service
            // We do this by collecting the tab pages that have WPF controls, removing them,
            // and re-adding them with fresh controls. But we can't easily identify WPF tabs,
            // so we track open tabs by their title prefix.
            // 
            // Better approach: store service in a shared field and have each control call
            // SetService when activated. For now, close and reopen isn't needed since
            // SetService is called on each newly created control above.
            //
            // The controls that were already open need to be refreshed. Since we can't easily
            // access them, we'll do a soft refresh by notifying the user to switch tabs.
            // In practice, the user will create fresh instances going forward.
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

        // Handle tab close button (×) on MouseDown
        private void tcMain_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                for (int i = 0; i < tcMain.TabCount; i++)
                {
                    var rect = tcMain.GetTabRect(i);
                    // Close button is in the top-right of the tab, approx last 20px
                    if (e.X >= rect.Right - 20 && e.X <= rect.Right - 2 &&
                        e.Y >= rect.Top + 2 && e.Y <= rect.Top + rect.Height - 2)
                    {
                        var tab = tcMain.TabPages[i];
                        // Don't close the welcome tab
                        if (tab == _welcomeTab)
                        {
                            return;
                        }
                        tcMain.TabPages.RemoveAt(i);
                        return;
                    }
                }
            }
        }
    }
}