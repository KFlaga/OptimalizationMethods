using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Qfe
{
    public class GaussSiedlerWithPowellPenalty : IterativeMinimalization
    {
        public PowellPenaltyFunction PowellPenalty { get; set; }
        public double MaxConstraintError { get; set; } = 1e-3;

        public override bool Terminated { get => base.Terminated; set { base.Terminated = value; gaussSiedler.Terminated = value; } }

        private GaussSiedler gaussSiedler = new GaussSiedler();
        
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
                PowellPenalty = new ZeroThetaPenalty(Task.Constraints, 2.0, 2.0);
            }
            else
            {
                PowellPenalty.Reset();
            }

            gaussSiedler.MaxIterations = MaxIterations;
            gaussSiedler.MinFunctionChange = MinFunctionChange;
            gaussSiedler.MinPositionChange = MinPositionChange;
            gaussSiedler.Task = new Task(Task.Dim, new CostFunction(cost, null), null, "");

            double fvalue = Task.Cost.Function(InitialPoint);
            double fcost = fvalue + penalty(InitialPoint);
            iterations = new List<IterationResults>(MaxIterations * Task.Dim * 4 + 1)
            {
                new IterationResults()
                {
                    Iteration = 0,
                    CurrentPoint = InitialPoint,
                    CurrentFunction = fvalue,
                    CurrentCost = fcost,
                    LastFunuctionChange = Math.Abs(fcost),
                    LastPointChange = InitialPoint.L2Norm(),
                    MaxConstraintValue = PowellPenalty.MaxConstraint(InitialPoint)
                }
            };
        }

        protected override IterationResults SolveIteration(Vector point, int iteration)
        {
            gaussSiedler.InitialPoint = point;
            gaussSiedler.Solve();

            foreach(var result in gaussSiedler.Results.Skip(1))
            {
                iterations.Add(new IterationResults()
                {
                    Iteration = iteration,
                    CurrentPoint = result.CurrentPoint,
                    LastFunuctionChange = result.LastFunuctionChange,
                    LastPointChange = result.LastPointChange,
                    
                    CurrentCost = cost(result.CurrentPoint),
                    CurrentFunction = Task.Cost.Function(result.CurrentPoint),
                    MaxConstraintValue = PowellPenalty.Evaluate(result.CurrentPoint)
                });
            }

            PowellPenalty.NextIteration(iterations.Last().CurrentPoint);
            return iterations.Last();
        }

        protected override bool ShouldEnd()
        {
            var lastIteration = iterations.Last();
            return Terminated ||
                   lastIteration.Iteration > MaxIterations ||
                   (lastIteration.MaxConstraintValue < MaxConstraintError &&
                    lastIteration.LastPointChange < MinPositionChange &&
                    lastIteration.LastFunuctionChange < MinFunctionChange);
        }
    }
}
