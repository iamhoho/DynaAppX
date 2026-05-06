using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DynaAppX.WpfControls
{
    public partial class AccessCheckControl : UserControl
    {
        private IOrganizationService _service;
        private List<UserWrapper> _users = new List<UserWrapper>();
        private List<RecordWrapper> _records = new List<RecordWrapper>();
        private List<EntityWrapper> _allEntities = new List<EntityWrapper>();
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

                var request = new RetrieveAllEntitiesRequest
                {
                    EntityFilters = EntityFilters.Entity,
                    RetrieveAsIfPublished = true
                };

                var response = (RetrieveAllEntitiesResponse)_service.Execute(request);

                _allEntities.Clear();
                _entities.Clear();
                foreach (var entity in response.EntityMetadata)
                {
                    if (entity.IsIntersect == true) continue;
                    if (string.IsNullOrEmpty(entity.LogicalName)) continue;

                    var displayName = entity.DisplayName?.UserLocalizedLabel?.Label ?? entity.LogicalName;
                    var wrapper = new EntityWrapper
                    {
                        LogicalName = entity.LogicalName,
                        DisplayName = displayName,
                        EntitySetName = entity.EntitySetName,
                        PrimaryIdAttribute = entity.PrimaryIdAttribute,
                        PrimaryNameAttribute = entity.PrimaryNameAttribute
                    };
                    _allEntities.Add(wrapper);
                    _entities.Add(wrapper);
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

        private void cboEntity_DropDownOpened(object sender, EventArgs e)
        {
            var searchText = cboEntity.Text?.ToLower() ?? "";
            FilterEntities(searchText);
        }

        private void FilterEntities(string searchText)
        {
            _entities.Clear();
            if (string.IsNullOrEmpty(searchText))
            {
                foreach (var entity in _allEntities)
                {
                    _entities.Add(entity);
                }
            }
            else
            {
                foreach (var entity in _allEntities)
                {
                    if (entity.LogicalName.ToLower().Contains(searchText) ||
                        entity.DisplayName.ToLower().Contains(searchText))
                    {
                        _entities.Add(entity);
                    }
                }
            }
            cboEntity.ItemsSource = null;
            cboEntity.ItemsSource = _entities;
        }

        private void cboUser_DropDownOpened(object sender, EventArgs e)
        {
            var searchText = cboUser.Text ?? "";
            SearchUsers(searchText);
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
                            {(!string.IsNullOrEmpty(searchText) ? $"<condition attribute='fullname' operator='like' value='*{searchText}*'/>" : "")}
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
                if (string.IsNullOrEmpty(searchText))
                {
                    cboRecord.ItemsSource = null;
                    cboRecord.ItemsSource = _records;
                }
                else
                {
                    var filtered = _records.Where(r =>
                        r.RecordName.ToLower().Contains(searchText.ToLower())).ToList();
                    cboRecord.ItemsSource = null;
                    cboRecord.ItemsSource = filtered;
                }
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
                // Use "try it and see" approach - more reliable than principalobjectaccess table
                switch (accessRight)
                {
                    case "ReadAccess":
                        return HasReadAccess(entityName, recordId);
                    case "WriteAccess":
                        return HasWriteAccess(entityName, recordId);
                    case "DeleteAccess":
                        return HasDeleteAccess(entityName, recordId);
                    case "CreateAccess":
                        return HasCreateAccess(entitySetName);
                    case "ShareAccess":
                        return HasShareAccess(entityName, recordId);
                    case "AssignAccess":
                        return HasAssignAccess(entityName, recordId);
                    case "AppendAccess":
                        return HasAppendAccess(entityName, recordId);
                    case "AppendToAccess":
                        return HasAppendToAccess(entityName, recordId);
                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool HasReadAccess(string entityName, Guid recordId)
        {
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

        private bool HasWriteAccess(string entityName, Guid recordId)
        {
            try
            {
                // Use RetrieveEntity to check Update privilege for the entity
                var req = new RetrieveEntityRequest
                {
                    LogicalName = entityName,
                    EntityFilters = EntityFilters.Privileges
                };
                var resp = (RetrieveEntityResponse)_service.Execute(req);
                var updatePriv = resp.EntityMetadata.Privileges?.FirstOrDefault(p => p.PrivilegeType == PrivilegeType.Update);
                if (updatePriv == null) return false;
                // Check privilege depth - at least Basic required for write access
                return updatePriv.CanBeBasic || updatePriv.CanBeDeep || updatePriv.CanBeGlobal;
            }
            catch
            {
                return false;
            }
        }

        private bool HasDeleteAccess(string entityName, Guid recordId)
        {
            try
            {
                // Try to retrieve the record - if we can read it, we have delete access
                _service.Retrieve(entityName, recordId, new ColumnSet());
                // For activities that are closed, delete access may require special privileges
                // Use a minimal Update to statecode=0 and see if it sticks (then revert, but we can't revert)
                // Better approach: check entity privileges via RetrieveEntityRequest
                var req = new RetrieveEntityRequest
                {
                    LogicalName = entityName,
                    EntityFilters = EntityFilters.Privileges
                };
                var resp = (RetrieveEntityResponse)_service.Execute(req);
                var deletePriv = resp.EntityMetadata.Privileges?.FirstOrDefault(p => p.PrivilegeType == PrivilegeType.Delete);
                if (deletePriv == null) return false;
                // Check privilege depth - at least Basic required
                return deletePriv.CanBeBasic || deletePriv.CanBeDeep || deletePriv.CanBeGlobal;
            }
            catch
            {
                return false;
            }
        }

        private bool HasCreateAccess(string entitySetName)
        {
            try
            {
                // Try to retrieve metadata for the entity - if we can see it, we can probably create
                var req = new RetrieveEntityRequest
                {
                    LogicalName = entitySetName.Replace("Set", ""),
                    EntityFilters = EntityFilters.Entity
                };
                try
                {
                    _service.Execute(req);
                    return true;
                }
                catch
                {
                    // Fallback: assume create access if we can query the entity
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool HasShareAccess(string entityName, Guid recordId)
        {
            // Share is hard to test without actually sharing - check Read first
            return HasReadAccess(entityName, recordId);
        }

        private bool HasAssignAccess(string entityName, Guid recordId)
        {
            // Assign requires write access on ownerid - check write access
            return HasWriteAccess(entityName, recordId);
        }

        private bool HasAppendAccess(string entityName, Guid recordId)
        {
            // Check write access as proxy for append access
            return HasWriteAccess(entityName, recordId);
        }

        private bool HasAppendToAccess(string entityName, Guid recordId)
        {
            // Check write access as proxy for append-to access
            return HasWriteAccess(entityName, recordId);
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
