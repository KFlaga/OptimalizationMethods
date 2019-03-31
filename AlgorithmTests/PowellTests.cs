using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Qfe;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Linq;

namespace AlgorithmTests
{
    [TestClass]
    public class PowellTests
    {
        double error = 1e-3;
        List<Func<Vector, double>> unconstrained = new List<Func<Vector, double>>();

        List<Func<Vector, double>> withConstraints(params Func<Vector, double>[] funcs)
        {
            return new List<Func<Vector, double>>(funcs);
        }

        GaussSiedlerWithPowellPenalty getAlgorithm(Func<Vector, double> func, int rank, List<Func<Vector, double>> constraints)
        {
            return new GaussSiedlerWithPowellPenalty()
            {
                MaxIterations = 100,
                MinFunctionChange = error,
                MinPositionChange = error,
                Task = new Task
                (
                    rank: rank,
                    costFunction: new CostFunction(func, null),
                    constraints: constraints.Select((c) => new Constraint(c, ConstraintType.GreaterEqual, null)).ToList(),
                    input: ""
                )
            };
        }

        [TestMethod]
        public void square_unconstrained()
        {
            GaussSiedlerWithPowellPenalty sut = getAlgorithm((x) => x[0] * x[0], 1, unconstrained);

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

                Assert.AreEqual(0.0, result.CurrentCost, error);
                Assert.AreEqual(0.0, result.CurrentPoint[0], error);
            }
        }

