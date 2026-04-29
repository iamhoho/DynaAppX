# DynaAppX XrmToolBox Plugin

Access Check & Flow Invocation Tool for Dynamics 365 CRM.

## 功能

- **AccessCheck**: 检查用户在 CRM 记录上的访问权限
- **InvokeFlow**: 执行 Dynamics 365 工作流和操作

## 开发环境要求

- Windows 10/11
- Visual Studio 2022
- .NET Framework 4.8
- XrmToolBox (用于测试和调试)

## 项目结构

```
XrmToolBoxPlugin/
├── DynaAppX.csproj        # 项目文件
├── DynaAppXPlugin.cs      # 插件主类 (含 UI)
└── Properties/
    └── AssemblyInfo.cs    # 程序集信息
```

## NuGet 包依赖

- `XrmToolBoxPackage` - XrmToolBox SDK
- `Microsoft.CrmSdk.CoreAssemblies` - Dynamics CRM SDK
- `Microsoft.CrmSdk.Workflow` - 工作流支持
- `Microsoft.CrmSdk.XrmTooling.CoreAssembly` - XrmTooling 核心
- `Microsoft.CrmSdk.XrmTooling.WpfControls` - WPF 控件

## 构建步骤

1. 在 Windows 上克隆仓库
2. 用 Visual Studio 打开 `DynaAppX.csproj`
3. 还原 NuGet 包（Visual Studio 会自动提示）
4. 生成项目（Ctrl+Shift+B）
5. DLL 输出到 `bin/Debug/` 或 `bin/Release/`

## 部署到 XrmToolBox

1. 编译项目生成 `DynaAppX.dll`
2. 打开 XrmToolBox
3. 进入 **Plugin Manager**（工具 → 插件管理器）
4. 点击 **Import** 导入编译好的 DLL
5. 重启 XrmToolBox
6. 在工具列表中找到 **DynaAppX**

## 使用方法

### AccessCheck

1. 从下拉列表选择用户
2. 选择实体类型并输入记录 ID (GUID)
3. 点击 "Check Access"
4. 查看用户的角色、团队和该记录上的访问权限

### InvokeFlow

1. 从下拉列表选择工作流或操作
2. 如果是工作流，需要选择记录类型并输入记录 ID
3. 如果是操作，根据配置可能需要输入记录 ID
4. 点击 "Invoke"
5. 查看执行历史

## 调试

在 Visual Studio 中：
1. 设置断点
2. **调试 → 附加到进程**
3. 选择 `XrmToolBox.exe` 进程
4. 在 XrmToolBox 中执行插件操作触发断点

## 认证

插件使用 XrmToolBox 内置的 CRM 连接管理，不需要额外配置认证。

## 许可证

MIT