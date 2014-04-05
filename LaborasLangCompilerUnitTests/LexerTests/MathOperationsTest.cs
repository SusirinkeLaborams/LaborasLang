﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NPEG;
using LaborasLangCompiler.LexingTools;

namespace LaborasLangCompilerUnitTests.LexerTests
{
    [TestClass]
    public class MathOperationsTest
    {
        [TestMethod]
        public void TestUnaryNegation()
        {
            var source = @"foo = -bar";
            AstNode tree = Lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: Negation: Symbol, EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestAddition()
        {
            var source = @"foo = 1 + bar;";
            AstNode tree = Lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, Value: Sum: (Product: Literal: IntegerLiteral, SumOperator, Product: Symbol)), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSubtraction()
        {
            var source = @"foo = bar - 1;";
            AstNode tree = Lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, Value: Sum: (Product: Symbol, SumOperator, Product: Literal: IntegerLiteral)), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMultiplication()
        {
            var source = @"foo = bar * 1.5;";
            AstNode tree = Lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, Value: Sum: Product: (Symbol, MultiplicationOperator, Literal: FloatLiteral)), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestDivision()
        {
            var source = @"foo = 1.5 / bar;";
            AstNode tree = Lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, Value: Sum: Product: (Literal: FloatLiteral, MultiplicationOperator, Symbol)), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestAdditionAndMultiplicationOrder()
        {
            var source = @"foo = 5 + 1.2 * bar;";
            AstNode tree = Lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, Value: Sum: (Product: Literal: IntegerLiteral, SumOperator, Product: (Literal: FloatLiteral, MultiplicationOperator, Symbol))), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSubtractionAndDivisionOrder()
        {
            var source = @"foo = 1.5  - bar / 15;";
            AstNode tree = Lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, Value: Sum: (Product: Literal: FloatLiteral, SumOperator, Product: (Symbol, MultiplicationOperator, Literal: IntegerLiteral))), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }
    }
}