using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;

namespace DynaAppX
{
    public class DynaAppXPlugin : PluginControlBase
    {
        private DynaAppXControl _control;

        public DynaAppXPlugin()
        {
            _control = new DynaAppXControl();
        }

        public override IXrmToolBoxPluginControl GetControl()
        {
            return _control;
        }

        public override void ClosingPlugin(PluginCloseInfo info)
        {
            base.ClosingPlugin(info);
        }
    }

    [Export(typeof(IXrmToolBoxPlugin)),
     ExportMetadata("Name", "DynaAppX"),
     ExportMetadata("Description", "Access Check & Flow Invocation Tool for Dynamics 365 CRM"),
     ExportMetadata("BackgroundColor", "White"),
     ExportMetadata("PrimaryFontColor", "Black"),
     ExportMetadata("SecondaryFontColor", "Gray")]
    public class DynaAppXPluginFactory : PluginFactory
    {
        public override IXrmToolBoxPluginControl GetControl()
        {
            return new DynaAppXPluginControl();
        }
    }

    public class DynaAppXPluginControl : PluginControlBase, IXrmToolBoxPluginControl
    {
        private TabControl _tabControl;

        // AccessCheck controls
        private ComboBox _userComboBox;
        private ComboBox _entityComboBox;
        private TextBox _recordIdTextBox;
        private CheckedListBox _accessRightsCheckedListBox;
        private ListBox _rolesListBox;
        private ListBox _teamsListBox;
        private Label _statusLabel;

        // InvokeFlow controls
        private ComboBox _flowComboBox;
        private ComboBox _recordTypeComboBox;
        private TextBox _recordIdTextBoxInvoke;
        private Button _invokeButton;
        private DataGridView _historyGridView;
        private Label _flowStatusLabel;

        // Data
        private EntityCollection _users;
        private EntityCollection _flows;

        public DynaAppXPluginControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            // AccessCheck Tab
            var accessCheckTab = new TabPage("AccessCheck");
            accessCheckTab.Controls.Add(CreateAccessCheckPanel());
            _tabControl.TabPages.Add(accessCheckTab);

            // InvokeFlow Tab
            var invokeFlowTab = new TabPage("InvokeFlow");
            invokeFlowTab.Controls.Add(CreateInvokeFlowPanel());
            _tabControl.TabPages.Add(invokeFlowTab);

            Controls.Add(_tabControl);
        }

        private Panel CreateAccessCheckPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            int leftLabel = 20;
            int leftControl = 110;
            int controlWidth = 300;

            // User selection
            var userLabel = new Label { Text = "User:", Top = 20, Left = leftLabel, Width = 80 };
            _userComboBox = new ComboBox { Top = 20, Left = leftControl, Width = controlWidth };
            _userComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _userComboBox.SelectedIndexChanged += (s, e) => LoadUserRolesAndTeams();

            // Entity selection
            var entityLabel = new Label { Text = "Entity:", Top = 50, Left = leftLabel, Width = 80 };
            _entityComboBox = new ComboBox { Top = 50, Left = leftControl, Width = 200 };
            _entityComboBox.DropDownStyle = ComboBoxStyle.DropDownList;

            // Record ID
            var recordLabel = new Label { Text = "Record ID:", Top = 80, Left = leftLabel, Width = 80 };
            _recordIdTextBox = new TextBox { Top = 80, Left = leftControl, Width = controlWidth };

            // Check button
            var checkButton = new Button { Text = "Check Access", Top = 110, Left = leftControl, Width = 120 };
            checkButton.Click += (s, e) => CheckAccess();

            _statusLabel = new Label { Text = "", Top = 110, Left = 250, Width = 200, ForeColor = System.Drawing.Color.Green };

            // User Roles
            var rolesLabel = new Label { Text = "User Roles:", Top = 150, Left = leftLabel, Width = 80 };
            _rolesListBox = new ListBox { Top = 170, Left = leftLabel, Width = 200, Height = 150 };

            // User Teams
            var teamsLabel = new Label { Text = "User Teams:", Top = 150, Left = 240, Width = 80 };
            _teamsListBox = new ListBox { Top = 170, Left = 240, Width = 200, Height = 150 };

