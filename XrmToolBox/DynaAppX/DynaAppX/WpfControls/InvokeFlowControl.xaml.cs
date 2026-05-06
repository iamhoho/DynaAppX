using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DynaAppX.WpfControls
{
    public partial class InvokeFlowControl : UserControl
    {
        private IOrganizationService _service;
        private List<FlowWrapper> _allFlows = new List<FlowWrapper>();
        private List<FlowWrapper> _flows = new List<FlowWrapper>();
        private List<RecordWrapper> _allRecords = new List<RecordWrapper>();
        private List<RecordWrapper> _records = new List<RecordWrapper>();
        private List<FlowParameterInfo> _paramControls = new List<FlowParameterInfo>();

        public InvokeFlowControl()
        {
            InitializeComponent();
            this.Loaded += InvokeFlowControl_Loaded;
        }

        public void SetService(IOrganizationService service)
        {
            _service = service;
            txtStatus.Text = "Service connected. Ready.";
            LoadFlows();
        }

        private void InvokeFlowControl_Loaded(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "Waiting for CRM connection...";
        }

        private void LoadFlows()
        {
            if (_service == null)
            {
                txtStatus.Text = "Error: Service not initialized";
                return;
            }

            try
            {
                txtStatus.Text = "Loading flows...";

                var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                  <entity name='workflow'>
                    <attribute name='workflowid'/>
                    <attribute name='name'/>
                    <attribute name='uniquename'/>
                    <attribute name='category'/>
                    <attribute name='primaryentity'/>
                    <attribute name='xaml'/>
                    <order attribute='name' descending='false'/>
                    <filter type='and'>
                      <condition attribute='statecode' operator='eq' value='1'/>
                      <condition attribute='category' operator='in'>
                        <value>0</value>
                        <value>3</value>
                      </condition>
                    </filter>
                  </entity>
                </fetch>";

                var result = _service.RetrieveMultiple(new FetchExpression(fetchXml));
                _allFlows.Clear();
                _flows.Clear();

                foreach (var entity in result.Entities)
                {
                    var category = entity.GetAttributeValue<OptionSetValue>("category")?.Value ?? -1;
                    var name = entity.GetAttributeValue<string>("name") ?? "(No name)";
                    var uniqueName = entity.GetAttributeValue<string>("uniquename") ?? "";
                    var primaryEntity = entity.GetAttributeValue<string>("primaryentity") ?? "";
                    var xaml = entity.GetAttributeValue<string>("xaml") ?? "";
                    var id = entity.Id;

                    _allFlows.Add(new FlowWrapper
                    {
                        Id = id,
                        Name = name,
                        UniqueName = uniqueName,
                        Category = category,
                        PrimaryEntity = primaryEntity,
                        Xaml = xaml,
                        DisplayName = $"[{GetCategoryLabel(category)}] {name}"
                    });
                    _flows.Add(_allFlows.Last());
                }

                cboFlow.ItemsSource = null;
                cboFlow.ItemsSource = _flows;
                txtStatus.Text = $"Loaded {_flows.Count} flows";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error loading flows: {ex.Message}";
            }
        }

        private string GetCategoryLabel(int category)
        {
            return category == 0 ? "Workflow" : (category == 3 ? "Action" : category.ToString());
        }

        private void cboFlow_DropDownOpened(object sender, EventArgs e)
        {
            var searchText = cboFlow.Text?.ToLower() ?? "";
            FilterFlows(searchText);
        }

        private void cboFlow_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = cboFlow.Text?.ToLower() ?? "";
            FilterFlows(searchText);
        }

        private void FilterFlows(string searchText)
        {
            _flows.Clear();
            if (string.IsNullOrEmpty(searchText))
            {
                _flows.AddRange(_allFlows);
            }
            else
            {
                _flows.AddRange(_allFlows.Where(f =>
                    f.Name.ToLower().Contains(searchText) ||
                    f.UniqueName.ToLower().Contains(searchText)));
            }
            cboFlow.ItemsSource = null;
            cboFlow.ItemsSource = _flows;
        }

        private void cboFlow_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _paramControls.Clear();
            if (cboFlow.SelectedItem is FlowWrapper flow)
            {
                // Show/hide record selector
                var hasEntity = !string.IsNullOrEmpty(flow.PrimaryEntity) && flow.PrimaryEntity != "none";
                lblRecord.Visibility = hasEntity ? Visibility.Visible : Visibility.Collapsed;
                cboRecord.Visibility = hasEntity ? Visibility.Visible : Visibility.Collapsed;

                // Show/hide parameter section
                pnlParameters.Visibility = (flow.Category == 3) ? Visibility.Visible : Visibility.Collapsed;

                // Show/hide toggle for raw/field mode only for actions
                tglRawMode.Visibility = (flow.Category == 3) ? Visibility.Visible : Visibility.Collapsed;

                // Parse and build parameter controls
                if (flow.Category == 3)
                {
                    ParseFlowParameters(flow.Xaml);
                    UpdateParamDisplay();
                }

                // Enable invoke if we have selected flow
                btnInvoke.IsEnabled = true;

                // Load records if primary entity exists
                if (hasEntity)
                {
                    LoadRecords(flow.PrimaryEntity);
                }
                else
                {
                    _allRecords.Clear();
                    _records.Clear();
                    cboRecord.ItemsSource = null;
                }
            }
            else
            {
                lblRecord.Visibility = Visibility.Collapsed;
                cboRecord.Visibility = Visibility.Collapsed;
                pnlParameters.Visibility = Visibility.Collapsed;
                tglRawMode.Visibility = Visibility.Collapsed;
                btnInvoke.IsEnabled = false;
            }
        }

        private void ParseFlowParameters(string xaml)
        {
            _paramControls.Clear();
            if (string.IsNullOrEmpty(xaml)) return;

            try
            {
                var doc = XDocument.Parse(xaml);
                var nsManager = new XmlNamespaceManager(new NameTable());
                nsManager.AddNamespace("mxsw", "http://schemas.microsoft.com/xrm/2011/workflow/strategies");
                nsManager.AddNamespace("x", "http://schemas.microsoft.com/winfx/2006/xaml");

                // Find Members elements within Activity that have.arguments.children
                var members = doc.Descendants()
                    .Where(x => x.Name.LocalName == "Members")
                    .ToList();

                if (members.Any())
                {
                    foreach (var member in members.Elements())
                    {
                        var name = member.Attribute("Name")?.Value;
                        var type = member.Attribute("Type")?.Value;

                        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(type)) continue;
                        if (name == "InputEntities" || name == "CreatedEntities" || name == "Target") continue;
                        if (!type.Contains("InArgument")) continue;

                        // Check required attribute - could be in ArgumentRequiredAttribute or similar
                        var requiredAttr = member.Elements()
                            .FirstOrDefault(x => x.Name.LocalName == "ArgumentRequired" &&
                                                 x.Name.NamespaceName.Contains("microsoft"));
                        var required = requiredAttr?.Attribute("Value")?.Value == "true";

                        _paramControls.Add(new FlowParameterInfo
                        {
                            Name = name,
                            Type = type,
                            Required = required
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error parsing xaml: {ex.Message}";
            }
        }

        private void UpdateParamDisplay()
        {
            ctrlParams.ItemsSource = null;
            ctrlParams.ItemsSource = _paramControls;
        }

        private void LoadRecords(string entityName)
        {
            if (_service == null || string.IsNullOrEmpty(entityName)) return;

            try
            {
                var entityMeta = GetEntityMetadata(entityName);
                var primaryNameAttr = entityMeta?.PrimaryNameAttribute ?? "name";
                var primaryIdAttr = entityMeta?.PrimaryIdAttribute ?? entityName + "id";

                // Escape dynamic values in FetchXML
                var safeEntityName = System.Security.SecurityElement.Escape(entityName) ?? entityName;
                var safePrimaryIdAttr = System.Security.SecurityElement.Escape(primaryIdAttr) ?? primaryIdAttr;
                var safePrimaryNameAttr = System.Security.SecurityElement.Escape(primaryNameAttr) ?? primaryNameAttr;

                var fetchXml = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' top='30'>
                  <entity name='{safeEntityName}'>
                    <attribute name='{safePrimaryIdAttr}'/>
                    <attribute name='{safePrimaryNameAttr}'/>
                    <order attribute='{safePrimaryNameAttr}' descending='false'/>
                  </entity>
                </fetch>";

                var result = _service.RetrieveMultiple(new FetchExpression(fetchXml));
                _allRecords.Clear();
                _records.Clear();

                foreach (var record in result.Entities)
                {
                    var recordName = record.GetAttributeValue<string>(primaryNameAttr) ?? "(No name)";
                    var wrapper = new RecordWrapper
                    {
                        Id = record.Id,
                        RecordName = recordName,
                        EntitySetName = entityMeta?.EntitySetName ?? entityName + "s"
                    };
                    _allRecords.Add(wrapper);
                    _records.Add(wrapper);
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
            if (string.IsNullOrEmpty(searchText))
            {
                _records.Clear();
                _records.AddRange(_allRecords);
            }
            else
            {
                _records.Clear();
                _records.AddRange(_allRecords.Where(r =>
                    r.RecordName.ToLower().Contains(searchText.ToLower())));
            }
            cboRecord.ItemsSource = null;
            cboRecord.ItemsSource = _records;
        }

        private void cboRecord_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Record selection change - can add validation here if needed
        }

        private void tglRawMode_Click(object sender, RoutedEventArgs e)
        {
            if (tglRawMode.IsChecked == true)
            {
                tglRawMode.Content = "Raw Model";
                pnlRawModel.Visibility = Visibility.Visible;
                pnlFieldModel.Visibility = Visibility.Collapsed;
            }
            else
            {
                tglRawMode.Content = "Field Model";
                pnlRawModel.Visibility = Visibility.Collapsed;
                pnlFieldModel.Visibility = Visibility.Visible;
            }
        }

        private void btnInvoke_Click(object sender, RoutedEventArgs e)
        {
            if (cboFlow.SelectedItem is FlowWrapper flow)
            {
                if (flow.Category == 0)
                {
                    InvokeWorkflow(flow);
                }
                else if (flow.Category == 3)
                {
                    InvokeAction(flow);
                }
            }
        }

        private void InvokeWorkflow(FlowWrapper flow)
        {
            try
            {
                var recordId = (cboRecord.SelectedItem as RecordWrapper)?.Id;
                if (recordId == null)
                {
                    txtStatus.Text = "Please select a target record";
                    return;
                }

                // Use ExecuteWorkflow message via OrganizationRequest
                var request = new ExecuteWorkflowRequest
                {
                    WorkflowId = flow.Id,
                    EntityId = recordId.Value
                };

                var response = _service.Execute(request);

                AddHistoryEntry(new InvokeHistoryEntry
                {
                    Name = flow.Name,
                    Url = $"workflows({flow.Id})/Microsoft.Dynamics.CRM.ExecuteWorkflow",
                    RequestBody = $"{{\"EntityId\":\"{recordId.Value}\"}}",
                    StatusCode = 200,
                    Response = "Workflow executed successfully.",
                    InvokeDate = DateTime.Now
                });

                txtStatus.Text = "Workflow invoked successfully";
            }
            catch (Exception ex)
            {
                AddHistoryEntry(new InvokeHistoryEntry
                {
                    Name = flow.Name,
                    Url = $"workflows({flow.Id})/Microsoft.Dynamics.CRM.ExecuteWorkflow",
                    RequestBody = "{}",
                    StatusCode = 500,
                    Response = ex.Message,
                    InvokeDate = DateTime.Now
                });
                txtStatus.Text = $"Error invoking workflow: {ex.Message}";
            }
        }

        private void InvokeAction(FlowWrapper flow)
        {
            string path = "";
            string requestBody = "";

            try
            {
                if (!string.IsNullOrEmpty(flow.PrimaryEntity) && flow.PrimaryEntity != "none")
                {
                    var record = cboRecord.SelectedItem as RecordWrapper;
                    if (record == null)
                    {
                        txtStatus.Text = "Please select a target record";
                        return;
                    }
                    path = $"{record.EntitySetName}({record.Id})/Microsoft.Dynamics.CRM.{flow.UniqueName}";
                }
                else
                {
                    path = flow.UniqueName;
                }

                if (tglRawMode.IsChecked == true)
                {
                    requestBody = txtRawParams.Text;
                }
                else
                {
                    requestBody = BuildParameterJson();
                }

                // Execute the action using OrganizationRequest
                var orgRequest = new OrganizationRequest(flow.UniqueName);

                // Parse requestBody JSON and add parameters to orgRequest
                if (!string.IsNullOrWhiteSpace(requestBody))
                {
                    try
                    {
                        using var jsonDoc = System.Text.Json.JsonDocument.Parse(requestBody);
                        foreach (var kvp in jsonDoc.RootElement.EnumerateObject())
                        {
                            var value = kvp.Value;
                            if (value.ValueKind == System.Text.Json.JsonValueKind.Object)
                            {
                                if (value.TryGetProperty("@odata.type", out var typeEl) &&
                                    typeEl.GetString() == "Microsoft.Dynamics.CRM.EntityReference")
                                {
                                    var logicalName = value.GetProperty("logicalname").GetString();
                                    var idStr = value.GetProperty("id").GetString();
                                    if (Guid.TryParse(idStr, out var guid))
                                    {
                                        orgRequest.Parameters[kvp.Key] = new Microsoft.Xrm.Sdk.EntityReference(logicalName, guid);
                                    }
                                }
                                else if (value.TryGetProperty("logicalname", out var lnEl))
                                {
                                    // Might be EntityReference without @odata.type
                                    var logicalName = lnEl.GetString();
                                    if (value.TryGetProperty("id", out var idEl) &&
                                        Guid.TryParse(idEl.GetString(), out var guid))
                                    {
                                        orgRequest.Parameters[kvp.Key] = new Microsoft.Xrm.Sdk.EntityReference(logicalName, guid);
                                    }
                                }
                                else
                                {
                                    orgRequest.Parameters[kvp.Key] = value.ToString();
                                }
                            }
                            else if (value.ValueKind == System.Text.Json.JsonValueKind.String)
                            {
                                orgRequest.Parameters[kvp.Key] = value.GetString();
                            }
                            else if (value.ValueKind == System.Text.Json.JsonValueKind.Number)
                            {
                                if (value.TryGetInt32(out var intVal))
                                    orgRequest.Parameters[kvp.Key] = intVal;
                                else
                                    orgRequest.Parameters[kvp.Key] = value.GetDouble();
                            }
                            else if (value.ValueKind == System.Text.Json.JsonValueKind.True || value.ValueKind == System.Text.Json.JsonValueKind.False)
                            {
                                orgRequest.Parameters[kvp.Key] = value.GetBoolean();
                            }
                            else if (value.ValueKind == System.Text.Json.JsonValueKind.Null)
                            {
                                orgRequest.Parameters[kvp.Key] = null;
                            }
                        }
                    }
                    catch (System.Text.Json.JsonException ex)
                    {
                        txtStatus.Text = $"Invalid JSON parameters: {ex.Message}";
                        return;
                    }
                }

                var response = _service.Execute(orgRequest);

                // Format response results for display
                string responseText = "Action executed successfully.";
                if (response?.Results != null && response.Results.Count > 0)
                {
                    var resultParts = response.Results
                        .Take(5)
                        .Select(p => $"{p.Key}={FormatResponseValue(p.Value)}");
                    responseText = string.Join("; ", resultParts);
                    if (response.Results.Count > 5)
                        responseText += $" (+{response.Results.Count - 5} more)";
                }

                AddHistoryEntry(new InvokeHistoryEntry
                {
                    Name = flow.Name,
                    Url = path,
                    RequestBody = requestBody.Length > 500 ? requestBody.Substring(0, 500) + "..." : requestBody,
                    StatusCode = 200,
                    Response = responseText,
                    InvokeDate = DateTime.Now
                });

                txtStatus.Text = "Action invoked successfully";
            }
            catch (Exception ex)
            {
                AddHistoryEntry(new InvokeHistoryEntry
                {
                    Name = flow.Name,
                    Url = path,
                    RequestBody = requestBody.Length > 500 ? requestBody.Substring(0, 500) + "..." : requestBody,
                    StatusCode = 500,
                    Response = ex.Message,
                    InvokeDate = DateTime.Now
                });
                txtStatus.Text = $"Error invoking action: {ex.Message}";
            }
        }

        private string FormatResponseValue(object value)
        {
            if (value == null) return "null";
            if (value is EntityReference er) return $"EntityReference({er.LogicalName},{er.Id})";
            if (value is Entity e) return $"Entity({e.LogicalName},{e.Id})";
            if (value is Guid g) return g.ToString();
            if (value is string s && s.Length > 50) return s.Substring(0, 50) + "...";
            return value.ToString();
        }

        private string BuildParameterJson()
        {
            var dict = new Dictionary<string, object>();
            foreach (var param in _paramControls)
            {
                if (param.Value != null)
                {
                    dict[param.Name] = param.Value;
                }
            }
            return System.Text.Json.JsonSerializer.Serialize(dict);
        }

        private void AddHistoryEntry(InvokeHistoryEntry entry)
        {
            dgHistory.Items.Insert(0, entry);
        }
    }

    public class FlowWrapper
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string UniqueName { get; set; }
        public int Category { get; set; }
        public string PrimaryEntity { get; set; }
        public string Xaml { get; set; }
        public string DisplayName { get; set; }
    }

    public class RecordWrapper
    {
        public Guid Id { get; set; }
        public string RecordName { get; set; }
        public string EntitySetName { get; set; }
        public Entity Entity { get; set; }
    }

    public class FlowParameterInfo : INotifyPropertyChanged
    {
        private object _value;

        public string Name { get; set; }
        public string Type { get; set; }
        public bool Required { get; set; }
        public object Value
        {
            get => _value;
            set { _value = value; OnPropertyChanged(); }
        }

        public bool IsString => Type.Contains("x:String");
        public bool IsBoolean => Type.Contains("x:Boolean");
        public bool IsNumber => Type.Contains("x:Double") || Type.Contains("x:Decimal") || Type.Contains("x:Int32") ||
                                 Type.Contains("mxs:Money") || Type.Contains("mxs:OptionSetValue");
        public bool IsDateTime => Type.Contains("s:DateTime");
        public bool IsEntityReference => Type.Contains("mxs:EntityReference");
        public bool IsEntity => Type.Contains("mxs:Entity");
        public bool IsEntityCollection => Type.Contains("mxs:EntityCollection");
        public bool IsUnsupported => !IsString && !IsBoolean && !IsNumber && !IsDateTime && !IsEntityReference && !IsEntity && !IsEntityCollection;
        public string UnsupportedInfo => IsUnsupported ? $"Unsupported: {Type}" : "";
        public List<object> EntityRefOptions => new List<object>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class InvokeHistoryEntry
    {
        public DateTime InvokeDate { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string RequestBody { get; set; }
        public string Response { get; set; }
        public int StatusCode { get; set; }
    }
}