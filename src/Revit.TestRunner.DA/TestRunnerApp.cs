using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using DesignAutomationFramework;
using Revit.TestRunner.Shared;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

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
            Document doc = null;
            var path = Directory.GetCurrentDirectory();

            Console.WriteLine($"********** Directory Files **********");

            var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (file.Contains("nunit"))
                {
                    var fileName = Path.GetFileName(file);
                    var rootFile = Path.Combine(path, fileName);
                    File.Copy(file, rootFile);
                    Console.WriteLine($"File: {rootFile}");
                }
                else
                {
                    Console.WriteLine($"File: {file}");
                }
            }

            Console.WriteLine("*************************************");

            var testSuitesPath = "testSuites.json";
            TestSuitesInput testSuitesInput = null;
            if (!File.Exists(testSuitesPath))
            {
                Console.WriteLine($"***** Test Suites file not found: {testSuitesPath}");
            }
            else
            {
                testSuitesInput = JsonHelper.FromFile<TestSuitesInput>(testSuitesPath);
            }

            var resultStringBuilder = new StringBuilder();
            if (testSuitesInput == null)
            {
                Console.WriteLine($"***** Test Suites file parsing error: {testSuitesPath}");
            }
            else
            {
                foreach (var testSuite in testSuitesInput.TestSuites)
                {
                    var assemblyPath = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories).FirstOrDefault(f => f.EndsWith(testSuite.Assembly));

                    if (string.IsNullOrWhiteSpace(assemblyPath))
                    {
                        Console.WriteLine($"***** Test Suite failed, the assembly {testSuite.Assembly} was not found.");
                        continue;
                    }
                    else
                    {
                        var fileName = Path.GetFileName(assemblyPath);
                        var rootFile = Path.Combine(path, fileName);
                        File.Copy(assemblyPath, rootFile);

                        assemblyPath = rootFile;

                        Console.WriteLine($"***** Started tests for: {assemblyPath}.");
                    }

                    if (!string.IsNullOrWhiteSpace(testSuite.RvtFile))
                    {
                        var normalizedRvtFile = testSuite.RvtFile.ToLower();
                        if (
                               !normalizedRvtFile.EndsWith(".rte")
                            && !normalizedRvtFile.EndsWith(".rvt")
                            && !normalizedRvtFile.EndsWith(".rfa")
                            && !normalizedRvtFile.EndsWith(".rtf")
                            )
                        {
                            Console.WriteLine($"***** Test Suite failed, the file {testSuite.RvtFile} is not a valid revit file.");
                            continue;
                        }

                        var filePath = files.FirstOrDefault(f => f.EndsWith(testSuite.RvtFile));
                        if (string.IsNullOrWhiteSpace(filePath))
                        {
                            Console.WriteLine($"***** Test Suite failed, the file {testSuite.RvtFile} is was not found.");
                            continue;
                        }

                        doc = app.OpenDocumentFile(filePath);
                    }
                    else
                    {
                        doc?.Close(false);
                    }

                    // Reflects TestCases from the Assembly
                    var flattenService = new NUnitCaseFlattenService();
                    var runnerService = new NUnitRunnerService(flattenService);
                    var request = runnerService.GetRequestFromAssembly(assemblyPath);

                    Console.WriteLine("***** Test Suite:");
                    Console.WriteLine(JsonHelper.ToString(request));

                    var testRunner = new TestRunnerService();
                    var results = testRunner.Run(app, request);
                    resultStringBuilder.Append(results);
                }
            }


            e.Succeeded = true;

            if (resultStringBuilder.Length == 0)
            {
                Console.WriteLine($"***** No Test Suite could be completed.");
            }
            else
            {
                Console.WriteLine(resultStringBuilder.ToString());
            }

            Console.WriteLine($"********** Design automation completed **********");
            Console.WriteLine("**************************************************");

        }

        public ExternalDBApplicationResult OnShutdown(ControlledApplication application)
        {
            return ExternalDBApplicationResult.Succeeded;
        }
    }
}
