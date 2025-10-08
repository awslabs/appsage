using AppSage.Core.Configuration;
using AppSage.Core.Logging;
using AppSage.Providers.DotNet.BasicCodeAnalysis;
using Microsoft.CodeAnalysis;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace AppSage.Providers.DotNet.Utility
{
    internal class ProjectAnalyzer
    {
        IAppSageLogger _logger;
        LinesOfCodeCounter _linesOfCodeCounter;
        int _DocumentMaxParallelism;

        internal ProjectAnalyzer(IAppSageLogger logger, IAppSageConfiguration config)
        {
            _logger = logger;
            _linesOfCodeCounter = new LinesOfCodeCounter(_logger);
            _DocumentMaxParallelism = config.Get<int>("AppSage.Providers.DotNet.Utility:DocumentMaxParallelism");
        }

        /// <summary>
        /// Gets the .NET version (TargetFramework) from a .NET project file.
        /// </summary>
        /// <param name="projectFilePath">Path to the proj file</param>
        /// <returns>.NET version</returns>
        internal string ParseDotNetVersion(string projectFilePath)
        {
            try
            {
                if (File.Exists(projectFilePath))
                {
                    var doc = System.Xml.Linq.XDocument.Load(projectFilePath);

                    // Look for TargetFramework property in the project file
                    var targetFrameworkElement = doc.Descendants()
                        .Where(e => e.Name.LocalName == "TargetFramework")
                        .FirstOrDefault();

                    if (targetFrameworkElement != null)
                    {
                        return targetFrameworkElement.Value;
                    }

                    // Also look for TargetFrameworkVersion as fallback
                    var targetFrameworkVersionElement = doc.Descendants()
                        .Where(e => e.Name.LocalName == "TargetFrameworkVersion")
                        .FirstOrDefault();

                    if (targetFrameworkVersionElement != null)
                    {
                        return targetFrameworkVersionElement.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving .NET version for project {ProjectFilePath}", ex, projectFilePath);
            }

            return ConstString.UNDEFINED;
        }
        /// <summary>
        /// Retrieves all NuGet package references, .NET project references, dll references from a .NET project file.
        /// </summary>
        /// <param name="projectFilePath"></param>
        /// <returns></returns>
        internal List<(string Name, string Version, string Category, string ReferenceType)> GetReferences(string projectFilePath)
        {
            var result = new List<(string Name, string Version, string Category, string ReferenceType)>();

            try
            {
                var doc = System.Xml.Linq.XDocument.Load(projectFilePath);

                // 1. Extract PackageReference elements (NuGet packages in SDK-style projects)
                var packageReferences = doc.Descendants().Where(r => r.Name.LocalName == "PackageReference");
                foreach (var packageRef in packageReferences)
                {
                    var packageName = packageRef.Attribute("Include")?.Value;
                    var packageVersion = packageRef.Attribute("Version")?.Value;

                    // Also check for Version element if not in attribute
                    if (string.IsNullOrEmpty(packageVersion))
                    {
                        packageVersion = packageRef.Element("Version")?.Value;
                    }

                    if (!string.IsNullOrEmpty(packageName))
                    {
                        string rName = packageName;
                        string rVersion = packageVersion ?? ConstString.UNDEFINED;
                        string rCategory = IsFrameworkPackage(packageName) ? ConstString.FRAMEWORK : ConstString.OTHER;
                        string rReferenceType = ConstString.ExternalReference.ReferenceType.NUGET;

                        result.Add((rName, rVersion, rCategory, rReferenceType));
                    }
                    else
                    {
                        _logger.LogWarning("PackageReference element in {ProjectFilePath} does not have Include attribute.", projectFilePath);
                    }
                }

                // 2. Extract ClassReferences elements (assembly references in traditional .NET Framework projects)
                var references = doc.Descendants().Where(r => r.Name.LocalName == "ClassReferences");
                foreach (var reference in references)
                {
                    var include = reference.Attribute("Include")?.Value;
                    var hintPath = reference.Element("HintPath")?.Value;
                    var specificVersion = reference.Element("SpecificVersion")?.Value;

                    if (!string.IsNullOrEmpty(include))
                    {
                        // Parse the Include attribute which can contain assembly name, version, culture, etc.
                        // Format: "AssemblyName, Version=x.x.x.x, Culture=neutral, PublicKeyToken=..."
                        var parts = include.Split(',');
                        string name = parts[0].Trim();
                        string version = ConstString.UNDEFINED;

                        // Try to extract version from the Include attribute
                        var versionPart = parts.FirstOrDefault(p => p.Trim().StartsWith("Version=", StringComparison.OrdinalIgnoreCase));
                        if (versionPart != null && versionPart.Contains('='))
                        {
                            version = versionPart.Split('=')[1].Trim();
                        }

                        if (!string.IsNullOrEmpty(name))
                        {
                            string rName = name;
                            string rVersion = version;
                            string rCategory = IsFrameworkPackage(name) ? ConstString.FRAMEWORK : ConstString.OTHER;
                            string rReferenceType = IsNuGetReference(hintPath) ? ConstString.ExternalReference.ReferenceType.NUGET : ConstString.ExternalReference.ReferenceType.DLL;

                            result.Add((rName, rVersion, rCategory, rReferenceType));
                        }
                    }
                }

                // 3. Extract ExternalReference elements (get all ExternalReference nodes regardless of the namespace)
                var externalReferences = doc.Descendants().Where(r => r.Name.LocalName == "ExternalReference");
                foreach (var externalRef in externalReferences)
                {
                    var hintPath = externalRef.Elements().FirstOrDefault(e => e.Name.LocalName == "HintPath")?.Value;
                    var include = externalRef.Attribute("Include")?.Value;

                    if (!string.IsNullOrEmpty(include))
                    {
                        var parts = include.Split(',');
                        string name = parts[0].Trim();
                        string version = parts.FirstOrDefault(p => p.Trim().StartsWith("Version=", StringComparison.OrdinalIgnoreCase))?.Split('=')[1].Trim();

                        if (!string.IsNullOrEmpty(name))
                        {
                            string rName = name;
                            string rVersion = version ?? ConstString.UNDEFINED;
                            // Determine if it's a framework package or other
                            string rCategory = IsFrameworkPackage(name) ? ConstString.FRAMEWORK : ConstString.OTHER;
                            string rReferenceType = IsNuGetReference(hintPath) ? ConstString.ExternalReference.ReferenceType.NUGET : ConstString.ExternalReference.ReferenceType.DLL;

                            result.Add((rName, rVersion, rCategory, rReferenceType));
                        }
                    }
                }

                // 4. Extract ProjectReference elements (project-to-project references)
                var projectReferences = doc.Descendants().Where(r => r.Name.LocalName == "ProjectReference");
                foreach (var projectRef in projectReferences)
                {
                    var include = projectRef.Attribute("Include")?.Value;
                    var name = projectRef.Element("ClassName")?.Value;
                    var projectGuid = projectRef.Element("Project")?.Value;

                    if (!string.IsNullOrEmpty(include))
                    {
                        // Use the ClassName element if available, otherwise extract from file path
                        string projectName = name ?? Path.GetFileNameWithoutExtension(include);

                        if (!string.IsNullOrEmpty(projectName))
                        {
                            string rName = projectName;
                            string rVersion = ConstString.UNDEFINED; // Project references typically don't have versions
                            string rCategory = ConstString.OTHER; // Project references are not framework packages
                            string rReferenceType = ConstString.ExternalReference.ReferenceType.PROJECT;

                            result.Add((rName, rVersion, rCategory, rReferenceType));
                        }
                    }
                }

                // 5. Extract COMReference elements (COM component references)
                var comReferences = doc.Descendants().Where(r => r.Name.LocalName == "COMReference");
                foreach (var comRef in comReferences)
                {
                    var include = comRef.Attribute("Include")?.Value;
                    var guid = comRef.Element("Guid")?.Value;
                    var versionMajor = comRef.Element("VersionMajor")?.Value;
                    var versionMinor = comRef.Element("VersionMinor")?.Value;
                    var lcid = comRef.Element("Lcid")?.Value;
                    var wrapperTool = comRef.Element("WrapperTool")?.Value;

                    if (!string.IsNullOrEmpty(include))
                    {
                        string version = ConstString.UNDEFINED;
                        if (!string.IsNullOrEmpty(versionMajor) && !string.IsNullOrEmpty(versionMinor))
                        {
                            version = $"{versionMajor}.{versionMinor}";
                        }

                        result.Add((include, version, ConstString.OTHER, "COM"));
                    }
                }

                // 6. Extract BootstrapperPackage elements (bootstrapper references)
                var bootstrapperPackages = doc.Descendants().Where(r => r.Name.LocalName == "BootstrapperPackage");
                foreach (var bootstrapper in bootstrapperPackages)
                {
                    var include = bootstrapper.Attribute("Include")?.Value;
                    if (!string.IsNullOrEmpty(include))
                    {
                        result.Add((include, ConstString.UNDEFINED, ConstString.OTHER, "Bootstrapper"));
                    }
                }

                // 7. Extract WebReferences elements (legacy web service references)
                var webReferences = doc.Descendants().Where(r => r.Name.LocalName == "WebReferences");
                foreach (var webRef in webReferences)
                {
                    var include = webRef.Attribute("Include")?.Value;
                    if (!string.IsNullOrEmpty(include))
                    {
                        result.Add((include, ConstString.UNDEFINED, ConstString.OTHER, "WebReference"));
                    }
                }

                // 8. Extract ServiceReference elements (WCF service references)
                var serviceReferences = doc.Descendants().Where(r => r.Name.LocalName == "ServiceReference");
                foreach (var serviceRef in serviceReferences)
                {
                    var include = serviceRef.Attribute("Include")?.Value;
                    if (!string.IsNullOrEmpty(include))
                    {
                        result.Add((include, ConstString.UNDEFINED, ConstString.OTHER, "ServiceReference"));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving references for project {ProjectFilePath}", ex, projectFilePath);
            }

            return result;
        }

        internal bool IsNuGetReference(string hintPath)
        {
            return hintPath != null &&
                   (hintPath.Contains(@"\packages\") ||
                    hintPath.Contains(@"\.nuget\packages\"));
        }
        internal bool IsFrameworkPackage(string packageName)
        {
            // Handle null or empty package names
            if (string.IsNullOrWhiteSpace(packageName))
                return false;

            // List of common Microsoft/.NET framework packages
            var frameworkPackages = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                // Microsoft core packages
                "Microsoft.",
                "System.",
                "NETStandard.",
                // Common .NET Framework assemblies
                "mscorlib",
                "WindowsBase",
                "PresentationCore",
                "PresentationFramework",
                "WindowsFormsIntegration"
            };

            // Check if the package name starts with any of the framework package prefixes
            // or exactly matches framework assembly names
            return frameworkPackages.Any(fp =>
                fp.EndsWith(".") ? packageName.StartsWith(fp, StringComparison.OrdinalIgnoreCase)
                                : packageName.Equals(fp, StringComparison.OrdinalIgnoreCase)
                );
        }

        internal string GetProjectType(Project project)
        {
            // Check if CompilationOptions is available
            var compilationOptions = project.CompilationOptions;

            if (compilationOptions != null)
            {
                // Determine the output kind
                switch (compilationOptions.OutputKind)
                {
                    case OutputKind.ConsoleApplication:
                        return "Console Application";
                    case OutputKind.WindowsApplication:
                        return "Windows Application";
                    case OutputKind.DynamicallyLinkedLibrary:
                        return "Class Library (DLL)";
                    case OutputKind.NetModule:
                        return "Net Module";
                    case OutputKind.WindowsRuntimeApplication:
                        return "Windows Runtime Application";
                    case OutputKind.WindowsRuntimeMetadata:
                        return "Windows Runtime Metadata";
                    default:
                        return "Unknown Project Type";
                }
            }

            return "Unknown Project Type";
        }

        /// <summary>
        /// Due to C# partial class/partial struct/partial interface/partial record, the content of a named type can be spread across multiple documents. 
        /// We need to analyze all documents before we can get the accurate statistics. 
        /// This method will analyze all documents in parallel, account for partial types and return the aggregated summary.
        /// </summary>
        /// <param name="documentList"></param>
        /// <returns></returns>
        internal DocumentSummary GetDocumentSummary(IEnumerable<Document> documentList)
        {

            //Note that we need to account for partial types. We assume partial types are only spread within the same project. 
            //Therefore we need to analyze all documents before we can count unique types. 
            var namedTypeInfoSet = new Dictionary<string, NameTypeSummary>();
            int methodCount = 0;
            int totalLinesOfCode = 0;

            documentList.Where(d => d.SourceCodeKind == SourceCodeKind.Regular).AsParallel().WithDegreeOfParallelism(_DocumentMaxParallelism).ForAll(document =>
            {
                _logger.LogInformation("Processing document: {DocumentFilePath}", document.FilePath);

                // Perform static code analysis

                var root = document.GetSyntaxTreeAsync().Result.GetRoot();
                var semanticModel = document.GetSemanticModelAsync().Result;

                int documentLinesOfCode = _linesOfCodeCounter.GetLinesOfCode(document);
                Interlocked.Add(ref totalLinesOfCode, documentLinesOfCode);

                var usedNamespaces = NamespaceExtractor.GetUsedNamespaces(document);

                if (document.Project.Language == LanguageNames.CSharp)
                {
                    // Find all type declarations: classes, interfaces, structs, and records
                    var typeDeclarations = root.DescendantNodes()
                        .Where(node => node is Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax ||
                                      node is Microsoft.CodeAnalysis.CSharp.Syntax.InterfaceDeclarationSyntax ||
                                      node is Microsoft.CodeAnalysis.CSharp.Syntax.StructDeclarationSyntax ||
                                      node is Microsoft.CodeAnalysis.CSharp.Syntax.RecordDeclarationSyntax);

                    foreach (var typeDeclaration in typeDeclarations)
                    {
                        var symbol = semanticModel.GetDeclaredSymbol(typeDeclaration) as INamedTypeSymbol;
                        if (symbol != null)
                        {
                            lock (namedTypeInfoSet)
                            {
                                string fullQualifiedName = symbol.ToDisplayString();
                                if (!namedTypeInfoSet.ContainsKey(fullQualifiedName))
                                {
                                    namedTypeInfoSet.Add(fullQualifiedName, new NameTypeSummary());
                                }
                                var currentTypeInfo = namedTypeInfoSet[fullQualifiedName];

                                // Get method declarations for each type
                                var methodDeclarations = GetMethodDeclarationsFromType(typeDeclaration);

                                currentTypeInfo.MethodCount += methodDeclarations.Count();
                                currentTypeInfo.LinesOfCode += GetLinesOfCodeFromSyntaxNode(typeDeclaration);
                                currentTypeInfo.References.UnionWith(usedNamespaces);

                                foreach (var methodDeclaration in methodDeclarations)
                                {
                                    string methodName = GetMethodName(methodDeclaration);
                                    int linesOfCode = GetLinesOfCodeFromSyntaxNode(methodDeclaration);
                                    int numberOfParameters = GetParameterCount(methodDeclaration);
                                    
                                    if (!currentTypeInfo.MethodInfo.ContainsKey(methodName))
                                    {
                                        var methodSummary = new MethodSummary
                                        {
                                            LinesOfCode = linesOfCode,
                                            NumberOfParameters = numberOfParameters
                                        };
                                        currentTypeInfo.MethodInfo.Add(methodName, methodSummary);
                                    }
                                    else
                                    {
                                        // If the method already exists, we can take the max of parameters and sum up lines of code
                                        var existingMethodInfo = currentTypeInfo.MethodInfo[methodName];
                                        existingMethodInfo.LinesOfCode += linesOfCode;
                                        existingMethodInfo.NumberOfParameters = Math.Max(numberOfParameters, existingMethodInfo.NumberOfParameters);
                                        currentTypeInfo.MethodInfo[methodName] = existingMethodInfo;
                                    }
                                }

                                namedTypeInfoSet[fullQualifiedName] = currentTypeInfo;
                            }
                        }
                    }

                    //Find all method declarations
                    var documentMethodDeclarations = root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>();
                    Interlocked.Add(ref methodCount, documentMethodDeclarations.Count());
                }
                else
                {
                    _logger.LogWarning("Unsupported language: {Language}", document.Project.Language);
                }

            });

            int namedTypeCount = namedTypeInfoSet.Keys.Count;
            int dataOnlyNameTypeCount = namedTypeInfoSet.Count(kv => kv.Value.MethodCount == 0); //Assuming data-only types have no methods

            DocumentSummary summary = new DocumentSummary
            {
                NameTypeCount = namedTypeCount,
                MethodCount = methodCount,
                DataOnlyNameTypeCount = dataOnlyNameTypeCount,
                LinesOfCode = totalLinesOfCode,
                NameTypeInfo = namedTypeInfoSet
            };

            return summary;
        }

        /// <summary>
        /// Gets method declarations from different types of type declarations
        /// </summary>
        private IEnumerable<SyntaxNode> GetMethodDeclarationsFromType(SyntaxNode typeDeclaration)
        {
            switch (typeDeclaration)
            {
                case Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax classDecl:
                    return classDecl.Members.OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>();
                
                case Microsoft.CodeAnalysis.CSharp.Syntax.InterfaceDeclarationSyntax interfaceDecl:
                    return interfaceDecl.Members.OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>();
                
                case Microsoft.CodeAnalysis.CSharp.Syntax.StructDeclarationSyntax structDecl:
                    return structDecl.Members.OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>();
                
                case Microsoft.CodeAnalysis.CSharp.Syntax.RecordDeclarationSyntax recordDecl:
                    return recordDecl.Members.OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>();
                
                default:
                    return Enumerable.Empty<SyntaxNode>();
            }
        }

        /// <summary>
        /// Gets the method name from a method declaration
        /// </summary>
        private string GetMethodName(SyntaxNode methodDeclaration)
        {
            if (methodDeclaration is Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax methodDecl)
            {
                return methodDecl.Identifier.Text;
            }
            return "Unknown";
        }

        /// <summary>
        /// Gets the parameter count from a method declaration
        /// </summary>
        private int GetParameterCount(SyntaxNode methodDeclaration)
        {
            if (methodDeclaration is Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax methodDecl)
            {
                return methodDecl.ParameterList.Parameters.Count;
            }
            return 0;
        }

        /// <summary>
        /// Gets lines of code from any syntax node using the lines of code counter
        /// </summary>
        private int GetLinesOfCodeFromSyntaxNode(SyntaxNode syntaxNode)
        {
            // For specific known types, use the specialized methods
            switch (syntaxNode)
            {
                case Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax classDecl:
                    return _linesOfCodeCounter.GetLinesOfCode(classDecl);
                
                case Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax methodDecl:
                    return _linesOfCodeCounter.GetLinesOfCode(methodDecl);
                
                default:
                    // Fallback: simple line count based on span
                    var text = syntaxNode.SyntaxTree.GetText();
                    var lines = text.Lines;
                    var startLine = lines.GetLineFromPosition(syntaxNode.SpanStart).LineNumber;
                    var endLine = lines.GetLineFromPosition(syntaxNode.Span.End).LineNumber;
                    return endLine - startLine + 1;
            }
        }
    }

    internal record DocumentSummary
    {
        public int NameTypeCount { get; set; }
        public int MethodCount { get; set; }

        /// <summary>
        /// The name type does not contain any method, assuming it's a data class/struct/enum/interface/record
        /// </summary>
        public int DataOnlyNameTypeCount { get; set; }
        public int LinesOfCode { get; set; }
        public Dictionary<string, NameTypeSummary> NameTypeInfo { get; set; }
    }

    internal record NameTypeSummary
    {
        public int MethodCount { get; set; }
        public int LinesOfCode { get; set; }
        public HashSet<string> References = new();
        public Dictionary<string, MethodSummary> MethodInfo = new();
    }

    internal record MethodSummary
    {
        public int LinesOfCode { get; set; }
        public int NumberOfParameters { get; set; }
    }
}
