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
                if (firstArgumentExpressionNode.Kind() != SyntaxKind.NameOfKeyword)
                {
                    var depPropName = context.SemanticModel.GetConstantValue(firstArgumentExpressionNode).Value;
                    context.ReportDiagnostic(Diagnostic.Create(Rule, firstArgumentNode.GetLocation(), depPropName));
                    
                }
            }
        }
    }
}