using System;
using System.Collections.Generic;
using Qfe;
using Qfe.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathNet.Numerics.LinearAlgebra.Double;


namespace AlgorithmTests
{
    [TestClass]
    public class ParserTests
    {
        DenseVector makePoint(params double[] values)
        {
            return new DenseVector(values);
        }

        TaskParser parser;
        AllSections testSections;

        [TestInitialize]
        public void Init()
        {
            parser = new TaskParser();
            testSections = new AllSections()
            {
                DimensionSection = new DimensionSection() { Dim = 2 },
                ParametersSection = new ParametersSection() { Content = "" },
                CostFunctionSection = new CostFunctionSection(),
                ConstraintsSection = new ConstraintsSection()
            };
        }

        [TestMethod]
        public void CompileCostFunction_simple()
        {
            testSections.CostFunctionSection.Function = "x[0] + 2.0 * x[1]";
            var func = parser.compileCostFunction(testSections).Function;

            Assert.AreEqual(0.0, func(makePoint( 0.0, 0.0 )));
            Assert.AreEqual(1.0, func(makePoint( 1.0, 0.0 )));
            Assert.AreEqual(-2.0, func(makePoint( 0.0, -1.0 )));
            Assert.AreEqual(8.0, func(makePoint( 2.0, 3.0 )));

            testSections.CostFunctionSection.Function = "x[0] * x[0] + 3.0";
            func = parser.compileCostFunction(testSections).Function;

            Assert.AreEqual(3.0, func(makePoint( 0.0 )));
            Assert.AreEqual(3.0, func(makePoint( 0.0 )));
            Assert.AreEqual(7.0, func(makePoint( 2.0 )));
        }

        [TestMethod]
        public void CompileCostFunction_withParameters()
        {
            testSections.ParametersSection.Content = "var a = 3.0;";
            testSections.CostFunctionSection.Function = "x[0] * a";
            var func = parser.compileCostFunction(testSections).Function;

            Assert.AreEqual(0.0, func(makePoint( 0.0 )));
            Assert.AreEqual(3.0, func(makePoint( 1.0 )));
            Assert.AreEqual(6.0, func(makePoint( 2.0 )));
        }
        
        [TestMethod]
        public void CompileCostFunction_specialMethods()
        {
            testSections.CostFunctionSection.Function = "sin(x[0])";
            var func = parser.compileCostFunction(testSections).Function;
            Assert.AreEqual(Math.Sin(2.0), func(makePoint( 2.0 )));

            testSections.CostFunctionSection.Function = "ln(x[0])";
            func = parser.compileCostFunction(testSections).Function;
            Assert.AreEqual(Math.Log(2.0), func(makePoint( 2.0 )));

            testSections.CostFunctionSection.Function = "pow(x[0], 2.0)";
            func = parser.compileCostFunction(testSections).Function;
            Assert.AreEqual(4.0, func(makePoint( 2.0 )));

            testSections.ParametersSection.Content = "double a = exp(3.0);";
            testSections.CostFunctionSection.Function = "pow4(x[0]) * a";
            func = parser.compileCostFunction(testSections).Function;
            Assert.AreEqual(16.0 * Math.Exp(3.0), func(makePoint( 2.0 )));
        }

        [TestMethod]
        public void CompileCostFunction_withFunctionInParameters()
        {
            testSections.ParametersSection.Content = "double a() { return 3.0; }";
            testSections.CostFunctionSection.Function = "x[0] * a()";
            var func = parser.compileCostFunction(testSections).Function;

            Assert.AreEqual(0.0, func(makePoint( 0.0 )));
            Assert.AreEqual(3.0, func(makePoint( 1.0 )));
            Assert.AreEqual(6.0, func(makePoint( 2.0 )));
        }

        [TestMethod]
        public void CompileCostFunction_tooFewInputs()
        {
            testSections.DimensionSection.Dim = 2;
            testSections.CostFunctionSection.Function = "x[0] + x[1] + x[2]";

            TestUtils.ExpectThrow(() => parser.compileCostFunction(testSections));
        }

        [TestMethod]
        public void CompileCostFunction_invalidCode()
        {
            testSections.CostFunctionSection.Function = "a";
            TestUtils.ExpectThrow<FunctionCompilationFailure>(() => parser.compileCostFunction(testSections));

            testSections.ParametersSection.Content = "double a() { return 3.0; };";
            testSections.CostFunctionSection.Function = "x[0]";
            TestUtils.ExpectThrow<FunctionCompilationFailure>(() => parser.compileCostFunction(testSections));
        }

