using Autodesk.Revit.ApplicationServices;
using Revit.TestRunner.Runner;
using Revit.TestRunner.Shared.Communication;
using System;
using System.Text;

namespace Revit.TestRunner
{
    public class TestRunnerService
    {
        public TestRunnerService()
        {
        }

        public string Run(Application app, RunRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var stringBuilder = new StringBuilder();

            ReflectionRunner runner = new ReflectionRunner();
            foreach (TestCase test in request.Cases)
            {
                stringBuilder.Append( Run(app, runner, test) );
            }

            return stringBuilder.ToString();
        }

        public string Run(Application app, ReflectionRunner runner, TestCase test)
        {
            var testResult = runner.RunTest(test, app).Result;
            string results;

            if (testResult.State == TestState.Failed)
            {
                results = $"====> Test FAILED #{testResult.Id}: {testResult.MethodName}:\n{testResult.Message} \n{testResult.StackTrace}\n\n";
            }
            else
            {
                results = $"====> Test #{testResult.Id}:\n{testResult.MethodName}\n\n";
            }

            return results;
        }
    }
}
