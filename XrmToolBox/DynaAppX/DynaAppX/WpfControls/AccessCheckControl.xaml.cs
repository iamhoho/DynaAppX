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
        private EntityCollection _entities;
        private List<Entity> _users = new List<Entity>();
        private List<RecordWrapper> _records = new List<RecordWrapper>();

        public event Action<string> OpenRecordRequested;

        public AccessCheckControl()
        {
            InitializeComponent();
            this.Loaded += AccessCheckControl_Loaded;
        }

        public void SetService(IOrganizationService service)
        {
            _service = service;
        }

        private void AccessCheckControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadEntities();
        }

        private void LoadEntities()
        {
            try
            {
                // Get custom entities
                var customQuery = new QueryExpression("entitydefinition")
                {
                    ColumnSet = new ColumnSet("LogicalName", "DisplayName", "EntitySetName", "PrimaryIdAttribute", "PrimaryNameAttribute"),
                    Orders = { new OrderExpression("DisplayName", OrderType.Ascending) }
                };
                customQuery.Criteria.AddCondition("IsIntersect", ConditionOperator.Equal, false);
                customQuery.Criteria.AddCondition("IsCustomEntity", ConditionOperator.Equal, true);

                _entities = _service.RetrieveMultiple(customQuery);

                // Get system entities
                var systemQuery = new QueryExpression("entitydefinition")
                {
                    ColumnSet = new ColumnSet("LogicalName", "DisplayName", "EntitySetName", "PrimaryIdAttribute", "PrimaryNameAttribute"),
                    Orders = { new OrderExpression("DisplayName", OrderType.Ascending) }
                };
                systemQuery.Criteria.AddCondition("IsIntersect", ConditionOperator.Equal, false);

                var systemEntities = _service.RetrieveMultiple(systemQuery);

                var allEntities = new List<Entity>();
                foreach (var entity in _entities.Entities)
                {
                    allEntities.Add(entity);
                }
                foreach (var entity in systemEntities.Entities)
                {
                    allEntities.Add(entity);
                }

                cboEntity.ItemsSource = null;
                cboEntity.ItemsSource = allEntities;
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
            if (cboEntity.SelectedItem is Entity entity)
            {
                LoadRecords(entity);
            }
        }

        private void cboUser_DropDownOpened(object sender, EventArgs e)
        {
            SearchUsers("");
        }

        private void cboRecord_DropDownOpened(object sender, EventArgs e)
        {
            if (cboRecord.IsEditable)
            {
                SearchRecords(cboRecord.Text);
            }
        }

        private void cboUser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboUser.SelectedItem is Entity user)
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
            try
            {
                var query = new QueryExpression("systemuser")
                {
                    ColumnSet = new ColumnSet("systemuserid", "fullname", "domainname"),
                    Orders = { new OrderExpression("fullname", OrderType.Ascending) },
                    TopCount = 30
                };

                if (!string.IsNullOrEmpty(searchText))
                {
                    query.Criteria.AddCondition("fullname", ConditionOperator.Like, $"%{searchText}%");
                }

                var results = _service.RetrieveMultiple(query);
                _users.Clear();
                foreach (var user in results.Entities)
                {
                    _users.Add(user);
                }

                cboUser.ItemsSource = null;
                cboUser.ItemsSource = _users;
                cboUser.DisplayMemberPath = "FullName";
                cboUser.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error searching users: {ex.Message}";
            }
        }

        private void LoadRecords(Entity entity)
        {
            try
            {
                var entityName = entity.GetAttributeValue<string>("LogicalName");
                var primaryNameAttr = GetPrimaryNameAttribute(entityName);

                var query = new QueryExpression(entityName)
                {
                    ColumnSet = new ColumnSet(primaryNameAttr),
                    Orders = { new OrderExpression(primaryNameAttr, OrderType.Ascending) },
                    TopCount = 30
                };

                var results = _service.RetrieveMultiple(query);
                _records.Clear();
                foreach (var record in results.Entities)
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
                cboRecord.DisplayMemberPath = "RecordName";
                cboRecord.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error loading records: {ex.Message}";
            }
        }

        private void SearchRecords(string searchText)
        {
            if (cboEntity.SelectedItem is Entity entity)
            {
                LoadRecords(entity);
            }
        }

        private string GetPrimaryNameAttribute(string entityName)
        {
            try
            {
                var query = new QueryExpression("entitydefinition")
                {
                    ColumnSet = new ColumnSet("PrimaryNameAttribute"),
                    Criteria = new FilterExpression()
                };
                query.Criteria.AddCondition("LogicalName", ConditionOperator.Equal, entityName);

                var results = _service.RetrieveMultiple(query);
                if (results.Entities.Count > 0)
                {
                    return results.Entities[0].GetAttributeValue<string>("PrimaryNameAttribute") ?? "name";
                }
            }
            catch { }
            return "name";
        }

        private void LoadUserRoles(Entity user)
        {
            try
            {
                var userId = user.Id;

                var query = new QueryExpression("role")
                {
                    ColumnSet = new ColumnSet("roleid", "name")
                };

                var link = new LinkEntity("role", "systemuserroles", "roleid", "roleid", JoinOperator.Inner);
                link.LinkCriteria.AddCondition("systemuserid", ConditionOperator.Equal, userId);
                query.LinkEntities.Add(link);

                var results = _service.RetrieveMultiple(query);
                lstRoles.ItemsSource = results.Entities;
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error loading roles: {ex.Message}";
            }
        }

        private void LoadUserTeams(Entity user)
        {
            try
            {
                var userId = user.Id;

                var query = new QueryExpression("team")
                {
                    ColumnSet = new ColumnSet("teamid", "name")
                };

                var link = new LinkEntity("team", "teammembership", "teamid", "teamid", JoinOperator.Inner);
                link.LinkCriteria.AddCondition("systemuserid", ConditionOperator.Equal, userId);
                query.LinkEntities.Add(link);

                var results = _service.RetrieveMultiple(query);
                lstTeams.ItemsSource = results.Entities;
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error loading teams: {ex.Message}";
            }
        }

        private void CheckAccessRights()
        {
            if (cboUser.SelectedItem == null || cboRecord.SelectedItem == null || cboEntity.SelectedItem == null)
            {
                lstAccessRights.ItemsSource = null;
                return;
            }

            try
            {
                var userId = (cboUser.SelectedItem as Entity).Id;
                var recordWrapper = cboRecord.SelectedItem as RecordWrapper;
                var recordId = recordWrapper?.Id ?? Guid.Empty;
                var entityName = (cboEntity.SelectedItem as Entity).GetAttributeValue<string>("LogicalName");
                var entitySetName = (cboEntity.SelectedItem as Entity).GetAttributeValue<string>("EntitySetName");

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
            try
            {
                // Check access via principalobjectaccess table
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

                // Fallback: try to retrieve the record as the user would
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

    public class AccessRightInfo
    {
        public string RightName { get; set; }
        public bool HasAccess { get; set; }
    }

    public class RecordWrapper
    {
        public Guid Id { get; set; }
        public string RecordName { get; set; }
        public Entity Entity { get; set; }
    }
}
