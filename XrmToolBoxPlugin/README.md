# DynaAppX XrmToolBox Plugin

Access Check & Flow Invocation Tool for Dynamics 365 CRM.

## Features

- **AccessCheck**: Check user permissions on CRM records
  - Select user from dropdown
  - View user's roles and teams
  - Check record access rights (Read, Write, Create, Delete, Share, Assign, Append, AppendTo)

- **InvokeFlow**: Execute Dynamics 365 workflows and actions
  - Select workflow or action from dropdown
  - Execute on selected record
  - View execution history

## Development Environment

- Windows 10/11
- Visual Studio 2022
- .NET Framework 4.8
- XrmToolBox (for testing)

## Project Structure

```
XrmToolBoxPlugin/
├── DynaAppX.csproj              # Project file
├── MyPlugin.cs                  # Plugin entry class (MEF Export)
├── MyPluginControl.cs           # Main UI logic
├── MyPluginControl.designer.cs  # UI designer code
├── Settings.cs                  # Plugin settings
└── Properties/                  # Assembly info
```

## Build & Deploy

1. Open `DynaAppX.csproj` in Visual Studio
2. Restore NuGet packages
3. Build project
4. DLL will be copied to `bin\XrmToolBox\Plugins\`
5. Copy the DLL to XrmToolBox's Plugins folder
6. Restart XrmToolBox

## Usage

1. Connect to your Dynamics 365 organization
2. Click "Reload Data" to load users, flows, and entities
3. **AccessCheck Tab**: Select user, entity, enter record ID, click "Check Access"
4. **InvokeFlow Tab**: Select flow, enter record ID, click "Invoke"

## Authentication

Uses XrmToolBox's built-in CRM connection - no additional authentication needed.

## License

MIT