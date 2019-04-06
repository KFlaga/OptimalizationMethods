using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using MathNet.Numerics.LinearAlgebra.Double;
using Qfe;
using Qfe.Parser;

namespace Benchmarks
{
    public class ParserBenchmark
    {
        Func<Vector, double> funcRank1Simple; // x ^ 2 + 10
        Func<Vector, double> funcRank4Simple; // x1 + x2 + x3 + x4
        Func<Vector, double> funcRank1Complex; // sin(x) * exp(x) + log(x) * x 
        Func<Vector, double> funcRank4Complex; // sin(x1) * exp(x2) + log(x3) * x4

        double fres = 0.0;
        double x0, x1, x2, x3;
        Vector point;

        [GlobalSetup]
        public void GlobalSetup()
        {
            // prepare some functions
            AllSections sections = new AllSections()
            {
                DimensionSection = new DimensionSection(),
                ParametersSection = new ParametersSection() { Content = "" },
                CostFunctionSection = new CostFunctionSection(),
                ConstraintsSection = new ConstraintsSection()
            };

            TaskParser parser = new TaskParser();

            sections.DimensionSection.Dim = 1;
            sections.CostFunctionSection.Function = "pow2(x[0]) + 10";
            funcRank1Simple = parser.compileCostFunction(sections).Function;

            sections.DimensionSection.Dim = 4;
            sections.CostFunctionSection.Function = "x[0] + x[1] + x[2] + x[3]";
            funcRank4Simple = parser.compileCostFunction(sections).Function;

            sections.DimensionSection.Dim = 1;
            sections.CostFunctionSection.Function = "sin(x[0]) * exp(x[0]) + ln(x[0]) * x[0]";
            funcRank1Complex = parser.compileCostFunction(sections).Function;

            sections.DimensionSection.Dim = 4;
            sections.CostFunctionSection.Function = "sin(x[0]) * exp(x[1]) + ln(x[2]) * x[3]";
            funcRank4Complex = parser.compileCostFunction(sections).Function;


            Random r = new Random();
            x0 = r.NextDouble();
            x1 = r.NextDouble();
            x2 = r.NextDouble();
            x3 = r.NextDouble();

            point = new DenseVector(new double[] { x0, x1, x2, x3 });
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            // To ensure that fres is used and not optimized away
            if(fres == 0.0)
            {
                Console.WriteLine();
            }
        }

        public double func1(Vector x)
        {
            return x[0] * x[0] + 10.0;
        }

        public double func2(Vector x)
        {
            return x[0] + x[1] + x[2] + x[3];
        }

        public double func3(Vector x)
        {
            return Math.Sin(x[0]) * Math.Exp(x[0]) + Math.Log(x[0]) * x[0];
        }

        public double func4(Vector x)
        {
            return Math.Sin(x[0]) * Math.Exp(x[1]) + Math.Log(x[2]) * x[3];
        }

        [Benchmark]
        public void SimpleFunction1_inlined()
        {
            fres = func1(point);
        }

        [Benchmark]
        public void SimpleFunction1_withAlloc()
        {
            fres = func1(new DenseVector(new double[1] { x0 }));
        }

        [Benchmark]
        public void SimpleFunction1_parsed()
        {
            fres = funcRank1Simple(point);
        }

        [Benchmark]
        public void SimpleFunction1_parsedWithAlloc()
        {
            fres = funcRank1Simple(new DenseVector(new double[1] { x0 }));
        }

        [Benchmark]
        public void SimpleFunction4_inlined()
        {
            fres = func2(point);
        }

        [Benchmark]
        public void SimpleFunction4_withAlloc()
        {
            fres = func2(new DenseVector(new double[4] { x0, x1, x2, x3 }));
        }

        [Benchmark]
        public void SimpleFunction4_parsed()
        {
            fres = funcRank4Simple(point);
        }

        [Benchmark]
        public void SimpleFunction4_parsedWithAlloc()
        {
            fres = funcRank4Simple(new DenseVector(new double[4] { x0, x1, x2, x3 }));
        }

        [Benchmark]
        public void ComplexFunction1_inlined()
        {
            fres = func3(point);
        }

        [Benchmark]
        public void ComplexFunction1_withAlloc()
        {
            fres = func3(new DenseVector(new double[1] { x0 }));
        }

        [Benchmark]
        public void ComplexFunction1_parsed()
        {
            fres = funcRank1Complex(point);
        }

        [Benchmark]
        public void ComplexFunction1_parsedWithAlloc()
        {
            fres = funcRank1Complex(new DenseVector(new double[1] { x0 }));
        }

        [Benchmark]
        public void ComplexFunction4_inlined()
        {
            fres = func4(point);
        }

        [Benchmark]
        public void ComplexFunction4_withAlloc()
        {
            fres = func4(new DenseVector(new double[4] { x0, x1, x2, x3 }));
        }

        [Benchmark]
        public void ComplexFunction4_parsed()
        {
            fres = funcRank4Complex(point);
        }

        [Benchmark]
        public void ComplexFunction4_parsedWithAlloc()
        {
            fres = funcRank4Complex(new DenseVector(new double[4] { x0, x1, x2, x3 }));
        }
    }
}
