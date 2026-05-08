# Loading Overlay System Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add full-screen loading overlay to prevent duplicate async operations during slow network calls in XrmToolBox plugin controls.

**Architecture:** Create a reusable LoadingOverlay UserControl with Show/Hide API. Integrate into AccessCheckControl, InvokeFlowControl, and GodPageControl. Fix entity selection reset in AccessCheckControl.

**Tech Stack:** WPF (.NET 4.8), C#, XAML

---

## File Structure

| File | Action |
|------|--------|
| `WpfControls/LoadingOverlay.xaml` | Create |
| `WpfControls/LoadingOverlay.xaml.cs` | Create |
| `WpfControls/AccessCheckControl.xaml` | Modify - add LoadingOverlay |
| `WpfControls/AccessCheckControl.xaml.cs` | Modify - add Show/Hide, fix entity selection |
| `WpfControls/InvokeFlowControl.xaml` | Modify - add LoadingOverlay |
| `WpfControls/InvokeFlowControl.xaml.cs` | Modify - add Show/Hide |
| `WpfControls/GodPageControl.xaml` | Modify - add LoadingOverlay |
| `WpfControls/GodPageControl.xaml.cs` | Modify - add Show/Hide |
| `DynaAppX.csproj` | Modify - add LoadingOverlay.xaml |

---

## Tasks

### Task 1: Create LoadingOverlay component

**Files:**
- Create: `XrmToolBox/DynaAppX/DynaAppX/WpfControls/LoadingOverlay.xaml`
- Create: `XrmToolBox/DynaAppX/DynaAppX/WpfControls/LoadingOverlay.xaml.cs`
- Test: Compile check

- [ ] **Step 1: Create LoadingOverlay.xaml**

```xaml
<UserControl x:Class="DynaAppX.WpfControls.LoadingOverlay"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid x:Name="RootGrid" Visibility="Collapsed" Background="#80000000">
        <Border Background="White" CornerRadius="8" Padding="30,20"
                HorizontalAlignment="Center" VerticalAlignment="Center"
                MinWidth="200">
            <StackPanel>
                <Ellipse x:Name="Spinner" Width="40" Height="40"
                         Stroke="#007ACC" StrokeThickness="4"
                         StrokeDashArray="2,2" RenderTransformOrigin="0.5,0.5">
                    <Ellipse.RenderTransform>
                        <RotateTransform x:Name="SpinnerRotation" Angle="0"/>
                    </Ellipse.RenderTransform>
                    <Ellipse.Triggers>
                        <EventTrigger RoutedEvent="Loaded">
                            <BeginStoryboard>
                                <Storyboard RepeatBehavior="Forever">
                                    <DoubleAnimation
                                        Storyboard.TargetName="SpinnerRotation"
                                        Storyboard.TargetProperty="Angle"
                                        From="0" To="360" Duration="0:0:1"/>
                                </Storyboard>
                            </BeginStoryboard>
                        </EventTrigger>
                    </Ellipse.Triggers>
                </Ellipse>
                <TextBlock x:Name="txtMessage" Text="Loading..."
                           HorizontalAlignment="Center" Margin="0,10,0,0"
                           FontSize="14" Foreground="#333333"/>
            </StackPanel>
        </Border>
    </Grid>
</UserControl>
```

- [ ] **Step 2: Create LoadingOverlay.xaml.cs**

```csharp
using System;
using System.Windows;
using System.Windows.Controls;

namespace DynaAppX.WpfControls
{
    public partial class LoadingOverlay : UserControl
    {
        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(LoadingOverlay),
                new PropertyMetadata(false, OnIsLoadingChanged));

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(nameof(Message), typeof(string), typeof(LoadingOverlay),
                new PropertyMetadata("Loading..."));

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        private static void OnIsLoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var overlay = (LoadingOverlay)d;
            overlay.RootGrid.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public LoadingOverlay()
        {
            InitializeComponent();
        }

        public void Show(string message = "Loading...")
        {
            Message = message;
            IsLoading = true;
        }

        public void Hide()
        {
            IsLoading = false;
        }
    }
}
```

- [ ] **Step 3: Add to DynaAppX.csproj**

Read current csproj and add:
```xml
<Page Include="WpfControls\LoadingOverlay.xaml">
  <Generator>MSBuild:Compile</Generator>
  <SubType>Designer</SubType>
</Page>
```

- [ ] **Step 4: Verify compiles**

Run: `"C:/Program Files/Microsoft Visual Studio/2022/Community/MSBuild/Current/Bin/MSBuild.exe" "XrmToolBox/DynaAppX/DynaAppX/DynaAppX.csproj" -p:Configuration=Debug -p:Platform="AnyCPU"`
Expected: 0 errors

