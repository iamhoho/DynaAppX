using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        private CancellationTokenSource _searchCts;

        public event Action<string> OpenRecordRequested;

        public AccessCheckControl()
        {
            InitializeComponent();
            this.Loaded += AccessCheckControl_Loaded;
            cboRecord.AddHandler(TextBox.TextChangedEvent, new TextChangedEventHandler(cboRecord_TextChanged), true);
            cboUser.AddHandler(TextBox.TextChangedEvent, new TextChangedEventHandler(cboUser_TextChanged), true);
            cboEntity.AddHandler(TextBox.TextChangedEvent, new TextChangedEventHandler(cboEntity_TextChanged), true);
            cboRecord.IsEnabled = false;
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
            LoadingOverlay.Show("Loading entities...");
            txtStatus.Text = "Loading entities...";

            try
            {
                await SharedMetadataCache.Instance.RefreshEntitiesAsync(_service);

                _allEntities = SharedMetadataCache.Instance.GetAllEntities(_service);
                _entities = new List<EntityWrapper>(_allEntities);

                cboEntity.ItemsSource = null;
                cboEntity.ItemsSource = _entities;
                _entitiesLoaded = true;

                txtStatus.Text = $"Loaded {_entities.Count} entities";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error loading entities: {ex.Message}";
            }
            finally
            {
                btnLoadEntities.IsEnabled = true;
                LoadingOverlay.Hide();
            }
        }

        private async void cboEntity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cboRecord.ItemsSource = null;
            _records.Clear();
            if (cboEntity.SelectedItem is EntityWrapper entityWrapper)
            {
                cboRecord.IsEnabled = true;
                await SearchRecordsAsync("");
            }
            else
            {
                cboRecord.IsEnabled = false;
                lstAccessRights.ItemsSource = null;
            }
        }

        private void cboEntity_DropDownOpened(object sender, EventArgs e)
        {
            if (!_entitiesLoaded) return;

            var searchText = cboEntity.Text?.ToLower() ?? "";
            FilterEntities(searchText);
        }

        private void cboEntity_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_entitiesLoaded) return;

            var searchText = cboEntity.Text?.ToLower() ?? "";
            if (string.IsNullOrWhiteSpace(searchText))
            {
                _entities.Clear();
                _entities.AddRange(_allEntities);
                cboEntity.ItemsSource = null;
                cboEntity.ItemsSource = _entities;
                return;
            }

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
            if (string.IsNullOrWhiteSpace(searchText))
            {
                // Show recent users or all users when dropdown opens with no search
                searchText = "";
            }
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

        private async void cboUser_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_service == null) return;

            var searchText = cboUser.Text ?? "";
            if (string.IsNullOrWhiteSpace(searchText))
            {
                cboUser.ItemsSource = null;
                _users.Clear();
                return;
            }

            if (searchText.Length < 2) return;

            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;

            try
            {
                _users = await SharedMetadataCache.Instance.SearchUsersAsync(_service, searchText);
                if (token.IsCancellationRequested) return;
                cboUser.ItemsSource = null;
                cboUser.ItemsSource = _users;
                txtStatus.Text = $"Found {_users.Count} users";
            }
            catch (OperationCanceledException)
            {
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
                _ = SearchRecordsAsync(cboRecord.Text ?? "");
            }
        }

        private async void cboRecord_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (cboEntity.SelectedItem != null)
            {
                await SearchRecordsAsync(cboRecord.Text ?? "");
            }
        }

        private void cboUser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboUser.SelectedItem is UserWrapper user)
            {
                LoadingOverlay.Show("Loading user info...");
                LoadUserRoles(user);
                LoadUserTeams(user);
                CheckAccessRights();
                LoadingOverlay.Hide();
            }
            else
            {
                lstRoles.ItemsSource = null;
                lstTeams.ItemsSource = null;
                lstAccessRights.ItemsSource = null;
            }
        }

        private void cboRecord_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckAccessRights();
        }

        private async Task SearchRecordsAsync(string searchText)
        {
            if (_service == null || !_entitiesLoaded) return;

            if (!(cboEntity.SelectedItem is EntityWrapper entityWrapper))
            {
                txtStatus.Text = "Please select an entity first";
                return;
            }

            // Cancel any previous search
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;

            try
            {
                LoadingOverlay.Show("Searching records...");
                var primaryNameAttr = entityWrapper.PrimaryNameAttribute ?? "name";

                await SharedMetadataCache.Instance.GetEntityAttributesAsync(_service, entityWrapper.LogicalName);
                if (token.IsCancellationRequested) return;

                var fetchXml = CrmHelper.BuildRecordSearchFetchXml(entityWrapper, searchText);
                var result = await Task.Run(() => _service.RetrieveMultiple(new FetchExpression(fetchXml)), token);
                if (token.IsCancellationRequested) return;

                _records.Clear();
                foreach (var record in result.Entities)
                {
                    var name = string.IsNullOrEmpty(primaryNameAttr)
                        ? null
                        : record.GetAttributeValue<object>(primaryNameAttr);
                    _records.Add(new RecordWrapper
                    {
                        Id = record.Id,
                        RecordName = name?.ToString() ?? "(No name)",
                        Entity = record
                    });
                }

                cboRecord.ItemsSource = null;
                cboRecord.ItemsSource = _records;
            }
            catch (OperationCanceledException)
            {
                // Expected when search is cancelled
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error searching records: {ex.Message}";
            }
            finally
            {
                LoadingOverlay.Hide();
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

            if (!(cboUser.SelectedItem is UserWrapper) || cboRecord.SelectedItem == null || cboEntity.SelectedItem == null)
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

                // Get all access rights in one call
                var allRights = GetAllAccessRights(userId, recordId, entityName);

                var accessRights = new List<AccessRightInfo>
                {
                    new AccessRightInfo { RightName = "ReadAccess", HasAccess = allRights.HasFlag(Microsoft.Crm.Sdk.Messages.AccessRights.ReadAccess) },
                    new AccessRightInfo { RightName = "WriteAccess", HasAccess = allRights.HasFlag(Microsoft.Crm.Sdk.Messages.AccessRights.WriteAccess) },
                    new AccessRightInfo { RightName = "DeleteAccess", HasAccess = allRights.HasFlag(Microsoft.Crm.Sdk.Messages.AccessRights.DeleteAccess) },
                    new AccessRightInfo { RightName = "CreateAccess", HasAccess = allRights.HasFlag(Microsoft.Crm.Sdk.Messages.AccessRights.CreateAccess) },
                    new AccessRightInfo { RightName = "ShareAccess", HasAccess = allRights.HasFlag(Microsoft.Crm.Sdk.Messages.AccessRights.ShareAccess) },
                    new AccessRightInfo { RightName = "AssignAccess", HasAccess = allRights.HasFlag(Microsoft.Crm.Sdk.Messages.AccessRights.AssignAccess) },
                    new AccessRightInfo { RightName = "AppendAccess", HasAccess = allRights.HasFlag(Microsoft.Crm.Sdk.Messages.AccessRights.AppendAccess) },
                    new AccessRightInfo { RightName = "AppendToAccess", HasAccess = allRights.HasFlag(Microsoft.Crm.Sdk.Messages.AccessRights.AppendToAccess) }
                };

                lstAccessRights.ItemsSource = accessRights;
                txtStatus.Text = $"Access checked for {entitySetName}/{recordId}";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error checking access: {ex.Message}";
            }
        }

        private Microsoft.Crm.Sdk.Messages.AccessRights GetAllAccessRights(Guid userId, Guid recordId, string entityName)
        {
            if (_service == null) return 0;

            try
            {
                var request = new Microsoft.Crm.Sdk.Messages.RetrievePrincipalAccessRequest
                {
                    Principal = new EntityReference("systemuser", userId),
                    Target = new EntityReference(entityName, recordId)
                };

                var response = (Microsoft.Crm.Sdk.Messages.RetrievePrincipalAccessResponse)_service.Execute(request);
                return response.AccessRights;
            }
            catch
            {
                return 0;
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
