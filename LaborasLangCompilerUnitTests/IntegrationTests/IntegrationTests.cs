﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LaborasLangCompilerUnitTests.IntegrationTests
{
    [TestClass]
    public class IntegrationTests : IntegrationTestBase
    {
        [TestMethod, TestCategory("Integration Tests")]
        public void Test_HelloWorld()
        {
            Test("HelloWorld.ll", "Hello, world!");
        }

        [TestMethod, TestCategory("Integration Tests")]
        public void Test_Bottles()
        {
            TestAgainstOutputInFile("Bottles");
        }

        [TestMethod, TestCategory("Integration Tests")]
        public void Test_Recursion()
        {
            var expectedOutput =
@"0 is even
3 is odd
8 is even";
            Test("Recursion.ll", expectedOutput);
        }

        [TestMethod, TestCategory("Integration Tests")]
        public void Test_StdInWorks()
        {
            var testInfo = new IntegrationTestInfo("StdInWorks.ll");

            testInfo.StdIn =
@"2
3
5
6
7
8
9
411
419
0
";

            testInfo.StdOut =
@"Enter 0 at any time to quit.
2 is a prime number
3 is a prime number
5 is a prime number
6 is not a prime number
7 is a prime number
8 is not a prime number
9 is not a prime number
411 is not a prime number
419 is a prime number";

            Test(testInfo);
        }

        [TestMethod, TestCategory("Integration Tests")]
        public void Test_ArrayItemSwap()
        {
            var testInfo = new IntegrationTestInfo("ArrayItemSwap.ll");

            testInfo.StdIn =
@"2 3 5 6 7 8 9 411 419 0
1 5
";

            testInfo.StdOut =
@"2 8 5 6 7 3 9 411 419 0
";

            Test(testInfo);
        }

        [TestMethod, TestCategory("Integration Tests")]
        public void Test_CastsAndForeach()
        {
            var testInfo = new IntegrationTestInfo("Casts.ll");

            testInfo.StdIn =
@"2 3 5 6 7 8 9 411 419 0
";

            testInfo.StdOut =
@"2 3 5 6 7 8 9 411 419 0
";

            Test(testInfo);
        }

        [TestMethod, TestCategory("Integration Tests")]
        public void Test_InlineFunctorCall()
        {
            Test("InlineFunctorCall.ll", "It Works!");
        }

        [TestMethod, TestCategory("Integration Tests")]
        public void Test_MultipleFiles()
        {
            Test(new string[]{"InlineFunctorCall.ll", "MultipleFiles.ll"}, "It Works!");
        }
                
        [TestMethod, TestCategory("Integration Tests")]
        public void Test_PrintPrimesWithInlineLambda()
        {
            var testInfo = new IntegrationTestInfo("PrintPrimesWithInlineLambda.ll");

            testInfo.StdIn =
@"2
3
5
6
7
8
9
411
419
0
";

            testInfo.StdOut =
@"Enter 0 at any time to quit.
2 is a prime number
3 is a prime number
5 is a prime number
6 is not a prime number
7 is a prime number
8 is not a prime number
9 is not a prime number
411 is not a prime number
419 is a prime number";

            Test(testInfo);

        }

        [TestMethod, TestCategory("Integration Tests")]
        public void Test_ImplicitRuntimeCast()
        {
            Test("ImplicitRuntimeCast.ll", "4");
        }

        [TestMethod, TestCategory("Integration Tests")]
        public void Test_AssignToPreIncrementedValue()
        {
            Test("AssignToPreIncrementedValue.ll", "5");
        }

        [TestMethod, TestCategory("Integration Tests")]
        public void Test_MinMaxValues()
        {
            var expected1 = string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, ",
                sbyte.MinValue, sbyte.MaxValue,
                byte.MinValue, byte.MaxValue,
                char.MinValue, char.MaxValue,
                short.MinValue, short.MaxValue,
                ushort.MinValue, ushort.MaxValue,
                int.MinValue, int.MaxValue,
                uint.MinValue, uint.MaxValue,
                long.MinValue, long.MaxValue,
                ulong.MinValue, ulong.MaxValue);

            var expected2 = Enumerable.Repeat<string>(string.Format("{0}, ", true), 18).Aggregate((x, y) => x + y);

            var expected3 = string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}",
                sbyte.MinValue > 0, sbyte.MaxValue > 0,
                byte.MinValue > 0, byte.MaxValue > 0,
                char.MinValue > 0, char.MaxValue > 0,
                short.MinValue > 0, short.MaxValue > 0,
                ushort.MinValue > 0, ushort.MaxValue > 0,
                int.MinValue > 0, int.MaxValue > 0,
                uint.MinValue > 0, uint.MaxValue > 0,
                long.MinValue > 0, long.MaxValue > 0,
                ulong.MinValue > 0, ulong.MaxValue > 0);

            var expected = expected1 + expected1 + expected2 + expected3;

            Test("MinMaxValues.ll", expected);
        }

        [TestMethod, TestCategory("Integration Tests")]
        public void Test_CanAddLiteralToUnsignedInt()
        {
            Test("CanAddLiteralToUnsignedInt.ll", "6");
        }

        [TestMethod, TestCategory("Integration Tests")]
        public void Test_CharLiterals()
        {
            Test("CharLiterals.ll", new[] { "some", "words", "separated", "by", "commas" }.Aggregate((x, y) => x + Environment.NewLine + y));
        }

        [TestMethod, TestCategory("Integration Tests"), TestCategory("CodeSamples")]
        public void Test_HttpRequest()
        {
            Test("HttpRequest.ll", "The World Wide Web project", new[] { "System.dll" });
        }

        [TestMethod, TestCategory("Integration Tests")]
        public void Test_AssignmentReturn()
        {
            Test("AssignmentReturn.ll", "True");
        }

        [TestMethod, TestCategory("Integration Tests"), TestCategory("Disabled")]
        public void Test_DuplicateMethods()
        {
            try
            {
                Test("DuplicateMethods.ll", "No, mister test, I expect you to die");
            }
            catch (Exception e)
            {
                Assert.IsTrue(e.Message.Contains("LL0040 MethodAlreadyDeclared"));
                return;
            }
            Assert.Fail();
        }

        [TestMethod, TestCategory("Integration Tests")]
        public void Test_Rank32Matrix()
        {
            var info = new IntegrationTestInfo("Rank32Matrix.ll");
            info.StdIn = 
@"5
";
            info.StdOut =
                @"Your number: 5
";
            Test(info);
        }

        [TestMethod, TestCategory("Integration Tests")]
        public void Test_ArrayList()
        {
            Test("ArrayList.ll", string.Format("1{0}2{0}3", Environment.NewLine));
        }

        [TestMethod, TestCategory("Integration Tests")]
        public void Test_FloatLiterals()
        {
            Test("FloatLiterals.ll", string.Format("{1}{0}{2}{0}{3}{0}{4}", Environment.NewLine, 0.0f, 1.0f, 1.5f, 1.5f));
        }

        [TestMethod, TestCategory("Integration Tests")]
        public void Test_FizzBuzz()
        {
            Test("FizzBuzz.ll", 
@"1
2
Fizz
4
Buzz
Fizz
7
8
Fizz
Buzz
11
Fizz
13
14
FizzBuzz
16
");
        }

        [TestMethod, TestCategory("Integration Tests")]
        public void Test_GlobalScope()
        {
            Test("GlobalScope.ll", @"Hello, World!
");
        }

        #region Helpers
        
        private string ExpectedOutputPath
        {
            get
            {
                return Path.Combine(IntegrationTestsPath, "ExpectedOutput");
            }
        }

        private void TestAgainstOutputInFile(string testName)
        {
            var expectedOutput = File.ReadAllText(Path.Combine(ExpectedOutputPath, testName) + ".txt");
            Test(testName + ".ll", expectedOutput);
        }

        #endregion
    }
}