        [TestMethod]
        public void square_simpleConstraint()
        {
            GaussSiedlerWithPowellPenalty sut = getAlgorithm((x) => x[0] * x[0], 1,
                withConstraints((x) => x[0] - 2.0) // x >= 2
            );

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

                Assert.AreEqual(4.0, result.CurrentCost, error);
                Assert.AreEqual(2.0, result.CurrentPoint[0], error);
            }
        }

        [TestMethod]
        public void square_multipleConstraints()
        {
            GaussSiedlerWithPowellPenalty sut = getAlgorithm((x) => x[0] * x[0], 1,
                withConstraints((x) => x[0] + 2.0, // x >= -2
                                (x) => -x[0] - 1.0,  // x <= -1
                                (x) => x[0] * x[0] - 2.0)  // x^2 >= 2
            );

            List<double[]> initialPoints = new List<double[]>()
            {
                new double[] { 0.0 },
                new double[] { -2.0 },
                new double[] { -Math.Sqrt(2.0) },
                new double[] { 100.0 },
                new double[] { -100.0 }
            };

            foreach (var x0 in initialPoints)
            {
                sut.InitialPoint = new DenseVector(x0);
                var result = sut.Solve();

                Assert.AreEqual(2.0, result.CurrentCost, error);
                Assert.AreEqual(-Math.Sqrt(2.0), result.CurrentPoint[0], error);
            }
        }

        [TestMethod]
        public void multipleInputs_singleMinimum_oneConstraint()
        {
            // f(x,y) = (x - 1) ^ 2 + (y + 2) ^ 2 + 4
            GaussSiedlerWithPowellPenalty sut = getAlgorithm((x) => (x[0] - 1.0) * (x[0] - 1.0) + (x[1] + 2.0) * (x[1] + 2.0) + 4.0, 2,
                withConstraints((x) => x[1] + 1.0) // y >= -1
            );

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

                Assert.AreEqual(5.0, result.CurrentCost, error);
                Assert.AreEqual(1.0, result.CurrentPoint[0], error);
                Assert.IsTrue(
                    Math.Abs(result.CurrentPoint[1] + 1.0) <= error ||
                    Math.Abs(result.CurrentPoint[1] + 3.0) <= error
                );
            }
        }


        [TestMethod]
        public void multipleInputs_singleMinimum_multipleConstraint()
        {
            // f(x,y) = (x - 1) ^ 2 + (y + 2) ^ 2 + 4
            GaussSiedlerWithPowellPenalty sut = getAlgorithm((x) => (x[0] - 1.0) * (x[0] - 1.0) + (x[1] + 2.0) * (x[1] + 2.0) + 4.0, 2,
                withConstraints((x) => -x[1] + 0.8, // y <= 0.8
                                (x) => -x[0] + 0.5, // x <= 0.5
                                (x) => x[0] + x[1] - 1.0) // x + y >= 1
            );

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

                Assert.AreEqual(10.5, result.CurrentCost, error);
                Assert.AreEqual(0.5, result.CurrentPoint[0], error);
                Assert.AreEqual(0.5, result.CurrentPoint[1], error);
            }
        }

        [TestMethod]
        public void independentMinima()
        {
            GaussSiedlerWithPowellPenalty sut = getAlgorithm((x) => Math.Sin(x[0]) + Math.Cos(x[1]), 2,
                withConstraints((x) => x[0], // x >= 0
                                (x) => -x[1]) // y <= 0
            );

            List<double[]> initialPoints = new List<double[]>()
            {
                new double[] { Math.PI * 1.5, -Math.PI }, // Minimum cost
                new double[] { -Math.PI * 0.5, Math.PI }, // Minimum function, but constraints not met, x[0] has local minima in x[0] = 0
                new double[] { Math.PI * 1.5 - 0.8, -Math.PI * 3.0 - 1.0 }, // Close to minimum cost
            };

            List<double[]> expectedPoints = new List<double[]>() // x = 2kPi + 3Pi/2, y = 2kPi + Pi
            {
                new double[] { Math.PI * 1.5, -Math.PI },
                new double[] { 0.0, -Math.PI },
                new double[] { Math.PI * 1.5, -Math.PI * 3.0 }
            };

            for (int i = 0; i < initialPoints.Count; ++i)
            {
                sut.InitialPoint = new DenseVector(initialPoints[i]);
                var result = sut.Solve();

                Assert.AreEqual(expectedPoints[i][0], result.CurrentPoint[0], error);
                Assert.AreEqual(expectedPoints[i][1], result.CurrentPoint[1], error);
            }
        }

        [TestMethod]
        public void dependentMinima()
        {
            GaussSiedlerWithPowellPenalty sut = getAlgorithm((x) => Math.Sin(x[0]) * Math.Cos(x[1]), 2,
                withConstraints((x) => x[0], // x >= 0
                                (x) => -x[1]) // y <= 0
            );

            List<double[]> initialPoints = new List<double[]>()
            {
                new double[] { Math.PI * 1.5, -Math.PI * 2.0 }, // Minimum cost
                new double[] { Math.PI * 2.5 - 0.8, -Math.PI * 3.0 - 1.0 }, // Close to minimum cost
                new double[] { -Math.PI * 1.5, Math.PI }, // Minimum function, but constraints not met, has local minima in x[0] = 0
                new double[] { -Math.PI * 0.5, -Math.PI }, // Minimum function, but constraints not met, should go over minima in x[0] = 0
            };

            List<double[]> expectedPoints = new List<double[]>() // (x = 2kPi + 3Pi/2 && y = 2kPi) || (x = 2kPi + Pi/2 && y = 2kPi + Pi)
            {
                new double[] { Math.PI * 1.5, -Math.PI * 2.0 },
                new double[] { Math.PI * 2.5, -Math.PI * 3.0 },
                new double[] { Math.PI * 0.0, -Math.PI * 0.0 },
                new double[] { Math.PI * 0.5, -Math.PI * 1.0 },
            };

            for (int i = 0; i < initialPoints.Count; ++i)
            {
                sut.InitialPoint = new DenseVector(initialPoints[i]);
                var result = sut.Solve();

                Assert.AreEqual(expectedPoints[i][0], result.CurrentPoint[0], error);
                Assert.AreEqual(expectedPoints[i][1], result.CurrentPoint[1], error);
            }
        }

        [TestMethod]
        public void noMinimaUnlessConstrined()
        {
            GaussSiedlerWithPowellPenalty sut = getAlgorithm((x) => x[0] * x[0] * x[0], 1,
                withConstraints((x) => x[0] - 2.0) // x >= 2
            );

            List<double[]> initialPoints = new List<double[]>()
            {
                new double[] { 0.0 },
                new double[] { 2.0 },
                new double[] { 100.0 },
                //new double[] { -100.0 } // TODO: this doesn't work -> f(x) >> c(x) and it goes to -inf. We need general fallback when such thing happens
            };

            foreach (var x0 in initialPoints)
            {
                sut.InitialPoint = new DenseVector(x0);
                var result = sut.Solve();

                Assert.AreEqual(8.0, result.CurrentCost, error);
                Assert.AreEqual(2.0, result.CurrentPoint[0], error);
            }
        }
    }
}