- [ ] **Step 5: Commit**

```bash
git add XrmToolBox/DynaAppX/DynaAppX/WpfControls/LoadingOverlay.xaml XrmToolBox/DynaAppX/DynaAppX/WpfControls/LoadingOverlay.xaml.cs XrmToolBox/DynaAppX/DynaAppX/DynaAppX.csproj
git commit -m "feat: add LoadingOverlay component with spinning animation"
```

---

### Task 2: Integrate LoadingOverlay into AccessCheckControl

**Files:**
- Modify: `XrmToolBox/DynaAppX/DynaAppX/WpfControls/AccessCheckControl.xaml`
- Modify: `XrmToolBox/DynaAppX/DynaAppX/WpfControls/AccessCheckControl.xaml.cs`
- Test: Compile + code review

- [ ] **Step 1: Add LoadingOverlay to AccessCheckControl.xaml**

Read current XAML. Find the root Grid element and change it to:
```xaml
<Grid Background="White">
    <Grid.RowDefinitions>...</Grid.RowDefinitions>
    <!-- existing content -->

    <!-- Add as last child, covers entire grid -->
    <LoadingOverlay x:Name="LoadingOverlay" Grid.RowSpan="99"/>
</Grid>
```

- [ ] **Step 2: Add Show/Hide to btnLoadEntities_Click**

Read current code. Find:
```csharp
private async void btnLoadEntities_Click(object sender, RoutedEventArgs e)
```

Wrap the entire method body with try/finally:
```csharp
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
```

- [ ] **Step 3: Wrap LoadUserRoles and LoadUserTeams**

Find both methods and add LoadingOverlay.Show/Hide with "Loading user info..." message. These are sync methods (no async), so wrap directly:

```csharp
private void LoadUserRoles(UserWrapper user)
{
    if (_service == null) return;
    LoadingOverlay.Show("Loading user info...");

    try
    {
        // ... existing code
    }
    catch (Exception ex)
    {
        txtStatus.Text = $"Error loading roles: {ex.Message}";
    }
    finally
    {
        LoadingOverlay.Hide();
    }
}

private void LoadUserTeams(UserWrapper user)
{
    if (_service == null) return;
    // Note: Don't stack Show/Hide here - call from cboUser_SelectionChanged once
}
```

- [ ] **Step 4: Update cboUser_SelectionChanged to show one overlay**

Replace the existing method body with:
```csharp
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
```

- [ ] **Step 5: Update SearchRecordsAsync with Show/Hide**

Find `SearchRecordsAsync`. Wrap method body with Show/Hide inside the try block:
```csharp
try
{
    LoadingOverlay.Show("Searching records...");
    // ... existing code
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
```

- [ ] **Step 6: Fix cboEntity_SelectionChanged to clear/reset cboRecord**

Replace existing method:
```csharp
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
```

- [ ] **Step 7: Initialize cboRecord.IsEnabled = false**

In the constructor after InitializeComponent(), add:
```csharp
cboRecord.IsEnabled = false;
```

- [ ] **Step 8: Verify compiles**

Run: `"C:/Program Files/Microsoft Visual Studio/2022/Community/MSBuild/Current/Bin/MSBuild.exe" "XrmToolBox/DynaAppX/DynaAppX/DynaAppX.csproj" -p:Configuration=Debug -p:Platform="AnyCPU"`
Expected: 0 errors

- [ ] **Step 9: Commit**

```bash
git add XrmToolBox/DynaAppX/DynaAppX/WpfControls/AccessCheckControl.xaml XrmToolBox/DynaAppX/DynaAppX/WpfControls/AccessCheckControl.xaml.cs
git commit -m "feat(AccessCheckControl): add LoadingOverlay and fix entity selection reset"
```

---

### Task 3: Integrate LoadingOverlay into InvokeFlowControl

**Files:**
- Modify: `XrmToolBox/DynaAppX/DynaAppX/WpfControls/InvokeFlowControl.xaml`
- Modify: `XrmToolBox/DynaAppX/DynaAppX/WpfControls/InvokeFlowControl.xaml.cs`
- Test: Compile + code review

- [ ] **Step 1: Add LoadingOverlay to InvokeFlowControl.xaml**

Read current XAML. Find root Grid, add LoadingOverlay as last child:
```xaml
<Grid>
    <Grid.RowDefinitions>...</Grid.RowDefinitions>
    <!-- existing content -->

    <LoadingOverlay x:Name="LoadingOverlay" Grid.RowSpan="99"/>
</Grid>
```

- [ ] **Step 2: Wrap LoadFlows with Show/Hide**

