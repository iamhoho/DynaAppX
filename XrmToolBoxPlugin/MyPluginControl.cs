using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using XrmToolBox.Extensibility;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using McTools.Xrm.Connection;
using System.Collections.Specialized;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Messages;

namespace DynaAppX
{
    public partial class MyPluginControl : MultipleConnectionsPluginControlBase
    {
        private Settings mySettings;

        public MyPluginControl()
        {
            InitializeComponent();
        }

        private void MyPluginControl_Load(object sender, EventArgs e)
        {
            if (!SettingsManager.Instance.TryLoad(GetType(), out mySettings))
            {
                mySettings = new Settings();
                LogWarning("Settings not found => a new settings file has been created!");
            }
            else
            {
                LogInfo("Settings found and loaded");
            }

            ExecuteMethod(LoadInitialData);
        }

        private void tsbClose_Click(object sender, EventArgs e)
        {
            CloseTool();
        }

        private void MyPluginControl_OnCloseTool(object sender, EventArgs e)
        {
            SettingsManager.Instance.Save(GetType(), mySettings);
        }

        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail, string actionName, object parameter)
        {
            base.UpdateConnection(newService, detail, actionName, parameter);

            if (mySettings != null && detail != null)
            {
                mySettings.LastUsedOrganizationWebappUrl = detail.WebApplicationUrl;
            }
        }

        protected override void ConnectionDetailsUpdated(NotifyCollectionChangedEventArgs e)
        {
        }