            // Access Rights
            var accessLabel = new Label { Text = "Access Rights:", Top = 330, Left = leftLabel, Width = 100 };
            _accessRightsCheckedListBox = new CheckedListBox { Top = 350, Left = leftLabel, Width = 400, Height = 120 };
            _accessRightsCheckedListBox.Items.AddRange(new[] {
                "ReadAccess", "WriteAccess", "CreateAccess", "DeleteAccess",
                "ShareAccess", "AssignAccess", "AppendAccess", "AppendToAccess"
            });

            panel.Controls.AddRange(new Control[] {
                userLabel, _userComboBox,
                entityLabel, _entityComboBox,
                recordLabel, _recordIdTextBox,
                checkButton, _statusLabel,
                rolesLabel, _rolesListBox,
                teamsLabel, _teamsListBox,
                accessLabel, _accessRightsCheckedListBox
            });

            return panel;
        }

        private Panel CreateInvokeFlowPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            int leftLabel = 20;
            int leftControl = 110;
            int controlWidth = 350;

            // Flow selection
            var flowLabel = new Label { Text = "Flow:", Top = 20, Left = leftLabel, Width = 80 };
            _flowComboBox = new ComboBox { Top = 20, Left = leftControl, Width = controlWidth };
            _flowComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _flowComboBox.DisplayMember = "Name";

            // Record Type
            var recordTypeLabel = new Label { Text = "Record Type:", Top = 50, Left = leftLabel, Width = 80 };
            _recordTypeComboBox = new ComboBox { Top = 50, Left = leftControl, Width = 200 };
            _recordTypeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;

            // Record ID
            var recordLabel = new Label { Text = "Record ID:", Top = 80, Left = leftLabel, Width = 80 };
            _recordIdTextBoxInvoke = new TextBox { Top = 80, Left = leftControl, Width = controlWidth };

            // Invoke button
            _invokeButton = new Button { Text = "Invoke", Top = 110, Left = leftControl, Width = 100 };
            _invokeButton.Click += (s, e) => InvokeFlow();

            _flowStatusLabel = new Label { Text = "", Top = 110, Left = 220, Width = 250, ForeColor = System.Drawing.Color.Green };

            // History
            var historyLabel = new Label { Text = "Invoke History:", Top = 150, Left = leftLabel, Width = 100 };

            _historyGridView = new DataGridView
            {
                Top = 170,
                Left = leftLabel,
                Width = 750,
                Height = 280,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                RowHeadersWidth = 50,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            _historyGridView.Columns.Add("Time", "Time");
            _historyGridView.Columns.Add("FlowName", "Flow Name");
            _historyGridView.Columns.Add("Status", "Status");
            _historyGridView.Columns.Add("Response", "Response");

            panel.Controls.AddRange(new Control[] {
                flowLabel, _flowComboBox,
                recordTypeLabel, _recordTypeComboBox,
                recordLabel, _recordIdTextBoxInvoke,
                _invokeButton, _flowStatusLabel,
                historyLabel, _historyGridView
            });

            return panel;
        }

        public override void OnConnectionUpdated(ConnectionUpdatedEventArgs e)
        {
            base.OnConnectionUpdated(e);
            LoadData();
        }

        private void LoadData()
        {
            if (Service == null) return;

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Loading data...",
                Work = (w, args) =>
                {
                    // Load users
                    var userQuery = new QueryExpression("systemuser")
                    {
                        ColumnSet = new ColumnSet("fullname", "systemuserid"),
                        Orders = new[] { new OrderExpression("fullname", OrderType.Ascending) }
                    };
                    _users = Service.RetrieveMultiple(userQuery);

                    // Load workflows
                    var workflowQuery = new QueryExpression("workflow")
                    {
                        ColumnSet = new ColumnSet("name", "uniquename", "category", "primaryentity"),
                        Criteria = new FilterExpression
                        {
                            Conditions = new[]
                            {
                                new ConditionExpression("statecode", ConditionOperator.Equal, 1),
                                new ConditionExpression("type", ConditionOperator.Equal, 1),
                                new ConditionExpression("ondemand", ConditionOperator.Equal, true)
                            }
                        }
                    };
                    _flows = Service.RetrieveMultiple(workflowQuery);

                    // Load entities
                    var entityQuery = new QueryExpression("entity")
                    {
                        ColumnSet = new ColumnSet("logicalname", "displayname"),
                        Criteria = new FilterExpression
                        {
                            Conditions = new[]
                            {
                                new ConditionExpression("isvalidforqueue", ConditionOperator.Equal, true)
                            }
                        }
                    };
                    var entities = Service.RetrieveMultiple(entityQuery);
                    args.Result = entities;
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        MessageBox.Show($"Error loading data: {args.Error.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Populate user combo
                    _userComboBox.Items.Clear();
                    foreach (var user in _users.Entities)
                    {
                        _userComboBox.Items.Add(user);
                    }
                    _userComboBox.DisplayMember = "FullName";

                    // Populate flow combo
                    _flowComboBox.Items.Clear();
                    foreach (var flow in _flows.Entities)
                    {
                        _flowComboBox.Items.Add(flow);
                    }

                    // Populate entity combo
                    _entityComboBox.Items.Clear();
                    var entities = (EntityCollection)args.Result;
                    foreach (var entity in entities.Entities)
                    {
                        var name = entity.GetAttributeValue<string>("logicalname");
                        _entityComboBox.Items.Add(name);
                    }

                    // Fallback entities if query returned nothing
                    if (_entityComboBox.Items.Count == 0)
                    {
                        _entityComboBox.Items.AddRange(new[] { "account", "contact", "lead", "opportunity", "incident" });
                    }
                }
            });
        }

        private void LoadUserRolesAndTeams()
        {
            if (Service == null || _userComboBox.SelectedItem == null) return;

            var user = (Entity)_userComboBox.SelectedItem;
            var userId = user.Id;

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Loading roles and teams...",
                Work = (w, args) =>
                {
                    // Load roles
                    var roleQuery = new QueryExpression("role")
                    {
                        ColumnSet = new ColumnSet("name", "roleid"),
                        LinkEntities = new[]
                        {
                            new LinkEntity("role", "systemuserroles", "roleid", "roleid", JoinOperator.Inner)
                            {
                                LinkCriteria = new FilterExpression
                                {
                                    Conditions = new[]
                                    {
                                        new ConditionExpression("systemuserid", ConditionOperator.Equal, userId)
                                    }
                                }
                            }
                        }
                    };
                    var roles = Service.RetrieveMultiple(roleQuery);

                    // Load teams
                    var teamQuery = new QueryExpression("team")
                    {
                        ColumnSet = new ColumnSet("name", "teamid"),
                        LinkEntities = new[]
                        {
                            new LinkEntity("team", "teammembership", "teamid", "teamid", JoinOperator.Inner)
                            {
                                LinkCriteria = new FilterExpression
                                {
                                    Conditions = new[]
                                    {
                                        new ConditionExpression("systemuserid", ConditionOperator.Equal, userId)
                                    }
                                }
                            }
                        }
                    };
                    var teams = Service.RetrieveMultiple(teamQuery);

                    args.Result = new { Roles = roles, Teams = teams };
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null) return;

                    var result = (dynamic)args.Result;
                    _rolesListBox.Items.Clear();
                    foreach (var role in result.Roles.Entities)
                    {
                        _rolesListBox.Items.Add(role.GetAttributeValue<string>("name"));
                    }

                    _teamsListBox.Items.Clear();
                    foreach (var team in result.Teams.Entities)
                    {
                        _teamsListBox.Items.Add(team.GetAttributeValue<string>("name"));
                    }
                }
            });
        }

        private void CheckAccess()
        {
            if (Service == null)
            {
                MessageBox.Show("Please connect to CRM first", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_userComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a user", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_entityComboBox.SelectedItem == null || string.IsNullOrEmpty(_recordIdTextBox.Text))
            {
                MessageBox.Show("Please select an entity and enter a record ID", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var user = (Entity)_userComboBox.SelectedItem;
            var entityLogicalName = _entityComboBox.SelectedItem.ToString();
            Guid recordId;

            if (!Guid.TryParse(_recordIdTextBox.Text.Trim(), out recordId))
            {
                MessageBox.Show("Invalid Record ID format. Please enter a valid GUID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Checking access...",
                Work = (w, args) =>
                {
                    var targetRef = new EntityReference(entityLogicalName, recordId);
                    var request = new OrganizationRequest("RetrievePrincipalAccess");
                    request["Target"] = targetRef;
                    request["Principal"] = new EntityReference("systemuser", user.Id);

                    var response = (OrganizationResponse)Service.Execute(request);
                    args.Result = response["AccessRights"].ToString().Split(',');
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        MessageBox.Show($"Error checking access: {args.Error.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Clear previous
                    for (int i = 0; i < _accessRightsCheckedListBox.Items.Count; i++)
                    {
                        _accessRightsCheckedListBox.SetItemChecked(i, false);
                    }

                    // Set new
                    foreach (var right in (string[])args.Result)
                    {
                        var trimmedRight = right.Trim();
                        var index = _accessRightsCheckedListBox.Items.IndexOf(trimmedRight);
                        if (index >= 0)
                        {
                            _accessRightsCheckedListBox.SetItemChecked(index, true);
                        }
                    }

                    _statusLabel.Text = "Access check completed";
                }
            });
        }

        private void InvokeFlow()
        {
            if (Service == null)
            {
                MessageBox.Show("Please connect to CRM first", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_flowComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a flow", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var workflow = (Entity)_flowComboBox.SelectedItem;
            var workflowId = workflow.Id;
            var category = workflow.GetAttributeValue<OptionSetValue>("category")?.Value;
            var primaryEntity = workflow.GetAttributeValue<string>("primaryentity");

            if (category == 0) // Workflow
            {
                if (_recordTypeComboBox.SelectedItem == null || string.IsNullOrEmpty(_recordIdTextBoxInvoke.Text))
                {
                    MessageBox.Show("Please select a record type and enter a record ID", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Guid recordId;
                if (!Guid.TryParse(_recordIdTextBoxInvoke.Text.Trim(), out recordId))
                {
                    MessageBox.Show("Invalid Record ID format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                WorkAsync(new WorkAsyncInfo
                {
                    Message = "Executing workflow...",
                    Work = (w, args) =>
                    {
                        var request = new OrganizationRequest("ExecuteWorkflow");
                        request["WorkflowId"] = workflowId;
                        request["EntityId"] = recordId;

                        Service.Execute(request);
                        args.Result = "Workflow executed successfully";
                    },
                    PostWorkCallBack = (args) =>
                    {
                        AddHistoryRow(workflow.GetAttributeValue<string>("name"),
                            args.Error == null ? 200 : 0,
                            args.Error?.Message ?? (string)args.Result);
                    }
                });
            }
            else if (category == 3) // Action
            {
                WorkAsync(new WorkAsyncInfo
                {
                    Message = "Executing action...",
                    Work = (w, args) =>
                    {
                        var uniqueName = workflow.GetAttributeValue<string>("uniquename");
                        var request = new OrganizationRequest(uniqueName);

                        if (!string.IsNullOrEmpty(primaryEntity) && primaryEntity != "none")
                        {
                            if (!Guid.TryParse(_recordIdTextBoxInvoke.Text.Trim(), out var recordId))
                            {
                                throw new Exception("Record ID is required for this action");
                            }
                            request["Target"] = new EntityReference(primaryEntity, recordId);
                        }

                        var response = Service.Execute(request);
                        args.Result = response.Results.Count > 0
                            ? string.Join(", ", response.Results.Select(kvp => $"{kvp.Key}: {kvp.Value}"))
                            : "Action executed successfully";
                    },
                    PostWorkCallBack = (args) =>
                    {
                        AddHistoryRow(workflow.GetAttributeValue<string>("name"),
                            args.Error == null ? 200 : 0,
                            args.Error?.Message ?? (string)args.Result);
                    }
                });
            }
        }

        private void AddHistoryRow(string flowName, int status, string response)
        {
            var rowIndex = _historyGridView.Rows.Add();
            var row = _historyGridView.Rows[rowIndex];
            row.Cells["Time"].Value = DateTime.Now.ToString("HH:mm:ss");
            row.Cells["FlowName"].Value = flowName;
            row.Cells["Status"].Value = status;
            row.Cells["Response"].Value = response?.Length > 100 ? response.Substring(0, 100) + "..." : response;

            if (_historyGridView.Rows.Count > 0)
            {
                _historyGridView.FirstDisplayedScrollingRowIndex = 0;
            }

            _flowStatusLabel.Text = status == 200 ? "Completed" : "Failed";
            _flowStatusLabel.ForeColor = status == 200 ? System.Drawing.Color.Green : System.Drawing.Color.Red;
        }
    }
}