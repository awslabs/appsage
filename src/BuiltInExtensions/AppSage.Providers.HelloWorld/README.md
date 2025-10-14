# AppSage.Providers.HelloWorld

A simple AppSage extension that demonstrates basic metrics collection with minimal dependencies. This extension serves as a perfect starting point for learning the AppSage extension system and developing your own custom extensions.

## 🎯 Overview

The HelloWorld extension provides a basic example of how to implement the `IMetricProvider` interface and create meaningful metrics for the AppSage platform. It demonstrates the fundamental concepts needed to build extensions for code analysis and metrics collection.

## ✨ Features

### 📊 **Sample Metrics Collection**
- **Simple String Metric**: Demonstrates basic string value metrics
- **Resource-based Metric**: Shows how to create metrics associated with specific resources
- **DataTable Metric**: Illustrates complex data structure metrics with tabular data

### 🎓 **What This Extension Demonstrates**

1. **IMetricProvider Implementation**: Shows how to implement the core interface for metric collection
2. **IExtension Interface**: Demonstrates the extension lifecycle management
3. **Minimal Dependencies**: Uses only core AppSage dependencies
4. **Multiple Metric Types**: Examples of different metric value types
5. **Extension Packaging**: Complete NuGet package configuration

## 📋 Metrics Provided

### **AppSage.HelloWorld.Message**
- **Type**: Simple string metric
- **Value**: "Hello, World!"
- **Purpose**: Demonstrates basic metric creation

### **AppSage.HelloWorld.WorldGDP**
- **Type**: Resource-based integer metric
- **Resource**: "2015/worldbank"
- **Value**: 75720 (billion USD)
- **Purpose**: Shows resource-associated metrics

### **AppSage.HelloWorld.CountryGDP**
- **Type**: DataTable metric
- **Structure**: Year, Country, GDPInBillion columns
- **Data**: Sample GDP data for USA and Germany (2023)
- **Purpose**: Demonstrates complex structured data metrics

## 🏗️ Extension Architecture

This extension implements both required interfaces:

### **IMetricProvider**
```csharp
public class AppSageHelloWorldProvider : IMetricProvider
{
    public string FullQualifiedName { get; }
    public string Description { get; }
    public void Run(IMetricCollector collectorQueue);
}
```

### **IExtension**
```csharp
public class HelloWorldExtension : IExtension
{
    public string ExtensionId { get; }
    public string DisplayName { get; }
    public string Version { get; }
    public string Description { get; }
    // Lifecycle methods...
}
```

## 📦 Installation

### **From NuGet Package**
1. Download the `.nupkg` file
2. Extract to your AppSage extensions directory
3. Run the AppSage Extension Manager

### **From Source**
```sh
# Build the extension
dotnet build BuiltInProviders\AppSage.Providers.HelloWorld\AppSage.Providers.HelloWorld.csproj

# Copy to extensions directory
xcopy /s /e "BuiltInProviders\AppSage.Providers.HelloWorld\bin\Debug\net8.0\*" "C:\MyWorkspace\Extensions\AppSage.Providers.HelloWorld\"
```

## 🚀 Usage

### **Command Line**
```sh
# Load this extension with other extensions
AppSage.Extension.exe "C:\MyWorkspace"

# Load only this extension
AppSage.Extension.exe "C:\MyWorkspace" "C:\MyWorkspace\Extensions\AppSage.Providers.HelloWorld.dll"
```

### **Expected Output**
When executed, you'll see output similar to:
```
=== AppSage Extension Manager ===
Workspace: C:\MyWorkspace
Extension Path: C:\MyWorkspace\Extensions

=== Loading Extensions ===
✓ Started extension: AppSage.Providers.HelloWorld - AppSage Hello World Extension

=== Discovering IMetricProvider Implementations ===
Scanning extension: AppSage.Providers.HelloWorld (AppSage Hello World Extension)
  ✓ Found 1 IMetricProvider implementations:
    - AppSageHelloWorldProvider

=== Executing IMetricProvider Implementations ===
▶ Executing: AppSage.Providers.HelloWorld.AppSageHelloWorldProvider
  Provider: AppSage.Providers.HelloWorld.AppSageHelloWorldProvider
  Description: Output hello world as a metric
  Version: 1.0.0.0
  ✓ Execution completed in 00:00.025
  📊 Collected 3 metrics
```

## 🔗 Dependencies

### **Host-Provided**
- `AppSage.Core` (>= 1.0.0)
- `AppSage.Extension` (>= 1.0.0)

### **Bundled**
None - this extension has minimal dependencies

### **External**
None - this extension doesn't require external packages

## 💻 Development

### **Extending This Example**
To create your own extension based on this example:

1. **Copy the project structure**
2. **Update the extension manifest** with your details
3. **Modify the IMetricProvider** to collect your specific metrics
4. **Update the README** with your extension's documentation
5. **Build and test** using the AppSage Extension Manager

### **Key Files**
- `AppSageHelloWorldProvider.cs` - Main metric provider implementation
- `HelloWorldExtension.cs` - Extension lifecycle management
- `extension.manifest.json` - Extension metadata and dependencies
- `README.md` - This documentation file

## 🎯 Use Cases

### 🎓 **Learning and Training**
- **New Developer Onboarding**: Perfect first extension to understand the system
- **Extension Development Tutorial**: Step-by-step example of all required components
- **Testing and Validation**: Simple extension for testing the extension loading system

### 🧪 **Development and Testing**
- **Extension System Validation**: Verify the extension loading mechanism works
- **Dependency Resolution Testing**: Test host-provided dependency resolution
- **Metrics Collection Testing**: Validate the metrics collection pipeline

## 🔧 Technical Details

- **Target Framework**: .NET 8.0
- **Extension System**: AppSage Extension Architecture
- **Isolation**: Runs in isolated AssemblyLoadContext
- **Dependencies**: Minimal host-provided dependencies only
- **Metrics**: Three sample metrics demonstrating different data types

## 📄 License

MIT License - See LICENSE file for details

---

*The HelloWorld extension demonstrates the simplicity and power of the AppSage extension system. Use it as a foundation for building your own sophisticated code analysis and metrics collection extensions.*