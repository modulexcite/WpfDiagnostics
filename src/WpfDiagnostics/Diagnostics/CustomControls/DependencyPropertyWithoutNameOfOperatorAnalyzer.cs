using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace WpfDiagnostics.Diagnostics.CustomControls
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DependencyPropertyWithoutNameOfOperatorAnalyzer : DiagnosticAnalyzer
    {
        private const string DiagnosticId = nameof(DependencyPropertyWithoutNameOfOperatorAnalyzer);
        private const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        private static readonly string Category = "Custom Controls";
        private static readonly string Message = "Dependency property '{0}' can use nameof() operator for DependencyProperty.Register() call";
        private static readonly string Title = "Use nameof";

        internal static DiagnosticDescriptor Rule => new DiagnosticDescriptor(DiagnosticId, Title, Message, Category, Severity, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeInvocationExpression, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context)
        {
            var invocationNode = (InvocationExpressionSyntax)context.Node;
            var memberAccessNode = (MemberAccessExpressionSyntax)invocationNode.Expression;
            var typeNameNode = (IdentifierNameSyntax) memberAccessNode.Expression;
            var methodNameNode = (IdentifierNameSyntax) memberAccessNode.Name;
            if (typeNameNode.Identifier.Text == "DependencyProperty" && methodNameNode.Identifier.Text == "Register")
            {
                var firstArgumentNode = invocationNode.ArgumentList.Arguments.First();
                var firstArgumentExpressionNode = firstArgumentNode.Expression;
                if (!IsNameOf(firstArgumentExpressionNode))
                {
                    var depPropName = context.SemanticModel.GetConstantValue(firstArgumentExpressionNode).Value;
                    context.ReportDiagnostic(Diagnostic.Create(Rule, firstArgumentNode.GetLocation(), depPropName));
                }
            }
        }

        private static bool IsNameOf(ExpressionSyntax firstArgumentExpressionNode)
        {
            return firstArgumentExpressionNode.Kind() == SyntaxKind.InvocationExpression
                   && ((InvocationExpressionSyntax) firstArgumentExpressionNode).IsNameOfExpression();
        }
    }

    internal static class NameOfExaminator
    {
        public static bool IsNameOfExpression(this InvocationExpressionSyntax invocationNode)
        {
            var identifierNode = invocationNode.Expression as IdentifierNameSyntax;
            if (identifierNode == null)
            {
                return false;
            }
            return identifierNode.IsNameOfIdentifier();
        }

        public static bool IsNameOfIdentifier(this IdentifierNameSyntax identifierNode)
        {
            return identifierNode.Identifier.IsKindOrHasMatchingText(SyntaxKind.NameOfKeyword);
        }
    }

    internal static class SyntaxTokenExtensions
    {
        public static bool IsKindOrHasMatchingText(this SyntaxToken token, SyntaxKind kind)
        {
            return token.Kind() == kind || token.HasMatchingText(kind);
        }

        public static bool HasMatchingText(this SyntaxToken token, SyntaxKind kind)
        {
            var text = SyntaxFacts.GetText(kind);
            return token.ToString() == text;
        }
    }
}