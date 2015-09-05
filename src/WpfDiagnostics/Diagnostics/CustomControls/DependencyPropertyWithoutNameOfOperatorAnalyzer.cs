using System;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace WpfDiagnostics.Diagnostics.CustomControls
{
    public class DependencyPropertyWithoutNameOfOperatorAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

        public override void Initialize(AnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}