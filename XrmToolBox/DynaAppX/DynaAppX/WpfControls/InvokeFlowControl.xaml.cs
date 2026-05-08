using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DynaAppX.Services;
using System.Xml;
using System.Web;
using System.IO;
using System.Text;

namespace DynaAppX.WpfControls
{
    public partial class InvokeFlowControl : UserControl
    {
        private IOrganizationService _service;
        private List<Services.FlowWrapper> _flows = new List<Services.FlowWrapper>();
        private Services.FlowWrapper _selectedFlow;
        private List<RecordWrapper> _records = new List<RecordWrapper>();
        private CancellationTokenSource _searchCts;
        private ObservableCollection<InvokeHistoryItem> _history = new ObservableCollection<InvokeHistoryItem>();
        private List<ParameterItem> _parameters = new List<ParameterItem>();

        public InvokeFlowControl()
        {
            InitializeComponent();
            this.Loaded += InvokeFlowControl_Loaded;
        }

        public void SetService(IOrganizationService service)
        {
            _service = service;
        }

        private void InvokeFlowControl_Loaded(object sender, RoutedEventArgs e)
        {
            lstParameters.ItemsSource = _parameters;
            dgHistory.ItemsSource = _history;
        }

        private void cboFlow_DropDownOpened(object sender, EventArgs e)
        {
            LoadFlows();
        }

        private void cboFlow_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterFlows(cboFlow.Text ?? "");
        }

        private void FilterFlows(string searchText)
        {
            var cachedFlows = SharedMetadataCache.Instance.GetFlows(_service);
            if (cachedFlows.Count == 0)
            {
                LoadFlows();
                cachedFlows = SharedMetadataCache.Instance.GetFlows(_service);
            }

            if (string.IsNullOrWhiteSpace(searchText))
            {
                _flows = cachedFlows;
            }
            else
            {
                var searchLower = searchText.ToLowerInvariant();
                _flows = cachedFlows.Where(f =>
                    (f.Name != null && f.Name.ToLowerInvariant().Contains(searchLower)) ||
                    (f.UniqueName != null && f.UniqueName.ToLowerInvariant().Contains(searchLower))
                ).ToList();
            }

            cboFlow.ItemsSource = null;
            cboFlow.ItemsSource = _flows;
        }

        private void LoadFlows()
        {
            if (_service == null) return;

            LoadingOverlay.Show("Loading flows...");

            try
            {
                // Check cache first
                var cachedFlows = SharedMetadataCache.Instance.GetFlows(_service);
                if (cachedFlows.Count > 0)
                {
                    _flows = cachedFlows;
                    cboFlow.ItemsSource = null;
                    cboFlow.ItemsSource = _flows;
                    return;
                }

                var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                    <entity name='workflow'>
                        <order attribute='name' descending='false'/>
                        <filter type='and'>
                            <condition attribute='statecode' operator='eq' value='1'/>
                            <filter type='or'>
                                <condition attribute='category' operator='eq' value='3'/>
                                <filter type='and'>
                                    <condition attribute='category' operator='eq' value='0'/>
                                    <condition attribute='ondemand' operator='eq' value='1'/>
                                </filter>
                            </filter>
                            <condition attribute='type' operator='eq' value='1'/>
                        </filter>
                    </entity>
                </fetch>";

                var results = _service.RetrieveMultiple(new FetchExpression(fetchXml));
                _flows = results.Entities.Select(e => new Services.FlowWrapper
                {
                    Id = e.Id,
                    Name = e.GetAttributeValue<string>("name"),
                    UniqueName = e.GetAttributeValue<string>("uniquename"),
                    Category = e.GetAttributeValue<OptionSetValue>("category")?.Value ?? 0,
                    CategoryName = (e.GetAttributeValue<OptionSetValue>("category")?.Value ?? 0) == 0 ? "Workflow" : "Action",
                    PrimaryEntity = e.GetAttributeValue<string>("primaryentity"),
                    Xaml = e.GetAttributeValue<string>("xaml")
                }).ToList();

                // Save to cache
                SharedMetadataCache.Instance.SetFlows(_service, _flows);

                cboFlow.ItemsSource = null;
                cboFlow.ItemsSource = _flows;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading flows: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoadingOverlay.Hide();
            }
        }

