using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using DesignAutomationFramework;
using Revit.TestRunner.Shared.Communication;
using System;
using System.IO;
using System.Linq;

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
            var path = Directory.GetCurrentDirectory();

            Console.WriteLine($"********** Directory Files**********");
            var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                Console.WriteLine($"File: {file}");
            }
            Console.WriteLine($"************************************");

            var assemblyPath = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories).FirstOrDefault(f => f.EndsWith("Revit.TestRunner.SampleTestProject.dll"));

            // Reflects TestCases from the Assembly
            var flattenService = new NUnitCaseFlattenService();
            var runnerService = new NUnitRunnerService(flattenService);
            var request = runnerService.GetRequestFromAssembly(assemblyPath);

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
