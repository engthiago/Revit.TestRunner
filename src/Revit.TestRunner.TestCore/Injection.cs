using Autodesk.Revit.DB;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;
using System;
using System.Collections.Generic;

namespace Revit.TestRunner.TestCore
{

    [AttributeUsage(AttributeTargets.Method)]
    public class InjectDocumentAttribute : TestAttribute, ITestBuilder
    {
        public static Document Document { get; set; }

        IEnumerable<TestMethod> ITestBuilder.BuildFrom(IMethodInfo method, Test suite)
        {
            var arguments = new object[] { Document };

            yield return new NUnitTestCaseBuilder()
                .BuildTestMethod(method, suite, new TestCaseParameters(arguments));
        }
    }

}
