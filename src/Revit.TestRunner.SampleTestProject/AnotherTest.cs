using Autodesk.Revit.DB;
using NUnit.Framework;
using Revit.TestRunner.TestCore;

namespace Revit.TestRunner.SampleTestProject
{
    public class AnotherTest
    {
        [InjectDocument]
        public void Test(Document doc)
        {
            Assert.NotNull(doc);
        }
    }
}
