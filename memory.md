---
name: DynaAppX WPF Migration - Project Memory
description: Complete project status, decisions, and technical notes
last_updated: 2026-05-06
---

# DynaAppX 项目记忆

## 项目信息
- **工作目录**: `/home/dangyeuihou/repos/DynaAppX-migration/`
- **原始目录**: `/home/dangyeuihou/repos/DynaAppX/`
- **GitHub**: https://github.com/iamhoho/DynaAppX
- **远程**: git@github.com:iamhoho/DynaAppX.git (SSH)

## 分支
- `feat/ai-assistant-migration` — WPF 迁移工作分支（所有改动在此分支）

## 项目结构
```
DynaAppX-migration/
├── vue-project/                    # Vue 参考实现（原始）
│   └── src/views/
│       ├── AccessCheckView.vue
│       ├── InvokeFlowView.vue
│       ├── GodPageView.vue
│       └── MetadataBrowserView.vue
└── XrmToolBox/DynaAppX/DynaAppX/  # WPF 迁移目标
    ├── MyPluginControl.cs           # 宿主：TabControl + ToolStrip
    ├── MyPluginControl.designer.cs
    ├── WpfControls/
    │   ├── AccessCheckControl.xaml/.cs
    │   ├── InvokeFlowControl.xaml/.cs
    │   ├── GodPageControl.xaml/.cs
    │   ├── MetadataBrowserControl.xaml/.cs
    │   └── SharedMetadataCache.cs    # 跨 Tab 共享缓存
    └── Converters/
        ├── AccessColorConverter.cs
        └── InvokeFlowConverters.cs
```

## 迁移完成状态
✅ AccessCheck — 用户权限检查
✅ InvokeFlow — 调用 Workflow/Action
✅ GodPage — 实体记录编辑器
✅ MetadataBrowser — CRM 工具跳转页面

## 架构决策

### 宿主架构（MyPluginControl）
- **ToolStrip**: 4 个按钮创建新 Tab（🔑⚡📝🌐）
- **TabControl**: 每个功能独立 Tab 页，可多开同一功能
- **ElementHost**: WPF UserControl 托管容器
- **Welcome Tab**: 首页，包含使用说明和功能入口
- **× 关闭按钮**: OwnerDrawFixed 自绘，关闭时 Dispose 释放资源

### 欢迎页布局
使用 TableLayoutPanel + FlowLayoutPanel 响应式布局，不再用绝对坐标

### 缓存策略
`SharedMetadataCache` 静态类，按 `IOrganizationService` 实例缓存：
- `_entities` — 实体列表（所有 Tab 共用）
- `_entityMetadataCache` — 实体元数据（用于权限检查）
- `_flows` — Flow/Workflow 列表
- CRM 连接变更时自动清空

### 加载策略
- Load Entities / Load Flows **不自动调用**，需点击按钮
- 首次加载后缓存，后续 Tab 直接复用

## 关键 Bug 修复记录
1. FetchXML 通配符 `*` → `%`
2. `PrivilegeType.Update` → `PrivilegeType.Write`（Update 不存在）
3. `RecordWrapper` 重复定义 → 重命名为 `FlowRecordWrapper` / `GodRecordWrapper`
4. `HasDeleteAccess` 曾真实删除记录 → 改用权限检查
5. `InvokeAction()` 空方法 → 实际执行 OrganizationRequest
6. EntityReference ComboBox 空数据源 → 改 TextBox + JSON
7. LoadEntities 自动调用导致卡顿 → 移除自动调用，加按钮
8. ElementHost 局部变量被 GC → 改为类字段或按需创建
9. `Label` 类型歧义 → 明确使用 `System.Windows.Forms.Label`
10. `bool?` 操作符错误 → `a.IsPrimaryId != true`

## 技术要点
- WPF UserControl 继承 `System.Windows.Controls.UserControl`
- ElementHost.Child 接受 `System.Windows.UIElement`
- TabControl.DrawMode 用 `TabDrawMode.OwnerDrawFixed`
- FetchXML like 用 `%` 而非 `*`
- CRM 权限用 `PrivilegeType.Write` 而非 Update
- `SecurityElement.Escape` 防止 FetchXML 注入

## 下次继续
- 测试各功能 Tab 的实际运行效果
- 确认 Load Entities / Load Flows 缓存共享正常
- 验证 × 关闭按钮资源释放
