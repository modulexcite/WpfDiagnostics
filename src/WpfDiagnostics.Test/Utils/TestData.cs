using Microsoft.CodeAnalysis.Text;

namespace WpfDiagnostics.Test.Utils
{
    public class TestData
    {
        public string Description { private get; set; }
        public string BrokenSource { get; set; }
        public string ExpectedFixedSource { get; set; }
        public LinePosition ExpectedDiagnositicLocation { get; set; }

        public override string ToString()
        {
            return Description;
        }
    }
}