        private void cboFlow_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboFlow.SelectedItem is Services.FlowWrapper flow)
            {
                _selectedFlow = flow;
                txtFlowCategory.Text = flow.CategoryName;
                lblRecord.Visibility = (flow.Category == 0 || !string.IsNullOrEmpty(flow.PrimaryEntity))
                    ? Visibility.Visible : Visibility.Collapsed;
                cboRecord.IsEnabled = (flow.Category == 0 || !string.IsNullOrEmpty(flow.PrimaryEntity));

                ParseParameters(flow);
                btnInvoke.IsEnabled = flow.Category == 3;
            }
        }

        private void ParseParameters(Services.FlowWrapper flow)
        {
            _parameters.Clear();

            if (string.IsNullOrEmpty(flow.Xaml))
            {
                txtNoParameters.Visibility = Visibility.Visible;
                lstParameters.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(flow.Xaml);

                var namespaceManager = new XmlNamespaceManager(doc.NameTable);
                namespaceManager.AddNamespace("x", "http://schemas.microsoft.com/winfx/2006/xaml");
                namespaceManager.AddNamespace("mxsw", "http://schemas.microsoft.com/dynamics/2008/01/shared");

                var members = doc.SelectNodes("//x:Members", namespaceManager);
                if (members != null && members.Count > 0)
                {
                    foreach (XmlNode member in members[0].ChildNodes)
                    {
                        var name = member.Attributes["Name"]?.Value;
                        var type = member.Attributes["Type"]?.Value;

                        if (string.IsNullOrEmpty(name) || type == null) continue;
                        if (name == "InputEntities" || name == "CreatedEntities" || name == "Target") continue;
                        if (!type.Contains("InArgument")) continue;

                        var required = false;
                        var reqAttr = member.SelectSingleNode(".//mxsw:ArgumentRequiredAttribute", namespaceManager);
                        if (reqAttr != null && reqAttr.Attributes["Value"] != null)
                        {
                            required = reqAttr.Attributes["Value"].Value.Equals("true", StringComparison.OrdinalIgnoreCase);
                        }

                        _parameters.Add(new ParameterItem
                        {
                            Name = name,
                            Type = type.Replace("InArgument(", "").Replace(")", ""),
                            Required = required,
                            IsEnabled = true
                        });
                    }
                }

                txtNoParameters.Visibility = _parameters.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                lstParameters.Visibility = _parameters.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                lstParameters.ItemsSource = null;
                lstParameters.ItemsSource = _parameters;
            }
            catch (Exception ex)
            {
                txtNoParameters.Text = $"Error parsing parameters: {ex.Message}";
                txtNoParameters.Visibility = Visibility.Visible;
                lstParameters.Visibility = Visibility.Collapsed;
            }
        }

        private void cboRecord_DropDownOpened(object sender, EventArgs e)
        {
            if (_selectedFlow == null || string.IsNullOrEmpty(_selectedFlow.PrimaryEntity) || _selectedFlow.Category == 3)
                return;

            _ = SearchRecordsAsync("");
        }

        private async void cboRecord_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (_selectedFlow == null || string.IsNullOrEmpty(_selectedFlow.PrimaryEntity) || _selectedFlow.Category == 3)
                return;

            if (e.Key == System.Windows.Input.Key.Enter)
            {
                await SearchRecordsAsync(cboRecord.Text ?? "");
            }
        }

        private async void cboRecord_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_selectedFlow == null || string.IsNullOrEmpty(_selectedFlow.PrimaryEntity) || _selectedFlow.Category == 3)
                return;

            await SearchRecordsAsync(cboRecord.Text ?? "");
        }

        private async Task SearchRecordsAsync(string searchText)
        {
            if (_service == null || _selectedFlow == null || string.IsNullOrEmpty(_selectedFlow.PrimaryEntity))
                return;

            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;

            try
            {
                LoadingOverlay.Show("Searching records...");
                var entityWrapper = SharedMetadataCache.Instance.GetAllEntities(_service)
                    .FirstOrDefault(e => e.LogicalName == _selectedFlow.PrimaryEntity);

                if (entityWrapper == null)
                {
                    MessageBox.Show($"Entity '{_selectedFlow.PrimaryEntity}' not found in cache. Please load entities first.",
                        "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await SharedMetadataCache.Instance.GetEntityAttributesAsync(_service, entityWrapper.LogicalName);
                if (token.IsCancellationRequested) return;

                var fetchXml = CrmHelper.BuildRecordSearchFetchXml(entityWrapper, searchText);
                var result = await Task.Run(() => _service.RetrieveMultiple(new FetchExpression(fetchXml)), token);
                if (token.IsCancellationRequested) return;

                _records.Clear();
                foreach (var record in result.Entities)
                {
                    var name = string.IsNullOrEmpty(entityWrapper.PrimaryNameAttribute)
                        ? null
                        : record.GetAttributeValue<object>(entityWrapper.PrimaryNameAttribute);
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
                MessageBox.Show($"Error searching records: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoadingOverlay.Hide();
            }
        }

        private void cboRecord_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnInvoke.IsEnabled = cboRecord.SelectedItem != null || _selectedFlow?.Category == 3;
        }

        private void chkRawMode_Changed(object sender, RoutedEventArgs e)
        {
            var isRawMode = chkRawMode.IsChecked == true;
            lstParameters.Visibility = isRawMode ? Visibility.Collapsed : Visibility.Visible;
            txtNoParameters.Visibility = Visibility.Collapsed;
            txtRawModeLabel.Visibility = isRawMode ? Visibility.Visible : Visibility.Collapsed;
            txtRawJson.Visibility = isRawMode ? Visibility.Visible : Visibility.Collapsed;
        }

        private void btnInvoke_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFlow == null)
            {
                MessageBox.Show("Please select a flow first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            LoadingOverlay.Show("Invoking flow/action...");

            try
            {
                string requestBody;
                string url;

                if (_selectedFlow.Category == 0)
                {
                    if (!(cboRecord.SelectedItem is RecordWrapper record))
                    {
                        MessageBox.Show("Please select a record.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    url = $"workflows({_selectedFlow.Id})/Microsoft.Dynamics.CRM.ExecuteWorkflow";
                    requestBody = $"{{\"EntityId\":\"{record.Id}\"}}";
                }
                else
                {
                    if (chkRawMode.IsChecked == true)
                    {
                        requestBody = txtRawJson.Text;
                    }
                    else
                    {
                        var paramObj = new Dictionary<string, object>();
                        foreach (var param in _parameters)
                        {
                            if (!string.IsNullOrEmpty(param.Value))
                            {
                                paramObj[param.Name] = ParseParameterValue(param.Value, param.Type);
                            }
                        }
                        requestBody = Newtonsoft.Json.JsonConvert.SerializeObject(paramObj);
                    }

                    if (!string.IsNullOrEmpty(_selectedFlow.PrimaryEntity) && _selectedFlow.PrimaryEntity != "none")
                    {
                        if (!(cboRecord.SelectedItem is RecordWrapper record))
                        {
                            MessageBox.Show("Please select a record.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        url = $"{_selectedFlow.PrimaryEntity}({record.Id})/Microsoft.Dynamics.CRM.{_selectedFlow.UniqueName}";
                    }
                    else
                    {
                        url = $"Microsoft.Dynamics.CRM.{_selectedFlow.UniqueName}";
                    }
                }

                var response = ExecuteAction(url, requestBody);

                _history.Insert(0, new InvokeHistoryItem
                {
                    Name = _selectedFlow.Name,
                    Url = url,
                    RequestBody = requestBody,
                    Response = response,
                    StatusCode = 200,
                    InvokeDate = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error invoking flow: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoadingOverlay.Hide();
            }
        }

        private object ParseParameterValue(string value, string type)
        {
            if (string.IsNullOrEmpty(value)) return null;

            if (type == "x:String")
            {
                return value;
            }
            if (type == "x:Int32" || type == "x:Double" || type == "x:Decimal")
            {
                if (decimal.TryParse(value, out var num))
                    return num;
            }
            else if (type == "x:Boolean")
            {
                if (bool.TryParse(value, out var b))
                    return b;
            }
            else if (type == "x:DateTime")
            {
                if (DateTime.TryParse(value, out var dt))
                    return dt;
            }
            return null;
        }

        private string ExecuteAction(string path, string body)
        {
            if (_service == null) return "{\"error\": \"Service not initialized\"}";

            try
            {
                object response = null;

                if (_selectedFlow.Category == 0)
                {
                    // Workflow - use ExecuteWorkflowRequest
                    if (cboRecord.SelectedItem is RecordWrapper record)
                    {
                        var request = new Microsoft.Crm.Sdk.Messages.ExecuteWorkflowRequest
                        {
                            WorkflowId = _selectedFlow.Id,
                            EntityId = record.Id
                        };
                        response = _service.Execute(request);
                    }
                }
                else
                {
                    // Action - use OrganizationRequest
                    var request = new OrganizationRequest(_selectedFlow.UniqueName);

                    if (!string.IsNullOrEmpty(_selectedFlow.PrimaryEntity) && _selectedFlow.PrimaryEntity != "none")
                    {
                        if (cboRecord.SelectedItem is RecordWrapper record)
                        {
                            request.Parameters["Target"] = new EntityReference(_selectedFlow.PrimaryEntity, record.Id);
                        }
                    }

                    if (!string.IsNullOrEmpty(body))
                    {
                        var paramDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(body);
                        foreach (var kvp in paramDict)
                        {
                            if (kvp.Value != null)
                            {
                                request.Parameters[kvp.Key] = kvp.Value;
                            }
                        }
                    }

                    response = _service.Execute(request);
                }

                return $"{{\"success\": true, \"result\": {(response != null ? Newtonsoft.Json.JsonConvert.SerializeObject(response) : "null")}}}";
            }
            catch (Exception ex)
            {
                return $"{{\"error\": {Newtonsoft.Json.JsonConvert.ToString(ex.Message)}}}";
            }
        }
    }

    public class ParameterItem : System.ComponentModel.INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool Required { get; set; }
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

    public class InvokeHistoryItem
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string RequestBody { get; set; }
        public string Response { get; set; }
        public int StatusCode { get; set; }
        public DateTime InvokeDate { get; set; }
    }
}
