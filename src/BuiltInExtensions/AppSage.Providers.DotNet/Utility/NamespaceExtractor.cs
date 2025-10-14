using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSage.Providers.DotNet.BasicCodeAnalysis
{
    internal class NamespaceExtractor
    {
        internal static HashSet<string> GetUsedNamespaces(Document document)
        {
            var syntaxTree = document.GetSyntaxTreeAsync().Result;
            SyntaxNode root = syntaxTree.GetRoot();
            var usedNamespaces = new HashSet<string>();
            var semanticModel = document.GetSemanticModelAsync().Result;

            if (document.Project.Language == LanguageNames.CSharp)
            {
                // 1. Find all using directives
                var usingDirectives = root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax>();
                foreach (var usingDirective in usingDirectives)
                {
                    usedNamespaces.Add(usingDirective.Name.ToString());
                }

                // 2. Add namespaces DEFINED in this document (the namespace context where code resides)
                var namespaceDeclarations = root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax>();
                foreach (var namespaceDecl in namespaceDeclarations)
                {
                    var namespaceName = namespaceDecl.Name.ToString();
                    if (!string.IsNullOrEmpty(namespaceName))
                    {
                        usedNamespaces.Add(namespaceName);
                    }
                }

                var fileScopedNamespaceDeclarations = root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.FileScopedNamespaceDeclarationSyntax>();
                foreach (var fileScopedNamespaceDecl in fileScopedNamespaceDeclarations)
                {
                    var namespaceName = fileScopedNamespaceDecl.Name.ToString();
                    if (!string.IsNullOrEmpty(namespaceName))
                    {
                        usedNamespaces.Add(namespaceName);
                    }
                }

                // 3. Analyze type references in the code, but EXCLUDE namespace declarations
                if (semanticModel != null)
                {
                    // Get all namespace declaration nodes to exclude them from analysis
                    var namespaceDeclarationsForExclusion = root.DescendantNodes()
                        .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax>()
                        .ToHashSet();

                    var fileScopedNamespaceDeclarationsForExclusion = root.DescendantNodes()
                        .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.FileScopedNamespaceDeclarationSyntax>()
                        .ToHashSet();

                    // Find all type syntax nodes that represent actual type usage (not namespace declarations)
                    var allTypeNodes = root.DescendantNodes()
                        .Where(node =>
                            // Include various type syntax nodes
                            node is Microsoft.CodeAnalysis.CSharp.Syntax.TypeSyntax ||
                            node is Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax ||
                            node is Microsoft.CodeAnalysis.CSharp.Syntax.QualifiedNameSyntax ||
                            node is Microsoft.CodeAnalysis.CSharp.Syntax.GenericNameSyntax)
                        .Where(node =>
                        {
                            // EXCLUDE nodes that are part of namespace declarations
                            return !namespaceDeclarationsForExclusion.Any(ns => ns.Name.Span.Contains(node.Span)) &&
                                   !fileScopedNamespaceDeclarationsForExclusion.Any(ns => ns.Name.Span.Contains(node.Span));
                        });

                    foreach (var typeNode in allTypeNodes)
                    {
                        try
                        {
                            var symbolInfo = semanticModel.GetSymbolInfo(typeNode);

                            // Handle different symbol types
                            INamespaceSymbol containingNamespace = null;

                            if (symbolInfo.Symbol is INamedTypeSymbol namedTypeSymbol)
                            {
                                containingNamespace = namedTypeSymbol.ContainingNamespace;
                            }
                            else if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
                            {
                                containingNamespace = methodSymbol.ContainingNamespace;
                            }
                            else if (symbolInfo.Symbol is IPropertySymbol propertySymbol)
                            {
                                containingNamespace = propertySymbol.ContainingNamespace;
                            }
                            else if (symbolInfo.Symbol is IFieldSymbol fieldSymbol)
                            {
                                containingNamespace = fieldSymbol.ContainingNamespace;
                            }
                            else if (symbolInfo.Symbol is IEventSymbol eventSymbol)
                            {
                                containingNamespace = eventSymbol.ContainingNamespace;
                            }

                            // Add the namespace if it's valid and not the global namespace
                            if (containingNamespace != null &&
                                !containingNamespace.IsGlobalNamespace &&
                                !string.IsNullOrEmpty(containingNamespace.Name))
                            {
                                var namespaceName = containingNamespace.ToDisplayString();
                                usedNamespaces.Add(namespaceName);
                            }
                        }
                        catch (Exception)
                        {
                            // If semantic analysis fails, skip this node
                            continue;
                        }
                    }

                    // 4. Analyze method invocations (static method calls, instance method calls, etc.)
                    var invocations = root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax>();
                    foreach (var invocation in invocations)
                    {
                        try
                        {
                            var symbolInfo = semanticModel.GetSymbolInfo(invocation);
                            if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
                            {
                                var containingType = methodSymbol.ContainingType;
                                if (containingType != null && !containingType.ContainingNamespace.IsGlobalNamespace)
                                {
                                    var namespaceName = containingType.ContainingNamespace.ToDisplayString();
                                    usedNamespaces.Add(namespaceName);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }

                    // 5. Analyze member access expressions (static property/field access, etc.)
                    var memberAccesses = root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax>();
                    foreach (var memberAccess in memberAccesses)
                    {
                        try
                        {
                            var symbolInfo = semanticModel.GetSymbolInfo(memberAccess);
                            INamespaceSymbol containingNamespace = null;

                            if (symbolInfo.Symbol is INamedTypeSymbol typeSymbol)
                            {
                                containingNamespace = typeSymbol.ContainingNamespace;
                            }
                            else if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
                            {
                                containingNamespace = methodSymbol.ContainingType?.ContainingNamespace;
                            }
                            else if (symbolInfo.Symbol is IPropertySymbol propertySymbol)
                            {
                                containingNamespace = propertySymbol.ContainingType?.ContainingNamespace;
                            }
                            else if (symbolInfo.Symbol is IFieldSymbol fieldSymbol)
                            {
                                containingNamespace = fieldSymbol.ContainingType?.ContainingNamespace;
                            }
                            else if (symbolInfo.Symbol is IEventSymbol eventSymbol)
                            {
                                containingNamespace = eventSymbol.ContainingType?.ContainingNamespace;
                            }

                            if (containingNamespace != null && !containingNamespace.IsGlobalNamespace)
                            {
                                var namespaceName = containingNamespace.ToDisplayString();
                                usedNamespaces.Add(namespaceName);
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }

                    // 6. Analyze object creation expressions (constructor calls)
                    var objectCreations = root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ObjectCreationExpressionSyntax>();
                    foreach (var objectCreation in objectCreations)
                    {
                        try
                        {
                            var symbolInfo = semanticModel.GetSymbolInfo(objectCreation);
                            if (symbolInfo.Symbol is IMethodSymbol constructorSymbol)
                            {
                                var containingType = constructorSymbol.ContainingType;
                                if (containingType != null && !containingType.ContainingNamespace.IsGlobalNamespace)
                                {
                                    var namespaceName = containingType.ContainingNamespace.ToDisplayString();
                                    usedNamespaces.Add(namespaceName);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }

                    // 7. Analyze attribute usage (like [System.Diagnostics.CodeAnalysis.SuppressMessage])
                    var attributeLists = root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax>();
                    foreach (var attributeList in attributeLists)
                    {
                        foreach (var attribute in attributeList.Attributes)
                        {
                            try
                            {
                                var symbolInfo = semanticModel.GetSymbolInfo(attribute);
                                if (symbolInfo.Symbol is IMethodSymbol attributeConstructor &&
                                    attributeConstructor.ContainingType != null)
                                {
                                    var containingNamespace = attributeConstructor.ContainingType.ContainingNamespace;
                                    if (containingNamespace != null && !containingNamespace.IsGlobalNamespace)
                                    {
                                        var namespaceName = containingNamespace.ToDisplayString();
                                        usedNamespaces.Add(namespaceName);
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                        }
                    }

                    // 8. Analyze variable declarations and their types
                    var variableDeclarations = root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax>();
                    foreach (var varDecl in variableDeclarations)
                    {
                        try
                        {
                            var typeInfo = semanticModel.GetTypeInfo(varDecl.Type);
                            AddTypeNamespaces(typeInfo.Type, usedNamespaces);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }

                    // 9. Analyze cast expressions
                    var castExpressions = root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.CastExpressionSyntax>();
                    foreach (var castExpr in castExpressions)
                    {
                        try
                        {
                            var typeInfo = semanticModel.GetTypeInfo(castExpr.Type);
                            AddTypeNamespaces(typeInfo.Type, usedNamespaces);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }

                    // 10. Analyze 'typeof' expressions
                    var typeofExpressions = root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.TypeOfExpressionSyntax>();
                    foreach (var typeofExpr in typeofExpressions)
                    {
                        try
                        {
                            var typeInfo = semanticModel.GetTypeInfo(typeofExpr.Type);
                            AddTypeNamespaces(typeInfo.Type, usedNamespaces);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
            }
            else if (document.Project.Language == LanguageNames.VisualBasic)
            {
                // 1. Find all imports statements in VB.NET
                var importsStatements = root.DescendantNodes().OfType<Microsoft.CodeAnalysis.VisualBasic.Syntax.ImportsStatementSyntax>();
                foreach (var importsStatement in importsStatements)
                {
                    foreach (var importsClause in importsStatement.ImportsClauses)
                    {
                        if (importsClause is Microsoft.CodeAnalysis.VisualBasic.Syntax.SimpleImportsClauseSyntax simpleImports)
                        {
                            usedNamespaces.Add(simpleImports.Name.ToString());
                        }
                    }
                }

                // 2. Add namespaces DEFINED in this VB.NET document
                var namespaceBlocks = root.DescendantNodes().OfType<Microsoft.CodeAnalysis.VisualBasic.Syntax.NamespaceBlockSyntax>();
                foreach (var namespaceBlock in namespaceBlocks)
                {
                    var namespaceName = namespaceBlock.NamespaceStatement.Name.ToString();
                    if (!string.IsNullOrEmpty(namespaceName))
                    {
                        usedNamespaces.Add(namespaceName);
                    }
                }

                // 3. Analyze type references in VB.NET, excluding namespace declarations
                if (semanticModel != null)
                {
                    var namespaceBlocksForExclusion = root.DescendantNodes()
                        .OfType<Microsoft.CodeAnalysis.VisualBasic.Syntax.NamespaceBlockSyntax>()
                        .ToHashSet();

                    var allTypeNodes = root.DescendantNodes()
                        .Where(node =>
                            node is Microsoft.CodeAnalysis.VisualBasic.Syntax.TypeSyntax ||
                            node is Microsoft.CodeAnalysis.VisualBasic.Syntax.IdentifierNameSyntax ||
                            node is Microsoft.CodeAnalysis.VisualBasic.Syntax.QualifiedNameSyntax ||
                            node is Microsoft.CodeAnalysis.VisualBasic.Syntax.GenericNameSyntax)
                        .Where(node =>
                        {
                            // EXCLUDE nodes that are part of namespace declarations
                            return !namespaceBlocksForExclusion.Any(ns => ns.NamespaceStatement.Name.Span.Contains(node.Span));
                        });

                    foreach (var typeNode in allTypeNodes)
                    {
                        try
                        {
                            var symbolInfo = semanticModel.GetSymbolInfo(typeNode);

                            INamespaceSymbol containingNamespace = null;

                            if (symbolInfo.Symbol is INamedTypeSymbol namedTypeSymbol)
                            {
                                containingNamespace = namedTypeSymbol.ContainingNamespace;
                            }
                            else if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
                            {
                                containingNamespace = methodSymbol.ContainingNamespace;
                            }
                            else if (symbolInfo.Symbol is IPropertySymbol propertySymbol)
                            {
                                containingNamespace = propertySymbol.ContainingNamespace;
                            }
                            else if (symbolInfo.Symbol is IFieldSymbol fieldSymbol)
                            {
                                containingNamespace = fieldSymbol.ContainingNamespace;
                            }

                            if (containingNamespace != null &&
                                !containingNamespace.IsGlobalNamespace &&
                                !string.IsNullOrEmpty(containingNamespace.Name))
                            {
                                var namespaceName = containingNamespace.ToDisplayString();
                                usedNamespaces.Add(namespaceName);
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }

                    // Also analyze VB.NET specific invocations and member accesses
                    var invocations = root.DescendantNodes().OfType<Microsoft.CodeAnalysis.VisualBasic.Syntax.InvocationExpressionSyntax>();
                    foreach (var invocation in invocations)
                    {
                        try
                        {
                            var symbolInfo = semanticModel.GetSymbolInfo(invocation);
                            if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
                            {
                                var containingType = methodSymbol.ContainingType;
                                if (containingType != null && !containingType.ContainingNamespace.IsGlobalNamespace)
                                {
                                    var namespaceName = containingType.ContainingNamespace.ToDisplayString();
                                    usedNamespaces.Add(namespaceName);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }

                    var memberAccesses = root.DescendantNodes().OfType<Microsoft.CodeAnalysis.VisualBasic.Syntax.MemberAccessExpressionSyntax>();
                    foreach (var memberAccess in memberAccesses)
                    {
                        try
                        {
                            var symbolInfo = semanticModel.GetSymbolInfo(memberAccess);
                            if (symbolInfo.Symbol is INamedTypeSymbol typeSymbol)
                            {
                                var containingNamespace = typeSymbol.ContainingNamespace;
                                if (containingNamespace != null && !containingNamespace.IsGlobalNamespace)
                                {
                                    var namespaceName = containingNamespace.ToDisplayString();
                                    usedNamespaces.Add(namespaceName);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
            }
            else if (document.Project.Language == LanguageNames.FSharp)
            {
                // F# uses 'open' statements for importing namespaces
                var sourceText = syntaxTree.GetText().ToString();
                var lines = sourceText.Split('\n');

                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    if (trimmedLine.StartsWith("open ") && !trimmedLine.StartsWith("open type"))
                    {
                        var namespaceName = trimmedLine.Substring(5).Trim();
                        // Remove any comments
                        var commentIndex = namespaceName.IndexOf("//");
                        if (commentIndex >= 0)
                        {
                            namespaceName = namespaceName.Substring(0, commentIndex).Trim();
                        }
                        if (!string.IsNullOrEmpty(namespaceName))
                        {
                            usedNamespaces.Add(namespaceName);
                        }
                    }
                }

                // Add F# module/namespace declarations
                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    if (trimmedLine.StartsWith("namespace "))
                    {
                        var namespaceName = trimmedLine.Substring(10).Trim();
                        // Remove any comments
                        var commentIndex = namespaceName.IndexOf("//");
                        if (commentIndex >= 0)
                        {
                            namespaceName = namespaceName.Substring(0, commentIndex).Trim();
                        }
                        if (!string.IsNullOrEmpty(namespaceName))
                        {
                            usedNamespaces.Add(namespaceName);
                        }
                    }
                }
            }

            return usedNamespaces;
        }

        private static void AddTypeNamespaces(ITypeSymbol typeSymbol, HashSet<string> usedNamespaces)
        {
            if (typeSymbol == null) return;

            // Handle the main type's namespace
            if (typeSymbol.ContainingNamespace != null && 
                !typeSymbol.ContainingNamespace.IsGlobalNamespace && 
                !string.IsNullOrEmpty(typeSymbol.ContainingNamespace.Name))
            {
                usedNamespaces.Add(typeSymbol.ContainingNamespace.ToDisplayString());
            }

            // Recursively handle different type constructs
            switch (typeSymbol)
            {
                case IArrayTypeSymbol arrayType:
                    // For arrays like string[], List<int>[]
                    AddTypeNamespaces(arrayType.ElementType, usedNamespaces);
                    break;

                case INamedTypeSymbol namedType when namedType.IsGenericType:
                    // For generics like List<string>, Dictionary<string, List<int>>, List<List<string>>
                    foreach (var typeArg in namedType.TypeArguments)
                    {
                        AddTypeNamespaces(typeArg, usedNamespaces);
                    }
                    break;

                case IPointerTypeSymbol pointerType:
                    // For pointer types like int*
                    AddTypeNamespaces(pointerType.PointedAtType, usedNamespaces);
                    break;

                case IFunctionPointerTypeSymbol functionPointer:
                    // For function pointers (C# 9+)
                    AddTypeNamespaces(functionPointer.Signature.ReturnType, usedNamespaces);
                    foreach (var param in functionPointer.Signature.Parameters)
                    {
                        AddTypeNamespaces(param.Type, usedNamespaces);
                    }
                    break;

                // Nullable reference types (string?) are automatically handled
                // as they preserve the underlying type's namespace
            }
        }
    }
}