        private void LoadInitialData()
        {
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Loading data...",
                Work = (worker, args) =>
                {
                    // Load users
                    var userQuery = new QueryExpression("systemuser")
                    {
                        ColumnSet = new ColumnSet("fullname", "systemuserid"),
                        Orders = new[] { new OrderExpression("fullname", OrderType.Ascending) }
                    };
                    var users = Service.RetrieveMultiple(userQuery);

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
                    var workflows = Service.RetrieveMultiple(workflowQuery);

                    // Load entities
                    var entityQuery = new QueryExpression("entity")
                    {
                        ColumnSet = new ColumnSet("logicalname"),
                        Criteria = new FilterExpression
                        {
                            Conditions = new[]
                            {
                                new ConditionExpression("isvalidforqueue", ConditionOperator.Equal, true)
                            }
                        }
                    };
                    var entities = Service.RetrieveMultiple(entityQuery);

                    args.Result = new { Users = users, Workflows = workflows, Entities = entities };
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        MessageBox.Show(args.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var data = (dynamic)args.Result;

                    // Populate user combo
                    cbUsers.Items.Clear();
                    foreach (var user in data.Users.Entities)
                    {
                        cbUsers.Items.Add(user);
                    }
                    cbUsers.DisplayMember = "FullName";

                    // Populate flow combo
                    cbFlows.Items.Clear();
                    foreach (var flow in data.Workflows.Entities)
                    {
                        cbFlows.Items.Add(flow);
                    }
                    cbFlows.DisplayMember = "Name";

                    // Populate entity combo
                    cbEntities.Items.Clear();
                    foreach (var entity in data.Entities.Entities)
                    {
                        var name = entity.GetAttributeValue<string>("logicalname");
                        cbEntities.Items.Add(name);
                        cbRecordType.Items.Add(name);
                    }

                    if (cbEntities.Items.Count == 0)
                    {
                        cbEntities.Items.AddRange(new[] { "account", "contact", "lead", "opportunity", "incident" });
                        cbRecordType.Items.AddRange(new[] { "account", "contact", "lead", "opportunity", "incident" });
                    }

                    if (cbEntities.Items.Count > 0) cbEntities.SelectedIndex = 0;
                    if (cbRecordType.Items.Count > 0) cbRecordType.SelectedIndex = 0;
                }
            });
        }

        #region AccessCheck Tab

        private void cbUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbUsers.SelectedItem == null) return;
            ExecuteMethod(LoadUserRolesAndTeams);
        }

        private void LoadUserRolesAndTeams()
        {
            if (cbUsers.SelectedItem == null) return;

            var user = (Entity)cbUsers.SelectedItem;
            var userId = user.Id;

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Loading roles and teams...",
                Work = (worker, args) =>
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

                    lbRoles.Items.Clear();
                    foreach (var role in result.Roles.Entities)
                    {
                        lbRoles.Items.Add(role.GetAttributeValue<string>("name"));
                    }

                    lbTeams.Items.Clear();
                    foreach (var team in result.Teams.Entities)
                    {
                        lbTeams.Items.Add(team.GetAttributeValue<string>("name"));
                    }
                }
            });
        }

        private void btnCheckAccess_Click(object sender, EventArgs e)
        {
            if (cbUsers.SelectedItem == null)
            {
                MessageBox.Show(this, "Please select a user", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cbEntities.SelectedItem == null || string.IsNullOrEmpty(txtRecordId.Text))
            {
                MessageBox.Show(this, "Please select an entity and enter a record ID", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var user = (Entity)cbUsers.SelectedItem;
            var entityLogicalName = cbEntities.SelectedItem.ToString();
            Guid recordId;

            if (!Guid.TryParse(txtRecordId.Text.Trim(), out recordId))
            {
                MessageBox.Show(this, "Invalid Record ID format. Please enter a valid GUID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Checking access...",
                Work = (worker, args) =>
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
                        MessageBox.Show(this, $"Error checking access: {args.Error.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Clear previous
                    for (int i = 0; i < clbAccessRights.Items.Count; i++)
                    {
                        clbAccessRights.SetItemChecked(i, false);
                    }

                    // Set new
                    foreach (var right in (string[])args.Result)
                    {
                        var trimmedRight = right.Trim();
                        var index = clbAccessRights.Items.IndexOf(trimmedRight);
                        if (index >= 0)
                        {
                            clbAccessRights.SetItemChecked(index, true);
                        }
                    }

                    lblAccessStatus.Text = "Access check completed";
                    lblAccessStatus.ForeColor = Color.Green;
                }
            });
        }

        #endregion

        #region InvokeFlow Tab

        private void btnInvoke_Click(object sender, EventArgs e)
        {
            if (cbFlows.SelectedItem == null)
            {
                MessageBox.Show(this, "Please select a flow", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var workflow = (Entity)cbFlows.SelectedItem;
            var workflowId = workflow.Id;
            var category = workflow.GetAttributeValue<OptionSetValue>("category")?.Value;
            var primaryEntity = workflow.GetAttributeValue<string>("primaryentity");

            if (category == 0) // Workflow
            {
                if (cbRecordType.SelectedItem == null || string.IsNullOrEmpty(txtInvokeRecordId.Text))
                {
                    MessageBox.Show(this, "Please select a record type and enter a record ID", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Guid recordId;
                if (!Guid.TryParse(txtInvokeRecordId.Text.Trim(), out recordId))
                {
                    MessageBox.Show(this, "Invalid Record ID format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                WorkAsync(new WorkAsyncInfo
                {
                    Message = "Executing workflow...",
                    Work = (worker, args) =>
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
                    Work = (worker, args) =>
                    {
                        var uniqueName = workflow.GetAttributeValue<string>("uniquename");
                        var request = new OrganizationRequest(uniqueName);

                        if (!string.IsNullOrEmpty(primaryEntity) && primaryEntity != "none")
                        {
                            if (!Guid.TryParse(txtInvokeRecordId.Text.Trim(), out var recordId))
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
            var rowIndex = dgvHistory.Rows.Add();
            var row = dgvHistory.Rows[rowIndex];
            row.Cells["colTime"].Value = DateTime.Now.ToString("HH:mm:ss");
            row.Cells["colFlowName"].Value = flowName;
            row.Cells["colStatus"].Value = status;
            row.Cells["colResponse"].Value = response?.Length > 100 ? response.Substring(0, 100) + "..." : response;

            if (dgvHistory.Rows.Count > 0)
            {
                dgvHistory.FirstDisplayedScrollingRowIndex = 0;
            }

            lblInvokeStatus.Text = status == 200 ? "Completed" : "Failed";
            lblInvokeStatus.ForeColor = status == 200 ? Color.Green : Color.Red;
        }

        private void btnClearHistory_Click(object sender, EventArgs e)
        {
            dgvHistory.Rows.Clear();
        }

        #endregion

        private void btnReloadData_Click(object sender, EventArgs e)
        {
            ExecuteMethod(LoadInitialData);
        }
    }
}