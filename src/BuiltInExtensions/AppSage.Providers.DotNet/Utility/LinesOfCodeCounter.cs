//An implementation of IGitRepoProvider
using AppSage.Core.Logging;
using Microsoft.CodeAnalysis;
namespace AppSage.Providers.DotNet.Utility
{
    internal class LinesOfCodeCounter
    {
        IAppSageLogger _logger;
        public LinesOfCodeCounter(IAppSageLogger logger)
        {
            _logger = logger;
        }

        public int GetLinesOfCode(Document document)
        {
            int linesOfCode = 0;

            try
            {
                if (document.SourceCodeKind != SourceCodeKind.Regular)
                {
                    return 0;
                }

                var sourceText = document.GetTextAsync().Result;
                if (sourceText == null)
                {
                    return 0;
                }

                var syntaxTree = document.GetSyntaxTreeAsync().Result;
                if (syntaxTree == null)
                {
                    return 0;
                }

                var language = document.Project.Language;
                var lines = sourceText.Lines;

                foreach (var line in lines)
                {
                    var lineText = sourceText.ToString(line.Span).Trim();

                    // Skip empty lines or whitespace-only lines
                    if (string.IsNullOrWhiteSpace(lineText))
                        continue;

                    // Check if line is a comment based on language
                    if (IsCommentLine(lineText, language))
                        continue;

                    // Check if line is within a multi-line comment or string literal
                    if (IsWithinMultiLineComment(line.Start, syntaxTree, language))
                        continue;

                    linesOfCode++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error counting lines of code for document {DocumentName}: {ErrorMessage}", document.Name, ex.Message);
            }

            return linesOfCode;
        }

        public int GetLinesOfCode(Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax syntax)
        {
            return GetLinesOfCodeFromSyntaxNode(syntax, LanguageNames.CSharp);
        }

        public int GetLinesOfCode(Microsoft.CodeAnalysis.VisualBasic.Syntax.ClassBlockSyntax syntax)
        {
            return GetLinesOfCodeFromSyntaxNode(syntax, LanguageNames.VisualBasic);
        }

        public int GetLinesOfCode(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax syntax)
        {
            return GetLinesOfCodeFromSyntaxNode(syntax, LanguageNames.CSharp);
        }

        public int GetLinesOfCode(Microsoft.CodeAnalysis.VisualBasic.Syntax.MethodBlockSyntax syntax)
        {
            return GetLinesOfCodeFromSyntaxNode(syntax, LanguageNames.VisualBasic);
        }

        private int GetLinesOfCodeFromSyntaxNode(SyntaxNode syntaxNode, string language)
        {
            int totalLinesOfCode = 0;
            try
            {
                if (syntaxNode?.SyntaxTree == null)
                {
                    return 0;
                }

                var sourceText = syntaxNode.SyntaxTree.GetText();
                var span = syntaxNode.Span;
                
                // Get the line span for this specific syntax node
                var lineSpan = sourceText.Lines.GetLinePositionSpan(span);
                var startLine = lineSpan.Start.Line;
                var endLine = lineSpan.End.Line;

                // Count lines only within the syntax node's span
                for (int lineIndex = startLine; lineIndex <= endLine; lineIndex++)
                {
                    var line = sourceText.Lines[lineIndex];
                    var lineText = sourceText.ToString(line.Span).Trim();

                    // Skip empty lines or whitespace-only lines
                    if (string.IsNullOrWhiteSpace(lineText))
                        continue;

                    // Check if line is a comment
                    if (IsCommentLine(lineText, language))
                        continue;

                    // Check if line is within a multi-line comment or string literal
                    if (IsWithinMultiLineComment(line.Start, syntaxNode.SyntaxTree, language))
                        continue;

                    totalLinesOfCode++;
                }
            }
            catch (Exception ex)
            {
                var identifier = syntaxNode switch
                {
                    Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax cls => cls.Identifier.Text,
                    Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax method => method.Identifier.Text,
                    Microsoft.CodeAnalysis.VisualBasic.Syntax.ClassBlockSyntax vbCls => vbCls.ClassStatement.Identifier.ValueText,
                    Microsoft.CodeAnalysis.VisualBasic.Syntax.MethodBlockSyntax vbMethod => vbMethod.SubOrFunctionStatement.Identifier.ValueText,
                    _ => "Unknown"
                };
                _logger.LogError("Error counting lines of code for {Identifier}: {ErrorMessage}", identifier, ex.Message);
            }
            return totalLinesOfCode;
        }

        private bool IsCommentLine(string lineText, string language)
        {
            return language switch
            {
                LanguageNames.CSharp => lineText.StartsWith("//") || lineText.StartsWith("///"),
                LanguageNames.VisualBasic => lineText.StartsWith("'") || lineText.StartsWith("'''") || lineText.StartsWith("Rem ", StringComparison.OrdinalIgnoreCase),
                LanguageNames.FSharp => lineText.StartsWith("//") || lineText.StartsWith("///") || lineText.StartsWith("(*") || lineText.EndsWith("*)"),
                _ => lineText.StartsWith("//") || lineText.StartsWith("///") // Default to C# style
            };
        }

        private bool IsWithinMultiLineComment(int position, SyntaxTree syntaxTree, string language)
        {
            try
            {
                var root = syntaxTree.GetRoot();
                var token = root.FindToken(position);

                // Check if position is within a comment trivia
                var leadingTrivia = token.LeadingTrivia;
                var trailingTrivia = token.TrailingTrivia;

                foreach (var trivia in leadingTrivia.Concat(trailingTrivia))
                {
                    if (IsMultiLineCommentTrivia(trivia, language) && trivia.Span.Contains(position))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Error checking multi-line comment: {ErrorMessage}", ex.Message);
                return false;
            }
        }

        private bool IsMultiLineCommentTrivia(SyntaxTrivia trivia, string language)
        {
            return language switch
            {
                LanguageNames.CSharp => trivia.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.MultiLineCommentTrivia) ||
                                       trivia.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.MultiLineDocumentationCommentTrivia),
                LanguageNames.VisualBasic => trivia.IsKind(Microsoft.CodeAnalysis.VisualBasic.SyntaxKind.CommentTrivia),
                LanguageNames.FSharp => trivia.ToString().StartsWith("(*") && trivia.ToString().EndsWith("*)"),
                _ => trivia.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.MultiLineCommentTrivia)
            };
        }
    }
}
