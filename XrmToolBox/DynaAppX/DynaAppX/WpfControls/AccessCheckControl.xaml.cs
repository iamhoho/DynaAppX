using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DynaAppX.WpfControls
{
    public partial class AccessCheckControl : UserControl
    {
        private IOrganizationService _service;
        private List<UserWrapper> _users = new List<UserWrapper>();
        private List<RecordWrapper> _records = new List<RecordWrapper>();
        private List<EntityWrapper> _entities = new List<EntityWrapper>();

        public event Action<string> OpenRecordRequested;

        public AccessCheckControl()
        {
            InitializeComponent();
            this.Loaded += AccessCheckControl_Loaded;
        }

        public void SetService(IOrganizationService service)
        {
            _service = service;
            txtStatus.Text = "Service connected. Ready.";
            LoadEntities();
        }

        private void AccessCheckControl_Loaded(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "Waiting for CRM connection...";
        }

        private void LoadEntities()
        {
            if (_service == null)
            {
                txtStatus.Text = "Error: Service not initialized";
                return;
            }

            try
            {
                txtStatus.Text = "Loading entities...";

                var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                    <entity name='entitydefinition'>
                        <attribute name='LogicalName'/>
                        <attribute name='DisplayName'/>
                        <attribute name='EntitySetName'/>
                        <attribute name='PrimaryIdAttribute'/>
                        <attribute name='PrimaryNameAttribute'/>
                        <order attribute='DisplayName' descending='false'/>
                        <filter type='and'>
                            <condition attribute='IsIntersect' operator='eq' value='0'/>
                        </filter>
                    </entity>
                </fetch>";

                var result = _service.RetrieveMultiple(new FetchExpression(fetchXml));

                _entities.Clear();
                foreach (var entity in result.Entities)
                {
                    var displayName = entity.GetAttributeValue<string>("DisplayName");
                    var label = displayName ?? entity.GetAttributeValue<string>("LogicalName");
                    _entities.Add(new EntityWrapper
                    {
                        LogicalName = entity.GetAttributeValue<string>("LogicalName"),
                        DisplayName = label,
                        EntitySetName = entity.GetAttributeValue<string>("EntitySetName"),
                        PrimaryIdAttribute = entity.GetAttributeValue<string>("PrimaryIdAttribute"),
                        PrimaryNameAttribute = entity.GetAttributeValue<string>("PrimaryNameAttribute"),
                        Entity = entity
                    });
                }

                cboEntity.ItemsSource = null;
                cboEntity.ItemsSource = _entities;
                txtStatus.Text = $"Loaded {_entities.Count} entities";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error loading entities: {ex.Message}";
            }
        }

        private void cboEntity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cboRecord.ItemsSource = null;
            _records.Clear();
            if (cboEntity.SelectedItem is EntityWrapper entityWrapper)
            {
                LoadRecords(entityWrapper);
            }
        }

        private void cboUser_DropDownOpened(object sender, EventArgs e)
        {
            SearchUsers("");
        }

        private void cboRecord_DropDownOpened(object sender, EventArgs e)
        {
            if (cboRecord.IsEditable && cboEntity.SelectedItem != null)
            {
                SearchRecords(cboRecord.Text);
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

        private void cboEntity_DropDownOpened(object sender, EventArgs e)
        {
            LoadEntities();
        }

        private void SearchUsers(string searchText)
        {
            if (_service == null)
            {
                txtStatus.Text = "Error: Service not initialized";
                return;
            }

            try
            {
                var fetchXml = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' top='30'>
                    <entity name='systemuser'>
                        <attribute name='systemuserid'/>
                        <attribute name='fullname'/>
                        <attribute name='domainname'/>
                        <order attribute='fullname' descending='false'/>
                        <filter type='and'>
                            <condition attribute='isdisabled' operator='eq' value='0'/>
                            {(!string.IsNullOrEmpty(searchText) ? $"<condition attribute='fullname' operator='like' value='%{searchText}%'/>" : "")}
                        </filter>
                    </entity>
                </fetch>";

                var result = _service.RetrieveMultiple(new FetchExpression(fetchXml));
                _users.Clear();
                foreach (var user in result.Entities)
                {
                    _users.Add(new UserWrapper
                    {
                        Id = user.Id,
                        FullName = user.GetAttributeValue<string>("fullname") ?? "(No name)",
                        DomainName = user.GetAttributeValue<string>("domainname"),
                        Entity = user
                    });
                }

                cboUser.ItemsSource = null;
                cboUser.ItemsSource = _users;
                txtStatus.Text = $"Found {_users.Count} users";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error searching users: {ex.Message}";
            }
        }

        private void LoadRecords(EntityWrapper entityWrapper)
        {
            if (_service == null)
            {
                txtStatus.Text = "Error: Service not initialized";
                return;
            }

            try
            {
                var entityName = entityWrapper.LogicalName;
                var primaryNameAttr = entityWrapper.PrimaryNameAttribute ?? "name";

                var fetchXml = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' top='30'>
                    <entity name='{entityName}'>
                        <attribute name='{entityWrapper.PrimaryIdAttribute}'/>
                        <attribute name='{primaryNameAttr}'/>
                        <order attribute='{primaryNameAttr}' descending='false'/>
                    </entity>
                </fetch>";

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
                txtStatus.Text = $"Error loading records: {ex.Message}";
            }
        }

        private void SearchRecords(string searchText)
        {
            if (cboEntity.SelectedItem is EntityWrapper entityWrapper)
            {
                LoadRecords(entityWrapper);
            }
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
                lstRoles.ItemsSource = results.Entities;
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
                lstTeams.ItemsSource = results.Entities;
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
                var query = new QueryExpression("principalobjectaccess")
                {
                    ColumnSet = new ColumnSet("accessrights"),
                    Criteria = new FilterExpression()
                };
                query.Criteria.AddCondition("principalid", ConditionOperator.Equal, userId);
                query.Criteria.AddCondition("objectid", ConditionOperator.Equal, recordId);

                var results = _service.RetrieveMultiple(query);
                if (results.Entities.Count > 0)
                {
                    var rights = results.Entities[0].GetAttributeValue<OptionSetValue>("accessrights")?.Value ?? 0;
                    return (rights & GetAccessRightMask(accessRight)) != 0;
                }

                try
                {
                    _service.Retrieve(entityName, recordId, new ColumnSet());
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private int GetAccessRightMask(string accessRight)
        {
            switch (accessRight)
            {
                case "ReadAccess": return 1;
                case "WriteAccess": return 2;
                case "DeleteAccess": return 4;
                case "CreateAccess": return 16;
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

    public class UserWrapper
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string DomainName { get; set; }
        public string DisplayName => FullName;
        public Entity Entity { get; set; }
    }

    public class EntityWrapper
    {
        public string LogicalName { get; set; }
        public string DisplayName { get; set; }
        public string EntityDisplayName => DisplayName;
        public string EntitySetName { get; set; }
        public string PrimaryIdAttribute { get; set; }
        public string PrimaryNameAttribute { get; set; }
        public Entity Entity { get; set; }
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
