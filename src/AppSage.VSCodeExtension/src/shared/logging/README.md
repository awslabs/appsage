# AppSage Logging System

## Overview
The AppSage extension uses a centralized logging system that provides consistent logging across all components. All logs are output to the "AppSage" output channel in VS Code.

## Usage

### Basic Usage
```typescript
import { AppSageLogger } from '../shared/logging';

// Get the singleton instance
const logger = AppSageLogger.getInstance();

// Log messages
logger.info('Information message');
logger.warning('Warning message');
logger.error('Error message');
logger.debug('Debug message');
```

### Component-Specific Logging
```typescript
import { AppSageLogger } from '../shared/logging';

const logger = AppSageLogger.getInstance();
const componentLogger = logger.forComponent('MyComponent');

// All messages will be prefixed with [MyComponent]
componentLogger.info('Component started');
componentLogger.error('Component failed', errorDetails);
```

### Initialization
In extension.ts, the logger is initialized and registered with VS Code:
```typescript
const logger = AppSageLogger.initialize(context);
```

## Log Levels

### ERROR
- Critical errors that prevent functionality
- Exceptions and failures
- Use for issues that need immediate attention

### WARNING  
- Issues that don't break functionality but should be noted
- Unexpected conditions that are handled gracefully
- Use for recoverable problems

### INFO
- Important operational information
- Component lifecycle events (start, stop, configuration)
- Major state changes

### DEBUG
- Detailed diagnostic information
- Message passing between components
- Data structures and values
- Use for troubleshooting

## Output Format
```
[timestamp] [LEVEL] [Component] Message [additional data]
```

Example:
```
2025-09-09T10:30:15.123Z INFO [GraphPropertyManager] Node selected: /Repositories/... in document: file:///...
2025-09-09T10:30:15.124Z ERROR [GraphViewer] Failed to parse graph data {"error": "Invalid JSON"}
```

## Components Using Logger

### Extension
- Extension activation/deactivation
- Component registration

### GraphViewer
- Custom text editor lifecycle
- Message handling from webview
- Graph data parsing

### GraphPropertyManager
- Graph data management
- Node/edge selection handling
- Property updates

### PropertyViewProvider
- Webview lifecycle
- Property update messaging

### BaseViewer
- Common webview operations
- Document change handling
- Error handling

## Best Practices

1. **Use appropriate log levels** - Don't use INFO for debug information
2. **Include context** - Add relevant IDs, filenames, or state information
3. **Use component loggers** - Create component-specific loggers for better organization
4. **Log errors with details** - Include error objects and context
5. **Avoid excessive debug logging** - Only log what's useful for troubleshooting

## Viewing Logs

1. Open VS Code
2. Go to **View** → **Output**
3. Select **"AppSage"** from the dropdown
4. All extension logs will appear here with timestamps and component names
