using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
//using Autodesk.Revit.UI;
using Revit.TestRunner.Shared.Communication;

namespace Revit.TestRunner.DA
{
    //[Transaction(TransactionMode.Manual)]
    //public class TestCommand : IExternalCommand
    //{
    //    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    //    {
    //        var request = new RunRequest();
    //        request.Id = "Runner";
    //        request.Cases = new TestCase[2];

    //        request.Cases[0] = new TestCase
    //        {
    //            Id = "1",
    //            AssemblyPath = "C:\\Users\\Thiago\\source\\repos\\Revit.TestRunner\\src\\bin\\Revit.TestRunner.SampleTestProject.dll",
    //            TestClass = "Revit.TestRunner.SampleTestProject.SampleTest",
    //            MethodName = "PassTest"
    //        };

    //        request.Cases[1] = new TestCase
    //        {
    //            Id = "2",
    //            AssemblyPath = "C:\\Users\\Thiago\\source\\repos\\Revit.TestRunner\\src\\bin\\Revit.TestRunner.SampleTestProject.dll",
    //            TestClass = "Revit.TestRunner.SampleTestProject.SampleTest",
    //            MethodName = "FailTest"
    //        };

    //        var testRunner = new TestRunnerService();
    //        var results = testRunner.Run(commandData.Application.Application, request);

    //        TaskDialog.Show("Results:", results);

    //        return Result.Succeeded;
    //    }
    //}
}
