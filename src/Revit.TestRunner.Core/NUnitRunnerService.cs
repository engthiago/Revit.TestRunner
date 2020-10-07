using NUnit;
using NUnit.Engine;
using NUnit.Engine.Services;
using Revit.TestRunner.Shared;
using Revit.TestRunner.Shared.Communication;
using Revit.TestRunner.Shared.NUnit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Revit.TestRunner
{
    public class NUnitRunnerService
    {
        private readonly NUnitCaseFlattenService nUnitCaseFlattenService;

        public NUnitRunnerService(NUnitCaseFlattenService nUnitCaseFlattenService)
        {
            this.nUnitCaseFlattenService = nUnitCaseFlattenService;
        }

        public XmlNode GetXmlNodeFromAssembly(string assemblyPath)
        {
            ITestRunner testRunner = null;
            XmlNode result = null;
            try
            {
                testRunner = CreateTestRunner(assemblyPath);
                result = testRunner.Explore(TestFilter.Empty);
            }
            finally
            {
                testRunner?.Unload();
            }

            return result;
        }

        public RunRequest GetRequestFromAssembly(string assemblyPath)
        {
            var node = GetXmlNodeFromAssembly(assemblyPath);
            if (node == null)
            {
                return null;
            }

            Console.WriteLine("****Node: ");
            Console.WriteLine(node.InnerXml);

            var nunitTestSuite = new NUnitTestSuite(node);
            var nUnitCases = nUnitCaseFlattenService.Flatten(nunitTestSuite);

            Console.WriteLine("UnitCases: ");
            Console.WriteLine(JsonHelper.ToString(nUnitCases));

            var testCases = new List<TestCase>();
            foreach (var nUnitCase in nUnitCases)
            {
                var testCase = new TestCase
                {
                    Id = nUnitCase.Id,
                    AssemblyPath = assemblyPath,
                    TestClass = nUnitCase.ClassName,
                    MethodName = nUnitCase.MethodName
                };

                testCases.Add(testCase);
            }


            RunRequest request = new RunRequest
            {
                Timestamp = DateTime.Now,
                Cases = testCases.ToArray()
            };

            return request;
        }

        /// <summary>
        /// Create the nUnit test runner.
        /// </summary>
        public ITestRunner CreateTestRunner(string assemblyPath)
        {
            ITestRunner result = null;
            ITestEngine engine = CreateTestEngine(assemblyPath);

            var dir = Path.GetDirectoryName(assemblyPath);
            TestPackage testPackage = new TestPackage(assemblyPath);

            //https://github.com/nunit/nunit-console/blob/master/src/NUnitEngine/nunit.engine/EnginePackageSettings.cs
            string processModel = "InProcess";
            string domainUsage = "None";
            testPackage.AddSetting(EnginePackageSettings.ProcessModel, processModel);
            testPackage.AddSetting(EnginePackageSettings.DomainUsage, domainUsage);
            testPackage.AddSetting(EnginePackageSettings.WorkDirectory, dir);
            result = engine.GetRunner(testPackage);

            var agency = engine.Services.GetService<TestAgency>();
            agency?.StopService();

            return result;
        }

        /// <summary>
        /// Create the nUnit test engine.
        /// </summary>
        public ITestEngine CreateTestEngine(string assemblyPath)
        {
            // Normal way to create a NUnit TestEngine.
            // Not possible because of use AppDomain.CurrentDomain.BaseDirectory which points to bin of Revit.
            //return TestEngineActivator.CreateInstance();

            // Private way to create NUnit TestEngine, using bin directory of TestRunner.
            const string defaultAssemblyName = "nunit.engine.dll";
            const string defaultTypeName = "NUnit.Engine.TestEngine";
            string directory = Path.GetDirectoryName(assemblyPath);
            string workingDirectory = Path.Combine(directory, defaultAssemblyName);

            var engineAssembly = Assembly.ReflectionOnlyLoadFrom(workingDirectory);
            var engine = (ITestEngine)AppDomain.CurrentDomain.CreateInstanceFromAndUnwrap(engineAssembly.CodeBase, defaultTypeName);

            return engine;
        }
    }
}
