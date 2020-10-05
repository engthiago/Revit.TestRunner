using Revit.TestRunner.Shared.NUnit;
using System.Collections.Generic;
using System.Linq;

namespace Revit.TestRunner
{
    public class NUnitCaseFlattenService
    {
        public NUnitCaseFlattenService()
        {

        }

        public List<NUnitTestCase> Flatten(NUnitTestSuite nUnitTestSuite)
        {
            var cases = nUnitTestSuite.TestCases.ToList();

            foreach (var suite in nUnitTestSuite.TestSuites)
            {
                cases.AddRange(Flatten(suite));
            }

            return cases;
        }
    }
}
