using System.Collections.Generic;

namespace Revit.TestRunner.DA
{
    public class TestSuitesInput
    {
        public List<TestSuite> TestSuites { get; set; }
    }

    public class TestSuite
    {
        public string RvtFile { get; set; }
        public string Assembly { get; set; }
    }
}
