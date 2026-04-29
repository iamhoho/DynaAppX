using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;

namespace DynaAppXPlugin
{
    public partial class DynaAppXControl : UserControl
    {
        private IOrganizationService _service;
        private TabControl _tabControl;

        // AccessCheck controls
        private ComboBox _userComboBox;
        private ComboBox _entityComboBox;
        private TextBox _recordIdTextBox;
        private CheckedListBox _accessRightsCheckedListBox;
        private ListBox _rolesListBox;
        private ListBox _teamsListBox;
        private Button _checkAccessButton;
        private Label _statusLabel;

        // InvokeFlow controls
        private ComboBox _flowComboBox;
        private ComboBox _recordTypeComboBoxInvoke;
        private TextBox _recordIdTextBoxInvoke;
        private Button _invokeButton;
        private DataGridView _historyGridView;
        private Label _flowStatusLabel;

        // Data
        private EntityCollection _users;
        private EntityCollection _flows;

        public DynaAppXControl()
        {
            InitializeComponent();
        }

        public void SetConnection(IOrganizationService service)
        {
            _service = service;
            LoadEntities();
            LoadUsers();
            LoadFlows();
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
            _entityComboBox.SelectedIndexChanged += (s, e) => UpdateRecordTypes();

            // Record ID
            var recordLabel = new Label { Text = "Record ID:", Top = 80, Left = leftLabel, Width = 80 };
            _recordIdTextBox = new TextBox { Top = 80, Left = leftControl, Width = controlWidth };

            // Check button
            _checkAccessButton = new Button { Text = "Check Access", Top = 110, Left = leftControl, Width = 120 };
            _checkAccessButton.Click += (s, e) => CheckAccess();

            // Status
            _statusLabel = new Label { Text = "", Top = 110, Left = 250, Width = 200, ForeColor = System.Drawing.Color.Green };

            // User Roles
            var rolesLabel = new Label { Text = "User Roles:", Top = 150, Left = leftLabel, Width = 80 };
            _rolesListBox = new ListBox { Top = 170, Left = leftLabel, Width = 200, Height = 150 };
            _rolesListBox.DoubleClick += (s, e) => OpenRolePage();

            // User Teams
            var teamsLabel = new Label { Text = "User Teams:", Top = 150, Left = 240, Width = 80 };
            _teamsListBox = new ListBox { Top = 170, Left = 240, Width = 200, Height = 150 };
            _teamsListBox.DoubleClick += (s, e) => OpenTeamPage();

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
                _checkAccessButton, _statusLabel,
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
            _recordTypeComboBoxInvoke = new ComboBox { Top = 50, Left = leftControl, Width = 200 };
            _recordTypeComboBoxInvoke.DropDownStyle = ComboBoxStyle.DropDownList;

            // Record ID
            var recordLabel = new Label { Text = "Record ID:", Top = 80, Left = leftLabel, Width = 80 };
            _recordIdTextBoxInvoke = new TextBox { Top = 80, Left = leftControl, Width = controlWidth };

            // Invoke button
            _invokeButton = new Button { Text = "Invoke", Top = 110, Left = leftControl, Width = 100 };
            _invokeButton.Click += (s, e) => InvokeFlow();

            // Flow status
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
                recordTypeLabel, _recordTypeComboBoxInvoke,
                recordLabel, _recordIdTextBoxInvoke,
                _invokeButton, _flowStatusLabel,
                historyLabel, _historyGridView
            });

