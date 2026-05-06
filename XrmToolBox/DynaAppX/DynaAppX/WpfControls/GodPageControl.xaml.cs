using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DynaAppX.Services;

namespace DynaAppX.WpfControls
{
    public partial class GodPageControl : UserControl
    {
        private IOrganizationService _service;
        private EntityWrapper _selectedEntity;
        private RecordWrapper _selectedRecord;
        private Entity _originalData;
        private List<AttributeItem> _attributes = new List<AttributeItem>();
        private List<ChangeItem> _changes = new List<ChangeItem>();
        private bool _isLoading = false;

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
        }

        private void GodPageControl_Loaded(object sender, RoutedEventArgs e)
        {
            lstAttributes.ItemsSource = _attributes;
        }

        private void cboEntity_DropDownOpened(object sender, EventArgs e)
        {
            var entities = SharedMetadataCache.Instance.GetAllEntities(_service);
            cboEntity.ItemsSource = null;
            cboEntity.ItemsSource = entities;
        }

        private void cboEntity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedEntity = cboEntity.SelectedItem as EntityWrapper;
            _selectedRecord = null;
            _originalData = null;
            _attributes.Clear();
            lstAttributes.ItemsSource = null;
            btnSave.IsEnabled = false;
            txtStatus.Text = "";
        }

        private void cboRecord_DropDownOpened(object sender, EventArgs e)
        {
            if (_selectedEntity == null) return;
            SearchRecords("");
        }

        private void cboRecord_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_selectedEntity == null) return;
            SearchRecords(cboRecord.Text ?? "");
        }

        private void SearchRecords(string searchText)
        {
            if (_service == null || _selectedEntity == null) return;

            try
            {
                var fetchXml = BuildRecordSearchFetchXml(_selectedEntity, searchText);
                var result = _service.RetrieveMultiple(new FetchExpression(fetchXml));

                var records = result.Entities.Select(r => new RecordWrapper
                {
                    Id = r.Id,
                    RecordName = r.GetAttributeValue<string>(_selectedEntity.PrimaryNameAttribute) ?? "(No name)",
                    Entity = r
                }).ToList();

                cboRecord.ItemsSource = null;
                cboRecord.ItemsSource = records;
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error: {ex.Message}";
            }
        }

        private string BuildRecordSearchFetchXml(EntityWrapper entityWrapper, string searchText)
        {
            var conditions = new List<string>();

            if (IsGuid(searchText))
            {
                conditions.Add($"<condition attribute='{entityWrapper.PrimaryIdAttribute}' operator='eq' value='{searchText}'/>");
            }
            else if (!string.IsNullOrWhiteSpace(searchText))
            {
                if (entityWrapper.Attributes != null)
                {
                    var stringAttrs = entityWrapper.Attributes
                        .Where(a => a.AttributeOf == null &&
                                    a.AttributeType == AttributeTypeCode.String &&
                                    (a.LogicalName.Contains("code", StringComparison.OrdinalIgnoreCase) ||
                                     a.LogicalName.Contains("name", StringComparison.OrdinalIgnoreCase) ||
                                     a.LogicalName.Contains("number", StringComparison.OrdinalIgnoreCase)))
                        .ToList();

                    foreach (var attr in stringAttrs)
                    {
                        conditions.Add($"<condition attribute='{attr.LogicalName}' operator='like' value='%{EscapeXml(searchText)}%'/>");
                    }
                }

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

        private void cboRecord_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoading) return;

            _selectedRecord = cboRecord.SelectedItem as RecordWrapper;
            if (_selectedRecord != null)
            {
                LoadRecordData();
            }
            else
            {
                _originalData = null;
                _attributes.Clear();
                lstAttributes.ItemsSource = null;
                btnSave.IsEnabled = false;
            }
        }

        private void LoadRecordData()
        {
            if (_service == null || _selectedEntity == null || _selectedRecord == null) return;

            try
            {
                _originalData = _service.Retrieve(_selectedEntity.LogicalName, _selectedRecord.Id, new ColumnSet(true));
                BuildAttributeItems();
                btnSave.IsEnabled = false;
                txtStatus.Text = $"Loaded record: {_selectedRecord.RecordName}";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error loading record: {ex.Message}";
            }
        }

        private void BuildAttributeItems()
        {
            _attributes.Clear();
            if (_originalData == null || _selectedEntity.Attributes == null) return;

            var editableAttrs = _selectedEntity.Attributes
                .Where(a => a.AttributeOf == null &&
                           !a.IsPrimaryId &&
                           !HiddenAttributes.Contains(a.LogicalName) &&
                           !DisabledAttributes.Contains(a.LogicalName))
                .ToList();

            var disabledAttrs = _selectedEntity.Attributes
                .Where(a => a.AttributeOf == null &&
                           !a.IsPrimaryId &&
                           !HiddenAttributes.Contains(a.LogicalName) &&
                           DisabledAttributes.Contains(a.LogicalName))
                .ToList();

            foreach (var attr in editableAttrs.Concat(disabledAttrs))
            {
                var displayName = attr.DisplayName?.UserLocalizedLabel?.Label ?? attr.LogicalName;
                var value = GetAttributeValue(_originalData, attr.LogicalName, attr.AttributeType);

                _attributes.Add(new AttributeItem
                {
                    LogicalName = attr.LogicalName,
                    DisplayName = displayName,
                    AttributeType = attr.AttributeType.ToString(),
                    Value = value,
                    IsEnabled = !DisabledAttributes.Contains(attr.LogicalName)
                });
            }

            lstAttributes.ItemsSource = null;
            lstAttributes.ItemsSource = _attributes;
        }

        private string GetAttributeValue(Entity entity, string logicalName, AttributeTypeCode type)
        {
            var value = entity.GetAttributeValue<object>(logicalName);
            if (value == null) return "";

            return type switch
            {
                AttributeTypeCode.Picklist or AttributeTypeCode.Status or AttributeTypeCode.State
                    => value is OptionSetValue ov ? ov.Value.ToString() : value.ToString(),
                AttributeTypeCode.Lookup or AttributeTypeCode.Owner
                    => entity.GetAttributeValue<EntityReference>(logicalName)?.Name ?? "",
                AttributeTypeCode.Money
                    => entity.GetAttributeValue<Money>(logicalName)?.Value.ToString() ?? "",
                AttributeTypeCode.Boolean
                    => entity.GetAttributeValue<bool>(logicalName).ToString(),
                AttributeTypeCode.DateTime
                    => entity.GetAttributeValue<DateTime>(logicalName).ToString("yyyy-MM-dd HH:mm:ss"),
                _ => value.ToString()
            };
        }

        private void txtValue_LostFocus(object sender, RoutedEventArgs e)
        {
            DetectChanges();
        }

        private void DetectChanges()
        {
            if (_originalData == null) return;

            _changes.Clear();
            foreach (var attr in _attributes)
            {
                var originalValue = GetAttributeValue(_originalData, attr.LogicalName,
                    Enum.Parse<AttributeTypeCode>(attr.AttributeType));

                if (attr.Value != originalValue)
                {
                    _changes.Add(new ChangeItem
                    {
                        DisplayName = attr.DisplayName,
                        AttributeName = attr.LogicalName,
                        OldValue = originalValue,
                        NewValue = attr.Value
                    });
                }
            }

            btnSave.IsEnabled = _changes.Count > 0;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_changes.Count == 0) return;

            dgChanges.ItemsSource = null;
            dgChanges.ItemsSource = _changes;
            pnlSaveDialog.Visibility = Visibility.Visible;
        }

        private void btnCancelSave_Click(object sender, RoutedEventArgs e)
        {
            pnlSaveDialog.Visibility = Visibility.Collapsed;
        }

        private void btnConfirmSave_Click(object sender, RoutedEventArgs e)
        {
            if (_service == null || _selectedEntity == null || _selectedRecord == null) return;

            try
            {
                var updateEntity = new Entity(_selectedEntity.LogicalName) { Id = _selectedRecord.Id };

                foreach (var change in _changes)
                {
                    var attrMeta = _selectedEntity.Attributes?.FirstOrDefault(a => a.LogicalName == change.AttributeName);
                    if (attrMeta == null) continue;

                    var newValue = ParseValue(change.NewValue, attrMeta.AttributeType);
                    updateEntity[change.AttributeName] = newValue;
                }

                _service.Update(updateEntity);
                pnlSaveDialog.Visibility = Visibility.Collapsed;
                txtStatus.Text = "Record saved successfully!";

                // Reload the record
                LoadRecordData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving record: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private object ParseValue(string value, AttributeTypeCode type)
        {
            if (string.IsNullOrEmpty(value)) return null;

            return type switch
            {
                AttributeTypeCode.String or AttributeTypeCode.Memo => value,
                AttributeTypeCode.Integer => int.TryParse(value, out var i) ? i : value,
                AttributeTypeCode.BigInt => long.TryParse(value, out var l) ? l : value,
                AttributeTypeCode.Decimal or AttributeTypeCode.Double => decimal.TryParse(value, out var d) ? d : value,
                AttributeTypeCode.Boolean => bool.TryParse(value, out var b) && b,
                AttributeTypeCode.DateTime => DateTime.TryParse(value, out var dt) ? dt : value,
                AttributeTypeCode.Picklist or AttributeTypeCode.Status or AttributeTypeCode.State
                    => int.TryParse(value, out var opt) ? new OptionSetValue(opt) : value,
                AttributeTypeCode.Uniqueidentifier => Guid.TryParse(value, out var g) ? g : value,
                AttributeTypeCode.Money => decimal.TryParse(value, out var m) ? new Money(m) : value,
                _ => value
            };
        }
    }

    public class AttributeItem : System.ComponentModel.INotifyPropertyChanged
    {
        public string LogicalName { get; set; }
        public string DisplayName { get; set; }
        public string AttributeType { get; set; }
        public bool IsEnabled { get; set; }

        private string _value;
        public string Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Value)));
                }
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }

    public class ChangeItem
    {
        public string DisplayName { get; set; }
        public string AttributeName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}
