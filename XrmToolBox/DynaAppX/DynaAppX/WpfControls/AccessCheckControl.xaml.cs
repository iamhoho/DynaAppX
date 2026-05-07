using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DynaAppX;
using DynaAppX.Services;

namespace DynaAppX.WpfControls
{
    public partial class AccessCheckControl : UserControl
    {
        private IOrganizationService _service;
        private List<UserWrapper> _users = new List<UserWrapper>();
        private List<RecordWrapper> _records = new List<RecordWrapper>();
        private List<EntityWrapper> _allEntities = new List<EntityWrapper>();
        private List<EntityWrapper> _entities = new List<EntityWrapper>();
        private bool _entitiesLoaded = false;

        public event Action<string> OpenRecordRequested;

        public AccessCheckControl()
        {
            InitializeComponent();
            this.Loaded += AccessCheckControl_Loaded;
        }

        public void SetService(IOrganizationService service)
        {
            _service = service;
            txtStatus.Text = "Service connected. Click 'Load Entities' to begin.";
        }

        private void AccessCheckControl_Loaded(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "Waiting for CRM connection...";
        }

        private async void btnLoadEntities_Click(object sender, RoutedEventArgs e)
        {
            if (_service == null)
            {
                txtStatus.Text = "Error: Service not initialized";
                return;
            }

            btnLoadEntities.IsEnabled = false;
            txtStatus.Text = "Loading entities...";

            try
            {
                var loadingDialog = new LoadingDialog("Loading entities...");
                loadingDialog.Show();

                await SharedMetadataCache.Instance.RefreshEntitiesAsync(_service);

                _allEntities = SharedMetadataCache.Instance.GetAllEntities(_service);
                _entities = new List<EntityWrapper>(_allEntities);

                cboEntity.ItemsSource = null;
                cboEntity.ItemsSource = _entities;
                _entitiesLoaded = true;

                loadingDialog.Close();
                txtStatus.Text = $"Loaded {_entities.Count} entities";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error loading entities: {ex.Message}";
            }
            finally
            {
                btnLoadEntities.IsEnabled = true;
            }
        }

        private void cboEntity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cboRecord.ItemsSource = null;
            _records.Clear();
            if (cboEntity.SelectedItem is EntityWrapper entityWrapper)
            {
                SearchRecords("");
            }
        }

        private void cboEntity_DropDownOpened(object sender, EventArgs e)
        {
            if (!_entitiesLoaded) return;

            var searchText = cboEntity.Text?.ToLower() ?? "";
            FilterEntities(searchText);
        }

        private void FilterEntities(string searchText)
        {
            _entities.Clear();
            if (string.IsNullOrEmpty(searchText))
            {
                _entities.AddRange(_allEntities);
            }
            else
            {
                var searchLower = searchText.ToLowerInvariant();
                _entities.AddRange(_allEntities.Where(entity =>
                    entity.LogicalName.ToLowerInvariant().Contains(searchLower) ||
                    entity.DisplayName.ToLowerInvariant().Contains(searchLower)));
            }
            cboEntity.ItemsSource = null;
            cboEntity.ItemsSource = _entities;
        }

        private async void cboUser_DropDownOpened(object sender, EventArgs e)
        {
            if (_service == null) return;

            var searchText = cboUser.Text ?? "";
            try
            {
                _users = await SharedMetadataCache.Instance.SearchUsersAsync(_service, searchText);
                cboUser.ItemsSource = null;
                cboUser.ItemsSource = _users;
                txtStatus.Text = $"Found {_users.Count} users";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error searching users: {ex.Message}";
            }
        }

        private void cboRecord_DropDownOpened(object sender, EventArgs e)
        {
            if (cboRecord.IsEditable && cboEntity.SelectedItem != null)
            {
                SearchRecords(cboRecord.Text ?? "");
            }
        }

