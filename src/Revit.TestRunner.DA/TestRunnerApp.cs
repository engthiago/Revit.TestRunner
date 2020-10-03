using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using DesignAutomationFramework;
using Revit.TestRunner.Shared.Communication;
using System;

namespace Revit.TestRunner.DA
{
    [Regeneration(RegenerationOption.Manual)]
    [Transaction(TransactionMode.Manual)]
    public class TestRunnerApp : IExternalDBApplication
    {
        public ExternalDBApplicationResult OnStartup(ControlledApplication application)
        {
            DesignAutomationBridge.DesignAutomationReadyEvent += OnAppReady;
            return ExternalDBApplicationResult.Succeeded;
        }

        private void OnAppReady(object sender, DesignAutomationReadyEventArgs e)
        {
            var appData = e.DesignAutomationData;
            var app = appData.RevitApp;
            var doc = appData.RevitDoc;

            e.Succeeded = true;

            var request = new RunRequest();
            request.Id = "Runner";
            request.Cases = new TestCase[2];

            request.Cases[0] = new TestCase
            {
                Id = "1",
                AssemblyPath = "Revit.TestRunner.SampleTestProject.dll",
                TestClass = "Revit.TestRunner.SampleTestProject.SampleTest",
                MethodName = "PassTest"
            };

            request.Cases[1] = new TestCase
            {
                Id = "2",
                AssemblyPath = "Revit.TestRunner.SampleTestProject.dll",
                TestClass = "Revit.TestRunner.SampleTestProject.SampleTest",
                MethodName = "FailTest"
            }; 
            
            var testRunner = new TestRunnerService();
            var results = testRunner.Run(app, request);

            Console.WriteLine(results);
        }

        public ExternalDBApplicationResult OnShutdown(ControlledApplication application)
        {
            return ExternalDBApplicationResult.Succeeded;
        }
    }
}
