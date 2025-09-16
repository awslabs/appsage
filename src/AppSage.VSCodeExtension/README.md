# AppSage VS Code Extension

This Visual Studio Code extension provides custom handlers for AppSage files including graphs and tables, plus comprehensive profile management for organizing your AppSage workspaces.

## Features

- **Graph Handler**: Visualize `.appsage.graph` files using Cytoscape.js
- **Table Handler**: Display `.appsage.table` files using AG Grid
- **Profile Management**: Organize and switch between different AppSage configurations
- **Activity Bar Integration**: Dedicated AppSage activity bar with profile explorer
- **Status Bar Integration**: Quick profile switching from the status bar
- **Modular Architecture**: Extensible design for future handler types

## File Types Supported

- `*.appsage.graph` - Graph visualization files
- `*.appsage.table` - Table data files

## Profile Management

The extension includes a powerful profile management system that allows you to:

- **Create Profiles**: Define workspace and output paths for different projects
- **Switch Profiles**: Quickly change between different AppSage configurations
- **Manage Settings**: Edit profile names, workspace paths, and output paths
- **Visual Indicators**: See which profile is currently active

### Quick Start with Profiles

1. **Open AppSage Activity Bar**: Click the AppSage icon in the left sidebar
2. **Create Your First Profile**: Click the "+" button in the Profile Explorer
3. **Configure Paths**: Set your AppSage workspace and output directories
4. **Start Working**: The profile is now active and ready to use

For detailed information about profile management, see [Profile Management Documentation](docs/ProfileManagement.md).

## Usage

1. Open any `.appsage.graph` or `.appsage.table` file in VS Code
2. The appropriate custom viewer will automatically activate
3. Use the toolbar controls to interact with the visualization
4. Manage your AppSage configurations using the profile system

## Development

### Prerequisites

- Node.js 16+
- TypeScript 4.9+
- VS Code 1.74+

### Setup

```bash
npm install
npm run compile
```

### Testing

Press `F5` in VS Code to launch the extension in a new Extension Development Host window.

### Building

```bash
npm run vscode:prepublish
```

## Graph File Format

```json
{
  "Nodes": [
    {
      "Id": "node1",
      "Name": "Node 1",
      "Type": "Project",
      "Attributes": {}
    }
  ],
  "Edges": [
    {
      "Source": "node1",
      "Target": "node2",
      "Type": "dependency",
      "Attributes": {}
    }
  ]
}
```

## Table File Format

```json
{
  "columns": [
    {
      "field": "name",
      "headerName": "Name",
      "width": 150,
      "sortable": true,
      "filter": true,
      "type": "text"
    }
  ],
  "rows": [
    {
      "name": "Example",
      "value": 123
    }
  ],
  "metadata": {
    "title": "Sample Table",
    "description": "Example table data"
  }
}
```
