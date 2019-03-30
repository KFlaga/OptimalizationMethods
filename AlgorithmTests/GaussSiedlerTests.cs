using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Qfe;
using MathNet.Numerics.LinearAlgebra.Double;

namespace AlgorithmTests
{
    [TestClass]
    public class GaussSiedlerTests
    {
        double error = 1e-3;
        GaussSiedler gaussSiedler(Func<Vector, double> func, int rank = 1)
        {
            return  new GaussSiedler()
            {
                MaxIterations = 100,
                MinFunctionChange = error,
                MinPositionChange = error,
                Task = new Task
                (
                    rank: rank,
                    costFunction: new CostFunction(func, null),
                    constraints: new List<Constraint>(),
                    input: ""
                )
            };
        }

        [TestMethod]
        public void square()
        {
            GaussSiedler sut = gaussSiedler((x) => x[0] * x[0]);

            List<double[]> initialPoints = new List<double[]>()
            {
                new double[] { 0.0 },
                new double[] { 1.0 },
                new double[] { 100.0 },
                new double[] { -100.0 }
            };

            foreach(var x0 in initialPoints)
            {
                sut.InitialPoint = new DenseVector(x0);
                var result = sut.Solve();

                Assert.AreEqual(0.0, result.CurrentCost, error);
                Assert.AreEqual(0.0, result.CurrentPoint[0], error);
            }
        }

        [TestMethod]
        public void quadric()
        {
            GaussSiedler sut = gaussSiedler((x) => 2.0 * x[0] * x[0] - 4 * x[0] + 4);

            List<double[]> initialPoints = new List<double[]>()
            {
                new double[] { 0.0 },
                new double[] { 1.0 },
                new double[] { 100.0 },
                new double[] { -100.0 }
            };

            foreach (var x0 in initialPoints)
            {
                sut.InitialPoint = new DenseVector(x0);
                var result = sut.Solve();

                Assert.AreEqual(2.0, result.CurrentCost, error);
                Assert.AreEqual(1.0, result.CurrentPoint[0], error);
            }
        }

        [TestMethod]
        public void periodicFunction_findsClosestMinimum()
        {
            GaussSiedler sut = gaussSiedler((x) => Math.Sin(x[0]));
            sut.MinFunctionChange = 1e-6;
            sut.MinPositionChange = 1e-6;

            List<double[]> initialPoints = new List<double[]>()
            {
                new double[] { 0.0 },
                new double[] { -Math.PI * 0.5 },
                new double[] { 4.0 },
                new double[] { 10.0 }
            };

            List<double[]> expectedPoints = new List<double[]>()
            {
                new double[] { -Math.PI * 0.5 },
                new double[] { -Math.PI * 0.5 },
                new double[] { Math.PI * 1.5 }, // It fails here, as there is maximum which hides minumum, so function goes to 2nd closest minumum
                new double[] { Math.PI * 3.5 }
            };

            for (int i = 0; i < initialPoints.Count; ++i)
            {
                sut.InitialPoint = new DenseVector(initialPoints[i]);
                var result = sut.Solve();

                Assert.AreEqual(-1.0, result.CurrentCost, error);
                Assert.AreEqual(expectedPoints[i][0], result.CurrentPoint[0], error);
            }
        }

        [TestMethod]
        public void multipleInputs_singleMinimum()
        {
            // f(x,y) = (x - 1) ^ 2 + (y + 2) ^ 2 + 4
            GaussSiedler sut = gaussSiedler((x) => (x[0] - 1.0) * (x[0] - 1.0) + (x[1] + 2.0) * (x[1] + 2.0) + 4.0, 2);

            List<double[]> initialPoints = new List<double[]>()
            {
                new double[] { 1.0, 2.0 },
                new double[] { 0.0, 0.0 },
                new double[] { 1.0, 4.0 },
                new double[] { -5.0, -2.0 },
                new double[] { -100.0, 100.0 },
            };

            for (int i = 0; i < initialPoints.Count; ++i)
            {
                sut.InitialPoint = new DenseVector(initialPoints[i]);
                var result = sut.Solve();

                Assert.AreEqual(4.0, result.CurrentCost, error);
                Assert.AreEqual(1.0, result.CurrentPoint[0], error);
                Assert.AreEqual(-2.0, result.CurrentPoint[1], error);
            }
        }

        [TestMethod]
        public void multipleInputs_multipleMinima()
        {
            GaussSiedler sut = gaussSiedler((x) => Math.Sin(x[0]) + Math.Cos(x[1]), 2);

            List<double[]> initialPoints = new List<double[]>()
            {
                new double[] { -Math.PI * 0.5, Math.PI },
                new double[] { -Math.PI * 0.5 + 1.0, Math.PI - 1.0 },
                new double[] { Math.PI * 1.5 - 0.8, -Math.PI * 5.0 + 0.4 }
            };

            List<double[]> expectedPoints = new List<double[]>()
            {
                new double[] { -Math.PI * 0.5, Math.PI }, // x = 2kPi + 3Pi/2, y = 1kPi + Pi
                new double[] { -Math.PI * 0.5, Math.PI },
                new double[] { Math.PI * 1.5, -Math.PI * 5.0 }
            };

            for (int i = 0; i < initialPoints.Count; ++i)
            {
                sut.InitialPoint = new DenseVector(initialPoints[i]);
                var result = sut.Solve();

                Assert.AreEqual(-2.0, result.CurrentCost, error);
                Assert.AreEqual(expectedPoints[i][0], result.CurrentPoint[0], error);
                Assert.AreEqual(expectedPoints[i][1], result.CurrentPoint[1], error);
            }
        }

        [TestMethod]
        public void startFromMiddleOfTwoMinimas()
        {
            GaussSiedler sut = gaussSiedler((x) => Math.Cos(x[0]));

            List<double[]> initialPoints = new List<double[]>()
            {
                new double[] { 0.0 },
                new double[] { Math.PI * 2.0 },
            };

            foreach (var x0 in initialPoints)
            {
                sut.InitialPoint = new DenseVector(x0);
                var result = sut.Solve();

                Assert.AreEqual(-1.0, result.CurrentCost, error);
            }
        }

        [TestMethod]
        public void noMinima()
        {
            GaussSiedler sut = gaussSiedler((x) => x[0] * x[0] * x[0]);
            // TODO: what to do in such case?
        }
    }
}
