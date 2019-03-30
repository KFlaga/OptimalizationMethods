using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Qfe;
using MathNet.Numerics.LinearAlgebra.Double;

namespace AlgorithmTests
{
    [TestClass]
    public class GoldenSectionTests
    {
        DenseVector makePoint(params double[] values)
        {
            return new DenseVector(values);
        }

        [TestMethod]
        public void GoldenSection_quadric() // TODO: test multimodal
        {
            double func(Vector x) => x[0] * x[0];
            GoldenSectionElementaryDirections sut = new GoldenSectionElementaryDirections()
            {
                Function = func,
                MaxError = 1e-3,
                Direction = 0,
                LeftInterval = -2,
                RightInterval = 1
            };

            var result = sut.FindMinimum(makePoint(0.0));

            Assert.AreEqual(0.0, result.Value, 1e-3);
            Assert.AreEqual(0.0, result.Point[0], 1e-3);

            sut.LeftInterval = -100;
            sut.RightInterval = 2;
            result = sut.FindMinimum(makePoint(0.0));

            Assert.AreEqual(0.0, result.Value, 1e-3);
            Assert.AreEqual(0.0, result.Point[0], 1e-3);

            sut.LeftInterval = 4;
            sut.RightInterval = 6;
            result = sut.FindMinimum(makePoint(0.0));

            Assert.AreEqual(16.0, result.Value, 16 * 1e-3);
            Assert.AreEqual(4.0, result.Point[0], 1e-3);
        }

        [TestMethod]
        public void GoldenSectionWithInterpolation_quadric() // TODO: test linear and multimodal
        {
            double func(Vector x) => x[0] * x[0];
            GoldenSectionWithInterpolatedStep sut = new GoldenSectionWithInterpolatedStep()
            {
                Function = func,
                MaxError = 1e-3,
                Direction = 0,
            };

            var result = sut.FindMinimum(makePoint(0.0));

            Assert.AreEqual(0.0, result.Value, 1e-3);
            Assert.AreEqual(0.0, result.Point[0], 1e-3);
            
            result = sut.FindMinimum(makePoint(2.0));

            Assert.AreEqual(0.0, result.Value, 1e-3);
            Assert.AreEqual(0.0, result.Point[0], 1e-3);
            
            result = sut.FindMinimum(makePoint(200.0));

            Assert.AreEqual(0.0, result.Value, 1e-3);
            Assert.AreEqual(0.0, result.Point[0], 1e-3);
            
            result = sut.FindMinimum(makePoint(-2000.0));

            Assert.AreEqual(0.0, result.Value, 1e-3);
            Assert.AreEqual(0.0, result.Point[0], 1e-3);
        }
    }

    [TestClass]
    public class NewtonMethodTests
    {
        DenseVector makePoint(params double[] values)
        {
            return new DenseVector(values);
        }
        
        [TestMethod]
        public void quadric()
        {
            double func(Vector x) => x[0] * x[0];
            NewtonMethod sut = new NewtonMethod()
            {
                Function = func,
                MaxError = 1e-3,
                MinPointChange = 1e-3,
                Direction = 0,
            };

            var result = sut.FindMinimum(makePoint(0.0));

            Assert.AreEqual(0.0, result.Value, 1e-3);
            Assert.AreEqual(0.0, result.Point[0], 1e-3);

            result = sut.FindMinimum(makePoint(2.0));

            Assert.AreEqual(0.0, result.Value, 1e-3);
            Assert.AreEqual(0.0, result.Point[0], 1e-3);

            result = sut.FindMinimum(makePoint(200.0));

            Assert.AreEqual(0.0, result.Value, 1e-3);
            Assert.AreEqual(0.0, result.Point[0], 1e-3);

            result = sut.FindMinimum(makePoint(-2000.0));

            Assert.AreEqual(0.0, result.Value, 1e-3);
            Assert.AreEqual(0.0, result.Point[0], 1e-3);
        }

        [TestMethod]
        public void multimodal()
        {
            // x ^ 4 - 0.5 * x ^ 3 - 2 * x ^ 2;
            // max12 -> 0.0, 0.0
            // min1 -> (-0.83 ~0.01, -0.617 ~0.002)
            // min2 -> ( 1.20 ~0.01, -1.670 ~0.002)

            double func(Vector x) => x[0] * x[0] * (x[0] * x[0] - 0.5 * x[0] - 2.0);
            NewtonMethod sut = new NewtonMethod()
            {
                Function = func,
                MaxError = 1e-3,
                MinPointChange = 1e-3,
                Direction = 0,
            };

            var result = sut.FindMinimum(makePoint(-100.0));

            Assert.AreEqual(-0.617, result.Value, 2e-3);
            Assert.AreEqual(-0.83, result.Point[0], 1e-2);

            result = sut.FindMinimum(makePoint(-0.01));

            Assert.AreEqual(-0.617, result.Value, 2e-3);
            Assert.AreEqual(-0.83, result.Point[0], 1e-2);

            result = sut.FindMinimum(makePoint(0.01));

            Assert.AreEqual(-1.670, result.Value, 2e-3);
            Assert.AreEqual(1.2, result.Point[0], 1e-2);

            result = sut.FindMinimum(makePoint(1.201));

            Assert.AreEqual(-1.670, result.Value, 2e-3);
            Assert.AreEqual(1.2, result.Point[0], 1e-2);

            result = sut.FindMinimum(makePoint(1000));

            Assert.AreEqual(-1.670, result.Value, 2e-3);
            Assert.AreEqual(1.2, result.Point[0], 1e-2);

            result = sut.FindMinimum(makePoint(0.0));

            Assert.AreEqual(-0.617, result.Value, 2e-3);
            Assert.AreEqual(-0.83, result.Point[0], 1e-2);
        }
    }
}
