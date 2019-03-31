using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Qfe
{
    public class GaussSiedlerWithPowellPenalty : IterativeMinimalization
    {
        public PowellPenaltyFunction PowellPenalty { get; set; }
        
        protected double cost(Vector x)
        {
            return Task.Cost.Function(x) + penalty(x);
        }

        protected double penalty(Vector x)
        {
            return PowellPenalty.Evaluate(x);
        }

        protected bool constraitsMet(Vector x)
        {
            return Task.Constraints.All((c) => c.IsMet(x));
        }

        protected override void Init()
        {
            if (PowellPenalty == null)
            {
                PowellPenalty = new ZeroThetaPenalty(Task.Constraints, ZeroThetaPenalty.SigmaChangeMethod.Multiplicative, 2.0, 2.0);
            }
            else
            {
                PowellPenalty.Reset();
            }

            double fvalue = Task.Cost.Function(InitialPoint);
            double fcost = fvalue + penalty(InitialPoint);
            iterations = new List<IterationResults>(MaxIterations * Task.Rank + 1)
            {
                new IterationResults()
                {
                    Iteration = 0,
                    CurrentPoint = InitialPoint,
                    CurrentFunction = fvalue,
                    CurrentCost = fcost,
                    CostraintsMet = constraitsMet(InitialPoint),
                    LastFunuctionChange = Math.Abs(fcost),
                    LastPointChange = InitialPoint.L2Norm(),

                }
            };
        }

        protected override IterationResults SolveIteration(Vector point, int iteration)
        {
            DirectionalMinimalization directionalMinimizer = new NewtonMethod()
            {
                Function = cost,
                MaxError = MinFunctionChange,
                MinPointChange = MinPositionChange
            };
            
            double lastValue = iterations.Last().CurrentCost;
            Vector lastPoint = iterations.Last().CurrentPoint;
            for (int direction = 0; direction < Task.Rank; ++direction)
            {
                directionalMinimizer.Direction = direction;
                var result = directionalMinimizer.FindMinimum(point);

                point = result.Point;

                iterations.Add(new IterationResults()
                {
                    Iteration = iteration,
                    CurrentPoint = point,
                    CurrentFunction = Task.Cost.Function(point),
                    CurrentCost = result.Value,
                    CostraintsMet = constraitsMet(point),
                    LastFunuctionChange = Math.Abs(result.Value - lastValue),
                    LastPointChange = (point - lastPoint).L2Norm(),
                });
            }
            PowellPenalty.NextIteration();
            return iterations.Last();
        }
    }
}