        private void cboRecord_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (cboEntity.SelectedItem != null)
            {
                SearchRecords(cboRecord.Text ?? "");
            }
        }

        private void cboUser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboUser.SelectedItem is UserWrapper user)
            {
                LoadUserRoles(user);
                LoadUserTeams(user);
                CheckAccessRights();
            }
        }

        private void cboRecord_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckAccessRights();
        }

        private void SearchRecords(string searchText)
        {
            if (_service == null || !_entitiesLoaded) return;

            if (!(cboEntity.SelectedItem is EntityWrapper entityWrapper))
            {
                txtStatus.Text = "Please select an entity first";
                return;
            }

            try
            {
                var entityName = entityWrapper.LogicalName;
                var primaryIdAttr = entityWrapper.PrimaryIdAttribute;
                var primaryNameAttr = entityWrapper.PrimaryNameAttribute ?? "name";

                var fetchXml = BuildRecordSearchFetchXml(entityWrapper, searchText);
                var result = _service.RetrieveMultiple(new FetchExpression(fetchXml));

                _records.Clear();
                foreach (var record in result.Entities)
                {
                    var recordName = record.GetAttributeValue<string>(primaryNameAttr) ?? "(No name)";
                    _records.Add(new RecordWrapper
                    {
                        Id = record.Id,
                        RecordName = recordName,
                        Entity = record
                    });
                }

                cboRecord.ItemsSource = null;
                cboRecord.ItemsSource = _records;
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error searching records: {ex.Message}";
            }
        }

        private string BuildRecordSearchFetchXml(EntityWrapper entityWrapper, string searchText)
        {
            var conditions = new List<string>();

            // Check if search text is a GUID
            if (IsGuid(searchText))
            {
                conditions.Add($"<condition attribute='{entityWrapper.PrimaryIdAttribute}' operator='eq' value='{searchText}'/>");
            }
            else if (!string.IsNullOrWhiteSpace(searchText))
            {
                // Search by string attributes containing "code", "name", or "number"
                if (entityWrapper.Attributes != null)
                {
                    var stringAttrs = entityWrapper.Attributes
                        .Where(a => a.AttributeOf == null &&
                                    a.AttributeType == AttributeTypeCode.String &&
                                    (a.LogicalName.ToLowerInvariant().Contains("code") ||
                                     a.LogicalName.ToLowerInvariant().Contains("name") ||
                                     a.LogicalName.ToLowerInvariant().Contains("number")))
                        .ToList();

                    foreach (var attr in stringAttrs)
                    {
                        conditions.Add($"<condition attribute='{attr.LogicalName}' operator='like' value='%{EscapeXml(searchText)}%'/>");
                    }
                }

                // Also search by primary name
                if (!string.IsNullOrEmpty(entityWrapper.PrimaryNameAttribute))
                {
                    conditions.Add($"<condition attribute='{entityWrapper.PrimaryNameAttribute}' operator='like' value='%{EscapeXml(searchText)}%'/>");
                }
            }

            var conditionXml = conditions.Count > 0
                ? $"<filter type='or'>{string.Join("", conditions)}</filter>"
                : "";

            return $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' top='30'>
                <entity name='{entityWrapper.LogicalName}'>
                    <attribute name='{entityWrapper.PrimaryIdAttribute}'/>
                    <attribute name='{entityWrapper.PrimaryNameAttribute}'/>
                    <order attribute='modifiedon' descending='true'/>
                    {conditionXml}
                </entity>
            </fetch>";
        }

        private bool IsGuid(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            return Guid.TryParse(value.Replace("{", "").Replace("}", ""), out _);
        }

        private string EscapeXml(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
        }

        private void LoadUserRoles(UserWrapper user)
        {
            if (_service == null) return;

            try
            {
                var userId = user.Id;

                var fetchXml = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
                    <entity name='role'>
                        <attribute name='roleid'/>
                        <attribute name='name'/>
                        <link-entity name='systemuserroles' from='roleid' to='roleid' visible='false' intersect='true'>
                            <filter type='and'>
                                <condition attribute='systemuserid' operator='eq' value='{userId}'/>
                            </filter>
                        </link-entity>
                    </entity>
                </fetch>";

                var results = _service.RetrieveMultiple(new FetchExpression(fetchXml));
                var roleWrappers = new List<RoleWrapper>();
                foreach (var r in results.Entities)
                {
                    roleWrappers.Add(new RoleWrapper
                    {
                        Id = r.Id,
                        Name = r.GetAttributeValue<string>("name") ?? "(No name)"
                    });
                }
                lstRoles.ItemsSource = roleWrappers;
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error loading roles: {ex.Message}";
            }
        }

        private void LoadUserTeams(UserWrapper user)
        {
            if (_service == null) return;

            try
            {
                var userId = user.Id;

                var fetchXml = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
                    <entity name='team'>
                        <attribute name='teamid'/>
                        <attribute name='name'/>
                        <link-entity name='teammembership' from='teamid' to='teamid' visible='false' intersect='true'>
                            <filter type='and'>
                                <condition attribute='systemuserid' operator='eq' value='{userId}'/>
                            </filter>
                        </link-entity>
                    </entity>
                </fetch>";

                var results = _service.RetrieveMultiple(new FetchExpression(fetchXml));
                var teamWrappers = new List<TeamWrapper>();
                foreach (var t in results.Entities)
                {
                    teamWrappers.Add(new TeamWrapper
                    {
                        Id = t.Id,
                        Name = t.GetAttributeValue<string>("name") ?? "(No name)"
                    });
                }
                lstTeams.ItemsSource = teamWrappers;
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error loading teams: {ex.Message}";
            }
        }

        private void CheckAccessRights()
        {
            if (_service == null) return;

            if (cboUser.SelectedItem == null || cboRecord.SelectedItem == null || cboEntity.SelectedItem == null)
            {
                lstAccessRights.ItemsSource = null;
                return;
            }

            try
            {
                var userId = (cboUser.SelectedItem as UserWrapper).Id;
                var recordWrapper = cboRecord.SelectedItem as RecordWrapper;
                var recordId = recordWrapper?.Id ?? Guid.Empty;
                var entityWrapper = cboEntity.SelectedItem as EntityWrapper;
                var entityName = entityWrapper.LogicalName;
                var entitySetName = entityWrapper.EntitySetName;

                var accessRights = new List<AccessRightInfo>
                {
                    new AccessRightInfo { RightName = "ReadAccess" },
                    new AccessRightInfo { RightName = "WriteAccess" },
                    new AccessRightInfo { RightName = "DeleteAccess" },
                    new AccessRightInfo { RightName = "CreateAccess" },
                    new AccessRightInfo { RightName = "ShareAccess" },
                    new AccessRightInfo { RightName = "AssignAccess" },
                    new AccessRightInfo { RightName = "AppendAccess" },
                    new AccessRightInfo { RightName = "AppendToAccess" }
                };

                foreach (var right in accessRights)
                {
                    right.HasAccess = HasAccess(userId, recordId, entityName, entitySetName, right.RightName);
                }

                lstAccessRights.ItemsSource = accessRights;
                txtStatus.Text = $"Access checked for {entitySetName}/{recordId}";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error checking access: {ex.Message}";
            }
        }

        private bool HasAccess(Guid userId, Guid recordId, string entityName, string entitySetName, string accessRight)
        {
            if (_service == null) return false;

            try
            {
                // Check user's direct access
                var query = new QueryExpression("principalobjectaccess")
                {
                    ColumnSet = new ColumnSet("accessrights"),
                    Criteria = new FilterExpression()
                };
                query.Criteria.AddCondition("principalid", ConditionOperator.Equal, userId);
                query.Criteria.AddCondition("objectid", ConditionOperator.Equal, recordId);
                query.Criteria.AddCondition("objecttypecode", ConditionOperator.Equal, entityName);

                var results = _service.RetrieveMultiple(query);
                if (results.Entities.Count > 0)
                {
                    var rights = results.Entities[0].GetAttributeValue<OptionSetValue>("accessrights")?.Value ?? 0;
                    if ((rights & GetAccessRightMask(accessRight)) != 0)
                        return true;
                }

                // Check team-based access
                if (HasTeamAccess(userId, recordId, entityName, accessRight))
                    return true;

                // No access found
                return false;
            }
            catch
            {
                return false;
            }
        }

        private bool HasTeamAccess(Guid userId, Guid recordId, string entityName, string accessRight)
        {
            try
            {
                // Get user's teams
                var teamQuery = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
                    <entity name='team'>
                        <attribute name='teamid'/>
                        <link-entity name='teammembership' from='teamid' to='teamid' visible='false' intersect='true'>
                            <filter type='and'>
                                <condition attribute='systemuserid' operator='eq' value='{userId}'/>
                            </filter>
                        </link-entity>
                    </entity>
                </fetch>";

                var teamResults = _service.RetrieveMultiple(new FetchExpression(teamQuery));
                foreach (var team in teamResults.Entities)
                {
                    var teamId = team.Id;

                    var query = new QueryExpression("principalobjectaccess")
                    {
                        ColumnSet = new ColumnSet("accessrights"),
                        Criteria = new FilterExpression()
                    };
                    query.Criteria.AddCondition("principalid", ConditionOperator.Equal, teamId);
                    query.Criteria.AddCondition("objectid", ConditionOperator.Equal, recordId);
                    query.Criteria.AddCondition("objecttypecode", ConditionOperator.Equal, entityName);

                    var results = _service.RetrieveMultiple(query);
                    if (results.Entities.Count > 0)
                    {
                        var rights = results.Entities[0].GetAttributeValue<OptionSetValue>("accessrights")?.Value ?? 0;
                        if ((rights & GetAccessRightMask(accessRight)) != 0)
                            return true;
                    }
                }
            }
            catch
            {
                // Ignore errors in team check
            }
            return false;
        }

        private int GetAccessRightMask(string accessRight)
        {
            switch (accessRight)
            {
                case "ReadAccess": return 1;
                case "WriteAccess": return 2;
                case "DeleteAccess": return 4;
                case "CreateAccess": return 1;  // Fixed: was 16, CRM CreateAccess = 1
                case "ShareAccess": return 65536;
                case "AssignAccess": return 32768;
                case "AppendAccess": return 256;
                case "AppendToAccess": return 512;
                default: return 0;
            }
        }

        private void btnRole_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Guid roleId)
            {
                OpenRecordRequested?.Invoke($"role:{roleId}");
            }
        }

        private void btnTeam_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Guid teamId)
            {
                OpenRecordRequested?.Invoke($"team:{teamId}");
            }
        }
    }

    public class RoleWrapper
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class TeamWrapper
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class RecordWrapper
    {
        public Guid Id { get; set; }
        public string RecordName { get; set; }
        public Entity Entity { get; set; }
    }

    public class AccessRightInfo
    {
        public string RightName { get; set; }
        public bool HasAccess { get; set; }
    }
}
