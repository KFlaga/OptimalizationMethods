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
                MinPointChange = MinPositionChange
            };

            double lastValue = iterations.Last().CurrentFunction;
            Vector lastPoint = iterations.Last().CurrentPoint;
            for(int direction = 0; direction < Task.Rank; ++direction)
            {
                directionalMinimizer.Direction = direction;
                var result = directionalMinimizer.FindMinimum(point);

                point = result.Point;

                iterations.Add(new IterationResults()
                {
                    Iteration = iteration,
                    CurrentPoint = point,
                    CurrentFunction = result.Value,
                    CurrentCost = result.Value,
                    CostraintsMet = true,
                    LastFunuctionChange = Math.Abs(result.Value - lastValue),
                    LastPointChange = (point - lastPoint).L2Norm(),
                });
            }
            return iterations.Last();
        }

        protected override void Init()
        {
            double fvalue = Task.Cost.Function(InitialPoint);
            iterations = new List<IterationResults>(MaxIterations * Task.Rank + 1)
            {
                new IterationResults()
                {
                    Iteration = 0,
                    CurrentPoint = InitialPoint,
                    CurrentFunction = fvalue,
                    CurrentCost = fvalue,
                    CostraintsMet = true,
                    LastFunuctionChange = Math.Abs(fvalue),
                    LastPointChange = InitialPoint.L2Norm(),
                }
            };
        }
    }
}