            return panel;
        }

        private void LoadEntities()
        {
            if (_service == null) return;

            try
            {
                var query = new QueryExpression("entity")
                {
                    ColumnSet = new ColumnSet("logicalname", "displayname"),
                    Criteria = new FilterExpression
                    {
                        Conditions = new[]
                        {
                            new ConditionExpression("isvalidforqueue", ConditionOperator.Equal, true),
                            new ConditionExpression("isactivity", ConditionOperator.Equal, false)
                        }
                    },
                    Orders = new[] { new OrderExpression("displayname", OrderType.Ascending) }
                };

                // Limit to common entities for performance
                var commonEntities = new[] { "account", "contact", "lead", "opportunity", "incident", "task", "email" };
                query.Criteria.Conditions[0].Values.Add(true);

                var entities = _service.RetrieveMultiple(query);

                _entityComboBox.Items.Clear();
                foreach (var entity in entities.Entities)
                {
                    var name = entity.GetAttributeValue<string>("logicalname");
                    _entityComboBox.Items.Add(name);
                }

                // Add common entities if metadata query returns empty
                if (_entityComboBox.Items.Count == 0)
                {
                    _entityComboBox.Items.AddRange(commonEntities);
                }
            }
            catch
            {
                // Fallback to common entities
                _entityComboBox.Items.AddRange(new[] {
                    "account", "contact", "lead", "opportunity",
                    "invoice", "salesorder", "quote", "incident", "task", "email"
                });
            }
        }

        private void LoadUsers()
        {
            if (_service == null) return;

            try
            {
                var query = new QueryExpression("systemuser")
                {
                    ColumnSet = new ColumnSet("fullname", "systemuserid", "domainname"),
                    Orders = new[] { new OrderExpression("fullname", OrderType.Ascending) }
                };

                _users = _service.RetrieveMultiple(query);

                _userComboBox.Items.Clear();
                foreach (var user in _users.Entities)
                {
                    _userComboBox.Items.Add(user);
                }
                _userComboBox.DisplayMember = "FullName";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadFlows()
        {
            if (_service == null) return;

            try
            {
                var query = new QueryExpression("workflow")
                {
                    ColumnSet = new ColumnSet("name", "uniquename", "category", "primaryentity", "statecode", "type", "ondemand"),
                    Criteria = new FilterExpression
                    {
                        Conditions = new[]
                        {
                            new ConditionExpression("statecode", ConditionOperator.Equal, 1), // Active
                            new ConditionExpression("type", ConditionOperator.Equal, 1), // Definition
                            new ConditionExpression("category", ConditionOperator.In, new[] { 0, 3 }) // Workflow or Action
                        }
                    },
                    Orders = new[] { new OrderExpression("name", OrderType.Ascending) }
                };

                var workflows = _service.RetrieveMultiple(query);

                _flows = workflows;
                _flowComboBox.Items.Clear();
                foreach (var workflow in workflows.Entities)
                {
                    _flowComboBox.Items.Add(workflow);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading flows: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateRecordTypes()
        {
            _recordTypeComboBoxInvoke.Items.Clear();
            foreach (var item in _entityComboBox.Items)
            {
                _recordTypeComboBoxInvoke.Items.Add(item);
            }
            if (_entityComboBox.Items.Count > 0)
            {
                _recordTypeComboBoxInvoke.SelectedIndex = 0;
            }
        }

        private void LoadUserRolesAndTeams()
        {
            if (_service == null || _userComboBox.SelectedItem == null)
            {
                _rolesListBox.Items.Clear();
                _teamsListBox.Items.Clear();
                return;
            }

            try
            {
                var user = (Entity)_userComboBox.SelectedItem;
                var userId = user.Id;

                // Load Roles
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

                var roles = _service.RetrieveMultiple(roleQuery);
                _rolesListBox.Items.Clear();
                foreach (var role in roles.Entities)
                {
                    _rolesListBox.Items.Add(new ListItem { Id = role.Id, Name = role.GetAttributeValue<string>("name"), Type = "role" });
                }

                // Load Teams
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

                var teams = _service.RetrieveMultiple(teamQuery);
                _teamsListBox.Items.Clear();
                foreach (var team in teams.Entities)
                {
                    _teamsListBox.Items.Add(new ListItem { Id = team.Id, Name = team.GetAttributeValue<string>("name"), Type = "team" });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading roles/teams: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CheckAccess()
        {
            if (_service == null)
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

            try
            {
                var user = (Entity)_userComboBox.SelectedItem;
                var entityLogicalName = _entityComboBox.SelectedItem.ToString();
                var recordId = new Guid(_recordIdTextBox.Text.Trim());

                // Clear previous access rights
                for (int i = 0; i < _accessRightsCheckedListBox.Items.Count; i++)
                {
                    _accessRightsCheckedListBox.SetItemChecked(i, false);
                }

                // RetrievePrincipalAccess request
                var targetRef = new EntityReference(entityLogicalName, recordId);
                var request = new OrganizationRequest("RetrievePrincipalAccess");
                request["Target"] = targetRef;
                request["Principal"] = new EntityReference("systemuser", user.Id);

                var response = (OrganizationResponse)_service.Execute(request);
                var accessRights = response["AccessRights"].ToString().Split(',');

                foreach (var right in accessRights)
                {
                    var trimmedRight = right.Trim();
                    var index = _accessRightsCheckedListBox.Items.IndexOf(trimmedRight);
                    if (index >= 0)
                    {
                        _accessRightsCheckedListBox.SetItemChecked(index, true);
                    }
                }

                _statusLabel.Text = "Access check completed";
                _statusLabel.ForeColor = System.Drawing.Color.Green;
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid Record ID format. Please enter a valid GUID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking access: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InvokeFlow()
        {
            if (_service == null)
            {
                MessageBox.Show("Please connect to CRM first", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_flowComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a flow", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var workflow = (Entity)_flowComboBox.SelectedItem;
                var workflowId = workflow.Id;
                var category = workflow.GetAttributeValue<OptionSetValue>("category")?.Value;
                var primaryEntity = workflow.GetAttributeValue<string>("primaryentity");

                string requestBody = "";
                int statusCode = 0;
                string responseText = "";

                if (category == 0) // Workflow
                {
                    if (_recordTypeComboBoxInvoke.SelectedItem == null || string.IsNullOrEmpty(_recordIdTextBoxInvoke.Text))
                    {
                        MessageBox.Show("Please select a record type and enter a record ID for workflow execution", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var recordId = new Guid(_recordIdTextBoxInvoke.Text.Trim());
                    var entityRef = new EntityReference(_recordTypeComboBoxInvoke.SelectedItem.ToString(), recordId);

                    var executeRequest = new OrganizationRequest("ExecuteWorkflow");
                    executeRequest["WorkflowId"] = workflowId;
                    executeRequest["EntityId"] = recordId;

                    var executeResponse = (OrganizationResponse)_service.Execute(executeRequest);
                    statusCode = 200;
                    responseText = "Workflow executed successfully";

                    requestBody = $"{{\"EntityId\":\"{recordId}\"}}";
                }
                else if (category == 3) // Action
                {
                    // For actions, we need to use the Web API endpoint directly
                    // This is a simplified implementation
                    var uniqueName = workflow.GetAttributeValue<string>("uniquename");
                    var request = new OrganizationRequest(uniqueName);

                    // Add target if primary entity is set
                    if (!string.IsNullOrEmpty(primaryEntity) && primaryEntity != "none")
                    {
                        if (string.IsNullOrEmpty(_recordIdTextBoxInvoke.Text))
                        {
                            MessageBox.Show("Record ID is required for this action", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        var recordId = new Guid(_recordIdTextBoxInvoke.Text.Trim());
                        var targetRef = new EntityReference(primaryEntity, recordId);
                        request["Target"] = targetRef;
                    }

                    var actionResponse = (OrganizationResponse)_service.Execute(request);
                    statusCode = 200;

                    // Serialize response to JSON-like string
                    responseText = actionResponse.Results.Count > 0
                        ? string.Join(", ", actionResponse.Results.Select(kvp => $"{kvp.Key}: {kvp.Value}"))
                        : "Action executed successfully";

                    requestBody = $"Action: {uniqueName}";
                }

                // Add to history
                var rowIndex = _historyGridView.Rows.Add();
                var row = _historyGridView.Rows[rowIndex];
                row.Cells["Time"].Value = DateTime.Now.ToString("HH:mm:ss");
                row.Cells["FlowName"].Value = workflow.GetAttributeValue<string>("name");
                row.Cells["Status"].Value = statusCode;
                row.Cells["Response"].Value = responseText.Length > 100 ? responseText.Substring(0, 100) + "..." : responseText;

                _flowStatusLabel.Text = "Flow invoked successfully";
                _flowStatusLabel.ForeColor = System.Drawing.Color.Green;

                // Auto-scroll to top (newest entry)
                if (_historyGridView.Rows.Count > 0)
                {
                    _historyGridView.FirstDisplayedScrollingRowIndex = 0;
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid Record ID format. Please enter a valid GUID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error invoking flow: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _flowStatusLabel.Text = "Invoke failed";
                _flowStatusLabel.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void OpenRolePage()
        {
            if (_rolesListBox.SelectedItem is ListItem item)
            {
                OpenCrmRecord("role", item.Id);
            }
        }

        private void OpenTeamPage()
        {
            if (_teamsListBox.SelectedItem is ListItem item)
            {
                OpenCrmRecord("team", item.Id);
            }
        }

        private void OpenCrmRecord(string entityName, Guid recordId)
        {
            try
            {
                // Get CRM URL from connection (if available)
                // This is a simplified version - in production you'd get the actual CRM URL
                var url = $"main.aspx?etn={entityName}&id={recordId}&pagetype=entityrecord";
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening CRM record: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private class ListItem
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }

            public override string ToString() => Name;
        }
    }
}