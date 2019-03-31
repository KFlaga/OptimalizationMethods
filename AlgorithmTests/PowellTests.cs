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
        const double error = 1e-3;

        public struct TestPoint
        {
            public double[] Initial { get; private set; }
            public double[] ExpectedResult { get; private set; }
            public Func<Vector, bool> CustomMatcher { get; private set; }

            public TestPoint(double[] i, double[] expectedPoint)
            {
                Initial = i;
                ExpectedResult = expectedPoint;
                CustomMatcher = null;
            }

            public TestPoint(double[] i, Func<Vector, bool> match)
            {
                Initial = i;
                ExpectedResult = null;
                CustomMatcher = match;
            }

            public void Check(Vector x)
            {
                if(CustomMatcher != null)
                {
                    Assert.IsTrue(CustomMatcher(x));
                }
                else
                {
                    for (int i = 0; i < ExpectedResult.Length; ++i)
                    {
                        Assert.AreEqual(ExpectedResult[i], x[i], error);
                    }
                }
            }
        }

        public class TestCase
        {
            public Func<Vector, double> Function;
            public int Rank;
            public List<Constraint> Constraints;
            public List<TestPoint> Points;
        }

        void Execute(TestCase test, PowellPenaltyFunction penaltyFunction)
        {
            GaussSiedlerWithPowellPenalty sut = new GaussSiedlerWithPowellPenalty()
            {
                MaxIterations = 100,
                MinFunctionChange = error,
                MinPositionChange = error,
                PowellPenalty = penaltyFunction,
                Task = new Task
                (
                    rank: test.Rank,
                    costFunction: new CostFunction(test.Function, null),
                    constraints: test.Constraints,
                    input: ""
                )
            };

            foreach(var p in test.Points)
            {
                sut.InitialPoint = new DenseVector(p.Initial);
                var result = sut.Solve();
                p.Check(result.CurrentPoint);
            }
        }
        
        static List<Constraint> makeConstraints(params Func<Vector, double>[] funcs)
        {
            return new List<Func<Vector, double>>(funcs).Select((c) => new Constraint(c, ConstraintType.GreaterEqual, null)).ToList();
        }

        static TestCase data_square_unconstrained()
        {
            return new TestCase
            {
                Function = (x) => x[0] * x[0],
                Rank = 1,
                Constraints = new List<Constraint>(),
                Points = new List<TestPoint>()
                {
                    new TestPoint(new double[] { 0.0 }, new double[] { 0.0 }),
                    new TestPoint(new double[] { 1.0 }, new double[] { 0.0 }),
                    new TestPoint(new double[] { 100.0 }, new double[] { 0.0 }),
                    new TestPoint(new double[] { -100.0 }, new double[] { 0.0 }),
                }
            };
        }

        [TestMethod]
        public void ZeroTheta_square_unconstrained()
        {
            var data = data_square_unconstrained();
            Execute(data, new ZeroThetaPenalty(data.Constraints, 2.0, 2.0));
        }

        [TestMethod]
        [Ignore]
        public void SimpleTheta_square_unconstrained()
        {
            var data = data_square_unconstrained();
            Execute(data, new SimpleThetaPenalty(data.Constraints, 1.0, 4.0));
        }

        [TestMethod]
        public void ProperTheta_square_unconstrained()
        {
            var data = data_square_unconstrained();
            Execute(data, new ProperThetaPenalty(data.Constraints));
        }

        static TestCase data_square_simpleConstraint()
        {
            return new TestCase
            {
                Function = (x) => x[0] * x[0],
                Rank = 1,
                Constraints = makeConstraints((x) => x[0] - 2.0),
                Points = new List<TestPoint>()
                {
                    new TestPoint(new double[] { 0.0 }, new double[] { 2.0 }),
                    new TestPoint(new double[] { 1.0 }, new double[] { 2.0 }),
                    new TestPoint(new double[] { 100.0 }, new double[] { 2.0 }),
                    new TestPoint(new double[] { -100.0 }, new double[] { 2.0 }),
                }
            };
        }

        [TestMethod]
        public void ZeroTheta_square_simpleConstraint()
        {
            var data = data_square_simpleConstraint();
            Execute(data, new ZeroThetaPenalty(data.Constraints, 2.0, 2.0));
        }

        [TestMethod]
        [Ignore]
        public void SimpleTheta_square_simpleConstraint()
        {
            var data = data_square_simpleConstraint();
            Execute(data, new SimpleThetaPenalty(data.Constraints, 1.0, 4.0));
        }

        [TestMethod]
        public void ProperTheta_square_simpleConstraint()
        {
            var data = data_square_simpleConstraint();
            Execute(data, new ProperThetaPenalty(data.Constraints));
        }

        static TestCase data_square_multipleConstraints()
        {
            return new TestCase
            {
                Function = (x) => x[0] * x[0],
                Rank = 1,
                Constraints = makeConstraints((x) => x[0] + 2.0, // x >= -2
                                              (x) => -x[0] - 1.0,  // x <= -1
                                              (x) => x[0] * x[0] - 2.0),  // x^2 >= 2,
                Points = new List<TestPoint>()
                {
                    new TestPoint(new double[] { 0.0 }, new double[] { -Math.Sqrt(2.0) }),
                    new TestPoint(new double[] { -2.0 }, new double[] { -Math.Sqrt(2.0) }),
                    new TestPoint(new double[] { -Math.Sqrt(2.0) }, new double[] { -Math.Sqrt(2.0) }),
                    new TestPoint(new double[] { 100.0 }, new double[] { -Math.Sqrt(2.0) }),
                    new TestPoint(new double[] { -100.0 }, new double[] { -Math.Sqrt(2.0) }),
                }
            };
        }

        [TestMethod]
        public void ZeroTheta_square_multipleConstraints()
        {
            var data = data_square_multipleConstraints();
            Execute(data, new ZeroThetaPenalty(data.Constraints, 2.0, 2.0));
        }

        [TestMethod]
        [Ignore]
        public void SimpleTheta_square_multipleConstraints() // It doesn't work well yet
        {
            var data = data_square_multipleConstraints();
            Execute(data, new SimpleThetaPenalty(data.Constraints, 1.0, 10.0));
        }

        [TestMethod]
        public void ProperTheta_square_multipleConstraints()
        {
            var data = data_square_multipleConstraints();
            Execute(data, new ProperThetaPenalty(data.Constraints));
        }

        static TestCase data_multipleInputs_singleMinimum_oneConstraint()
        {
            // f(x,y) = (x - 1) ^ 2 + (y + 2) ^ 2 + 4
            return new TestCase
            {
                Function = (x) => (x[0] - 1.0) * (x[0] - 1.0) + (x[1] + 2.0) * (x[1] + 2.0) + 4.0,
                Rank = 2,
                Constraints = makeConstraints((x) => x[1] + 1.0), // y >= -1
                Points = new List<TestPoint>()
                {
                    new TestPoint(new double[] { 1.0, 2.0 }, new double[] { 1.0, -1.0 }),
                    new TestPoint(new double[] { 0.0, 0.0 }, new double[] { 1.0, -1.0 }),
                    new TestPoint(new double[] { 1.0, 4.0 }, new double[] { 1.0, -1.0 }),
                    new TestPoint(new double[] { -5.0, -2.0 }, new double[] { 1.0, -1.0 }),
                    new TestPoint(new double[] { -5.0, -5.0 }, new double[] { 1.0, -1.0 }),
                    new TestPoint(new double[] { -100.0, 100.0 }, new double[] { 1.0, -1.0 }),
                }
            };
        }

        [TestMethod]
        public void ZeroTheta_multipleInputs_singleMinimum_oneConstraint()
        {
            var data = data_multipleInputs_singleMinimum_oneConstraint();
            Execute(data, new ZeroThetaPenalty(data.Constraints, 2.0, 2.0));
        }
        
        [TestMethod]
        [Ignore]
        public void SimpleTheta_multipleInputs_singleMinimum_oneConstraint()
        {
            var data = data_multipleInputs_singleMinimum_oneConstraint();
            Execute(data, new SimpleThetaPenalty(data.Constraints, 1.0, 4.0));
        }

        [TestMethod]
        public void ProperTheta_multipleInputs_singleMinimum_oneConstraint()
        {
            var data = data_multipleInputs_singleMinimum_oneConstraint();
            Execute(data, new ProperThetaPenalty(data.Constraints));
        }

        static TestCase data_multipleInputs_singleMinimum_multipleConstraintt()
        {
            // f(x,y) = (x - 1) ^ 2 + (y + 2) ^ 2 + 4
            return new TestCase
            {
                Function = (x) => (x[0] - 1.0) * (x[0] - 1.0) + (x[1] + 2.0) * (x[1] + 2.0) + 4.0,
                Rank = 2,
                Constraints = makeConstraints((x) => -x[1] + 0.8, // y <= 0.8
                                              (x) => -x[0] + 0.5, // x <= 0.5
                                              (x) => x[0] + x[1] - 1.0), // x + y >= 1
                Points = new List<TestPoint>()
                {
                    new TestPoint(new double[] { 1.0, 2.0 }, new double[] { 0.5, 0.5 }),
                    new TestPoint(new double[] { 0.0, 0.0 }, new double[] { 0.5, 0.5 }),
                    new TestPoint(new double[] { 1.0, 4.0 }, new double[] { 0.5, 0.5 }),
                    new TestPoint(new double[] { -5.0, -2.0 }, new double[] { 0.5, 0.5 }),
                    new TestPoint(new double[] { -100.0, 100.0 }, new double[] { 0.5, 0.5 }),
                }
            };
        }
        
        [TestMethod]
        public void ZeroTheta_multipleInputs_singleMinimum_multipleConstraint()
        {
            var data = data_multipleInputs_singleMinimum_multipleConstraintt();
            Execute(data, new ZeroThetaPenalty(data.Constraints, 2.0, 2.0));
        }

        [TestMethod]
        [Ignore]
        public void SimpleTheta_multipleInputs_singleMinimum_multipleConstraint()
        {
            var data = data_multipleInputs_singleMinimum_multipleConstraintt();
            Execute(data, new SimpleThetaPenalty(data.Constraints, 1.0, 4.0));
        }

        [TestMethod]
        public void ProperTheta_multipleInputs_singleMinimum_multipleConstraint()
        {
            var data = data_multipleInputs_singleMinimum_multipleConstraintt();
            Execute(data, new ProperThetaPenalty(data.Constraints));
        }

        static TestCase data_independentMinima()
        {
            Func<Vector, double> func = (x) => Math.Sin(x[0]) + Math.Cos(x[1]);
            return new TestCase
            {
                Function = func,
                Rank = 2,
                Constraints = makeConstraints((x) => x[0], // x >= 0
                                              (x) => -x[1]), // y <= 0
                Points = new List<TestPoint>()
                {
                    new TestPoint(new double[] { Math.PI * 1.5, -Math.PI }, new double[] { Math.PI * 1.5, -Math.PI }),  // Minimum cost
                    new TestPoint(new double[] { Math.PI * 1.5 - 0.8, -Math.PI * 3.0 - 1.0 }, new double[] { Math.PI * 1.5, -Math.PI * 3.0 }), // Close to minimum cost
                    // Minimum function, but constraints not met, x[0] has local minima in x[0] = 0
                    // Method may fall into this minima, but it may as well skip over it if directional minimizer make long enough step
                    new TestPoint(new double[] { -Math.PI * 0.5, Math.PI }, (x) =>
                        {
                            return (Math.Abs(x[0]) <= 5.0 * error && Math.Abs(x[1] + Math.PI) <= 5.0 * error) ||
                                    Math.Abs(func(x) + 2.0) <= error;
                        }
                    ), 
                }
            };
        }

        [TestMethod]
        public void ZeroTheta_independentMinima()
        {
            var data = data_independentMinima();
            Execute(data, new ZeroThetaPenalty(data.Constraints, 2.0, 2.0));
        }

        [TestMethod]
        [Ignore]
        public void SimpleTheta_independentMinima()
        {
            var data = data_independentMinima();
            Execute(data, new SimpleThetaPenalty(data.Constraints, 1.0, 4.0));
        }

        [TestMethod]
        public void ProperTheta_independentMinima()
        {
            var data = data_independentMinima();
            Execute(data, new ProperThetaPenalty(data.Constraints));
        }

        static TestCase data_dependentMinima()
        {
            Func<Vector, double> func = (x) => Math.Sin(x[0]) * Math.Cos(x[1]);
            return new TestCase
            {
                Function = func,
                Rank = 2,
                Constraints = makeConstraints((x) => x[0], // x >= 0
                                              (x) => -x[1]), // y <= 0
                Points = new List<TestPoint>()
                {
                    new TestPoint(new double[] { Math.PI * 1.5, -Math.PI * 2.0 }, new double[] { Math.PI * 1.5, -Math.PI * 2.0 }),  // Minimum cost
                    new TestPoint(new double[] { Math.PI * 2.5 - 0.8, -Math.PI * 3.0 - 1.0 }, new double[] { Math.PI * 2.5, -Math.PI * 3.0 }), // Close to minimum cost
                    // Minimum function, but constraints not met, has local minima in x[0] = 0
                    // Method may fall into this minima, but it may as well skip over it if directional minimizer make long enough step
                    new TestPoint(new double[] { -Math.PI * 1.5, Math.PI }, (x) =>
                        {
                            return (Math.Abs(x[0]) <= 5.0 * error && Math.Abs(x[1]) <= 5.0 * error) ||
                                    Math.Abs(func(x) + 1.0) <= error;
                        }
                    ),
                    new TestPoint(new double[] { -Math.PI * 0.5, -Math.PI }, new double[] { Math.PI * 0.5, -Math.PI * 1.0 }), // Minimum function, but constraints not met, should go over minima in x[0] = 0
                }
            };
        }

        [TestMethod]
        public void ZeroTheta_dependentMinima()
        {
            var data = data_dependentMinima();
            Execute(data, new ZeroThetaPenalty(data.Constraints, 2.0, 2.0));
        }

        [TestMethod]
        [Ignore] // 4th point doesn;t work
        public void SimpleTheta_dependentMinima()
        {
            var data = data_dependentMinima();
            Execute(data, new SimpleThetaPenalty(data.Constraints, 1.0, 4.0));
        }

        [TestMethod]
        public void ProperTheta_dependentMinima()
        {
            var data = data_dependentMinima();
            Execute(data, new ProperThetaPenalty(data.Constraints));
        }

        static TestCase data_noMinimaUnlessConstrined()
        {
            return new TestCase
            {
                Function = (x) => x[0] * x[0] * x[0],
                Rank = 1,
                Constraints = makeConstraints((x) => x[0] - 2.0), // x >= 2
                Points = new List<TestPoint>()
                {
                    new TestPoint(new double[] { 0.0 }, new double[] { 2.0 }),
                    new TestPoint(new double[] { 2.0 }, new double[] { 2.0 }),
                    new TestPoint(new double[] { 100.0 }, new double[] { 2.0 }),
                    //new TestPoint(new double[] { -100.0 }, new double[] { 2.0 }), // TODO: this doesn't work -> f(x) >> c(x) and it goes to -inf. We need general fallback when such thing happens
                }
            };
        }

        [TestMethod]
        public void ZeroTheta_noMinimaUnlessConstrined()
        {
            var data = data_noMinimaUnlessConstrined();
            Execute(data, new ZeroThetaPenalty(data.Constraints, 2.0, 2.0));
        }

        [TestMethod]
        [Ignore]
        public void SimpleTheta_noMinimaUnlessConstrined()
        {
            var data = data_noMinimaUnlessConstrined();
            Execute(data, new SimpleThetaPenalty(data.Constraints, 1.0, 4.0));
        }

        [TestMethod]
        public void ProperTheta_noMinimaUnlessConstrined()
        {
            var data = data_noMinimaUnlessConstrined();
            Execute(data, new ProperThetaPenalty(data.Constraints));
        }
    }
}
