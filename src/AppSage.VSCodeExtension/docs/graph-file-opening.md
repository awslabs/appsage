# Graph Double-Click File Opening

The AppSage Graph Viewer supports double-clicking nodes to automatically open associated files in VS Code.

## How It Works

When you double-click on a node in the graph viewer:

1. **ResourceFilePath Check**: The system checks if the node contains a `ResourceFilePath` attribute
2. **Path Resolution**: The relative path is resolved against the VS Code workspace root folder
3. **File Opening**: If the file exists, it opens in a new VS Code editor tab
4. **Error Handling**: If the file doesn't exist, a warning message is displayed in the AppSage output channel

## Node Requirements

For a node to support file opening, it must have:
- A `ResourceFilePath` attribute in its `Attributes` collection
- The path should be relative to the workspace root folder (leading slash optional)

## Example Node Structure

```json
{
  "Id": "Bookstore.Web.AuthenticationConfig",
  "Name": "AuthenticationConfig", 
  "Type": "Class",
  "Attributes": {
    "ResourceFilePath": "/Repositories/bobs-used-bookstore-classic/app/Bookstore.Web/App_Start/AuthenticationSetup.cs",
    "NameTypePosition": "[16,4,92,5]",
    "NameTypeMethodCount": "3",
    "NameTypeLinesOfCode": "67"
  }
}
```

## Path Resolution

The `ResourceFilePath` is resolved as follows:

1. **Leading slash removal**: `/path/to/file.cs` becomes `path/to/file.cs`
2. **Workspace resolution**: Joined with the first workspace folder
3. **Final path**: `{workspace}/path/to/file.cs`

## Error Messages

- **File not found**: Warning displayed in VS Code and logged to AppSage output channel
- **No workspace**: Warning if no VS Code workspace is open
- **Invalid path**: Error logged if path resolution fails

## Usage

1. Open an `.appsagegraph` file in VS Code
2. Find a node with a file icon or that represents a class/file
3. Double-click the node
4. If the node has a `ResourceFilePath`, the file will open automatically

This feature is particularly useful when working with code analysis graphs where nodes represent classes, methods, or other code elements that have corresponding source files.
