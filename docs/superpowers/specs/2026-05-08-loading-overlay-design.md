# 防呆设计 - Loading Overlay System

## Status

- Date: 2026-05-08
- Author: dengrh1
- Status: Draft

## Problem Statement

DynaAppX XrmToolBox 插件的异步操作（网络请求）缺乏防呆保护：
1. 异步操作进行时，用户可继续操作UI，触发重复/多次调用
2. Race condition：快速点击搜索，最新结果被旧请求覆盖
3. 长时间操作（如加载 record 所有属性）无反馈，用户不知是否在进行中
4. AccessCheckControl：选择/取消 entity 时，record 控件未重置或禁用

## Solution Overview

为所有耗时异步操作添加全屏遮罩 LoadingOverlay 组件：
- 操作开始 → 显示遮罩，禁用所有控件
- 操作完成 → 隐藏遮罩，恢复交互
- 支持 Cancel 机制（可选，长时间操作）

---

## 受保护的异步操作

| Control | 异步操作 | 触发时机 | 操作类型 |
|---------|---------|---------|---------|
| **AccessCheckControl** | `SearchRecordsAsync` | 搜索 record | 只读 |
| | `LoadUserRoles/Teams` | 选择 user | 只读 |
| | `CheckAccessRights` | 选择 user/record/entity | 只读 |
| | `btnLoadEntities_Click` | 加载实体列表 | 只读 |
| **InvokeFlowControl** | `LoadFlows` | 打开 flow 下拉 | 只读 |
| | `SearchRecordsAsync` | 搜索 record | 只读 |
| | `btnInvoke_Click` | 调用流程/动作 | 写 |
| **GodPageControl** | `SearchRecordsAsync` | 搜索 record | 只读 |
| | `LoadRecordData` | 选择 record | 只读 |

---

## LoadingOverlay Component Design

### XAML 结构

```xaml
<UserControl x:Class="DynaAppX.WpfControls.LoadingOverlay"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid x:Name="RootGrid" Visibility="Collapsed" Background="#80000000">
        <Border Background="White" CornerRadius="8" Padding="30,20"
                HorizontalAlignment="Center" VerticalAlignment="Center"
                MinWidth="200">
            <StackPanel>
                <!-- Spinning ring animation -->
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

### C# Code-Behind

```csharp
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
```

---

## 每个 Control 的集成方式

### 集成模板 (每个 UserControl XAML)

```xaml
<Grid>
    <!-- 原内容 -->
    <Grid.RowDefinitions>...</Grid.RowDefinitions>
    ...

    <!-- 遮罩层放最上层 -->
    <LoadingOverlay x:Name="LoadingOverlay" Grid.RowSpan="99"/>
</Grid>
```

### 集成模板 (每个 UserControl Code-Behind)

```csharp
public partial class SomeControl : UserControl
{
    private IOrganizationService _service;

    // 异步操作前显示遮罩
    private async void SomeAsyncOperation()
    {
        if (_service == null) return;
        LoadingOverlay.Show("Searching records...");

        try
        {
            // ... 异步操作
        }
        finally
        {
            LoadingOverlay.Hide();
        }
    }
}
```

---

## AccessCheckControl 额外修复

### 问题
- 选择/取消 entity 时，record 控件未重置
- 未选择 entity 时，record 仍可操作

### 修复逻辑

```csharp
private void cboEntity_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    cboRecord.ItemsSource = null;
    _records.Clear();

    if (cboEntity.SelectedItem is EntityWrapper)
    {
        cboRecord.IsEnabled = true;
        _ = SearchRecordsAsync("");
    }
    else
    {
        cboRecord.IsEnabled = false;
        lstAccessRights.ItemsSource = null;
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
    else
    {
        lstRoles.ItemsSource = null;
        lstTeams.ItemsSource = null;
        lstAccessRights.ItemsSource = null;
    }
}
```

---

## UI 改进建议

### 1. AccessCheckControl 禁用状态视觉区分
- 禁用的 ComboBox 使用灰色背景 (`#F5F5F5`)
- 已启用的 ComboBox 使用白色背景

### 2. 状态文字国际化 (可选)
- 当前硬编码英文，可改为资源文件支持

### 3. 加载动画颜色主题化
- Spinner 颜色使用 `#007ACC` (VS 主题蓝)
- 未来可通过依赖属性支持主题定制

---

## Implementation Plan

### Phase 1: LoadingOverlay 组件
1. 创建 `WpfControls/LoadingOverlay.xaml` 及 code-behind
2. 测试组件独立功能

### Phase 2: 集成到 AccessCheckControl
1. 添加 `LoadingOverlay` 到 XAML
2. 为 `btnLoadEntities_Click`、`LoadUserRoles`、`LoadUserTeams`、`SearchRecordsAsync` 添加 `Show/Hide`
3. 修复 entity selection changed 时 record 控件状态

### Phase 3: 集成到 InvokeFlowControl
1. 添加 `LoadingOverlay` 到 XAML
2. 为 `LoadFlows`、`SearchRecordsAsync`、`btnInvoke_Click` 添加 `Show/Hide`

### Phase 4: 集成到 GodPageControl
1. 添加 `LoadingOverlay` 到 XAML
2. 为 `SearchRecordsAsync`、`LoadRecordData` 添加 `Show/Hide`

---

## Files to Modify

| File | Change |
|------|--------|
| `WpfControls/LoadingOverlay.xaml` | NEW |
| `WpfControls/LoadingOverlay.xaml.cs` | NEW |
| `WpfControls/AccessCheckControl.xaml` | 添加 LoadingOverlay |
| `WpfControls/AccessCheckControl.xaml.cs` | 添加 Show/Hide 调用，修复 selection 逻辑 |
| `WpfControls/InvokeFlowControl.xaml` | 添加 LoadingOverlay |
| `WpfControls/InvokeFlowControl.xaml.cs` | 添加 Show/Hide 调用 |
| `WpfControls/GodPageControl.xaml` | 添加 LoadingOverlay |
| `WpfControls/GodPageControl.xaml.cs` | 添加 Show/Hide 调用 |
| `DynaAppX.csproj` | 添加 LoadingOverlay.xaml 引用 |

---

## Verification

1. 编译通过，0 errors
2. 打开 AccessCheckControl，Load Entities 期间遮罩出现
3. 选择 user 加载 roles/teams 期间遮罩出现
4. 选择 entity 后清空 record，record 控件禁用
5. 搜索 records 期间遮罩出现（快速输入多个字符不会触发多次请求）
6. InvokeFlowControl 选择 flow 加载 parameters 期间遮罩出现
7. Invoke flow/action 期间遮罩出现
8. GodPageControl 选择 record 加载 attributes 期间遮罩出现