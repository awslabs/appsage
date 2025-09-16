# Template System

This document describes the template system used for VS Code extension webviews.

## Overview

The extension uses a template-based approach for managing HTML content in webviews. This provides better separation of concerns, easier maintenance, and improved developer experience.

## Structure

### HTML Templates
- **Location**: `webview/{handler}/`
- **Purpose**: Contains the HTML structure with placeholder markers
- **Example**: `webview/graph/graph.html`, `webview/graph/properties.html`

### CSS Files
- **Location**: `webview/{handler}/`
- **Purpose**: Contains styling separated from HTML for easier maintenance
- **Example**: `webview/graph/graph.css`, `webview/graph/properties.css`

### JavaScript Files  
- **Location**: `webview/{handler}/`
- **Purpose**: Contains client-side logic
- **Example**: `webview/graph/graph.js`

## Template Placeholders

Templates use double-brace syntax for placeholder replacement: `{{PLACEHOLDER_NAME}}`

### Common Placeholders
- `{{CSP_SOURCE}}` - Content Security Policy
- `{{NONCE}}` - Random nonce for script security
- `{{STYLE_URI}}` - URI to the CSS file
- `{{SCRIPT_URI}}` - URI to the JavaScript file

### Graph Template Placeholders
- `{{CYTOSCAPE_URI}}` - URI to Cytoscape library
- `{{CYTOSCAPE_COSE_BILKENT_URI}}` - URI to Cose Bilkent layout extension

## Usage

### Using TemplateLoader

```typescript
import { TemplateLoader } from '../../shared/utils/templateLoader';

// In your viewer class
protected getHtmlForWebview(webview: vscode.Webview, document: vscode.TextDocument): string {
    const styleUri = webview.asWebviewUri(
        vscode.Uri.joinPath(this.context.extensionUri, 'webview', 'graph', 'graph.css')
    );
    
    const nonce = TemplateLoader.generateNonce();
    const csp = TemplateLoader.createCSP(webview, nonce);

    return TemplateLoader.loadTemplate(this.context, 'graph/graph.html', webview, {
        CSP_SOURCE: csp,
        NONCE: nonce,
        STYLE_URI: styleUri.toString(),
        // Add other placeholders as needed
    });
}
```

### Content Providers

For content providers that need templates:

```typescript
public getWebviewContent(): string {
    const styleUri = vscode.Uri.joinPath(this._context.extensionUri, 'webview', 'graph', 'properties.css');
    
    return TemplateLoader.loadTemplate(this._context, 'graph/properties.html', undefined as any, {
        STYLE_URI: styleUri.toString()
    });
}
```

## Benefits

1. **Separation of Concerns**: HTML, CSS, and TypeScript are in separate files
2. **Easier Maintenance**: Edit templates without rebuilding TypeScript
3. **Better IDE Support**: Full HTML/CSS syntax highlighting and formatting
4. **Reusability**: Templates can be shared across different components
5. **Version Control**: Better diffs when templates change

## Best Practices

1. **Use External CSS**: Keep styles in separate CSS files rather than inline
2. **Meaningful Placeholders**: Use descriptive placeholder names
3. **Document Dependencies**: Include comments about required placeholders
4. **Error Handling**: Handle missing template files gracefully
5. **Security**: Always use nonces and proper CSP for script tags

## Migration from Embedded HTML

When converting from embedded HTML strings to templates:

1. Extract HTML to a `.html` file in the appropriate webview directory
2. Extract CSS to a `.css` file 
3. Replace dynamic values with `{{PLACEHOLDER}}` syntax
4. Update TypeScript to use `TemplateLoader.loadTemplate()`
5. Test compilation and functionality

## File Organization

```
webview/
├── graph/
│   ├── graph.html          # Main graph viewer template
│   ├── graph.css           # Graph viewer styles
│   ├── graph.js           # Graph viewer client logic
│   ├── properties.html     # Properties panel template
│   └── properties.css      # Properties panel styles
├── table/
│   └── ... (table templates)
└── shared/
    └── ... (shared resources)
```
