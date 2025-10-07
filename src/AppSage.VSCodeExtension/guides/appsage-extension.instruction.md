---
applyTo: 'AppSage.VSCodeExtension/**'
---
GitHub Copilot Guidelines for AppSage VS Code Extension

# Extension Overview
- Visual Studio Code extension for viewing and analyzing AppSage output files 
- Provides interactive visualization capabilities within the VS Code editor to view different output types
- Enables developers to explore code relationships, dependencies, and architectural insights, tables, charts etc. directly in their development environment
- Integrates seamlessly with the AppSage ecosystem for comprehensive code analysis workflows


# Architecture
The extension follows VS Code's extensibility patterns

# Technical Stack
- **Extension Framework**: VS Code Extension API, TypeScript 4.9+
- **Build System**: TypeScript compiler, NPM scripts, VSCE packaging
- **Logging** : Any errors must be logged to vs code output. 

# Project Structure
Each handler's related files are organized in its own folder following a consistent structure.

```
AppSage.VSCodeExtension/
├── src/
│   ├── extension.ts              # Main extension entry point
│   ├── shared/                   # Shared components and utilities
│   │   ├── components/           # Reusable components (BaseViewer, etc.)
│   │   ├── providers/            # Common providers (FileProvider, etc.)
│   │   ├── types/                # Shared type definitions
│   │   └── utils/                # Common utilities (Logger, etc.)
│   └── handlers/                 # File type handlers
│       ├── {type}/               # Handler for specific file type
│       │   ├── components/       # Handler-specific components
│       │   ├── providers/        # Handler-specific providers
│       │   ├── types/            # Handler-specific types
│       │   └── {type}Viewer.ts   # Main viewer implementation
├── webview/                      # Static webview assets
│   ├── shared/                   # Common webview assets
│   └── {type}/                   # Handler-specific webview assets
│       ├── {type}.html           # HTML template
│       ├── {type}.css            # Styles
│       └── {type}.js             # Client-side logic
├── samples/                      # Sample files for testing
└── package.json                  # Extension manifest
```

# UI/UX Guidelines
- **UI Components**: HTML5, CSS3 with VS Code theme variables, vanilla JavaScript
- **Responsive Design**: Support different panel sizes and orientations
- **Accessibility**: Keyboard navigation, screen reader compatibility
- **Webview Integration**: Uses VS Code webview APIs for rich content rendering
   - Secure Content Security Policy (CSP) configuration
   - Message passing between extension and webview content
   - VS Code theme integration for consistent UI
- **VS Code Theme Integration**: Use CSS variables for consistent theming
  Examples. 
  - `var(--vscode-foreground)`, `var(--vscode-background)`
  - `var(--vscode-button-background)`, `var(--vscode-button-foreground)`
  - `var(--vscode-dropdown-background)`, `var(--vscode-panel-border)`

  
# Technologies to avoid
- Do not use React, Vue, or other frontend frameworks (use vanilla JS/HTML)
- Avoid jQuery dependencies
- Do not keep external CDN references. All javascript libraries must be locally available. 

# File Format
Extension handles AppSage output files with the pattern: `{filename}.appsage{type}`

- `filename`: Any file name
- `.appsage`: Identifies AppSage file format
- `type`: Handler type (graph, table, etc.)

Examples: `analysis.appsagegraph`, `metrics.appsagetable`

Sample files for testing are available in `/samples/`.
 - SampleTable.appsagetable

# Coding Standards
- Follow VS Code extension development best practices
- Use TypeScript strict mode and proper type definitions
- Implement proper error boundaries and logging
- Use async/await patterns for asynchronous operations
- Follow VS Code API patterns and conventions
- Maintain compatibility with VS Code version requirements
- Use semantic versioning for extension releases

# Extension Packaging
- Use VSCE (Visual Studio Code Extension) tool for packaging
- Include proper extension manifest with activation events
- Configure extension categories and keywords appropriately
- Provide comprehensive README and CHANGELOG documentation
- Include sample files for testing and demonstration

# Handlers
Each handler implements `vscode.CustomTextEditorProvider` to process specific file types.

## Common Handler Architecture
- Extend `BaseViewer` abstract class from `shared/components/`
- Implement file content parsing and validation
- Handle webview lifecycle and messaging
- Register with VS Code's custom editor API

## Graph handler
- **File Type**: `.appsagegraph`
- **Visualization**: Interactive graphs using Cytoscape.js 3.26.0
- **Features**: Multiple layout algorithms, zoom/pan controls, node selection, double-click file opening
- **Layouts**: Cose, Cose-Bilkent, Circle, Grid, Concentric, Breadth-First
- **Filtering**: A multi select drop down to select node types. A multi select drop down to select edge types.
- **File Opening**: Double-click nodes with ResourceFilePath attribute to open files in VS Code
- **Sample file**: `samples/SampleGraph.appsagegraph`

## Table handler
- **File Type**: `.appsagetable`
- **Visualization**: Structured data tables with AG Grid
- **Features**: Sorting, filtering, search, CSV export
- **Sample file**: `samples/SampleTable.appsagetable`

## Chart handler
- **File Type**: `.appsagechart`
- **Visualization**: Interactive charts using Apache ECharts 5.4.3
- **Features**: Multiple chart types, customizable appearance, multi-table support
- **Chart Types**: Bar, Line, Pie, Scatter, Area, Heatmap, Radar, Gauge
- **Customization**: Chart titles, legend positioning, color themes, grid display, animations
- **Data Format**: JSON-serialized DataTable arrays with custom serialization
- **Single Table Selection**: Dropdown to select which table to visualize
- **Sample files**: `samples/SampleChart.appsagechart`, `samples/ComprehensiveChart.appsagechart`