        [TestMethod]
        public void CompileConstraints()
        {
            testSections.ParametersSection.Content = "double a = 3.0;";
            testSections.ConstraintsSection.Constraints = new List<RawConstraint>()
            {
                new RawConstraint()
                {
                    Lhs = "x[0]", Rhs = "0", Operator = ">=", Type = ConstraintType.GreaterEqual
                },
                new RawConstraint()
                {
                    Lhs = "x[1] - 100", Rhs = "0", Operator = "<=", Type = ConstraintType.LessEqual
                },
                new RawConstraint()
                {
                    Lhs = "x[0] * a", Rhs = "0", Operator = "<=", Type = ConstraintType.LessEqual
                },
                new RawConstraint()
                {
                    Lhs = "x[1]", Rhs = "200.0", Operator = "<=", Type = ConstraintType.LessEqual
                }
            };
            var constraints = parser.compileConstraints(testSections);

            Assert.AreEqual(4, constraints.Count);
            Assert.AreEqual(ConstraintType.GreaterEqual, constraints[0].Type);
            Assert.AreEqual(ConstraintType.LessEqual, constraints[1].Type);
            Assert.AreEqual(2.0, constraints[0].Function(makePoint( 2.0, 0.0 )));
            Assert.AreEqual(3.0, constraints[0].Function(makePoint( 3.0, 2.0 )));
            Assert.AreEqual(-100.0, constraints[1].Function(makePoint( 3.0, 0.0 )));
            Assert.AreEqual(-110.0, constraints[1].Function(makePoint( 0.0, -10.0 )));
            Assert.AreEqual(0.0, constraints[2].Function(makePoint( 0.0, 0.0 )));
            Assert.AreEqual(9.0, constraints[2].Function(makePoint( 3.0, 0.0 )));
            Assert.AreEqual(-200.0, constraints[3].Function(makePoint( 0.0, 0.0 )));
            Assert.AreEqual(-100.0, constraints[3].Function(makePoint( 0.0, 100.0 )));
        }

        [TestMethod]
        public void ParseTask()
        {
            string input = @"
                $dim: 3;
                $parameters:
                    double[] a = new double[] { 2.0, 1.0 };
                $function:
                    x[0] * x[0] + a[0] * x[1] + a[1] * x[2];
                $constraints:
                    x[0] <= 100.0;
                    x[1] >= a[0];
                    x[2] >= a[1];
            ";

            Task task = parser.Parse(input);

            Assert.AreEqual(3, task.Dim);
            Assert.AreEqual(4.0 + 2.0 * 3.0 + 1.0 * 4.0, task.Cost.Function(makePoint( 2.0, 3.0, 4.0 )));
            Assert.AreEqual(3, task.Constraints.Count);
        }

        [TestMethod]
        public void ChangeVaraiblesIndexing()
        {
            string input = "x0 + 2 * x11 + a2 + xx1 - x-1 + x[1]";
            string result = InputPreprocesor.ReplaceNumberedVaraiblesWithIndexedOnes(input);
            Assert.AreEqual("x[0] + 2 * x[11] + a2 + xx[1] - x-1 + x[1]", result);
        }

        [TestMethod]
        public void FindDimension()
        {
            string input = "x[1] + x[2]; 4 + x3;";
            uint result = InputPreprocesor.FindDimension(input);
            Assert.AreEqual(3u, result);

            input = InputPreprocesor.ReplaceNumberedVaraiblesWithIndexedOnes(input);
            result = InputPreprocesor.FindDimension(input);
            Assert.AreEqual(4u, result);
        }

        [TestMethod]
        public void ParseTaskWithPreprocess()
        {
            string input = @"
                $function:
                    x0 * x2 + x1 * x3;
                $constraints:
                    x1 >= 2;
                    x2 >= 4;
            ";

            Task task = parser.Parse(input);

            Assert.AreEqual(4, task.Dim);
            Assert.AreEqual(4.0, task.Cost.Function(makePoint(1.0, 1.0, 2.0, 2.0)));
            Assert.AreEqual(2, task.Constraints.Count);
        }
    }
}