Find `LoadFlows()` method. The method is sync but calls CRM. Wrap the body:
```csharp
private void LoadFlows()
{
    if (_service == null) return;

    LoadingOverlay.Show("Loading flows...");

    try
    {
        // ... existing code up to cboFlow.ItemsSource = _flows;
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
```

- [ ] **Step 3: Wrap SearchRecordsAsync in InvokeFlowControl**

Find `SearchRecordsAsync`. Add Show/Hide:
```csharp
try
{
    LoadingOverlay.Show("Searching records...");
    // ... existing code
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
```

- [ ] **Step 4: Wrap btnInvoke_Click with Show/Hide**

Find `btnInvoke_Click`. Add Show/Hide with message "Invoking...":
```csharp
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
        // ... existing code
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
```

- [ ] **Step 5: Verify compiles**

Run: `"C:/Program Files/Microsoft Visual Studio/2022/Community/MSBuild/Current/Bin/MSBuild.exe" "XrmToolBox/DynaAppX/DynaAppX/DynaAppX.csproj" -p:Configuration=Debug -p:Platform="AnyCPU"`
Expected: 0 errors

- [ ] **Step 6: Commit**

```bash
git add XrmToolBox/DynaAppX/DynaAppX/WpfControls/InvokeFlowControl.xaml XrmToolBox/DynaAppX/DynaAppX/WpfControls/InvokeFlowControl.xaml.cs
git commit -m "feat(InvokeFlowControl): add LoadingOverlay for async operations"
```

---

### Task 4: Integrate LoadingOverlay into GodPageControl

**Files:**
- Modify: `XrmToolBox/DynaAppX/DynaAppX/WpfControls/GodPageControl.xaml`
- Modify: `XrmToolBox/DynaAppX/DynaAppX/WpfControls/GodPageControl.xaml.cs`
- Test: Compile + code review

- [ ] **Step 1: Add LoadingOverlay to GodPageControl.xaml**

Read current XAML. Find root Grid, add LoadingOverlay as last child:
```xaml
<Grid>
    <Grid.RowDefinitions>...</Grid.RowDefinitions>
    <!-- existing content -->

    <LoadingOverlay x:Name="LoadingOverlay" Grid.RowSpan="99"/>
</Grid>
```

- [ ] **Step 2: Wrap SearchRecordsAsync with Show/Hide**

Find `SearchRecordsAsync` in GodPageControl. Add Show/Hide:
```csharp
try
{
    LoadingOverlay.Show("Searching records...");
    // ... existing code
}
catch (OperationCanceledException)
{
    // Expected when search is cancelled
}
catch (Exception ex)
{
    txtStatus.Text = $"Error: {ex.Message}";
}
finally
{
    LoadingOverlay.Hide();
}
```

- [ ] **Step 3: Wrap LoadRecordData with Show/Hide**

Find `LoadRecordData()`. This is the slowest operation loading all attributes. Add Show with "Loading record...":
```csharp
private async void LoadRecordData()
{
    _detectChangesTimer.Stop();
    if (_service == null || _selectedEntity == null || _selectedRecord == null) return;

    LoadingOverlay.Show("Loading record attributes...");

    try
    {
        // ... existing code
    }
    catch (Exception ex)
    {
        txtStatus.Text = $"Error loading record: {ex.Message}";
    }
    finally
    {
        LoadingOverlay.Hide();
    }
}
```

- [ ] **Step 4: Verify compiles**

Run: `"C:/Program Files/Microsoft Visual Studio/2022/Community/MSBuild/Current/Bin/MSBuild.exe" "XrmToolBox/DynaAppX/DynaAppX/DynaAppX.csproj" -p:Configuration=Debug -p:Platform="AnyCPU"`
Expected: 0 errors

- [ ] **Step 5: Commit**

```bash
git add XrmToolBox/DynaAppX/DynaAppX/WpfControls/GodPageControl.xaml XrmToolBox/DynaAppX/DynaAppX/WpfControls/GodPageControl.xaml.cs
git commit -m "feat(GodPageControl): add LoadingOverlay for async operations"
```

---

## Verification Checklist

After all tasks:

1. [ ] `MSBuild.exe` compile produces 0 errors
2. [ ] AccessCheckControl: Load Entities shows overlay during load
3. [ ] AccessCheckControl: Selecting user shows overlay while loading roles/teams
4. [ ] AccessCheckControl: Selecting entity then deselecting disables cboRecord
5. [ ] AccessCheckControl: Search records shows overlay
6. [ ] InvokeFlowControl: Load flows shows overlay
7. [ ] InvokeFlowControl: Search records shows overlay
8. [ ] InvokeFlowControl: Invoke flow/action shows overlay
9. [ ] GodPageControl: Search records shows overlay
10. [ ] GodPageControl: Select record to load attributes shows overlay