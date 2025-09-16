using AppSage.Core.ComplexType.Graph;
using AppSage.Core.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelContextProtocol;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace AppSage.MCPServer
{
    internal class DynamicCompiler:IDynamicCompiler
    {
        IAppSageLogger _logger;
        public DynamicCompiler(IAppSageLogger logger)
        {
            _logger = logger;
        }
        
        public T CompileAndExecute<T>(string code, DirectedGraph sourceGraph)
        {
            try
            {
                _logger.LogInformation($"code to complile:\r\n{code}");
                // Parse the code
                var syntaxTree = CSharpSyntaxTree.ParseText(code);
                
                // Ensure required using statements are present
                var requiredNamespaces = new[]
                {
                    "AppSage.Core.ComplexType.Graph",
                    "System",
                    "System.Collections.Generic",
                    "System.Linq",
                    "System.Data"
                };
                syntaxTree = AddUsingStatements(syntaxTree, requiredNamespaces);

                // Get references to required assemblies using a more robust approach
                var references = new List<MetadataReference>();

                // Core fundamental references - these contain the basic types like object
                references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location)); // System.Private.CoreLib
                references.Add(MetadataReference.CreateFromFile(typeof(Console).Assembly.Location)); // Console
                references.Add(MetadataReference.CreateFromFile(typeof(DirectedGraph).Assembly.Location)); // AppSage.Core
                references.Add(MetadataReference.CreateFromFile(typeof(DataTable).Assembly.Location));
                references.Add(MetadataReference.CreateFromFile(typeof(TypeConverter).Assembly.Location));
                // Add additional assemblies that might be needed
                var additionalAssemblies = new[]
                {
                    "System",
                    "System.Private.Xml",
                    "System.ComponentModel",
                    "System.ComponentModel.Primitives",
                    "System.Xml.ReaderWriter",
                    "System.Data",
                    "System.Collections",
                    "System.Collections.Generic",
                    "System.IO",
                    "System.Text",
                    "System.Collections.Concurrent",
                    "System.Threading",
                    "System.Threading.Tasks",
                    "System.ObjectModel",
                    "System.Runtime",
                    "System.Linq",
                    "netstandard"
                };
                
                foreach (var assemblyName in additionalAssemblies)
                {
                    try
                    {
                        var additionalAssembly = Assembly.Load(assemblyName);
                        references.Add(MetadataReference.CreateFromFile(additionalAssembly.Location));
                    }
                    catch
                    {
                        // Assembly doesn't exist or can't be loaded, skip it
                    }
                }

                // Add Microsoft.CSharp if available
                try
                {
                    references.Add(MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location));
                }
                catch
                {
                    // Microsoft.CSharp may not be available in all environments
                }

                // Create compilation
                var compilation = CSharpCompilation.Create(
                    assemblyName: $"DynamicQuery_{Guid.NewGuid():N}",
                    syntaxTrees: new[] { syntaxTree },
                    references: references,
                    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                // Compile to memory
                using var ms = new MemoryStream();
                var result = compilation.Emit(ms);

                if (!result.Success)
                {
                    // Log compilation errors for debugging
                    _logger.LogError("Compilation failed:");
                    StringBuilder error = new StringBuilder();
                    error.AppendLine("There are compilaation errors of the provided code. Remember to follow all the rules and recreate and resubmit the code.");
                    foreach (var diagnostic in result.Diagnostics)
                    {
                        error.AppendLine(diagnostic.ToString());
                        _logger.LogError($"  {diagnostic}");
                    }
                    throw new McpException(error.ToString());
                }

                // Load and execute the compiled assembly
                ms.Seek(0, SeekOrigin.Begin);
                var context = new AssemblyLoadContext(null, isCollectible: true);
                var compiledAssembly = context.LoadFromStream(ms);

                var type=compiledAssembly.GetTypes().Where(t=>t.Name=="MyQuery").First();

             
                var method = type.GetMethod("Execute", BindingFlags.Static | BindingFlags.Public);

                if (method == null)
                {
                    _logger.LogError("Could not find MyQuery.Execute(DirectedGraph graph) method");
                    context.Unload();
                    throw new InvalidOperationException("Can't find the method MyQuery.Execute(DirectedGraph graph).");
                }

                var executionResult = method.Invoke(null, new object[] { sourceGraph });

                context.Unload();

                return (T)executionResult;
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                _logger.LogError($"Error during dynamic compilation",ex);
                throw;
            }
        }


        /// <summary>
        /// Adds the specified using statements to the syntax tree if they don't already exist
        /// </summary>
        /// <param name="syntaxTree">The original syntax tree</param>
        /// <param name="namespaces">List of namespaces to add (without 'using' keyword)</param>
        /// <returns>Modified syntax tree with the using statements added if needed</returns>
        private SyntaxTree AddUsingStatements(SyntaxTree syntaxTree, IEnumerable<string> namespaces)
        {
            var root = syntaxTree.GetRoot() as CompilationUnitSyntax;
            if (root == null) return syntaxTree;

            // Get existing using directives
            var existingUsings = root.Usings
                .Select(u => u.Name?.ToString())
                .Where(name => !string.IsNullOrEmpty(name))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Find namespaces that need to be added
            var namespacesToAdd = namespaces
                .Where(ns => !string.IsNullOrWhiteSpace(ns) && !existingUsings.Contains(ns))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // If no new usings needed, return original tree
            if (!namespacesToAdd.Any())
            {
                return syntaxTree;
            }

            // Create new using directives with proper spacing and formatting
            var newUsings = namespacesToAdd.Select(ns =>
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(ns))
                    .WithUsingKeyword(
                        SyntaxFactory.Token(SyntaxKind.UsingKeyword)
                            .WithTrailingTrivia(SyntaxFactory.Space)) // Ensure space after "using"
                    .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed))
                .ToArray();

            // Add the new using statements to the beginning of the existing usings list
            var updatedUsings = root.Usings.InsertRange(0, newUsings);

            // Create new root with updated usings
            var newRoot = root.WithUsings(updatedUsings);

            return syntaxTree.WithRootAndOptions(newRoot, syntaxTree.Options);
        }
    }
}
