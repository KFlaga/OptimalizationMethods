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

            double fvalue = 0.0;
            for(int direction = 0; direction < Task.Rank; ++direction)
            {
                directionalMinimizer.Direction = direction;
                var result = directionalMinimizer.FindMinimum(point);

                point = result.Point;
                fvalue = result.Value;

                iterations.Add(new IterationResults()
                {
                    Iteration = iteration,
                    CurrentPoint = point,
                    CurrentFunction = fvalue,
                    CurrentCost = fvalue,
                    CostraintsMet = true,
                    LastFunuctionChange = Math.Abs(fvalue - iterations.Last().CurrentFunction),
                    LastPointChange = (point - iterations.Last().CurrentPoint).L2Norm(),
                });
            }
            return iterations.Last();
        }

        protected override void Init()
        {
            double fvalue = cost(InitialPoint);
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
