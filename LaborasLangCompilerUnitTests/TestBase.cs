﻿using LaborasLangCompiler.FrontEnd;
using LaborasLangCompiler.Codegen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace LaborasLangCompilerUnitTests
{
    [TestClass]
    public class TestBase
    {
        public TestBase(bool recreateAssemblyRegistry = true)
        {
            Environment.SetEnvironmentVariable("LLC_IGNORE_NON_EXISTING_SOURCE_FILES", "1");

            if (recreateAssemblyRegistry)
            {
                var compilerArgs = CompilerArguments.Parse(new[] { "ExecuteTest.il" });
                AssemblyRegistry.CreateAndOverrideIfNeeded(compilerArgs.References);
            }
        }

        public static string GetTestDirectory()
        {
            var testDirectory = Path.Combine(Path.GetTempPath(), "llc_tests");

            try
            {
                if (Directory.Exists(testDirectory))
                {
                    var directoryInfo = new DirectoryInfo(testDirectory);

                    foreach (var file in directoryInfo.GetFiles())
                    {
                        file.Delete();
                    }

                    foreach (var dir in directoryInfo.GetDirectories())
                    {
                        dir.Delete(true);
                    }
                }
                else
                {
                    Directory.CreateDirectory(testDirectory);
                }
            }
            catch
            {
            }

            return testDirectory;
        }
    }
}
