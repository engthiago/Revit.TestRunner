using Autodesk.Revit.ApplicationServices;
using NUnit.Framework;

namespace Revit.TestRunner.SampleTestProject
{
    public class ParameterTest
    {
        [SetUp]
        public void SetUp(Application application)
        {
            Assert.NotNull(application);
        }

        [TearDown]
        public void TearDown(Application application )
        {
            Assert.NotNull( application );
        }


        [Test]
        public void UiApplicationTest(Application application)
        {
            Assert.NotNull(application);
        }

        [Test]
        public void ApplicationTest( Application application )
        {
            Assert.IsNotNull( application );
        }

        [Test]
        public void MultiParameterTest1( Application application )
        {
            Assert.IsNotNull( application );
        }

        [Test]
        public void MultiParameterTest2( Application application )
        {
            Assert.IsNotNull( application );
        }

        [TestCase( 12, 3, ExpectedResult = 15 )]
        [TestCase( 13, 7, ExpectedResult = 20 )]
        [TestCase( 15, 4, ExpectedResult = 19 )]
        public int SumTest( int n, int d )
        {
            return n + d;
        }
    }
}
