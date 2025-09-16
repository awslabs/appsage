# AppSage VS Code Extension Release

This folder contains the packaged VS Code extension files (.vsix) for distribution.

## Release Process

### Using VS Code Tasks (Recommended)

1. **From Command Palette**:
   - Press `Ctrl+Shift+P`
   - Type "Tasks: Run Task"
   - Select "publish-vscode-extension"

2. **From Terminal Panel**:
   - Press `Ctrl+Shift+`
   - Choose "Run Task" from the dropdown
   - Select "publish-vscode-extension"

### Using NPM Scripts

```bash
# Navigate to extension directory
cd AppSage.VSCodeExtension

# Install packaging tool (first time only)
npm run package:install

# Build and package
npm run release
```

### Manual Process

```bash
# Navigate to extension directory
cd AppSage.VSCodeExtension

# Compile TypeScript
npm run compile

# Package extension
vsce package --out release/
```

## Installing the Extension

1. **From VS Code**:
   - Press `Ctrl+Shift+P`
   - Type "Extensions: Install from VSIX"
   - Select the `.vsix` file from this folder

2. **From Command Line**:
   ```bash
   code --install-extension path/to/extension.vsix
   ```

## Files in this Folder

- `*.vsix` - Packaged extension files ready for installation
- Each file includes version number and build timestamp

## Note

This folder is excluded from Git (see .gitignore) to avoid committing large binary files.
