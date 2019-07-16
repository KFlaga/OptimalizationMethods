using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Qfe
{
    public class GaussSiedler : IterativeMinimalization
    {
        protected override IterationResults SolveIteration(Vector point, int iteration)
        {
            DirectionalMinimalization directionalMinimizer = new NewtonMethod()
            {
                Function = Task.Cost.Function,
                MaxError = MinFunctionChange,
                MinPointChange = MinPositionChange,
                DisableModTwo = DisableModTwo
            };

            double lastValue = iterations.Last().CurrentFunction;
            Vector lastPoint = iterations.Last().CurrentPoint;


            for (int direction = 0; direction < Task.Dim; ++direction)
            {
                directionalMinimizer.Direction = direction;
                var result = directionalMinimizer.FindMinimum(point);

                iterattions1 += directionalMinimizer.iterations1;
                iterattions2 += directionalMinimizer.iterations2;

                point = result.Point;
                if (Math.Abs(point[0] - 1.970) < 0.01 && Math.Abs(point[1] - 3.574) < 0.01)
                {
                    int x = 0;
                }

                iterations.Add(new IterationResults()
                {
                    Iteration = iteration,
                    CurrentPoint = point,
                    CurrentFunction = result.Value,
                    CurrentCost = result.Value,
                    LastFunuctionChange = Math.Abs(result.Value - lastValue),
                    LastPointChange = (point - lastPoint).L2Norm(),
                    MaxConstraintValue = 0.0
                });
            }
            return iterations.Last();
        }

        protected override void Init()
        {
            double fvalue = Task.Cost.Function(InitialPoint);
            iterations = new List<IterationResults>(MaxIterations * Task.Dim + 1)
            {
                new IterationResults()
                {
                    Iteration = 0,
                    CurrentPoint = InitialPoint,
                    CurrentFunction = fvalue,
                    CurrentCost = fvalue,
                    LastFunuctionChange = Math.Abs(fvalue),
                    LastPointChange = InitialPoint.L2Norm(),
                    MaxConstraintValue = 0.0
                }
            };
        }
    }
}
