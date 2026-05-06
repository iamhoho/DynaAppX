using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DynaAppX.WpfControls
{
    public partial class GodPageControl : UserControl
    {
        private IOrganizationService _service;
        private List<EntityWrapper> _allEntities = new List<EntityWrapper>();
        private List<EntityWrapper> _entities = new List<EntityWrapper>();
        private List<RecordWrapper> _allRecords = new List<RecordWrapper>();
        private List<RecordWrapper> _records = new List<RecordWrapper>();
        private EntityWrapper _selectedEntity;
        private RecordWrapper _selectedRecord;
        private Entity _originalEntity;
        private List<AttributeInfo> _attributes = new List<AttributeInfo>();
        private List<ChangeInfo> _changedData = new List<ChangeInfo>();

        private static readonly string[] HiddenAttributes = new[]
        {
            "versionnumber", "utcconversiontimezonecode", "timezoneruleversionnumber",
            "owninguser", "owningteam", "owningbusinessunit", "overriddencreatedon",
            "modifiedonbehalfby", "importsequencenumber", "createdonbehalfby"
        };

        private static readonly string[] DisabledAttributes = new[]
        {
            "ownerid", "modifiedby", "createdon", "createdby", "modifiedon"
        };

        public GodPageControl()
        {
            InitializeComponent();
            this.Loaded += GodPageControl_Loaded;
        }

        public void SetService(IOrganizationService service)
        {
            _service = service;
            txtStatus.Text = "Service connected. Ready.";
            LoadEntities();
        }

        private void GodPageControl_Loaded(object sender, RoutedEventArgs e)
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
                        PrimaryNameAttribute = entity.PrimaryNameAttribute,
                        Metadata = entity
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

        private void cboEntity_DropDownOpened(object sender, EventArgs e)
        {
            FilterEntities(cboEntity.Text);
        }

        private void cboEntity_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterEntities(cboEntity.Text);
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
                var lower = searchText.ToLower();
                _entities.AddRange(_allEntities.Where(e =>
                    e.LogicalName.ToLower().Contains(lower) ||
                    e.DisplayName.ToLower().Contains(lower)));
            }
            cboEntity.ItemsSource = null;
            cboEntity.ItemsSource = _entities;
        }

        private void cboEntity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cboRecord.ItemsSource = null;
            _allRecords.Clear();
            _records.Clear();
            _selectedRecord = null;
            _originalEntity = null;

            _selectedEntity = cboEntity.SelectedItem as EntityWrapper;
            btnSave.IsEnabled = false;

            if (_selectedEntity != null)
            {
                LoadRecords(_selectedEntity);
            }
        }

        private void LoadRecords(EntityWrapper entityWrapper)
        {
            if (_service == null) return;

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
                _allRecords.Clear();
                _records.Clear();

                // Get entity set name for reference
                var entityMeta = GetEntityMetadata(entityName);
                var entitySetName = entityMeta?.EntitySetName ?? entityName + "s";

                foreach (var record in result.Entities)
                {
                    var recordName = record.GetAttributeValue<string>(primaryNameAttr) ?? "(No name)";
                    _allRecords.Add(new RecordWrapper
                    {
                        Id = record.Id,
                        RecordName = recordName,
                        EntitySetName = entitySetName,
                        Entity = record
                    });
                    _records.Add(_allRecords.Last());
                }

                cboRecord.ItemsSource = null;
                cboRecord.ItemsSource = _records;
                txtStatus.Text = $"Loaded {_records.Count} records";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error loading records: {ex.Message}";
            }
        }

        private void cboRecord_DropDownOpened(object sender, EventArgs e)
        {
            SearchRecords(cboRecord.Text);
        }

        private void cboRecord_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchRecords(cboRecord.Text);
        }

        private void SearchRecords(string searchText)
        {
            _records.Clear();
            if (string.IsNullOrEmpty(searchText))
            {
                _records.AddRange(_allRecords);
            }
            else
            {
                var lower = searchText.ToLower();
                _records.AddRange(_allRecords.Where(r =>
                    r.RecordName.ToLower().Contains(lower)));
            }
            cboRecord.ItemsSource = null;
            cboRecord.ItemsSource = _records;
        }

        private void cboRecord_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedRecord = cboRecord.SelectedItem as RecordWrapper;
            if (_selectedRecord != null && _selectedEntity != null)
            {
                LoadSelectedRecordData();
                btnSave.IsEnabled = true;
            }
            else
            {
                _attributes.Clear();
                ctrlAttributes.ItemsSource = null;
                btnSave.IsEnabled = false;
            }
        }

        private void LoadSelectedRecordData()
        {
            if (_service == null || _selectedEntity == null || _selectedRecord == null) return;

            try
            {
                // Retrieve full record
                _originalEntity = _service.Retrieve(
                    _selectedEntity.LogicalName,
                    _selectedRecord.Id,
                    new ColumnSet());

                // Build attribute controls
                BuildAttributeControls();

                txtStatus.Text = $"Loaded record: {_selectedRecord.RecordName}";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error loading record: {ex.Message}";
            }
        }

        private void BuildAttributeControls()
        {
            _attributes.Clear();

            if (_selectedEntity?.Metadata?.Attributes == null) return;

            var meta = _selectedEntity.Metadata;

            // Editable attributes first
            var editableAttrs = meta.Attributes
                .Where(a => a.AttributeOf == null && !a.IsPrimaryId
                    && !HiddenAttributes.Contains(a.LogicalName)
                    && !DisabledAttributes.Contains(a.LogicalName))
                .ToList();

            // Disabled attributes at the end
            var disabledAttrs = meta.Attributes
                .Where(a => a.AttributeOf == null && !a.IsPrimaryId
                    && !HiddenAttributes.Contains(a.LogicalName)
                    && DisabledAttributes.Contains(a.LogicalName))
                .ToList();

            var allAttrs = editableAttrs.Concat(disabledAttrs).ToList();

            foreach (var attr in allAttrs)
            {
                var info = new AttributeInfo
                {
                    LogicalName = attr.LogicalName,
                    DisplayName = attr.DisplayName?.UserLocalizedLabel?.Label ?? attr.LogicalName,
                    AttributeType = attr.AttributeType ?? AttributeTypeCode.String,
                    IsEditable = !DisabledAttributes.Contains(attr.LogicalName),
                    IsSystemField = DisabledAttributes.Contains(attr.LogicalName)
                };

                // Set current value
                if (_originalEntity.Contains(attr.LogicalName))
                {
                    info.Value = _originalEntity[attr.LogicalName];
                }

                // Build type-specific properties
                switch (info.AttributeType)
                {
                    case AttributeTypeCode.Picklist:
                    case AttributeTypeCode.State:
                    case AttributeTypeCode.Status:
                        info.IsPicklistType = true;
                        BuildPicklistOptions(info, attr);
                        break;

                    case AttributeTypeCode.Boolean:
                        info.IsBooleanType = true;
                        break;

                    case AttributeTypeCode.Integer:
                    case AttributeTypeCode.BigInt:
                    case AttributeTypeCode.Double:
                    case AttributeTypeCode.Decimal:
                    case AttributeTypeCode.Money:
                        info.IsNumberType = true;
                        break;

                    case AttributeTypeCode.DateTime:
                        info.IsDateTimeType = true;
                        break;

                    case AttributeTypeCode.String:
                    case AttributeTypeCode.Memo:
                        info.IsStringType = true;
                        if (info.AttributeType == AttributeTypeCode.Memo)
                            info.IsMemoType = true;
                        break;

                    case AttributeTypeCode.Lookup:
                    case AttributeTypeCode.Owner:
                        info.IsLookupType = true;
                        BuildLookupOptions(info, attr);
                        // Set lookup id and name from original entity
                        var lookupAttrName = GetLookupAttributeName(attr.LogicalName);
                        if (_originalEntity.Contains(lookupAttrName))
                        {
                            var refEntity = _originalEntity.GetAttributeValue<EntityReference>(lookupAttrName);
                            if (refEntity != null)
                            {
                                info.LookupId = refEntity.Id;
                                info.LookupName = refEntity.Name;
                            }
                        }
                        break;

                    default:
                        info.IsUnsupportedType = true;
                        break;
                }

                _attributes.Add(info);
            }

            ctrlAttributes.ItemsSource = null;
            ctrlAttributes.ItemsSource = _attributes;
        }

        private void BuildPicklistOptions(AttributeInfo info, AttributeMetadata attr)
        {
            var options = new List<OptionSetItem>();

            if (attr is PicklistAttributeMetadata picklist && picklist.OptionSet?.Options != null)
            {
                foreach (var opt in picklist.OptionSet.Options)
                {
                    if (opt.Value.HasValue)
                    {
                        var label = opt.Label?.UserLocalizedLabel?.Label ?? opt.Value.Value.ToString();
                        options.Add(new OptionSetItem
                        {
                            Label = $"{label} ({opt.Value.Value})",
                            Value = opt.Value.Value
                        });
                    }
                }
            }
            else if (attr is StateAttributeMetadata state && state.OptionSet?.Options != null)
            {
                foreach (var opt in state.OptionSet.Options)
                {
                    if (opt.Value.HasValue)
                    {
                        var label = opt.Label?.UserLocalizedLabel?.Label ?? "(No label)";
                        options.Add(new OptionSetItem
                        {
                            Label = label,
                            Value = opt.Value.Value
                        });
                    }
                }
            }
            else if (attr is StatusAttributeMetadata status && status.OptionSet?.Options != null)
            {
                foreach (var opt in status.OptionSet.Options)
                {
                    if (opt.Value.HasValue)
                    {
                        var label = opt.Label?.UserLocalizedLabel?.Label ?? "(No label)";
                        options.Add(new OptionSetItem
                        {
                            Label = label,
                            Value = opt.Value.Value
                        });
                    }
                }
            }

            info.Options = options;

            // Set current value
            if (_originalEntity.Contains(info.LogicalName))
            {
                var val = _originalEntity[info.LogicalName];
                if (val is OptionSetValue osv)
                    info.Value = osv.Value;
                else if (val != null)
                    info.Value = val;
            }
        }

        private void BuildLookupOptions(AttributeInfo info, AttributeMetadata attr)
        {
            if (attr is LookupAttributeMetadata lookup && lookup.Targets != null)
            {
                info.TargetEntities = lookup.Targets.ToList();
                if (info.TargetEntities.Count > 0)
                {
                    LoadLookupOptions(info, info.TargetEntities[0]);
                }
                else
                {
                    info.LookupOptions = new List<RecordWrapper>();
                    info.FilteredLookupOptions = new List<RecordWrapper>();
                }
            }
            else
            {
                info.LookupOptions = new List<RecordWrapper>();
                info.FilteredLookupOptions = new List<RecordWrapper>();
            }
        }

        private void LoadLookupOptions(AttributeInfo info, string entityName)
        {
            if (_service == null) return;

            try
            {
                var entityMeta = GetEntityMetadata(entityName);
                var primaryNameAttr = entityMeta?.PrimaryNameAttribute ?? "name";
                var primaryIdAttr = entityMeta?.PrimaryIdAttribute ?? entityName + "id";

                var fetchXml = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' top='30'>
                    <entity name='{entityName}'>
                        <attribute name='{primaryIdAttr}'/>
                        <attribute name='{primaryNameAttr}'/>
                        <order attribute='{primaryNameAttr}' descending='false'/>
                    </entity>
                </fetch>";

                var result = _service.RetrieveMultiple(new FetchExpression(fetchXml));
                var options = new List<RecordWrapper>();

                foreach (var record in result.Entities)
                {
                    var recordName = record.GetAttributeValue<string>(primaryNameAttr) ?? "(No name)";
                    options.Add(new RecordWrapper
                    {
                        Id = record.Id,
                        RecordName = recordName,
                        Entity = record
                    });
                }

                info.LookupOptions = options;
            }
            catch
            {
                info.LookupOptions = new List<RecordWrapper>();
            }
        }

        private EntityMetadata GetEntityMetadata(string entityName)
        {
            try
            {
                var req = new RetrieveEntityRequest
                {
                    LogicalName = entityName,
                    EntityFilters = EntityFilters.Entity
                };
                var resp = (RetrieveEntityResponse)_service.Execute(req);
                return resp.EntityMetadata;
            }
            catch
            {
                return null;
            }
        }

        private string GetLookupAttributeName(string logicalName)
        {
            // Most lookups are just the logical name
            // but some have _id suffix in early-stage dynamics
            return logicalName;
        }

        private void cboLookup_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is ComboBox cbo && cbo.DataContext is AttributeInfo info)
            {
                FilterLookupByText(info, cbo.Text);
            }
        }

        private void cboLookup_DropDownOpened(object sender, EventArgs e)
        {
            if (sender is ComboBox cbo && cbo.DataContext is AttributeInfo info)
            {
                FilterLookupByText(info, cbo.Text);
                cbo.ItemsSource = null;
                cbo.ItemsSource = info.FilteredLookupOptions;
            }
        }

        private void FilterLookupByText(AttributeInfo info, string searchText)
        {
            if (info.LookupOptions == null || info.LookupOptions.Count == 0) return;

            if (string.IsNullOrEmpty(searchText))
            {
                info.FilteredLookupOptions = info.LookupOptions.ToList();
            }
            else
            {
                var lower = searchText.ToLower();
                info.FilteredLookupOptions = info.LookupOptions
                    .Where(r => r.RecordName.ToLower().Contains(lower))
                    .ToList();
            }
        }

        private void btnClearLookup_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string attrName)
            {
                var info = _attributes.FirstOrDefault(a => a.LogicalName == attrName);
                if (info != null)
                {
                    info.LookupId = Guid.Empty;
                    info.LookupName = null;
                    // Null out the original entity so DetectChanges sees a change from previous value to null
                    if (_originalEntity != null)
                    {
                        var lookupAttrName = GetLookupAttributeName(attrName);
                        if (_originalEntity.Contains(lookupAttrName))
                        {
                            _originalEntity[lookupAttrName] = null;
                        }
                        else
                        {
                            _originalEntity.Attributes[attrName] = null;
                        }
                    }
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRecord == null || _selectedEntity == null) return;

            // Collect changed fields
            _changedData.Clear();
            DetectChanges();

            if (_changedData.Count == 0)
            {
                MessageBox.Show("No changes detected.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Show confirmation dialog
            var result = MessageBox.Show(
                $"You have {_changedData.Count} change(s). Do you want to save?",
                "Confirm Save",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            // Check for external data changes
            if (HasExternalChanges())
            {
                var continueResult = MessageBox.Show(
                    "The record has been modified externally. Continue saving?",
                    "External Change Detected",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (continueResult != MessageBoxResult.Yes)
                {
                    LoadSelectedRecordData();
                    return;
                }
            }

            SaveChanges();
        }

        private void DetectChanges()
        {
            foreach (var info in _attributes)
            {
                if (!info.IsEditable) continue;

                var oldValue = _originalEntity.Contains(info.LogicalName) ? _originalEntity[info.LogicalName] : null;
                object newValue = null;
                string updateAttrName = null;
                object updateValue = null;

                switch (info.AttributeType)
                {
                    case AttributeTypeCode.Picklist:
                    case AttributeTypeCode.State:
                    case AttributeTypeCode.Status:
                        var newIntVal = info.IntValue;
                        if (newIntVal.HasValue)
                        {
                            var origVal = oldValue is OptionSetValue osv2 ? osv2.Value : (oldValue as int?);
                            if (origVal != newIntVal.Value)
                            {
                                newValue = $"{GetOptionLabel(info, newIntVal.Value)} ({newIntVal.Value})";
                                updateAttrName = info.LogicalName;
                                updateValue = newIntVal.Value;  // for update, use int directly
                            }
                        }
                        break;

                    case AttributeTypeCode.Boolean:
                        if (info.Value is bool boolVal)
                        {
                            var origBool = oldValue is bool b ? b : false;
                            if (origBool != boolVal)
                            {
                                newValue = boolVal.ToString();
                                updateAttrName = info.LogicalName;
                                updateValue = boolVal;
                            }
                        }
                        break;

                    case AttributeTypeCode.Lookup:
                    case AttributeTypeCode.Owner:
                        var lookupAttrName = GetLookupAttributeName(info.LogicalName);
                        var origRef = _originalEntity.Contains(lookupAttrName) ? _originalEntity.GetAttributeValue<EntityReference>(lookupAttrName) : null;
                        var newRefId = info.LookupId;
                        var newRefName = info.LookupName;

                        if (origRef?.Id != newRefId)
                        {
                            var oldDisplay = origRef != null ? $"{origRef.Name} ({origRef.Id})" : null;
                            var newDisplay = newRefId != Guid.Empty && !string.IsNullOrEmpty(newRefName)
                                ? $"{newRefName} ({newRefId})" : null;

                            if (newRefId != Guid.Empty)
                            {
                                // Get entity set name for the target
                                var targetEntity = info.TargetEntities?.FirstOrDefault();
                                if (!string.IsNullOrEmpty(targetEntity))
                                {
                                    var targetMeta = GetEntityMetadata(targetEntity);
                                    var entitySetName = targetMeta?.EntitySetName ?? targetEntity + "s";
                                    updateAttrName = info.LogicalName + "@odata.bind";
                                    updateValue = $"/{entitySetName}({newRefId})";
                                }
                            }
                            else
                            {
                                // Deleting the reference - track for separate delete
                                updateAttrName = info.LogicalName + "@odata.bind";
                                updateValue = null; // null means remove the reference
                            }

                            _changedData.Add(new ChangeInfo
                            {
                                DisplayName = info.DisplayName,
                                AttributeName = info.LogicalName,
                                OldValue = oldDisplay,
                                NewValue = newDisplay,
                                UpdateAttributeName = updateAttrName,
                                UpdateValue = updateValue,
                                IsLookup = true,
                                OldLookupId = origRef?.Id,
                                NewLookupId = newRefId
                            });
                            continue;
                        }
                        break;

                    case AttributeTypeCode.DateTime:
                        if (oldValue is DateTime origDt && info.Value is DateTime newDt)
                        {
                            if (origDt != newDt)
                            {
                                newValue = newDt.ToString("o");
                                updateAttrName = info.LogicalName;
                                updateValue = newDt;
                            }
                        }
                        else if (oldValue == null && info.Value != null)
                        {
                            newValue = ((DateTime)info.Value).ToString("o");
                            updateAttrName = info.LogicalName;
                            updateValue = info.Value;
                        }
                        else if (oldValue != null && info.Value == null)
                        {
                            // Clearing a date
                            newValue = null;
                            updateAttrName = info.LogicalName;
                            updateValue = null;
                        }
                        break;

                    case AttributeTypeCode.Integer:
                    case AttributeTypeCode.BigInt:
                    case AttributeTypeCode.Double:
                    case AttributeTypeCode.Decimal:
                    case AttributeTypeCode.Money:
                    case AttributeTypeCode.String:
                    case AttributeTypeCode.Memo:
                        if (!Equals(oldValue, info.Value))
                        {
                            newValue = info.Value?.ToString();
                            updateAttrName = info.LogicalName;
                            updateValue = info.Value;
                        }
                        break;
                }

                if (!string.IsNullOrEmpty(updateAttrName))
                {
                    _changedData.Add(new ChangeInfo
                    {
                        DisplayName = info.DisplayName,
                        AttributeName = info.LogicalName,
                        OldValue = oldValue?.ToString(),
                        NewValue = newValue,
                        UpdateAttributeName = updateAttrName,
                        UpdateValue = updateValue
                    });
                }
            }
        }

        private string GetOptionLabel(AttributeInfo info, int value)
        {
            var opt = info.Options?.FirstOrDefault(o => o.Value == value);
            return opt?.Label ?? value.ToString();
        }

        private bool HasExternalChanges()
        {
            try
            {
                var currentVersion = _service.Retrieve(
                    _selectedEntity.LogicalName,
                    _selectedRecord.Id,
                    new ColumnSet("versionnumber"));

                // Check versionnumber attribute for external change detection
                if (_originalEntity.Contains("versionnumber") && currentVersion.Contains("versionnumber"))
                {
                    var versionNum = currentVersion.GetAttributeValue<long>("versionnumber");
                    var origVersion = _originalEntity.GetAttributeValue<long>("versionnumber");
                    return versionNum != origVersion;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private void SaveChanges()
        {
            if (_service == null || _selectedEntity == null || _selectedRecord == null) return;

            var updateEntity = new Entity(_selectedEntity.LogicalName) { Id = _selectedRecord.Id };
            bool hasUpdate = false;

            // Collect update values (excluding lookups which need special handling)
            // Lookups need special handling - collect them separately
            var lookupUpdates = new List<ChangeInfo>();
            var lookupClears = new List<string>();

            foreach (var change in _changedData)
            {
                if (change.IsLookup)
                {
                    if (change.NewLookupId == Guid.Empty)
                    {
                        // Clear the lookup reference
                        lookupClears.Add(change.AttributeName);
                    }
                    else
                    {
                        // Setting a lookup - use @odata.bind format
                        lookupUpdates.Add(change);
                    }
                }
                else
                {
                    updateEntity[change.UpdateAttributeName] = change.UpdateValue;
                    hasUpdate = true;
                }
            }

            bool hasError = false;
            string lastError = null;

            // Update entity (non-lookup fields)
            if (hasUpdate)
            {
                try
                {
                    _service.Update(updateEntity);
                }
                catch (Exception ex)
                {
                    hasError = true;
                    lastError = $"Update failed: {ex.Message}";
                }
            }

            // Handle lookup updates (set new references) - each as separate Update call
            foreach (var change in lookupUpdates)
            {
                if (hasError) break;
                try
                {
                    var lookupEntity = new Entity(_selectedEntity.LogicalName) { Id = _selectedRecord.Id };
                    lookupEntity[change.UpdateAttributeName] = change.UpdateValue;
                    _service.Update(lookupEntity);
                }
                catch (Exception ex)
                {
                    hasError = true;
                    lastError = $"Failed to set lookup '{change.DisplayName}': {ex.Message}";
                }
            }

            // Handle lookup clears (set to null) - each as separate Update call
            foreach (var attrName in lookupClears)
            {
                if (hasError) break;
                try
                {
                    var clearEntity = new Entity(_selectedEntity.LogicalName) { Id = _selectedRecord.Id };
                    clearEntity[attrName] = null;
                    _service.Update(clearEntity);
                }
                catch (Exception ex)
                {
                    hasError = true;
                    lastError = $"Failed to clear lookup '{attrName}': {ex.Message}";
                }
            }

            if (!hasError)
            {
                MessageBox.Show("Success!", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadSelectedRecordData();
            }
            else
            {
                MessageBox.Show($"Error saving: {lastError}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                LoadSelectedRecordData();
            }
        }

        private string FormatValue(object value)
        {
            if (value == null) return "(null)";
            return value.ToString();
        }
    }

    public class EntityWrapper
    {
        public string LogicalName { get; set; }
        public string DisplayName { get; set; }
        public string EntityDisplayName => DisplayName;
        public string EntitySetName { get; set; }
        public string PrimaryIdAttribute { get; set; }
        public string PrimaryNameAttribute { get; set; }
        public EntityMetadata Metadata { get; set; }
        public Entity Entity { get; set; }
    }

    public class RecordWrapper
    {
        public Guid Id { get; set; }
        public string RecordName { get; set; }
        public string EntitySetName { get; set; }
        public Entity Entity { get; set; }
    }

    public class AttributeInfo : INotifyPropertyChanged
    {
        private object _value;
        private Guid _lookupId;
        private string _lookupName;

        public string LogicalName { get; set; }
        public string DisplayName { get; set; }
        public AttributeTypeCode AttributeType { get; set; }
        public bool IsEditable { get; set; }
        public bool IsSystemField { get; set; }
        public bool IsRequired { get; set; }
        public List<string> TargetEntities { get; set; } = new List<string>();

        public object Value
        {
            get => _value;
            set { _value = value; OnPropertyChanged(); }
        }

        public Guid LookupId
        {
            get => _lookupId;
            set { _lookupId = value; OnPropertyChanged(); }
        }

        public string LookupName
        {
            get => _lookupName;
            set { _lookupName = value; OnPropertyChanged(); }
        }

        // Type flags for XAML binding
        public bool IsPicklistType { get; set; }
        public bool IsBooleanType { get; set; }
        public bool IsNumberType { get; set; }
        public bool IsDateTimeType { get; set; }
        public bool IsStringType { get; set; }
        public bool IsMemoType { get; set; }
        public bool IsLookupType { get; set; }
        public bool IsUnsupportedType { get; set; }

        public List<OptionSetItem> Options { get; set; } = new List<OptionSetItem>();
        public List<RecordWrapper> LookupOptions { get; set; } = new List<RecordWrapper>();
        public List<RecordWrapper> FilteredLookupOptions { get; set; } = new List<RecordWrapper>();

        // Typed value accessors for specific attribute types
        public int? IntValue
        {
            get
            {
                if (_value is int i) return i;
                if (_value is OptionSetValue osv) return osv.Value;
                if (_value is null) return null;
                return null;
            }
            set
            {
                if (value.HasValue)
                    _value = value.Value;
                else
                    _value = null;
                OnPropertyChanged();
            }
        }

        public bool? BoolValue
        {
            get
            {
                if (_value is bool b) return b;
                return null;
            }
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        public DateTime? DateTimeValue
        {
            get
            {
                if (_value is DateTime dt) return dt;
                return null;
            }
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class OptionSetItem
    {
        public string Label { get; set; }
        public int Value { get; set; }
    }

    public class ChangeInfo
    {
        public string DisplayName { get; set; }
        public string AttributeName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string UpdateAttributeName { get; set; }
        public object UpdateValue { get; set; }
        public bool IsLookup { get; set; }
        public Guid OldLookupId { get; set; }
        public Guid NewLookupId { get; set; }
    }
}