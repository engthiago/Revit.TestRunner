using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using DesignAutomationFramework;
using NUnit;
using NUnit.Engine;
using Revit.TestRunner.Shared;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

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

            Console.WriteLine("**************************************************");
            Console.WriteLine($"**************** Directory Files ****************");

            var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (file.Contains(@"\nunit\") || file.Contains(@"\workitemBundle\"))
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

            Console.WriteLine("**************************************************");

            var testSuitesPath = "testSuite.json";
            TestSuite testSuite = null;
            if (!File.Exists(testSuitesPath))
            {
                Console.WriteLine($"***** Test Suite file not found: {testSuitesPath}");
            }
            else
            {
                testSuite = JsonHelper.FromFile<TestSuite>(testSuitesPath);
            }

            if (testSuite == null)
            {
                Console.WriteLine($"***** Test Suite json parsing error: {testSuitesPath}");
                e.Succeeded = false;
            }
            else
            {
                var workitemId = Path.GetFileName(path);
                var assemblyPath = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories).FirstOrDefault(f => f.EndsWith(testSuite.Assembly));

                if (string.IsNullOrWhiteSpace(assemblyPath))
                {
                    Console.WriteLine($"***** Test Suite failed, the assembly {testSuite.Assembly} was not found.");
                    e.Succeeded = false;
                    return;
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
                        e.Succeeded = false;
                        return;
                    }

                    var filePath = files.FirstOrDefault(f => f.EndsWith(testSuite.RvtFile));
                    if (string.IsNullOrWhiteSpace(filePath))
                    {
                        Console.WriteLine($"***** Test Suite failed, the file {testSuite.RvtFile} is was not found.");
                        e.Succeeded = false;
                        return;
                    }

                    doc = app.OpenDocumentFile(filePath);
                }

                var testCorePath = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories).FirstOrDefault(f => f.EndsWith("Revit.TestRunner.TestCore.dll"));
                var coreAssembly = Assembly.LoadFrom(testCorePath);
                var attType = coreAssembly.GetTypes().FirstOrDefault(t => t.Name == "InjectDocumentAttribute");

                if (attType == null)
                {
                    Console.WriteLine($"***** Test Suite failed, the attribute not found on {coreAssembly.FullName}");
                    foreach (var tt in coreAssembly.GetTypes())
                    {
                        Console.WriteLine($"****** {tt.FullName}");
                    }
                    return;
                }

                var prop = attType.GetProperty("Document", BindingFlags.Public | BindingFlags.Static);
                prop.SetValue(null, doc);

                ITestRunner testRunner = null;
                var flattenService = new NUnitCaseFlattenService();
                var runnerService = new NUnitRunnerService(flattenService);
                try
                {
                    testRunner = runnerService.CreateTestRunner(assemblyPath);
                    var result = testRunner.Run(null, TestFilter.Empty);

                    result.AddAttribute("workitemId", workitemId);
                    if (!string.IsNullOrWhiteSpace(testSuite.Id))
                    {
                        result.AddAttribute("testSuiteId", testSuite.Id);
                    }

                    XmlSerializer serializer = new XmlSerializer(typeof(XmlNode));
                    TextWriter writer = new StreamWriter("results.xml");
                    serializer.Serialize(writer, result);
                    writer.Close();

                    Console.WriteLine("**********          Results             **********");
                    Console.WriteLine(File.ReadAllText("results.xml"));
                }
                finally
                {
                    testRunner?.Unload();
                }
            }

            e.Succeeded = true;

            Console.WriteLine($"********** Design automation completed ***********");
            Console.WriteLine("**************************************************");

        }

        public ExternalDBApplicationResult OnShutdown(ControlledApplication application)
        {
            return ExternalDBApplicationResult.Succeeded;
        }
    }
}
