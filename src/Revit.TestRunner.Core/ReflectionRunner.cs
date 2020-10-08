﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using NUnit.Framework;
using Revit.TestRunner.Shared.Communication;

namespace Revit.TestRunner.Runner
{
    /// <summary>
    /// This Runner runs the corresponding Test using Reflection.
    /// </summary>
    public class ReflectionRunner
    {

        /// <summary>
        /// Execute Test described in <paramref name="test"/>.
        /// Returns a new <see cref="TestCase"/> object with the test result.
        /// </summary>
        public async Task<TestCase> RunTest(TestCase test, Application application)
        {
            TestCase result = new TestCase
            {
                Id = test.Id,
                AssemblyPath = test.AssemblyPath,
                TestClass = test.MethodName,
                MethodName = test.MethodName,
                State = TestState.Unknown
            };

            if (string.IsNullOrEmpty(test.Id)) result.Message = "Missing ID";
            if (string.IsNullOrEmpty(test.AssemblyPath)) result.Message = "Missing AssemblyPath";
            if (string.IsNullOrEmpty(test.TestClass)) result.Message = "Missing ClassName";
            if (string.IsNullOrEmpty(test.MethodName)) result.Message = "Missing MethodName";
            if (test.State != TestState.Unknown) result.Message = $"Wrong not in State '{TestState.Unknown}'";

            if (!string.IsNullOrEmpty(test.Message))
            {
                test.State = TestState.Failed;
                return result;
            }

            var possibleParams = new object[] { application };

            object obj = null;
            MethodInfo setUp = null;
            MethodInfo tearDown = null;
            MethodInfo testMethod = null;

            try
            {
                if (!File.Exists(test.AssemblyPath))
                {
                    throw new FileNotFoundException($"Assembly not found! {test.AssemblyPath}");
                }

                Assembly assembly = Assembly.LoadFile(test.AssemblyPath);

                Type type = assembly.GetType(test.TestClass);
                if (type == null)
                {
                    throw new TypeLoadException($"Test class: {test.TestClass} not found on dll: {test.AssemblyPath}\n\nMake sure you have specified the right dll and the right class path");
                }

                var constructor = type.GetConstructors().FirstOrDefault();
                if (constructor.GetParameters().Length > 0)
                {
                    obj = Activator.CreateInstance(type, new object[] { application });
                }
                else
                {
                    obj = Activator.CreateInstance(type);
                }

                setUp = GetMethodByAttribute(type, typeof(SetUpAttribute));
                testMethod = type.GetMethod(test.MethodName);
                tearDown = GetMethodByAttribute(type, typeof(TearDownAttribute));

                var customAttributes = testMethod.CustomAttributes;
                var extendedParams = new List<object>();

                foreach (CustomAttributeData customAttribute in customAttributes)
                {
                    extendedParams.AddRange(customAttribute.ConstructorArguments.Select(a => a.Value));
                }

                await InvokeMethod(obj, setUp, new object[0]);
                await InvokeMethod(obj, testMethod, extendedParams.ToArray());

                result.State = TestState.Passed;
            }
            catch (Exception e)
            {
                ReportException(result, e);
            }
            finally
            {
                try
                {
                    await InvokeMethod(obj, tearDown, new object[0]);
                }
                catch (Exception e)
                {
                    ReportException(result, e);
                }
            }

            //Log.Info($" >> {result.TestClass}.{result.MethodName} - {result.State} - {result.Message}");

            return result;
        }

        /// <summary>
        /// Invoke <paramref name="method"/> on <paramref name="obj"/>, passing <paramref name="possibleParams"/>.
        /// </summary>
        private async Task InvokeMethod( object obj, MethodInfo method, object[] possibleParams )
        {
            if( method != null ) {
                var methodParams = OrderParameters(method, possibleParams);

                if ( method.ReturnType == typeof( Task ) ) {
                    Task task = (Task)method.Invoke( obj, methodParams);
                    await task;
                }
                else {
                    method.Invoke( obj, methodParams);
                }
            }
        }

        /// <summary>
        /// Enrich test case with exception information.
        /// </summary>
        private void ReportException( TestCase @case, Exception e )
        {
            @case.State = TestState.Failed;

            Exception toLogEx = e.InnerException ?? e;

            @case.Message = toLogEx.Message;
            @case.StackTrace = toLogEx.StackTrace;
        }

        /// <summary>
        /// Order parameters according to the method info.
        /// </summary>
        private object[] OrderParameters( MethodInfo methodInfo, object[] possibleParams )
        {
            var result = new List<object>();
            var parameters = methodInfo.GetParameters();
            var possibleParamsList = possibleParams.ToList();

            foreach( ParameterInfo parameter in parameters ) {
                object o = possibleParamsList.FirstOrDefault( i => i.GetType() == parameter.ParameterType );
                result.Add( o );
            }

            return result.ToArray();
        }

        private MethodInfo GetMethodByAttribute( Type type, Type attributeType )
        {
            var listOfMethods = new List<MethodInfo>();

            foreach( MethodInfo method in type.GetMethods() ) {
                if( MarkedByAttribute( method, attributeType ) ) {
                    listOfMethods.Add( method );
                }
            }

            if( listOfMethods.Count > 1 ) throw new InvalidOperationException( $"More than one method marked with '{attributeType.Name}' attribute found!" );

            return listOfMethods.SingleOrDefault();
        }

        private bool MarkedByAttribute( MethodInfo methodInfo, Type attributeType )
        {
            return methodInfo.GetCustomAttributes( true ).Select( a => a.ToString() ).Contains( attributeType.FullName );
        }
    }
}
