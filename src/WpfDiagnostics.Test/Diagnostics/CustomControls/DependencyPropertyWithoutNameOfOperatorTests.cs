using Microsoft.CodeAnalysis.Diagnostics;

using RoslynTester.Helpers.CSharp;

using Xunit;

namespace WpfDiagnostics.Test.Diagnostics.CustomControls
{
    public class DependencyPropertyWithoutNameOfOperatorTests : CSharpDiagnosticVerifier
    {
        [Fact]
        public void WithEmptySourceFile_ShouldNotFindAnything()
        {
            var test = @"";
            VerifyDiagnostic(test);
        }

        protected override DiagnosticAnalyzer DiagnosticAnalyzer { get; } = new WpfDiagnosticsAnalyzer();
    }
}