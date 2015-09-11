using System.Windows;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using TestHelper;

using WpfDiagnostics.Diagnostics.CustomControls;

using Xunit;

namespace WpfDiagnostics.Test.Diagnostics.CustomControls
{
    public class DependencyPropertyWithoutNameOfOperatorTests : DiagnosticVerifier
    {
        [Fact]
        public void WithEmptySourceFile_ShouldNotFindAnything()
        {
            var test = @"";
            VerifyCSharpDiagnostic(test);
        }

        // scenario's:
        // * class without dep prop, should find nothing
        // * class with depProp, with call to DependencyProperty.Register(), first argument is string
        // * same as above, first argument is nameof()-expression
        // * class is not derived from DepObj
        // * compilation unit is not referencing "Wpf"-assembly

        [Fact]
        public void RegisterCall_WithoutFirstArgumentNameOfExpression_ShouldReport()
        {
            var brokenSource = @"
    using System.Windows;

    public class ClassWithDependencyProperty : DependencyObject
    {
        public static readonly DependencyProperty DependencyPropertyNameProperty =
            DependencyProperty.Register(
                ""DependencyPropertyName"",
                typeof(string),
                typeof(ClassWithDependencyProperty),
                new PropertyMetadata(default(string)));

        public string DependencyPropertyName
        {
            get { return (string)GetValue(DependencyPropertyNameProperty); }
            set { SetValue(DependencyPropertyNameProperty, value); }
        }
    }";
            var expectedLocation = new LinePosition(8, 17);
            var expected = new DiagnosticResult
            {
                Id = "DependencyPropertyWithoutNameOfOperatorAnalyzer",
                Message = "Dependency property 'DependencyPropertyName' can use nameof() operator for DependencyProperty.Register() call",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", expectedLocation.Line, expectedLocation.Character)
                        }
            };
            VerifyCSharpDiagnostic(brokenSource, WpfReferences, expected);
        }

        [Fact]
        public void RegisterCall_WithFirstArgumentNameOfExpression_ShouldNotReport()
        {
            var nonBrokenSource = @"
    using System.Windows;

    public class ClassWithDependencyProperty : DependencyObject
    {
        public static readonly DependencyProperty DependencyPropertyNameProperty =
            DependencyProperty.Register(
                nameof(DependencyPropertyName),
                typeof(string),
                typeof(ClassWithDependencyProperty),
                new PropertyMetadata(default(string)));

        public string DependencyPropertyName
        {
            get { return (string)GetValue(DependencyPropertyNameProperty); }
            set { SetValue(DependencyPropertyNameProperty, value); }
        }
    }";
            VerifyCSharpDiagnostic(nonBrokenSource);
        }

        [Fact]
        public void Class_WithoutRegisterCall_ShouldNotReport()
        {
            var nonBrokenSource = @"
    public class AClass
    {
        public string DependencyPropertyName { get; set; }
    }";
            VerifyCSharpDiagnostic(nonBrokenSource);
        }
        private static MetadataReference[] WpfReferences => new[] { DependencyPropertyReference };

        private static readonly MetadataReference DependencyPropertyReference =
            MetadataReference.CreateFromFile(typeof(DependencyProperty).Assembly.Location);

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DependencyPropertyWithoutNameOfOperatorAnalyzer();
        }
    }
}